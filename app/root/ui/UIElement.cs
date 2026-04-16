namespace App.Root.UI;
using System.Collections.Generic;

class UIElement {
    public string type;
    public string id;
    public string text;
    public string fontFamily;
    public int x;
    public int y;
    public int width;
    public int height;
    public float scale;
    public float[] color;
    public float[] backgroundColor;
    public string action;
    public Dictionary<string, string> attr;
    public bool visible;
    public bool hasBackground;

    public float borderWidth;
    public float[] borderColor;

    public bool hasShadow = false;
    public float shadowOffsetX = 0f;
    public float shadowOffsetY = 0f;
    public float shadowBlur = 0f;
    public float[] shadowColor = new float[]{ 0f, 0f, 0f, 0.5f };

    public bool hoverable = false;
    public bool isHovered = false;
    public float[]? hoverColor = null;
    public float[]? hoverTextColor = null;
    public float[]? hoverBorderColor = null;
    public float hoverScale = 1.0f;

    private float[] originalColor;
    private float[] originalBorderColor;
    public float[] originalBackgroundColor;
    private float originalScale;

    public int textureId = -1;
    public bool hasTexture = false;

    public int imgWidth = 0;
    public int imgHeight = 0;

    public UIElement(
        string type,
        string id,
        string text,
        string fontFamily,
        int x, int y,
        int width, int height,
        float scale,
        float[] color,
        bool hasBackground,
        string action
    ) {
        this.type = type;
        this.id = id;
        this.text = text;
        this.fontFamily = fontFamily ?? "arial";
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.scale = scale;
        this.color = color;
        this.hasBackground = hasBackground;
        this.action = action;
        this.attr = new Dictionary<string, string>();
        this.visible = true;
        this.borderWidth = 0.0f;
        this.borderColor = new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        this.backgroundColor = new float[]{ 0f, 0f, 0f, 0f };
        this.originalBackgroundColor = new float[]{ 0f, 0f, 0f, 0f };

        this.originalColor = color != null ? (float[])color.Clone() : new float[]{ 1.0f, 1.0f, 1.0f, 1.0f };
        this.originalBorderColor = (float[])borderColor.Clone();
        this.originalScale = scale;
    }

    public float getRed() {
        float val = color.Length > 0 ? color[0] : 1.0f;
        return val;
    } 

    public float getGreen() {
        float val = color.Length > 1 ? color[1] : 1.0f;
        return val;
    }

    public float getBlue() {
        float val = color.Length > 2 ? color[2] : 1.0f;
        return val;
    }

    public float getAlpha() {
        float val = color.Length > 3 ? color[3] : 1.0f;
        return val;
    }

    public bool hasBorder() {
        bool val = borderWidth > 0.0f;
        return val;
    }

    public void setText(string newText) {
        this.text = newText;
    }

    public void setVisible(bool visible) {
        this.visible = visible;
    }

    // Hover
    public void applyHover() {
        if(!hoverable || isHovered) return;
        
        isHovered = true;
        if(hoverColor != null) color = hoverColor;
        if(hoverBorderColor != null) borderColor = hoverBorderColor;
        if(hoverScale > 0) scale = hoverScale;
    }

    public void removeHover() {
        if(!hoverable || !isHovered) return;
        
        isHovered = false;
        color = (float[])originalColor.Clone();
        borderColor = (float[])originalBorderColor.Clone();
        scale = originalScale;
    }

    // Contains Point
    public bool containsPoint(int mouseX, int mouseY) {
        return mouseX >= x && mouseX <= x + width &&
            mouseY >= y && mouseY <= y + height;
    }
}