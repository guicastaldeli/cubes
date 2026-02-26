using System.Text;

namespace App.Root.Shaders;

class ShaderLoader {
    private static readonly Dictionary<string, string> loadedShaders = new();
    private static readonly string INCLUDE_PREFIX = "#include ";
    private static readonly string DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shaders/main/");

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

        return res.ToString();
    }

    private static string getParentDir(string path) {
        int lastSlash = path.LastIndexOf('/');
        return lastSlash > 0 ? path[..(lastSlash+1)] : ""; 
    }

    private static string resRelativePath(string baseDir, string relativePath) {
        string[] baseParts = baseDir.Split('/');
        string[] relativeParts = relativePath.Split('/');

        int upCount = 0;
        int relativeStart = 0;
        for(int i = 0; i < relativeParts.Length; i++) {
            if(relativeParts[i] == "..") {
                upCount++;
                relativeStart = i+1;
            }
            else if(relativeParts[i] == ".") {
                relativeStart = i+1;
            }
            else {
                break;
            }
        }

        StringBuilder res = new();
        for(int i = 0; i < baseParts.Length - upCount; i++) {
            if(!string.IsNullOrEmpty(baseParts[i])) {
                res.Append(baseParts[i] + "/");
            }
        }
        for(int i = relativeStart; i < relativeParts.Length; i++) {
            res.Append(relativeParts[i]);
            if(i < relativeParts.Length - 1) res.Append('/');
        }

        return res.ToString();
    }

    private static string stripVerDirective(string content) {
        StringBuilder res = new();
        bool verFound = false;
        foreach(string l in content.Split('\n')) {
            if(l.Trim().StartsWith("#version")) {
                if(!verFound) {
                    res.AppendLine(l);
                    verFound = true;
                }
            } else {
                res.AppendLine(l);
            }
        }

        return res.ToString();
    }

    /// 
    /// Load
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

    public static void clearCache() => loadedShaders.Clear();
    public static string getDir() => DIR;
}