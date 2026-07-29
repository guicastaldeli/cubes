using System.Collections;

namespace App.Root.Utils;

public static class CollectionFactory {
    private static Dictionary<Type, Func<IEnumerable, object>> listFactories = new();
    private static Dictionary<Type, Func<IDictionary<string, object>, object>> dictFactories = new();

    static CollectionFactory() {
        /**
            *******
            *   
            
                List Factories 

                *
                *
            ***
         */
        listFactories[typeof(string)] = items => {
            var list = new List<string>();
            foreach(var item in items) {
                list.Add(item.ToString()!);
            }
            return list;
        };
        listFactories[typeof(int)] = items => {
            var list = new List<int>();
            foreach(var item in items) {
                list.Add(Convert.ToInt32(item));
            }
            return list;
        };
        listFactories[typeof(float)] = items => {
            var list = new List<float>();
            foreach(var item in items) {
                list.Add(Convert.ToSingle(item));
            }
            return list;
        };
        listFactories[typeof(double)] = items => {
            var list = new List<double>();
            foreach(var item in items) {
                list.Add(Convert.ToDouble(item));
            }
            return list;
        };
        listFactories[typeof(bool)] = items => {
            var list = new List<bool>();
            foreach(var item in items) {
                list.Add(Convert.ToBoolean(item));
            }
            return list;
        };
        listFactories[typeof(object)] = items => {
            var list = new List<object>();
            foreach(var item in items) {
                list.Add(item);
            }
            return list;
        };

        /******
            ******

            
            Dictionary Factories ***


            ***
            *****

         */
        dictFactories[typeof(Dictionary<string, string>)] = items => {
            var dict = new Dictionary<string, string>();
            foreach(var i in items) {
                dict[i.Key] = i.Value.ToString()!;
            }
            return dict;
        };
        dictFactories[typeof(Dictionary<string, int>)] = items => {
            var dict = new Dictionary<string, int>();
            foreach(var i in items) {
                dict[i.Key] = Convert.ToInt32(i.Value);
            }
            return dict;
        };
        dictFactories[typeof(Dictionary<string, float>)] = items => {
            var dict = new Dictionary<string, float>();
            foreach(var i in items) {
                dict[i.Key] = Convert.ToSingle(i.Value);
            }
            return dict;
        };
        dictFactories[typeof(Dictionary<string, object>)] = items => {
            var dict = new Dictionary<string, object>();
            foreach(var i in items) {
                dict[i.Key] = i.Value;
            }
            return dict;
        };
    }

    /********
    
        ********************

        Register

        *************
    
        */
    // Register List Factory
    public static void RegisterListFactory(Type elementType, Func<IEnumerable, object> factory) {
        listFactories[elementType] = factory;
    }

    // Register Dictionary Factory
    public static void RegisterDictionaryFactory(Type dictType, Func<IDictionary<string, object>, object> factory) {
        dictFactories[dictType] = factory;
    }

    /***
    
        Create
    
        *****/
    // Create List
    public static object CreateList(Type elementType, IEnumerable items) {
        if(listFactories.TryGetValue(elementType, out var factory)) {
            return factory(items);
        }
        if(listFactories.TryGetValue(typeof(object), out var fbFactory)) {
            return fbFactory(items);
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach(var item in items) list.Add(item);
        return list;
    }

    // Create Dictionary
    public static object CreateDictionary(Type keyType, Type valueType, IDictionary<string, object> items) {
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        if(dictFactories.TryGetValue(dictType, out var factory)) {
            return factory(items);
        }

        var dict = (IDictionary)Activator.CreateInstance(dictType)!;
        foreach(var i in items) {
            var key = Convert.ChangeType(i.Key, keyType);
            var value = Convert.ChangeType(i.Value, valueType);
            dict.Add(key, value);
        } 

        return dict;
    }
}