
/**

    Mesh Model Loader to load
    models or data models.

    */
namespace App.Root.Mesh;
using App.Root.Utils;
using OpenTK.Mathematics;
using System.Globalization;
using NLua;

/**

    Model Data

    */
class ModelData {
    public string name = "";
    
    public string modelPath = "";
    
    public string? texPath = "";

    public float[]? scale = null;

    public float[]? rotation = null;
    public string? rotationAxis = null;
    public float rotationSpeed = 0.0f;

    public string? colliderShape = null;
    public float colliderRadius = 1.0f;
}

/**

    Main Mesh Model Loader class.

    */
class MeshModelLoader {
    private static string DATA_DIR = AppDomain.CurrentDomain.BaseDirectory;

    // Generate Normals
    private static float[] generateNormals(float[] verts, int[] indices) {
        float[] norms = new float[verts.Length];

        for(int i = 0; i < indices.Length; i++) {
            int i0 = indices[i] * 3;
            int i1 = indices[i+1] * 3;
            int i2 = indices[i+2] * 3;

            Vector3 v0 = new Vector3(
                verts[i0],
                verts[i0+1],
                verts[i0+2]
            );
            Vector3 v1 = new Vector3(
                verts[i1],
                verts[i1+1],
                verts[i1+2]
            );
            Vector3 v2 = new Vector3(
                verts[i2],
                verts[i2+1],
                verts[i2+2]
            );

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 norm = Vector3.Cross(edge1, edge2);
            norm.Normalize();

            for(int j = 0; j < 3; j++) {
                int idx = indices[i+j] * 3;
                norms[idx] += norm.X;
                norms[idx+1] += norm.Y;
                norms[idx+2] += norm.Z;
            }
        }

        for(int i = 0; i < norms.Length; i += 3) {
            Vector3 n = new Vector3(
                norms[i],
                norms[i+1],
                norms[i+2]
            );
            n.Normalize();
            norms[i] = n.X;
            norms[i+1] = n.Y;
            norms[i+2] = n.Z;
        }

        return norms;
    }

    /**
    
        Parse
    
        */
    private static ModelData parseModelData(LuaTable table) {
        ModelData data = new ModelData();

        if(table["name"] is string name) data.name = name;
        if(table["path"] is string path) data.modelPath = path;
        if(table["texture"] is string texture) data.texPath = texture;

        if(table["scale"] is LuaTable scaleTable) {
            data.scale = ToFloatArray.C(scaleTable);
        } else if(table["scale"] is double scaleVal){
            float s = (float)scaleVal;
            data.scale = new float[] { s, s, s };
        }

        if(table["rotation"] is LuaTable rotationTable) {
            if(rotationTable["axis"] is string axis) data.rotationAxis = axis;
            if(rotationTable["speed"] is double speed) data.rotationSpeed = (float)speed;
        }

        if(table["collider"] is LuaTable colliderTable) {
            if(colliderTable["shape"] is string shape) data.colliderShape = shape;
            if(colliderTable["radius"] is double radius) data.colliderRadius = (float)radius; 
        }

        return data;
    }

    private static ModelData parseModelDataFromFile(Lua file) {
        ModelData data = new ModelData();

        if(file["name"] is string name) data.name = name;
        if(file["path"] is string path) data.modelPath = path;
        if(file["texture"] is string texture) data.texPath = texture;

        if(file["scale"] is LuaTable scaleTable) {
            data.scale = ToFloatArray.C(scaleTable);
        } else if(file["scale"] is double scaleVal){
            float s = (float)scaleVal;
            data.scale = new float[] { s, s, s };
        }

        if(file["rotation"] is LuaTable rotationTable) {
            if(rotationTable["axis"] is string axis) data.rotationAxis = axis;
            if(rotationTable["speed"] is double speed) data.rotationSpeed = (float)speed;
        }

        if(file["collider"] is LuaTable colliderTable) {
            if(colliderTable["shape"] is string shape) data.colliderShape = shape;
            if(colliderTable["radius"] is double radius) data.colliderRadius = (float)radius; 
        }

        return data;
    }
    /**
    
        Load
    
        */
    // Load Model
    public static MeshData loadModel(string fileName) {
        string ext = Path.GetExtension(fileName).ToLower();
        
        MeshData val = ext switch {
            LoadMeshData.EXT_NAME => loadObj(fileName),
            _ => throw new NotSupportedException($"File format {ext} not supported!")
        };
        return val;
    }

