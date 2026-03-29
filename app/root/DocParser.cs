namespace App.Root.Screen;
using App.Root.Resource;
using App.Root.Shaders;
using App.Root.Text;
using App.Root.ui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OpenTK.Graphics.OpenGL;

class DocParser {
    public static readonly string IMG_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource/img/");

    private static int uiVao = 0;
    private static int uiVbo = 0;
    private static int uiEbo = 0;
    private static bool uiBuffersInitialized = false;

    ///
    /// Parse
    ///
    public static ScreenData parseScreen(string filePath, int screenWidth, int screenHeight) {
        ScreenData screenData = new ScreenData(filePath);
        try {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            XmlElement root = document.DocumentElement!;
            screenData.screenType = root.Name;

            parseAttr(root, screenData.screenAttr);
            parseEl(root, screenData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            Console.Error.WriteLine("Error parsing screen XML: " + err.Message);
        }
        return screenData;
    }

    public static UIData parseUI(string filePath, int screenWidth, int screenHeight) {
        UIData uiData = new UIData(filePath);
        try {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            XmlElement root = document.DocumentElement!;
            uiData.uiType = root.Name;

            parseAttr(root, uiData.uiAttr);
            parseEl(root, uiData.elements, screenWidth, screenHeight, null);
        } catch(Exception err) {
            Console.Error.WriteLine("Error parsing UI XML: " + err.Message);
        }
        return uiData;
    }

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

    private static void parseAttr(XmlElement element, Dictionary<string, string> attributes) {
        foreach(XmlAttribute attr in element.Attributes) {
            attributes[attr.Name] = attr.Value;
        }
    }

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

    
    public static List<ScreenElement> parseButtons(string xmlFilePath, int screenWidth, int screenHeight) =>
        getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "button");

    public static List<ScreenElement> parseLabels(string xmlFilePath, int screenWidth, int screenHeight) =>
        getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "label");

    public static List<ScreenElement> parseDivs(string xmlFilePath, int screenWidth, int screenHeight) =>
        getElementsByType(parseScreen(xmlFilePath, screenWidth, screenHeight), "div");

    ///
    /// Create Element
    ///
    private static ScreenElement? createScreenElement(
        XmlElement element,
        string type,
        int screenWidth,
        int screenHeight,
        ScreenElement? parentElement
    ) {
        string text = element.InnerText.Trim();
        string id   = element.HasAttribute("id") ? element.GetAttribute("id") : "";

        int x = parseCoordinate(element, "x", screenWidth, 1280);
        int y = parseCoordinate(element, "y", screenHeight, 720);
        if(parentElement != null) {
            x += parentElement.x;
            y += parentElement.y;
        }

        int width  = parseSize(element, "width",  screenWidth,  1280, 100);
        int height = parseSize(element, "height", screenHeight, 720,  50);

        float scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1.0f;

        float[] color = new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        if(element.HasAttribute("color")) {
            float[]? parsed = parseColor(element.GetAttribute("color"));
            if(parsed != null) color = parsed;
        }

        string fontFamily = element.HasAttribute("fontFamily")
            ? element.GetAttribute("fontFamily").ToLower()
            : "arial";

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

    ///
    /// Get Elements
    ///
    public static List<ScreenElement> getElementsByType(ScreenData screenData, string type) {
        var result = new List<ScreenElement>();
        foreach(var el in screenData.elements) {
            if(el.type == type && el.visible) result.Add(el);
        }
        return result;
    }

    public static ScreenElement? getElementById(ScreenData screenData, string id) {
        foreach(var el in screenData.elements) {
            if(el.id == id && el.visible) return el;
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

    ///
    /// Render Element
    ///
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

    ///
    /// Render Screen
    ///
    public static void renderScreen(
        ScreenData? screenData,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram,
        TextRenderer? textRenderer
    ) {
        if(screenData == null || screenData.elements.Count == 0) return;

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

    ///
    /// Create UI Element
    ///
    private static UIElement? createUIElement(
        XmlElement element,
        string type,
        int screenWidth,
        int screenHeight,
        UIElement? parentElement
    ) {
        string text = element.InnerText.Trim();
        string id   = element.HasAttribute("id") ? element.GetAttribute("id") : "";

        int x = parseCoordinate(element, "x", screenWidth, 1280);
        int y = parseCoordinate(element, "y", screenHeight, 720);
        if(parentElement != null) {
            x += parentElement.x;
            y += parentElement.y;
        }

        int width  = parseSize(element, "width",  screenWidth,  1280, 100);
        int height = parseSize(element, "height", screenHeight, 720,  50);

        float scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1.0f;

        float[] color = new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        if(element.HasAttribute("color")) {
            float[]? parsed = parseColor(element.GetAttribute("color"));
            if(parsed != null) color = parsed;
        }

        string fontFamily = element.HasAttribute("fontFamily")
            ? element.GetAttribute("fontFamily").ToLower()
            : "arial";

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
            type, id, text, fontFamily,
            x, y, width, height,
            scale, color, hasBackground, action
        );

        uiElement.backgroundColor = backgroundColor;
        uiElement.originalBackgroundColor = (float[])backgroundColor.Clone();
        uiElement.borderWidth = borderWidth;
        uiElement.borderColor = borderColor;
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
            // IMPLEMENT UI HOVER BACKGROUND LATER... if(hbg != null) screenElement.hoverBackgroundColor = hbg;
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

    ///
    /// Render UI
    ///
    public static void renderUI(
        UIData? uiData,
        int screenWidth,
        int screenHeight,
        ShaderProgram shaderProgram,
        TextRenderer? textRenderer
    ) {
        if(uiData == null || uiData.elements.Count == 0) return;

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

    ///
    /// Cleanup
    ///
    public static void cleanup() {
        if(uiVao != 0) GL.DeleteVertexArray(uiVao);
        if(uiVbo != 0) GL.DeleteBuffer(uiVbo);
        if(uiEbo != 0) GL.DeleteBuffer(uiEbo);
        uiBuffersInitialized = false;
    }

    ///
    /// Expression Evaluator
    ///
    private static string evaluateExpression(string text) {
        if(string.IsNullOrEmpty(text) || !text.Contains("${")) return text;

        return Regex.Replace(text, @"\$\{(.*?)\}", match =>
            evaluateSimpleExpression(match.Groups[1].Value)
        );
    }

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
}