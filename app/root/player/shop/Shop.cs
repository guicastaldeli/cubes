namespace App.Root.Player.Shop;
using App.Root.Chat;
using App.Root.UI;
using App.Root.Input;
using App.Root.World.Platform;
using App.Root.Screen;
using App.Root.Player.Skills;
using OpenTK.Windowing.GraphicsLibraryFramework;
using App.Root.Utils;
using System.Reflection;

/**

    Shop Purchase system...

    */
[AttributeUsage(AttributeTargets.Method)]
public class ShopPriceChecker : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class ShopPurchase : Attribute {}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ShopPurchasable : Attribute {
    public string? Type { get; set; }

    public ShopPurchasable(string? Type = null) {
        this.Type = Type;
    }
}

public static class ShopPurchaseHandler {
    private static Dictionary<string, Func<object, bool>> priceCheckers = new();
    private static Dictionary<string, Action<object>> purchasers = new();
    private static Dictionary<string, string> typeToHandlerMap = new();
    
    private static bool initialized = false;

    // Can Afford
    public static bool CanAfford(object item) {
        if(item == null) return false;

        var typeName = item.GetType().Name.ToLower();
        if(priceCheckers.TryGetValue(typeName, out var checker)) return checker(item);

        var singular = WordInflector.ToSingular(typeName);
        if(priceCheckers.TryGetValue(singular, out checker)) return checker(item);
        
        Console.WriteLine($"[ShopPurchaseHandler] No price checker found for {typeName}");

        return false;
    }

    // Purchase
    public static bool Purchase(object item) {
        if(item == null) return false;

        var typeName = item.GetType().Name.ToLower();
        if(purchasers.TryGetValue(typeName, out var purchaser)) {
            if(!CanAfford(item)) {
                Console.WriteLine($"[ShopPurchaseHandler] Cannot afford {typeName}");
                return false;
            }

            purchaser(item);
            return true;
        }

        var singular = WordInflector.ToSingular(typeName);
        if(purchasers.TryGetValue(singular, out purchaser)) {
            if(!CanAfford(item)) {
                Console.WriteLine($"[ShopPurchaseHandler] Cannot afford {typeName}");
                return false;
            }

            purchaser(item);
            return true;
        }

        Console.WriteLine($"[ShopPurchaseHandler] No purchase handler found for {typeName}");
        return false;
    }

    /**
     *
     * Register
     *
     */
    // Register Type
    public static void RegisterType(Type type) {
        if(!initialized) RegisterAll();

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach(var method in methods) {
            var attr = method.GetCustomAttribute<ShopPriceChecker>();
            if(attr != null) {
                var param = method.GetParameters();
                if(param.Length == 1) {
                    var paramType = param[0].ParameterType;
                    var typeName = paramType.Name.ToLower();

                    if(method.IsStatic) {
                        var func = (Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), method);
                        priceCheckers[typeName] = func;
                        Console.WriteLine($"[ShopPurchaseHandler] Registered price checker for {typeName}");
                    }
                }
            }
        }
        foreach(var method in methods) {
            var attr = method.GetCustomAttribute<ShopPurchase>();
            if(attr != null) {
                var param = method.GetParameters();
                if(param.Length == 1) {
                    var paramType = param[0].ParameterType;
                    var typeName = paramType.Name.ToLower();

                    if(method.IsStatic) {
                        var action = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), method);
                        purchasers[typeName] = action;
                        Console.WriteLine($"[ShopPurchaseHandler] Registered price checker for {typeName}");
                    }
                }
            }
        }
        foreach(var method in methods) {
            var attr = method.GetCustomAttribute<ShopPurchasable>();
            if(attr != null) {
                var param = method.GetParameters();
                if(param.Length == 1) {
                    var paramType = param[0].ParameterType;
                    var typeName = attr.Type ?? paramType.Name.ToLower();

                    if(method.ReturnType == typeof(bool)) {
                        var func = (Func<object, bool>)Delegate.CreateDelegate(typeof(Func<object, bool>), method);
                        priceCheckers[typeName] = func;
                        Console.WriteLine($"[ShopPurchaseHandler] Registered combined price checker for {typeName}");
                    } else {
                        if(method.IsStatic) {
                            var action = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), method);
                            purchasers[typeName] = action;
                            Console.WriteLine($"[ShopPurchaseHandler] Registered combined purchaser for {typeName}");
                        }
                    }
                }
            }
        }
    }

    // Register All
    public static void RegisterAll() {
        if(initialized) return;
        initialized = true;

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => {
                try { return a.GetTypes(); }
                catch { return new Type[0]; }
            })
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach(var type in types) {
            var hasAttribute = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(m => m.GetCustomAttribute<ShopPurchase>() != null ||
                    m.GetCustomAttribute<ShopPriceChecker>() != null ||
                    m.GetCustomAttribute<ShopPurchasable>() != null);
            if(hasAttribute) {
                RegisterType(type);
            }
        }
    }
}

