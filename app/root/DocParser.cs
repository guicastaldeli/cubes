namespace App.Root.Screen;
using App.Root.Resource;
using App.Root.Shaders;
using App.Root.Text;
using App.Root.UI;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using System.Data;
using System.Reflection;

/**

    Source helper class

    */
public class Source {
    public enum SourceType {
        File,
        String
    }

    public string Content { get; set; }
    public SourceType Type { get; set; }

    private Source(string Content, SourceType Type) {
        this.Content = Content;
        this.Type = Type;
    }

    public static Source FromFile(string path) {
        Source val = new Source(path, SourceType.File);
        return val;
    }

    public static Source FromString(string content) {
        Source val = new Source(content, SourceType.String);
        return val;
    }
}

/**

    Doc Parser main class

    */
class DocParser {
    public static readonly string IMG_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource/");

    private static int uiVao = 0;
    private static int uiVbo = 0;
    private static int uiEbo = 0;
    private static bool uiBuffersInitialized = false;

    private static Dictionary<string, string> variables = new();
    private static Dictionary<string, object> dataObjects = new();
    private static Dictionary<string, object> loopContext = new();

    private static readonly int WINDOW_WIDTH = Window.WIDTH;
    private static readonly int WINDOW_HEIGHT = Window.HEIGHT;

    private static readonly (
        string a, 
        string b, 
        string c, 
        string d, 
        string e, 
        string repeat, 
        string grid,
        string loop,
        string idx,
        string testLoop,
        string ifCondition 
    ) Exp = (
        @"\$\{(.*?)\}",
        @"\{(\w+)\}",
        @"\{(\d+)\}\{(\w+)\}",
        @"\{(\w+)\|([^}]*)\}",
        @"\{([a-zA-Z_][a-zA-Z0-9_]*)(\.[a-zA-Z0-9_]+)?\}",
        @"^'(.*?)'\.repeat\((\d+)\)$",
        @"\{#grid\((\d+),(\d+)\)\}",
        @"\{#foreach\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+as\s+([a-zA-Z_][a-zA-Z0-9_]*)\}([\s\S]*?)\{#endfor\}",
        @"\{([^{}]+?)\}(?:%)?",
        @"{#foreach\s+[a-zA-Z_][a-zA-Z0-9_]*\s+as\s+[a-zA-Z_][a-zA-Z0-9_]*\}([\s\S]*?){#endfor\}",
        @"\{#if\s+([^}]+?)\}((?:(?!\{#if\b)[\s\S])*?)(?:\{#else\}((?:(?!\{#if\b)[\s\S])*?))?\{#endif\}"
    );

    // Split Instance Name
    private static List<string>? splitInstanceName(string name, List<string> names) {
        var result = new List<string>();

        string remaning = name.ToLower();
        while(remaning.Length > 0) {
            string? match = names.FirstOrDefault(k => remaning.StartsWith(k));
            if(match == null) return null;

            result.Add(match);
            remaning = remaning[match.Length..];
        }

        return result;
    }

    // Get Property Value
    private static object? GetPropertyValue(object obj, string path) {
        if(obj == null || string.IsNullOrEmpty(path)) return null;

        var parts = path.Split('.');
        object curr = obj;

        foreach(var part in parts) {
            if(curr == null) return null;

            var prop = curr.GetType().GetProperty(part);
            if(prop == null) return null;
            curr = prop.GetValue(curr)!;
        }

        return curr;
    }

    // Calculate Grid
    private static void calculateGrid(int i, int cols, int rows) {
        float f = 100.0f;

        int row = i / cols;
        int col = i % cols;

        Console.WriteLine($"Item {i}: row={row}, col={col}");

        loopContext["col"] = col;
        loopContext["cols"] = cols;
        loopContext["row"] = row;
        loopContext["rows"] = rows;

        float x = (col * f / cols) + (f / cols / 2);
        float y = (row * f / rows) + (f / rows / 2);
        loopContext["x"] = x;
        loopContext["y"] = y;
    }

    // Remove Grid
    private static void removeGrid() {
        loopContext.Remove("row");
        loopContext.Remove("rows");
        loopContext.Remove("col");
        loopContext.Remove("cols");
        loopContext.Remove("x");
        loopContext.Remove("y");
    }

    // Call Static Method
    private static bool callStaticMethod(string methodName, List<string> args) {
        try {
            Console.WriteLine($"[DocParser] Looking for method: {methodName}");
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach(var assembly in assemblies) {
                foreach(var type in assembly.GetTypes()) {
                    var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                    
                    if(method != null && method.ReturnType == typeof(bool)) {
                        Console.WriteLine($"[DocParser] Found method in {type.Name}");
                        var param = method.GetParameters();
                        
                        if(param.Length == args.Count) {
                            var convertedArgs = new List<object>();

                            for(int i = 0; i < args.Count; i++) {
                                var paramType = param[i].ParameterType;
                                object? parsedArg = parseArgument(args[i], paramType);
                                if(parsedArg == null && paramType.IsValueType) parsedArg = Activator.CreateInstance(paramType);
                                convertedArgs.Add(parsedArg ?? Convert.ChangeType(args[i], paramType));
                            }

                            var result = method.Invoke(null, convertedArgs.ToArray());
                            return result is bool b && b;
                        }
                    }
                }
            }
        } catch(Exception err) {
            Console.WriteLine($"[DocParser] Error calling method {methodName}: {err.Message}");
        }
        return false;
    }

    /**
     * 
     * Replace
     *
     */
    public static void Replace(string key, string val) {
        variables[key] = val;
    }

    public static void Replace(string key, object val) {
        string v = val.ToString() ?? "";
        variables[key] = v;
    }

    public static void ReplaceObject(string key, object data) {
        dataObjects[key] = data;
    }