    // Load Obj
    private static MeshData loadObj(string fileName) {
        string path = Path.Combine(DATA_DIR, fileName);
        if(!File.Exists(path)) throw new IOException("Model file not found! " + path);

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> texCoords = new List<Vector2>();
        List<int> indices = new List<int>();

        List<float> finalVerts = new List<float>();
        List<float> finalNorms = new List<float>();
        List<float> finalTexCoords = new List<float>();

        Dictionary<string, int> vertCache = new Dictionary<string, int>();
        int currentIndex = 0;

        try {
            string[] lines = File.ReadAllLines(path);
            
            foreach(string line in lines) {
                string trimmed = line.Trim();
                if(string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length == 0) continue;

                switch(parts[0]) {
                    case "v":
                        if(parts.Length >= 4) {
                            verts.Add(new Vector3(
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture),
                                float.Parse(parts[3], CultureInfo.InvariantCulture)
                            ));
                        }
                        break;
                    case "vt":
                        if(parts.Length >= 3) {
                            texCoords.Add(new Vector2(
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture)
                            ));
                        }
                        break;
                    case "vn":
                        if(parts.Length >= 4) {
                            norms.Add(new Vector3(
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture),
                                float.Parse(parts[3], CultureInfo.InvariantCulture)
                            ));
                        }
                        break;
                    case "f":
                    if(parts.Length >= 4) {
                        for(int i = 1; i <= 3; i++) {
                            string vertStr = parts[i];
                            
                            if(!vertCache.ContainsKey(vertStr)) {
                                string[] vertParts = vertStr.Split('/');

                                int vIdx = int.Parse(vertParts[0]) - 1;
                                finalVerts.Add(verts[vIdx].X);
                                finalVerts.Add(verts[vIdx].Y);
                                finalVerts.Add(verts[vIdx].Z);

                                if(vertParts.Length > 1 && !string.IsNullOrEmpty(vertParts[1])) {
                                    int vtIdx = int.Parse(vertParts[1]) - 1;
                                    if(vtIdx >= 0 && vtIdx < texCoords.Count) {
                                        finalTexCoords.Add(texCoords[vtIdx].X);
                                        finalTexCoords.Add(texCoords[vtIdx].Y);
                                    } else {
                                        finalTexCoords.Add(0.0f);
                                        finalTexCoords.Add(0.0f);
                                    }
                                } else {
                                    finalTexCoords.Add(0.0f);
                                    finalTexCoords.Add(0.0f);
                                }

                                if(vertParts.Length > 2 && !string.IsNullOrEmpty(vertParts[2])) {
                                    int vnIdx = int.Parse(vertParts[2]) - 1;
                                    if(vnIdx >= 0 && vnIdx < norms.Count) {
                                        finalNorms.Add(norms[vnIdx].X);
                                        finalNorms.Add(norms[vnIdx].Y);
                                        finalNorms.Add(norms[vnIdx].Z);
                                    } else {
                                        finalNorms.Add(0.0f);
                                        finalNorms.Add(1.0f);
                                        finalNorms.Add(0.0f);
                                    }
                                } else {
                                    finalNorms.Add(0.0f);
                                    finalNorms.Add(1.0f);
                                    finalNorms.Add(0.0f);
                                }

                                vertCache[vertStr] = currentIndex;
                                indices.Add(currentIndex);
                                currentIndex++;
                            } else {
                                indices.Add(vertCache[vertStr]);
                            }
                        }
                        
                        if(parts.Length >= 5) {
                            indices.Add(vertCache[parts[1]]);
                            indices.Add(vertCache[parts[3]]);
                            
                            string vertStr = parts[4];
                            if(!vertCache.ContainsKey(vertStr)) {
                                string[] vertParts = vertStr.Split('/');

                                int vIdx = int.Parse(vertParts[0]) - 1;
                                finalVerts.Add(verts[vIdx].X);
                                finalVerts.Add(verts[vIdx].Y);
                                finalVerts.Add(verts[vIdx].Z);

                                if(vertParts.Length > 1 && !string.IsNullOrEmpty(vertParts[1])) {
                                    int vtIdx = int.Parse(vertParts[1]) - 1;
                                    if(vtIdx >= 0 && vtIdx < texCoords.Count) {
                                        finalTexCoords.Add(texCoords[vtIdx].X);
                                        finalTexCoords.Add(texCoords[vtIdx].Y);
                                    } else {
                                        finalTexCoords.Add(0.0f);
                                        finalTexCoords.Add(0.0f);
                                    }
                                } else {
                                    finalTexCoords.Add(0.0f);
                                    finalTexCoords.Add(0.0f);
                                }

                                if(vertParts.Length > 2 && !string.IsNullOrEmpty(vertParts[2])) {
                                    int vnIdx = int.Parse(vertParts[2]) - 1;
                                    if(vnIdx >= 0 && vnIdx < norms.Count) {
                                        finalNorms.Add(norms[vnIdx].X);
                                        finalNorms.Add(norms[vnIdx].Y);
                                        finalNorms.Add(norms[vnIdx].Z);
                                    } else {
                                        finalNorms.Add(0.0f);
                                        finalNorms.Add(1.0f);
                                        finalNorms.Add(0.0f);
                                    }
                                } else {
                                    finalNorms.Add(0.0f);
                                    finalNorms.Add(1.0f);
                                    finalNorms.Add(0.0f);
                                }

                                vertCache[vertStr] = currentIndex;
                                indices.Add(currentIndex);
                                currentIndex++;
                            } else {
                                indices.Add(vertCache[vertStr]);
                            }
                        }
                    }
                    break;
                }
            }

            Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for(int i = 0; i < finalVerts.Count; i += 3) {
                Vector3 vert = new Vector3(
                    finalVerts[i],
                    finalVerts[i+1],
                    finalVerts[i+2]
                );

                minBounds = Vector3.ComponentMin(minBounds, vert);
                maxBounds = Vector3.ComponentMax(maxBounds, vert);
            }

            Vector3 center = (minBounds + maxBounds) / 2.0f;
            Vector3 size = maxBounds - minBounds;
            Vector3 vertexOffset = new Vector3(center.X, minBounds.Y, center.Z);
            float bottomY = minBounds.Y;
            float offset = -minBounds.Y;
            
            for(int i = 0; i < finalVerts.Count; i += 3) {
                finalVerts[i] -= center.X;
                finalVerts[i+1] -= bottomY;
                finalVerts[i+2] -= center.Z;
            }

            string meshId = Path.GetFileNameWithoutExtension(fileName);
            MeshData meshData = new MeshData(meshId, meshId);

            meshData.setVertices(finalVerts.ToArray());
            meshData.setIndices(indices.ToArray());

            if(finalNorms.Count > 0) {
                meshData.setNormals(finalNorms.ToArray());
            } else {
                meshData.setNormals(generateNormals(finalVerts.ToArray(), indices.ToArray()));
            }

            if(finalTexCoords.Count > 0) {
                meshData.setTexCoords(finalTexCoords.ToArray());
            }

            meshData.originalSize = size;
            meshData.originalMinBounds = minBounds;
            meshData.originalMaxBounds = maxBounds;
            meshData.collisionOffset = new Vector3(0, offset, 0);

            return meshData;
        } catch(Exception err) {
            throw new Exception("Failed to load OBJ file: " + fileName, err);
        }
    }

    // Load Data
    public static List<ModelData> loadData(string fileName) {
        string path = Path.Combine(DATA_DIR, fileName);
        if(!File.Exists(path)) throw new IOException("Data file not found: " + path);

        try {
            using Lua data = new Lua();
            data.DoFile(path);

            List<ModelData> models = new List<ModelData>();

            if(data["models"] is LuaTable modelsTable) {
                for(int i = 1; i <= modelsTable.Values.Count; i++) {
                    if(modelsTable[i] is LuaTable modelTable) {
                        models.Add(parseModelData(modelTable));
                    }
                }
            } else {
                models.Add(parseModelDataFromFile(data));
            }

            return models;
        } catch(Exception err) {
            throw new Exception("Failed to load file: " + fileName, err);
        }
    }

    // Load With Config
    public static MeshData loadWithConfig(ModelData config) {
        MeshData meshData = loadModel(config.modelPath);
        
        if(config.scale != null) {
            meshData.setScale(config.scale);
        }
        if(config.rotationAxis != null && config.rotationSpeed > 0) {
            meshData.addData(MeshData.DataType.ROTATION_SPEED, config.rotationAxis);
            meshData.addData(MeshData.DataType.ROTATION_SPEED, config.rotationSpeed);
        }
        if(config.colliderShape != null) {
            meshData.colliderShape = config.colliderShape;
            meshData.colliderRadius = config.colliderRadius;
        }

        return meshData;
    }
}