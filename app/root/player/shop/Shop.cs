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

        Mapper.Set<Shop>();
    }

    /**
     * 
     * Open
     *
     */
    public void open() {
        // Open Key
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

        // Close Key
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