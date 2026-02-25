using System.Text;

namespace App.Root.Shaders;

class ShaderLoader {
    private static readonly Dictionary<string, string> loadedShaders = new();
    private static readonly string INCLUDE_PREFIX = "#include ";
    private static readonly string DIR = "shaders/";

    // Process Includes
    private static string processIncludes(string content, string parentFile) {
        StringBuilder res = new();
        string parentDir = getParentDir(parentFile);

        foreach(string l in content.Split('\n')) {
            string trimmed = l.Trim();
            if(trimmed.StartsWith(INCLUDE_PREFIX)) { 
                string file = trimmed[INCLUDE_PREFIX.Length..].Trim().Trim('"', '\'');

                string path;
                if(file.StartsWith("/")) {
                    path = file[1..];
                } else if(file.Contains("../") || file.Contains("./")) {
                    path = resRelativePath(parentDir, file);
                } else {
                    path = parentDir + file;
                }

                string includeContent;
                try {
                    includeContent = loadFile(path);
                } catch {
                    Console.WriteLine("err!");
                    includeContent = loadFile(file);
                }
                includeContent = processIncludes(includeContent, path);
                includeContent = stripVerDirective(includeContent);
                res.AppendLine(includeContent);
            } else {
                res.AppendLine(l);
            }
        }

        res.ToString();
    }

    ///
    /// 
    /// Load
    ///  
    /// 
    public static string load(string fileName) {
        if(loadedShaders.ContainsKey(fileName)) return loadedShaders[fileName];

        string content = loadFile(fileName);
        content = processIncludes(content, fileName);
        loadedShaders[fileName] = content;
        return content;
    }

    private static string loadFile(string fileName) {
        string path = Path.Combine(DIR, fileName);
        if(!File.Exists(path)) {
            throw new IOException("Shader file not found!: " + fileName);
        }
        return File.ReadAllText(path);
    }
}