/**

    Shop main class.

    */
class Shop {
    private class Data {
        private class List {
            [Convert("data")] private List<PlatformThemes.Theme>? Themes;
            [Convert("data")] private List<SkillsData.Skill>? Skills;

            public List() {}
        }

        private static List instance = new List();
        private static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
            .ToDictionary(
                m => m.GetCustomAttribute<ConverterKey>()!.Key,
                m => m
            );

        // Handle Mouse Click
        public static void HandleMouseClick() {
            GlobalInputHandler.HandleMouseClick();
        }
 
        /**
         *
         * Init
         *
         */
        public static void Init() {
            var type = typeof(List);
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach(var field in fields) {
                var attr = field.GetCustomAttribute<ConvertAttribute>();
                if(attr == null) continue;

                var fieldType = field.FieldType;
                var converterKey = attr.Converter;

                if(converters.TryGetValue(converterKey, out var converter)) {
                    try {
                        object? result = null;

                        var param = converter.GetParameters();
                        object?[] args = new object?[param.Length];

                        for(int i = 0; i < param.Length; i++) {
                            var paramType = param[i].ParameterType;

                            if(paramType == typeof(Type)) {
                                args[i] = fieldType;
                            } else if(paramType == typeof(object)) {
                                args[i] = null;
                            } else if(paramType.IsValueType) {
                                args[i] = Activator.CreateInstance(paramType);
                            } else {
                                args[i] = null;
                            }
                        }

                        result = converter.Invoke(null, args);
                        if(result != null) {
                            field.SetValue(instance, result);

                            var id = ThisData.FindId(result);
                            if(id != null) {
                                DocParser.ReplaceObject(id, result);
                                Console.WriteLine($"[Shop.Data] Loaded {field.Name} with {id}");
                            }

                            var typeName = field.Name.ToLower();
                            var handlerType = GlobalInputHandler.FindHandlerType(result);
                            if(handlerType != null) {
                                GlobalInputHandler.RegisterType(typeName, handlerType);
                                Console.WriteLine($"[Shop.Data] Registered {typeName} -> {handlerType.Name}");
                            }
                        }
                    } catch (Exception err) {
                        Console.WriteLine($"[Shop.Data] Error converting {field.Name}: {err.Message}");
                    }
                }
            }
        }
    }

    public const string ID = "shop";

    private Input input;
    private UIController uiController;

    public Shop(Input input, UIController uiController) {
        this.input = input;
        this.uiController = uiController;

        Data.Init();

        GlobalInputHandler.Register();

        Mapper.Set<Shop>();
    }

    // Handle Mouse Click
    public void handleMouseClick() {
        Data.HandleMouseClick();
    }

    /**
     * 
     * Open
     *
     */
    public void open() {
        Mapper.Key(Keys.O, pressed => {
            if(!pressed) return;
            if(ChatController.getInstance().isOpen()) return;
            if(input.onPauseOverlayOpen()) return;

            uiController.toggle(ID);
            bool isActive = uiController.getActive() == ID;

            Action action = isActive ? () =>
                input.unlockMouse() : () =>
                input.lockMouse();
            action();
        });

        Mapper.Key(Keys.Escape, pressed => {
            if(!pressed) return;
            if(uiController.getActive() != ID) return;

            uiController.hide();
            input.lockMouse();
        });
    }

    /**
     * 
     * Close
     *
     */
    public void close() {
        
    }
}