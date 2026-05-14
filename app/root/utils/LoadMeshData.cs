/**
    
    Util Load Mesh Data to dynamically
    loads data or models.
    
    */
namespace App.Root.Utils;
using App.Root.Mesh;

public static class LoadMeshData {
    public const string EXT_NAME = ".obj";

    /**
    
        Load
    
        */
    public static MeshData? L(string meshType, MeshData? data, string? colliderShape = null, float? colliderRadius = null) {
        MeshData? result = null;
        
        if(data != null && data.isModel) {
            string modelPath = data.modelPath ?? meshType;
            result = MeshModelLoader.loadModel(modelPath);
            result.isModel = true;
            result.modelPath = modelPath;
            result.isEntity = data.isEntity;
        } 
        else if(data != null) {
            result = data;
        } 
        else if(meshType.Contains("/") && meshType.EndsWith(EXT_NAME)) {
            result = MeshModelLoader.loadModel(meshType);
            result.isModel = true;
            result.modelPath = meshType;
        } 
        else {
            try {
                Console.WriteLine($"Loading from Lua config: {meshType}");
                result = MeshDataLoader.load(meshType);
            } catch (Exception ex) {
                Console.Error.WriteLine($"Failed to load {meshType}: {ex.Message}");
                return null;
            }
        }
        
        if(result != null && colliderShape != null) {
            float radius = colliderRadius ?? 1.0f;
            
            result.colliderShape = colliderShape;
            result.colliderRadius = radius;
        }

        return result;
    }
}