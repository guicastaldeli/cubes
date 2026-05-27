/**
    
    Element Entry util class
    
    */
namespace App.Root.Utils;
using System.Dynamic;
using App.Root.Screen;
using App.Root.UI;

static class ElementEntry {
    /**

        Entry
    
        */
    public class Entry<T> {
        public readonly string id;
        public T? el;

        public Entry(string id, T? el) {
            this.id = id;
            this.el = el;
        }

        public void setVisible(bool visible) {
            if(el is UIElement uiEl) uiEl.visible = visible;
            else if(el is ScreenElement screenEl) screenEl.visible = visible;
        }
    }

    /**
    
        Object
    
        */
    public class Object<T> : DynamicObject {
        private Dictionary<string, Entry<T>> map = new();

        public Object(Func<string, T?> resolver, params string[] ids) {
            foreach(var id in ids) {
                map[id] = new Entry<T>(id, resolver(id));
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result) {
            string key = binder.Name;
            
            if(map.TryGetValue(key, out var entry)) {
                result = entry;
                return true;
            }
            
            result = null;
            return false;
        }
    }

    /**
    
        Create
    
        */
    public static dynamic C<T>(Func<string, T?> resolver, IEnumerable<string> ids) {
        Object val = new Object<T>(resolver, ids.ToArray());
        return val;
    }

    public static dynamic C<T>(Func<string, T?> resolver, params string[] ids) {
        Object val = new Object<T>(resolver, ids);
        return val;
    }
}
