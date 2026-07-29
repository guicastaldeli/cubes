namespace App.Root.Text;

class FontMetrics {
    public float lineHeight;
    public float ascent;
    public float descent;
    public float xHeight;
    public float boundingWidth;
    public float boundingHeight;

    public FontMetrics(
        float lineHeight, 
        float ascent, 
        float descent, 
        float xHeight, 
        float boundingWidth, 
        float boundingHeight
    ) {
        this.lineHeight = lineHeight;
        this.ascent = ascent;
        this.descent = descent;
        this.xHeight = xHeight;
        this.boundingWidth = boundingWidth;
        this.boundingHeight = boundingHeight;
    }
}