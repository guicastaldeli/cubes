namespace App.Root.Resource;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

class TextureLoader {
    private static string DIR = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "resource/texture/"
    );

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