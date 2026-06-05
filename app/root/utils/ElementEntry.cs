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
    public class Entry<T> : DynamicObject {
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

        public override bool TryGetMember(GetMemberBinder binder, out object? result) {
            var prop = typeof(T).GetProperty(binder.Name);
            if(prop != null && el != null) {
                result = prop.GetValue(el);
                return true;
            }

            var field = typeof(T).GetField(binder.Name);
            if(field != null && el != null) {
                result = field.GetValue(el);
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value) {
            var prop = typeof(T).GetProperty(binder.Name);
            if(prop != null && el != null) {
                prop.GetValue(el);
                return true;
            }

            var field = typeof(T).GetField(binder.Name);
            if(field != null && el != null) {
                field.GetValue(el);
                return true;
            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
            var method = typeof(T).GetMethod(binder.Name);
            if(method != null && el != null) {
                result = method.Invoke(el, args);
                return true;
            }
            result = null;
            return false;
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
                result = entry.el;
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
