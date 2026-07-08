namespace App.Root.World.Entity;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using App.Root.Mesh;
using App.Root.Utils;

static class CacheMeshEntity {
    public static MethodInfo? cachedUpdateMethod = null;
    public static MethodInfo? cachedSetMethod = null;
    
    public static string[]? cachedParamNames = null;

    public static Type[]? cachedUpdateParamTypes = null;
    public static Type[]? cachedSetParamTypes = null;

    public static FieldInfo[] cachedFields = null!;
    public static Dictionary<FieldInfo, (string key, MethodInfo? converter)>? cachedFieldMeta = null;
    public static Func<Instance, object>[]? cachedFieldGetters = null;

    public static Dictionary<string, List<object>> cachedData = new();
    public static Dictionary<string, List<Instance>> cachedByMeshType = new();
    public static Dictionary<string, (object?[] args, IList[] lists)> cachedArgsByMeshType = new();

    /**
     *
     * Cache Fields
     *
     */
    // Set Cache Field
    public static void SetCacheField(string key, MethodInfo converter, FieldInfo field) {
        if(cachedFieldMeta != null) cachedFieldMeta[field] = (key, converter);
    }

    // Cache Fields
    public static void CacheFields() {
        if(cachedFields != null) return;
    
        cachedFields = typeof(Instance).GetFields(BindingFlags.Public | BindingFlags.Instance);
        cachedFieldMeta = new();
        
        foreach(var field in cachedFields) {
            var keyAttr = field.GetCustomAttribute<ConverterKey>();
            var converterAttr = field.GetCustomAttribute<ConvertAttribute>();
            string key = keyAttr?.Key ?? field.Name.ToLower();
            MethodInfo? converter = null;
            if(converterAttr != null) Instance.converters.TryGetValue(converterAttr.Converter, out converter);
            cachedFieldMeta[field] = (key, converter);
        }
    }

    // Cacge Field Getters
    public static void CacheFieldGettes() {
        if(cachedFieldGetters != null) return;

        CacheFields();

        cachedFieldGetters = cachedFields!.Select(field => {
            var param = Expression.Parameter(typeof(Instance), field.Name);
            var access = Expression.Field(param, field);
            var convert = Expression.Convert(access, typeof(object));
            return Expression.Lambda<Func<Instance, object>>(convert, param).Compile();
        }).ToArray();
    } 

    /**
     *
     * Cache Data
     *
     */
    public static void CacheData(string key, object val) {
        if(!cachedData.ContainsKey(key)) cachedData[key] = new List<object>();
        cachedData[key].Add(val);
    }

    /**
     *
     * Cache By Mesh Type
     *
     */
    public static void CacheByMeshType(string meshType, List<Instance> list) {
        if(!cachedByMeshType.ContainsKey(meshType)) cachedByMeshType[meshType] = new();
        cachedByMeshType[meshType].AddRange(list);
    }

    /**
     *
     * Cache Args
     *
     */
    public static void CacheArgs(string meshType, Type[] paramTypes, out (object?[] args, IList[] lists) cached) {
        if(cachedArgsByMeshType.TryGetValue(meshType, out cached)) return;

        var lists = cachedParamNames!.Select((key, i) => {
            var elemType = paramTypes[i].GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elemType);
            return (IList)Activator.CreateInstance(listType)!;
        }).ToArray();

        var args = lists.Cast<object?>().ToArray();
        cached = (args, lists);
        cachedArgsByMeshType[meshType] = cached;
    }

    /**
     *
     * Cache Param Names
     *
     */
    public static void CacheParamNames(IList[] lists, Dictionary<string, List<object>> data) {
        for(int i = 0; i < cachedParamNames!.Length; i++) {
            string key = cachedParamNames[i];
            lists[i].Clear();
            if(data.TryGetValue(key, out var val)) {
                foreach(var item in val) lists[i].Add(item);
            }
        }
    }

    /**
     *
     * Sync
     *
     */
    public static void Sync(MethodInfo[] methods) {
        cachedUpdateMethod = methods.First(m => m.Name == nameof(MeshRenderer.updateInstanceData) && m.GetParameters().Length > 1);
        cachedSetMethod = methods.First(m => m.Name == nameof(MeshRenderer.setInstanceData) && m.GetParameters().Length > 1);
        cachedParamNames = cachedUpdateMethod.GetParameters().Select(p => p.Name!.ToLower()).ToArray();
        cachedUpdateParamTypes = cachedUpdateMethod.GetParameters().Select(p => p.ParameterType).ToArray();
        cachedSetParamTypes = cachedSetMethod.GetParameters().Select(p => p.ParameterType).ToArray();
    }

    /**
     *
     * Clear
     *
     */
    // Clear Cached Data
    public static void ClearCachedData() {
        foreach(var l in cachedData.Values) l.Clear();
    }

    // Clear Cached by Mesh Types
    public static void ClearCachedByMeshTypes() {
        foreach(var l in cachedByMeshType.Values) l.Clear();
    }
}

