/**
    
    Util Load Mesh Data to dynamically
    loads data or models.
    
    */
using App.Root.Mesh;

class LoadMeshData {
    public const string EXT_NAME = ".obj";

    /**
    
        Load
    
        */
    public static MeshData? L(string meshType, MeshData data) {
        if(data != null && data.isModel) {
            string modelPath = data.modelPath ?? meshType;
            Console.WriteLine($"Reloading model from: {modelPath}");
            return MeshModelLoader.loadModel(modelPath);
        }

        if(data != null) {
            Console.WriteLine($"Using provided mesh data for: {meshType}");
            return data;
        }

        if(meshType.Contains("/") && meshType.EndsWith(EXT_NAME)) {
            Console.WriteLine($"Loading OBJ directly from path: {meshType}");
            MeshData objData = MeshModelLoader.loadModel(meshType);
            objData.isModel = true;
            objData.modelPath = meshType;
            return objData;
        }

        try {
            Console.WriteLine($"Loading from Lua config: {meshType}");
            return MeshDataLoader.load(meshType);
        } catch (Exception ex) {
            Console.Error.WriteLine($"Failed to load {meshType}: {ex.Message}");
            return null;
        }
    }
}