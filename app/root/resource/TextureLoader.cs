namespace App.Root.Resource;

using App.Root.Mesh;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;

class TextureLoader {
    private static string DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource/texture/");

    public class TextureData {
        public int id;
        public int width;
        public int height;

        public TextureData(int id, int width, int height) {
            this.id = id;
            this.width = width;
            this.height = height;
        }
    }
    
    // Get Size
    public static (int width, int height) getSize(int texId) {
        GL.BindTexture(TextureTarget.Texture2D, texId);
        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int w);
        GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int h);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        return (w, h);
    }

    // Set Texture
    public static int setTex(MeshData meshData) {
        int val = meshData.texPath is string texPath 
            ? TextureLoader.load(texPath) 
            : -1;

        return val;
    }

    // Get Or Load Tex Id
    public static int getOrLoadTexId(string? path, Dictionary<string, int> pathCache) {
        if(path == null) return -1;

        if(!pathCache.TryGetValue(path, out int id)) {
            try {
                id = load(path);
                pathCache[path] = id;
            } catch {
                id = -1;
                pathCache[path] = -1;
            }
        }

        return id;
    }

    // Set Instanced Texture
    public static List<int> setInstancedTex(MeshRenderer meshRenderer, List<Vector3> positions, List<int>? texIds, List<string?>? texPaths = null) {
        List<int> finalTexIds = new List<int>();

        if(texIds != null && texIds.Count == positions.Count) {
            finalTexIds = texIds;
            meshRenderer.cacheInstanceTexIds = texIds;
        } else if(texPaths != null && texPaths.Count == positions.Count) {
            finalTexIds = texPaths.Select(path => {
                if(string.IsNullOrEmpty(path)) return -1;
                return getOrLoadTexId(path, meshRenderer.texPathCache);
            }).ToList();

            meshRenderer.cacheInstanceTexIds = finalTexIds;
        } else {
            meshRenderer.cacheInstanceTexIds = new List<int>();
        }

        return finalTexIds;
    }

    /**
    
        Load
    
        */
    public static TextureData loadTexData(string fileName) {
        string path = Path.Combine(DIR, fileName);
        try {
            StbImage.stbi_set_flip_vertically_on_load(0);
            using var stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data
            );
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return new TextureData(texId, image.Width, image.Height);
        } catch(Exception err) {
            Console.Error.WriteLine("Failed to load texture: " + path + " - " + err.Message);
            return new TextureData(-1, 0, 0);
        }
    }

    public static int load(string fileName) {
        return loadTexData(fileName).id;
    }
}