    /**
     * 
     * Resolve
     *
     */
    // Resolve
    public static string Resolve(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        text = Regex.Replace(text, Exp.c, match => {
            string defaultVal = match.Groups[1].Value;
            string key = match.Groups[2].Value;
            return variables.TryGetValue(key, out var val) ? val : defaultVal;
        });
        text = Regex.Replace(text, Exp.d, match => {
            string key = match.Groups[1].Value;
            string defaultVal = match.Groups[2].Value;
            return variables.TryGetValue(key, out var val) ? val : defaultVal;
        });
        text = Regex.Replace(text, Exp.b, match => {
            string key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var val) ? val : match.Value;
        });

        return text;
    }

    // Loop Resolve
    public static string LResolve(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        text = resolveWithLoop(text);
        text = Resolve(text);

        return text;
    }

    // Resolve With Loop
    private static string resolveWithLoop(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        int currentGridCols = 0;
        int currentGridRows = 0;
        bool hasGrid = false;

        text = Regex.Replace(text, Exp.grid, match => {
            currentGridCols = int.Parse(match.Groups[1].Value);
            currentGridRows = int.Parse(match.Groups[2].Value);
            hasGrid = true;
            return "";
        });

        text = Regex.Replace(text, Exp.loop, match => {
            string collectionName = match.Groups[1].Value;
            string itemName = match.Groups[2].Value;
            string template = match.Groups[3].Value;
            if(!dataObjects.TryGetValue(collectionName, out var collObj)) return match.Value;
        
            var list = collObj as IList;
            if(list == null || list.Count == 0) return "";

            int cols = currentGridCols > 0 ? currentGridCols : 0;
            int rows = currentGridRows > 0 ? currentGridRows : 0;

            var result = new StringBuilder();

            for(int i = 0; i < list.Count; i++) {
                var item = list[i];
                if(item == null) continue;

                string idx = "index";

                loopContext[itemName] = item;
                loopContext[idx] = i;
                if(hasGrid) calculateGrid(i, currentGridCols, currentGridRows);

                var resolved = resolveWithLoopContext(template);
                resolved = resolveExpressions(resolved);
                resolved = resolveWithIf(resolved);
                result.Append(resolved);

                loopContext.Remove(itemName);
                loopContext.Remove(idx);
                removeGrid();
            }

            currentGridCols = 0;
            currentGridRows = 0;

            return result.ToString();
        }, RegexOptions.Singleline);

        return text;
    }

    // Resolve With Loop Context
    private static string resolveWithLoopContext(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        return Regex.Replace(text, Exp.e, match => {
            string objKey = match.Groups[1].Value;
            string? propPath = match.Groups[2].Success ? match.Groups[2].Value.Substring(1) : null;

            if(loopContext.TryGetValue(objKey, out var loopObj)) {
                if(string.IsNullOrEmpty(propPath)) {
                    return loopObj?.ToString() ?? "";
                }

                var value = GetPropertyValue(loopObj, propPath);
                return value?.ToString() ?? "";
            }

            if(string.IsNullOrEmpty(propPath) && variables.ContainsKey(objKey)) {
                return variables[objKey];
            }

            if(dataObjects.TryGetValue(objKey, out var obj)) {
                if(string.IsNullOrEmpty(propPath)) {
                    if(obj is IList list) {
                        return list.Count.ToString();
                    }
                    return obj?.ToString() ?? "";
                }

                var value = GetPropertyValue(obj, propPath);
                return value?.ToString() ?? "";
            }

            return match.Value;
        });
    }

    // Resolve Imports
    private static void resolveImports(XmlElement root, string filePath) {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        var imports = root.SelectNodes(".//import")?.Cast<XmlNode>().ToList();
        if(imports == null) return;

        foreach(XmlNode node in imports) {
            if(node is not XmlElement importEl) continue;

            string src = importEl.GetAttribute("src");
            string importPath = Path.Combine(baseDir, src);

            if(!File.Exists(importPath)) {
                Console.Error.WriteLine($"Import not found!: {importPath}");
                importEl?.ParentNode?.RemoveChild(importEl);
                continue;
            }

            string importContent = File.ReadAllText(importPath);
            string resolvedContent = LResolve(importContent);

            XmlDocument importDoc = new XmlDocument();
            importDoc.LoadXml(resolvedContent);

            XmlElement importRoot = importDoc.DocumentElement!;
            XmlNode parent = importEl.ParentNode!;

            foreach(XmlNode child in importRoot.ChildNodes.Cast<XmlNode>().ToList()) {
                if(child.NodeType != XmlNodeType.Element) continue;
                XmlNode imported = root.OwnerDocument.ImportNode(child, true);

                if(imported is XmlElement importedEl) {
                    foreach(XmlAttribute attr in importEl.Attributes) {
                        if(attr.Name == "src") continue;
                        importedEl.SetAttribute(attr.Name, attr.Value);
                    }
                }

                parent.InsertBefore(imported, importEl);
            }

            importEl?.ParentNode?.RemoveChild(importEl);
        }
    }

    // Resolve Instance
    private static void resolveInstance(XmlElement root) {
        var current = Controller.getCurrent();
        var instances = Controller.getInstances();

        var names = instances.Keys.OrderByDescending(k => k.Length).ToList();

        string regex = ".//*";
        var allElements = root.SelectNodes(regex)?.Cast<XmlNode>().OfType<XmlElement>();
        if(allElements == null) return;
        
        var tagged = allElements.Where(el => {
            var parts = splitInstanceName(el.LocalName, names);
            return parts != null && parts.Count > 0 && parts.All(p => instances.ContainsKey(p));
        }).ToList();

        foreach(var el in tagged) {
            var parts = splitInstanceName(el.LocalName, names)!;
            bool matches = parts.Any(name => instances.TryGetValue(name, out var inst) && inst == current);

            XmlNode parent = el.ParentNode!;

            if(matches) {
                foreach(XmlNode child in el.ChildNodes.Cast<XmlNode>().ToList()) {
                    if(child.NodeType != XmlNodeType.Element) continue;
                    parent.InsertBefore(child, el);
                }
            }

            parent.RemoveChild(el);
        }
    }

    // Resolve Expressions
    private static string resolveExpressions(string text) {
        (string idx, string a, string b, string c, string d, string e) Dict = (
            "index",
            "+",
            "-",
            "*",
            "/",
            "%"
        );

        if(string.IsNullOrEmpty(text)) return text;

        return Regex.Replace(text, Exp.idx, match => {
            string fullMatch = match.Value;
            string exp = match.Groups[1].Value.Trim();
            bool hasPercent = fullMatch.EndsWith(Dict.e);
            
            if(loopContext.TryGetValue(exp, out var val)) {
                string result = val?.ToString() ?? "";
                return hasPercent ? result + Dict.e : result;
            }

            if(exp.Contains(Dict.a) || 
                exp.Contains(Dict.b) || 
                exp.Contains(Dict.c) || 
                exp.Contains(Dict.idx)) {
                try {
                    if(loopContext.TryGetValue(Dict.idx, out var i)) {
                        exp = exp.Replace(Dict.idx, i?.ToString() ?? "");
                    }

                    foreach(var l in loopContext) {
                        if(l.Key != Dict.idx && l.Value != null) {
                            exp = exp.Replace(l.Key, l.Value.ToString() ?? "");
                        }   
                    }
                        
                    var result = evaluateExpressionData(exp);
                    string resultStr = result.ToString();

                    return hasPercent ? resultStr + Dict.e : resultStr;
                } catch {
                    return match.Value;
                }
            }

            return match.Value;
        });
    }

    // Resolve With If
    private static string resolveWithIf(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        string prev;
        do {
            prev = text;
            text = Regex.Replace(text, Exp.ifCondition, match => {
                string condition = match.Groups[1].Value.Trim();
                string trueContent = match.Groups[2].Value;
                string falseContent = match.Groups[3].Success ? match.Groups[3].Value : "";

                foreach(var l in loopContext) {
                    if(l.Value == null) continue;

                    var props = l.Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach(var prop in props) {
                        string placeholder = $"{l.Key}.{prop.Name}";
                        var val = prop.GetValue(l.Value);
                        condition = condition.Replace(placeholder, val?.ToString() ?? "");
                    }
                    if(l.Value.GetType().IsPrimitive) {
                        condition = condition.Replace(l.Key, l.Value.ToString());
                    }
                }

                bool result = evaluateCondition(condition);
                return result ? trueContent : falseContent; 
            }, RegexOptions.Singleline);
        } while(text != prev);

        return text;
    }

    /**
     * 
     * Parse
     *
     */
    // Parse Screen
    public static ScreenData parseScreen(Source source, int screenWidth, int screenHeight) {
        ScreenData screenData = new ScreenData(source.Content);
        
        try {
            XmlDocument document = new XmlDocument();
            if(source.Type == Source.SourceType.String) {
                document.LoadXml(source.Content);
            } else {
                document.Load(source.Content);
            }

            XmlElement root = document.DocumentElement!;
            resolveInstance(root);
            resolveImports(root, source.Content);

            screenData.screenType = root.Name;
            parseAttr(root, screenData.screenAttr);
            parseEl(root, screenData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            Console.Error.WriteLine("Error parsing screen XML (Source): " + err.Message);
        }
        
        return screenData;
    }

    public static ScreenData parseScreen(string filePath, int screenWidth, int screenHeight) {
        ScreenData screenData = new ScreenData(filePath);

        try {
            string content = File.ReadAllText(filePath);
            bool hasLoop = Regex.IsMatch(content, Exp.testLoop);
            
            string resolvedContent = hasLoop ? LResolve(content) : content;
            
            XmlDocument document = new XmlDocument();
            document.LoadXml(resolvedContent);

            XmlElement root = document.DocumentElement!;
            resolveInstance(root);
            resolveImports(root, filePath);

            screenData.screenType = root.Name;
            parseAttr(root, screenData.screenAttr);
            parseEl(root, screenData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            if(!Regex.IsMatch(File.ReadAllText(filePath), Exp.testLoop)) {
                Console.Error.WriteLine("Error parsing screen XML (filePath): " + err.Message);
            }
        }

        return screenData;
    }

    // Parse UI
    public static UIData parseUI(Source source, int screenWidth, int screenHeight) {
        UIData uiData = new UIData(source.Content);
        
        try {
            XmlDocument document = new XmlDocument();
            if(source.Type == Source.SourceType.String) {
                document.LoadXml(source.Content);
            } else {
                document.Load(source.Content);
            }

            XmlElement root = document.DocumentElement!;
            resolveInstance(root);
            resolveImports(root, source.Content);

            uiData.uiType = root.Name;
            parseAttr(root, uiData.uiAttr);
            parseEl(root, uiData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            Console.Error.WriteLine("Error parsing UI XML (Source): " + err.Message);
        }

        return uiData;
    }

    public static UIData parseUI(string filePath, int screenWidth, int screenHeight) {
        UIData uiData = new UIData(filePath);
        
        try {
            string content = File.ReadAllText(filePath);
            bool hasLoop = Regex.IsMatch(content, Exp.testLoop);
            string resolvedContent = hasLoop ? LResolve(content) : content;
            
            XmlDocument document = new XmlDocument();
            document.LoadXml(resolvedContent);

            XmlElement root = document.DocumentElement!;
            resolveInstance(root);
            resolveImports(root, filePath);

            uiData.uiType = root.Name;
            parseAttr(root, uiData.uiAttr);
            parseEl(root, uiData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            if(!Regex.IsMatch(File.ReadAllText(filePath), Exp.testLoop)) {
                Console.Error.WriteLine("Error parsing screen XML (filePath): " + err.Message);
            }
        }
        
        return uiData;
    }

    // Parse Element
    private static void parseEl(
        XmlElement parent,
        List<ScreenElement> elements,
        int screenWidth,
        int screenHeight,
        ScreenElement? parentElement
    ) {
        foreach(XmlNode node in parent.ChildNodes) {
            if(node is not XmlElement element) continue;

            ScreenElement? screenElement = createScreenElement(
                element,
                element.Name,
                screenWidth,
                screenHeight,
                parentElement
            );

            if(screenElement != null) {
                elements.Add(screenElement);
                parseEl(element, elements, screenWidth, screenHeight, screenElement);
            }
        }
    }

    private static void parseEl(
        XmlElement parent,
        List<UIElement> elements,
        int screenWidth,
        int screenHeight,
        UIElement? parentElement
    ) {
        foreach(XmlNode node in parent.ChildNodes) {
            if(node is not XmlElement element) continue;

            UIElement? uiElement = createUIElement(
                element,
                element.Name,
                screenWidth,
                screenHeight,
                parentElement
            );

            if(uiElement != null) {
                elements.Add(uiElement);
                parseEl(element, elements, screenWidth, screenHeight, uiElement);
            }
        }
    }

    // Parse Attribute
    private static void parseAttr(XmlElement element, Dictionary<string, string> attributes) {
        foreach(XmlAttribute attr in element.Attributes) {
            attributes[attr.Name] = attr.Value;
        }
    }

    // Parse Size
    private static int parseSize(XmlElement element, string attrName, int currentScreenSize, int originalScreenSize, int defaultValue) {
        if(!element.HasAttribute(attrName)) return defaultValue;

        string sizeStr = element.GetAttribute(attrName);
        if(sizeStr.EndsWith("%")) {
            float pct = float.Parse(sizeStr.Replace("%", "")) / 100.0f;
            return (int)(currentScreenSize * pct);
        } else if(sizeStr == "auto") {
            return defaultValue;
        } else {
            int originalSize = int.Parse(sizeStr);
            float scaleFactor = (float)currentScreenSize / originalScreenSize;
            return (int)(originalSize * scaleFactor);
        }
    }

    // Parse Coordinate
    private static int parseCoordinate(XmlElement element, string attrName, int currentScreenSize, int originalScreenSize) {
        if(!element.HasAttribute(attrName)) return 0;

        string coordStr = element.GetAttribute(attrName);
        if(coordStr.EndsWith("%")) {
            float pct = float.Parse(coordStr.Replace("%", "")) / 100.0f;
            return (int)(currentScreenSize * pct);
        } else {
            int originalCoord = int.Parse(coordStr);
            float scaleFactor = (float)currentScreenSize / originalScreenSize;
            return (int)(originalCoord * scaleFactor);
        }
    }

    // Parse Color
    private static float[]? parseColor(string? colorStr) {
        if(string.IsNullOrWhiteSpace(colorStr)) return null;

        string[] parts = colorStr.Split(',');
        if(parts.Length >= 3) {
            return new float[] {
                float.Parse(parts[0].Trim()),
                float.Parse(parts[1].Trim()),
                float.Parse(parts[2].Trim()),
                parts.Length >= 4 ? float.Parse(parts[3].Trim()) : 1.0f
            };
        }
        return null;
    }

    // Parse Buttons
    public static List<ScreenElement> parseButtons(string xmlFilePath, int screenWidth, int screenHeight) {
        List<ScreenElement> val = getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "button");
        return val;
    }

    // Parse Labels
    public static List<ScreenElement> parseLabels(string xmlFilePath, int screenWidth, int screenHeight) {
        List<ScreenElement> val = getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "label");
        return val;
    }

    // Parse Divs
    public static List<ScreenElement> parseDivs(string xmlFilePath, int screenWidth, int screenHeight) {
        List<ScreenElement> val = getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "div");
        return val;
    }

    // Parse Argument
    private static object? parseArgument(string arg, Type targetType) {
        arg = arg.Trim();

        if(targetType == typeof(int)) return int.TryParse(arg, out int result) ? result : null;
        if(targetType == typeof(float)) return float.TryParse(arg, out float result) ? result : null;
        if(targetType == typeof(bool)) return bool.TryParse(arg, out bool result) ? result : null;
        if(targetType == typeof(string)) return arg.Trim('"', '\'');

        return null;
    }

    /**
     * 
     * Create Element
     *
     */
    private static ScreenElement? createScreenElement(
        XmlElement element,
        string type,
        int screenWidth,
        int screenHeight,
        ScreenElement? parentElement
    ) {
        string text = "";
        bool hasChildElements = element.ChildNodes.Cast<XmlNode>().Any(n => n.NodeType == XmlNodeType.Element);

        if(element.HasAttribute("text")) {
            text = evaluateExpression(element.GetAttribute("text"));
        } else if(!hasChildElements) {
            text = evaluateExpression(element.InnerText.Trim());
        }
        
        string id = element.HasAttribute("id") ? element.GetAttribute("id") : "";

        int x = parseCoordinate(element, "x", screenWidth, WINDOW_WIDTH);
        int y = parseCoordinate(element, "y", screenHeight, WINDOW_HEIGHT);
        if(parentElement != null) {
            x += parentElement.x;
            y += parentElement.y;
        }

        int width  = parseSize(element, "width",  screenWidth,  WINDOW_WIDTH, 100);
        int height = parseSize(element, "height", screenHeight, WINDOW_HEIGHT, 50);

        float scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1.0f;

        float[] color = new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        if(element.HasAttribute("color")) {
            float[]? parsed = parseColor(element.GetAttribute("color"));
            if(parsed != null) color = parsed;
        }

        string fontFamily = element.HasAttribute("fontFamily")
            ? element.GetAttribute("fontFamily").ToLower()
            : FontLoader.DEFAULT_FONT;

        string action = element.HasAttribute("action") ? element.GetAttribute("action") : "";

        float borderWidth = element.HasAttribute("border")
            ? float.Parse(element.GetAttribute("border"))
            : 0.0f;

        float[] borderColor = new float[]{ 0.0f, 0.0f, 0.0f, 1.0f };
        if(element.HasAttribute("borderColor")) {
            float[]? bc = parseColor(element.GetAttribute("borderColor"));
            if(bc != null) borderColor = bc;
        }

        if(element.HasAttribute("text")) {
            text = evaluateExpression(element.GetAttribute("text"));
        } else {
            text = evaluateExpression(text);
        }

        ScreenElement screenElement = new ScreenElement(
            type, id, text, fontFamily,
            x, y, width, height,
            scale, color, action
        );

        bool hasBackground = type == "div" || type == "container" || type == "input";
        if(element.HasAttribute("background")) {
            screenElement.hasBackground = true;
            float[]? bg = parseColor(element.GetAttribute("background"));
            if(bg != null) {
                screenElement.backgroundColor = bg;
                screenElement.originalBackgroundColor = (float[])bg.Clone();
            }
        }

        if(element.HasAttribute("textOffset")) {
            string[] parts = element.GetAttribute("textOffset").Split(',');
            if(parts.Length >= 2) {
                string xPart = parts[0].Trim();
                string yPart = parts[1].Trim();
                if(xPart.EndsWith("%")) {
                    float pct = float.Parse(xPart.Replace("%", "")) / 100.0f;
                    screenElement.textOffsetX = screenElement.width * pct;
                } else {
                    screenElement.textOffsetX = float.Parse(xPart);
                }
                if(yPart.EndsWith("%")) {
                    float pct = float.Parse(yPart.Replace("%", "")) / 100.0f;
                    screenElement.textOffsetY = screenElement.height * pct;
                } else {
                    screenElement.textOffsetY = float.Parse(yPart);
                }
            }
        }

        screenElement.borderWidth = borderWidth;
        screenElement.borderColor = borderColor;

        screenElement.template = text;
        text = evaluateExpression(text);
        screenElement.text = text;

        if(element.HasAttribute("background")) screenElement.hasBackground = true;
        parseAttr(element, screenElement.attr);

        if(type == "img") {
            screenElement.hasBackground = true;
            if(element.HasAttribute("src")) {
                string fullPath = IMG_PATH + element.GetAttribute("src");
                int texId = TextureLoader.load(fullPath);
                if(texId != -1) {
                    screenElement.textureId = texId;
                    screenElement.hasTexture = true;
                } else {
                    Console.Error.WriteLine("Failed to load texture: " + fullPath);
                }
            }
        }

        if(element.HasAttribute("hoverable")) {
            screenElement.hoverable = bool.Parse(element.GetAttribute("hoverable"));
        }

        if(type == "button" || type == "container" || type == "div") {
            screenElement.hoverable = true;
        }

        if(element.HasAttribute("hoverColor")) {
            float[]? hc = parseColor(element.GetAttribute("hoverColor"));
            if(hc != null) screenElement.hoverColor = hc;
        }

        if(element.HasAttribute("hoverTextColor")) {
            screenElement.hoverTextColor = parseColor(element.GetAttribute("hoverTextColor"));
            if(screenElement.hoverColor == null && screenElement.hoverTextColor != null)
                screenElement.hoverColor = (float[])screenElement.hoverTextColor.Clone();
        }

        if(element.HasAttribute("hoverBorderColor")) {
            float[]? hbc = parseColor(element.GetAttribute("hoverBorderColor"));
            if(hbc != null) screenElement.hoverBorderColor = hbc;
        }

        if(element.HasAttribute("hoverBackground")) {
            float[]? hbg = parseColor(element.GetAttribute("hoverBackground"));
            if(hbg != null) screenElement.hoverBackgroundColor = hbg;
        }

        if(element.HasAttribute("hoverScale"))
            screenElement.hoverScale = float.Parse(element.GetAttribute("hoverScale"));

        if(type == "input") {
            screenElement.hasBackground = true;
            if(!element.HasAttribute("color"))
                screenElement.color = new float[]{ 0.0f, 0.0f, 0.0f, 1.0f };
        }

        if(element.HasAttribute("visible"))
            screenElement.visible = bool.Parse(element.GetAttribute("visible"));

        if(element.HasAttribute("textShadow")) {
            string shadowStr = element.GetAttribute("textShadow");
            string[] shadowParts = shadowStr.Split(' ');

            if(shadowParts.Length >= 2) {
                screenElement.hasShadow = true;

                string[] offsetParts = shadowParts[0].Split(',');
                if(offsetParts.Length >= 2) {
                    screenElement.shadowOffsetX = float.Parse(offsetParts[0].Replace("px", "").Trim());
                    screenElement.shadowOffsetY = float.Parse(offsetParts[1].Replace("px", "").Trim());
                }

                screenElement.shadowBlur = float.Parse(shadowParts[1].Replace("px", "").Trim());

                if(shadowParts.Length >= 3) {
                    float[]? sc = parseColor(shadowParts[2]);
                    if(sc != null) screenElement.shadowColor = sc;
                }
            }
        }

        return screenElement;
    }

    /**
     * 
     * Get Elements
     *
     */
    public static List<ScreenElement> getElementsByType(ScreenData screenData, string type) {
        var result = new List<ScreenElement>();
        foreach(var el in screenData.elements) {
            if(el.type == type && el.visible) result.Add(el);
        }
        return result;
    }

    public static ScreenElement? getElementById(ScreenData screenData, string id) {
        foreach(var el in screenData.elements) {
            if(el.id == id) return el;
        }
        return null;
    }

    public static void initElRendering() {
        if(uiBuffersInitialized) return;

        uiVao = GL.GenVertexArray();
        uiVbo = GL.GenBuffer();
        uiEbo = GL.GenBuffer();

        GL.BindVertexArray(uiVao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, uiVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 4 * 8 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(3);

        int[] indices = { 0, 1, 2, 2, 3, 0 };
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, uiEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
        uiBuffersInitialized = true;
    }

    /**
     * 
     * Render Elements
     *
     */
    public static void renderScreenElement(
        ScreenElement element,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram
    ) {
        if(!element.visible || !element.hasBackground) return;

        initElRendering();

        float x1 = element.x;
        float y1 = element.y;
        float x2 = element.x + element.width;
        float y2 = element.y + element.height;

        bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
        if(depthTest) GL.Disable(EnableCap.DepthTest);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram.bind();

        if(element.hasTexture && element.textureId != -1) {
            shaderProgram.setUniform("shaderType", 3);
            shaderProgram.setUniform("hasTex", 1);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, element.textureId);
            shaderProgram.setUniform("uSampler", 0);
        } else {
            shaderProgram.setUniform("shaderType", 3);
            shaderProgram.setUniform("hasTex", 0);
        }

        float r = element.backgroundColor[0];
        float g = element.backgroundColor[1];
        float b = element.backgroundColor[2];
        float a = element.backgroundColor[3];

        shaderProgram.setUniform("screenSize", (float)screenWidth, (float)screenHeight);
        shaderProgram.setUniform("uColor", r, g, b, a);

        float[] verts = {
            x1, y1,  r, g, b, a,  0.0f, 0.0f,
            x1, y2,  r, g, b, a,  0.0f, 1.0f,
            x2, y2,  r, g, b, a,  1.0f, 1.0f,
            x2, y1,  r, g, b, a,  1.0f, 0.0f,
        };

        GL.BindVertexArray(uiVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, uiVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        shaderProgram.unbind();

        if(depthTest) GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
    }

    /**
     * 
     * Render Screen
     *
     */
    public static void renderScreen(
        ScreenData? screenData,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram,
        TextRenderer? textRenderer
    ) {
        if(screenData == null || screenData.elements.Count == 0) return;

        foreach(var el in screenData.elements) {
            if(el.template != null && el.template.Contains('{')) {
                el.text = DocParser.Resolve(el.template);
            }
        }
        foreach(var el in screenData.elements) {
            if(el.visible && el.type == "div")
                renderScreenElement(el, screenWidth, screenHeight, shaderProgram);
        }
        foreach(var el in screenData.elements) {
            if(el.visible && el.type == "img")
                renderScreenElement(el, screenWidth, screenHeight, shaderProgram);
        }
        foreach(var el in screenData.elements) {
            if(el.visible && el.type == "input") {
                renderScreenElement(el, screenWidth, screenHeight, shaderProgram);
                if(textRenderer != null && !string.IsNullOrEmpty(el.text))
                    textRenderer.renderText(
                        el.text,
                        el.x + el.textOffsetX,
                        el.y + el.textOffsetY,
                        el.scale, el.color, el.fontFamily
                    );
            }
        }
        foreach(var el in screenData.elements) {
            if(el.visible && el.type == "button") {
                renderScreenElement(el, screenWidth, screenHeight, shaderProgram);

                if(textRenderer != null && !string.IsNullOrEmpty(el.text)) {
                    if(el.hasShadow) {
                        textRenderer.renderTextWithShadow(
                            el.text, el.x + el.textOffsetX, el.y + el.textOffsetY, el.scale, el.color,
                            el.shadowOffsetX, el.shadowOffsetY, el.shadowBlur,
                            el.shadowColor, el.fontFamily
                        );
                    } else {
                        textRenderer.renderText(
                            el.text, 
                            el.x + el.textOffsetX,
                            el.y + el.textOffsetY, 
                            el.scale, el.color, el.fontFamily
                        );
                    }
                }
            }
        }
        foreach(var el in screenData.elements) {
            if(el.visible && el.type == "label") {
                if(textRenderer != null && !string.IsNullOrEmpty(el.text)) {
                    if(el.hasShadow) {
                        textRenderer.renderTextWithShadow(
                            el.text, el.x + el.textOffsetX, el.y + el.textOffsetY, el.scale, el.color,
                            el.shadowOffsetX, el.shadowOffsetY, el.shadowBlur,
                            el.shadowColor, el.fontFamily
                        );
                    } else {
                        textRenderer.renderText(
                            el.text, 
                            el.x + el.textOffsetX,
                            el.y + el.textOffsetY, 
                            el.scale, el.color, el.fontFamily
                        );
                    }
                }
            }
        }
    }

    /**
     * 
     * Create UI Element
     *
     */
    private static UIElement? createUIElement(
        XmlElement element,
        string type,
        int screenWidth,
        int screenHeight,
        UIElement? parentElement
    ) {
        string text = "";
        bool hasChildElements = element.ChildNodes.Cast<XmlNode>().Any(n => n.NodeType == XmlNodeType.Element);

        if(element.HasAttribute("text")) {
            text = evaluateExpression(element.GetAttribute("text"));
        } else if(!hasChildElements) {
            text = evaluateExpression(element.InnerText.Trim());
        }

        string id = element.HasAttribute("id") ? element.GetAttribute("id") : "";

        int x = parseCoordinate(element, "x", screenWidth, WINDOW_WIDTH);
        int y = parseCoordinate(element, "y", screenHeight, WINDOW_HEIGHT);
        if(parentElement != null) {
            x += parentElement.x;
            y += parentElement.y;
        }

        int width  = parseSize(element, "width",  screenWidth,  WINDOW_WIDTH, 100);
        int height = parseSize(element, "height", screenHeight, WINDOW_HEIGHT,  50);

        float scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1.0f;

        float[] color = new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        if(element.HasAttribute("color")) {
            float[]? parsed = parseColor(element.GetAttribute("color"));
            if(parsed != null) color = parsed;
        }

        string fontFamily = element.HasAttribute("fontFamily")
            ? element.GetAttribute("fontFamily").ToLower()
            : FontLoader.DEFAULT_FONT;

        string action = element.HasAttribute("action") ? element.GetAttribute("action") : "";

        float borderWidth = element.HasAttribute("border")
            ? float.Parse(element.GetAttribute("border"))
            : 0.0f;

        float[] borderColor = new float[]{ 0.0f, 0.0f, 0.0f, 1.0f };
        if(element.HasAttribute("borderColor")) {
            float[]? bc = parseColor(element.GetAttribute("borderColor"));
            if(bc != null) borderColor = bc;
        }

        if(element.HasAttribute("text")) {
            text = evaluateExpression(element.GetAttribute("text"));
        } else {
            text = evaluateExpression(text);
        }

        bool hasBackground = type == "div" || type == "container" || type == "input";
        float[] backgroundColor = new float[]{ 0f, 0f, 0f, 0f };
        if(element.HasAttribute("background")) {
            hasBackground = true;
            float[]? bg = parseColor(element.GetAttribute("background"));
            if(bg != null) backgroundColor = bg;
        }

        UIElement uiElement = new UIElement(
            type, id, text,
            x, y, width, height,
            scale, color, hasBackground, action,
            fontFamily
        );

        uiElement.backgroundColor = backgroundColor;
        uiElement.originalBackgroundColor = (float[])backgroundColor.Clone();
        uiElement.borderWidth = borderWidth;
        uiElement.borderColor = borderColor;

        uiElement.template = text;
        text = evaluateExpression(text);
        uiElement.text = text;
        
        if(element.HasAttribute("background")) uiElement.hasBackground = true;
        parseAttr(element, uiElement.attr);

        // img
        if(type == "img") {
            uiElement.hasBackground = true;
            if(element.HasAttribute("src")) {
                string fullPath = IMG_PATH + element.GetAttribute("src");
                int texId = TextureLoader.load(fullPath);
                if(texId != -1) {
                    uiElement.textureId = texId;
                    uiElement.hasTexture = true;

                    var (imgW, imgH) = TextureLoader.getSize(texId);
                    uiElement.imgWidth = imgW;
                    uiElement.imgHeight = imgH;
                } else {
                    Console.Error.WriteLine("Failed to load texture: " + fullPath);
                }
            }
        }

        if(element.HasAttribute("hoverable"))
            uiElement.hoverable = bool.Parse(element.GetAttribute("hoverable"));

        if(type == "button")
            uiElement.hoverable = true;

        if(element.HasAttribute("hoverColor")) {
            float[]? hc = parseColor(element.GetAttribute("hoverColor"));
            if(hc != null) uiElement.hoverColor = hc;
        }

        if(element.HasAttribute("hoverTextColor")) {
            float[]? htc = parseColor(element.GetAttribute("hoverTextColor"));
            if(htc != null) uiElement.hoverTextColor = htc;
        }

        if(element.HasAttribute("hoverBorderColor")) {
            float[]? hbc = parseColor(element.GetAttribute("hoverBorderColor"));
            if(hbc != null) uiElement.hoverBorderColor = hbc;
        }

        if(element.HasAttribute("hoverBackground")) {
            float[]? hbg = parseColor(element.GetAttribute("hoverBackground"));
            if(hbg != null) uiElement.hoverBackgroundColor = hbg;
        }

        if(element.HasAttribute("hoverScale"))
            uiElement.hoverScale = float.Parse(element.GetAttribute("hoverScale"));

        if(type == "input") {
            uiElement.hasBackground = true;
            if(!element.HasAttribute("color"))
                uiElement.color = new float[]{ 0.0f, 0.0f, 0.0f, 1.0f };
        }

        if(element.HasAttribute("visible"))
            uiElement.visible = bool.Parse(element.GetAttribute("visible"));

        if(element.HasAttribute("textShadow")) {
            string shadowStr = element.GetAttribute("textShadow");
            string[] shadowParts = shadowStr.Split(' ');

            if(shadowParts.Length >= 2) {
                uiElement.hasShadow = true;

                string[] offsetParts = shadowParts[0].Split(',');
                if(offsetParts.Length >= 2) {
                    uiElement.shadowOffsetX = float.Parse(offsetParts[0].Replace("px", "").Trim());
                    uiElement.shadowOffsetY = float.Parse(offsetParts[1].Replace("px", "").Trim());
                }

                uiElement.shadowBlur = float.Parse(shadowParts[1].Replace("px", "").Trim());

                if(shadowParts.Length >= 3) {
                    float[]? sc = parseColor(shadowParts[2]);
                    if(sc != null) uiElement.shadowColor = sc;
                }
            }
        }

        return uiElement;
    }

    public static List<UIElement> getElementsByType(UIData uiData, string type) {
        var result = new List<UIElement>();
        foreach(var el in uiData.elements) {
            if(el.type == type && el.visible) result.Add(el);
        }
        return result;
    }

    public static UIElement? getElementById(UIData uiData, string id) {
        foreach(var el in uiData.elements) {
            if(el.id == id) return el;
        }
        return null;
    }

    public static void renderUIElement(
        UIElement element,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram
    ) {
        if(!element.visible || !element.hasBackground) return;

        initElRendering();

        float x1 = element.x;
        float y1 = element.y;
        float x2 = element.x + element.width;
        float y2 = element.y + element.height;

        bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
        if(depthTest) GL.Disable(EnableCap.DepthTest);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram.bind();

        if(element.hasTexture && element.textureId != -1) {
            shaderProgram.setUniform("shaderType", 3);
            shaderProgram.setUniform("hasTex", 1);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, element.textureId);
            shaderProgram.setUniform("uSampler", 0);
        } else {
            shaderProgram.setUniform("shaderType", 3);
            shaderProgram.setUniform("hasTex", 0);
        }

        float r = element.backgroundColor[0];
        float g = element.backgroundColor[1];
        float b = element.backgroundColor[2];
        float a = element.backgroundColor[3];

        shaderProgram.setUniform("screenSize", (float)screenWidth, (float)screenHeight);
        shaderProgram.setUniform("uColor", r, g, b, a);

        float[] verts = {
            x1, y1,  r, g, b, a,  0.0f, 0.0f,
            x1, y2,  r, g, b, a,  0.0f, 1.0f,
            x2, y2,  r, g, b, a,  1.0f, 1.0f,
            x2, y1,  r, g, b, a,  1.0f, 0.0f,
        };

        GL.BindVertexArray(uiVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, uiVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verts.Length * sizeof(float), verts);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        shaderProgram.unbind();

        if(depthTest) GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Blend);
    }

    /**
     * 
     * Render UI
     *
     */
    public static void renderUI(
        UIData? uiData,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram,
        TextRenderer? textRenderer
    ) {
        if(uiData == null || uiData.elements.Count == 0) return;

        foreach(var el in uiData.elements) {
            if(el.template != null && el.template.Contains('{')) {
                el.text = DocParser.Resolve(el.template);
            }
        }
        foreach(var el in uiData.elements) {
            if(el.visible && el.type == "div") {
                renderUIElement(el, screenWidth, screenHeight, shaderProgram);

                if(textRenderer != null && !string.IsNullOrEmpty(el.text)) {
                    string[] lines = el.text.Split('\n');
                    float lineHeight = 32.0f * el.scale;
                    for(int i = 0; i < lines.Length; i++) {
                        if(string.IsNullOrEmpty(lines[i])) continue;
                        float lineY = el.y + (i * lineHeight);
                        if(el.hasShadow) {
                            textRenderer.renderTextWithShadow(
                                lines[i], el.x, lineY, el.scale, el.color,
                                el.shadowOffsetX, el.shadowOffsetY, el.shadowBlur,
                                el.shadowColor, el.fontFamily
                            );
                        } else {
                            textRenderer.renderText(lines[i], el.x, lineY, el.scale, el.color, el.fontFamily);
                        }
                    }
                }
            }
        }
        foreach(var el in uiData.elements) {
            if(el.visible && el.type == "img")
                renderUIElement(el, screenWidth, screenHeight, shaderProgram);
        }
        foreach(var el in uiData.elements) {
            if(el.visible && el.type == "button") {
                renderUIElement(el, screenWidth, screenHeight, shaderProgram);

                if(textRenderer != null && !string.IsNullOrEmpty(el.text)) {
                    if(el.hasShadow) {
                        textRenderer.renderTextWithShadow(
                            el.text, el.x, el.y, el.scale, el.color,
                            el.shadowOffsetX, el.shadowOffsetY, el.shadowBlur,
                            el.shadowColor, el.fontFamily
                        );
                    } else {
                        textRenderer.renderText(el.text, el.x, el.y, el.scale, el.color, el.fontFamily);
                    }
                }
            }
        }
        foreach(var el in uiData.elements) {
            if(el.visible && el.type == "input") {
                renderUIElement(el, screenWidth, screenHeight, shaderProgram);
                if(textRenderer != null && !string.IsNullOrEmpty(el.text))
                    textRenderer.renderText(el.text, el.x, el.y, el.scale, el.color, el.fontFamily);
            }
        }
        foreach(var el in uiData.elements) {
            if(el.visible && el.type == "label") {
                if(textRenderer != null && !string.IsNullOrEmpty(el.text)) {
                    if(el.hasShadow) {
                        textRenderer.renderTextWithShadow(
                            el.text, el.x, el.y, el.scale, el.color,
                            el.shadowOffsetX, el.shadowOffsetY, el.shadowBlur,
                            el.shadowColor, el.fontFamily
                        );
                    } else {
                        textRenderer.renderText(el.text, el.x, el.y, el.scale, el.color, el.fontFamily);
                    }
                }
            }
        }
    }

    /**
     * 
     * Evaluate Expression
     *
     */
    // Evaluate Expression
    private static string evaluateExpression(string text) {
        if(string.IsNullOrEmpty(text)) return text;

        text = Regex.Replace(text, Exp.a, match =>
            evaluateSimpleExpression(match.Groups[1].Value)
        );

        return text;
    }

    // Evaluate Simple Expression
    private static string evaluateSimpleExpression(string expression) {
        expression = expression.Trim();

        var repeatMatch = Regex.Match(expression, @"^'(.*?)'\.repeat\((\d+)\)$");
        if(repeatMatch.Success) {
            string repeatText = repeatMatch.Groups[1].Value;
            int count = int.Parse(repeatMatch.Groups[2].Value);
            return string.Concat(Enumerable.Repeat(repeatText, count));
        }

        if(expression.Contains("+")) {
            var sb = new StringBuilder();
            foreach(var part in expression.Split('+'))
                sb.Append(part.Trim().Trim('\''));
            return sb.ToString();
        }

        if(expression.StartsWith("'") && expression.EndsWith("'"))
            return expression[1..^1];

        return "${" + expression + "}";
    }

    // Evaluate Expression Data
    private static double evaluateExpressionData(string exp) {
        var data = new DataTable();
        var result = data.Compute(exp, "");

        double val = Convert.ToDouble(result);
        return val;
    }

    /**
     *
     * Evaluate Condition
     *
     */
    private static bool evaluateCondition(string condition) {
        try {
            bool negate = false;
            if(condition.StartsWith("!")) {
                negate = true;
                condition = condition.Substring(1).Trim();
            }

            string p = @"^([a-zA-Z_][a-zA-Z0-9_]*)\((.*)\)$";
            var match = Regex.Match(condition, p);
            if(match.Success) {
                string funcName = match.Groups[1].Value;
                string args = match.Groups[2].Value;

                var argList = args.Split(',').Select(a => a.Trim()).ToList();

                var result = callStaticMethod(funcName, argList);
                return negate ? !result : result;
            }

            var dataTable = new DataTable();
            var result2 = dataTable.Compute(condition, "");
            bool boolResult = Convert.ToBoolean(result2);
            return negate ? !boolResult : boolResult;
        } catch(Exception err) {
            Console.WriteLine($"[DocParser] Error evaluating condition: {err.Message}");
            return false;
        }
    }

    /**
     * 
     * Cleanup
     *
     */
    public static void cleanup() {
        if(uiVao != 0) GL.DeleteVertexArray(uiVao);
        if(uiVbo != 0) GL.DeleteBuffer(uiVbo);
        if(uiEbo != 0) GL.DeleteBuffer(uiEbo);
        uiBuffersInitialized = false;
    }
}