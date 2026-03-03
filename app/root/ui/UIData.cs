namespace App.Root.ui;
using System.Collections.Generic;

class UIData {
    public string uiType;
    public List<UIElement> elements;
    public Dictionary<string, string> uiAttr;

    public UIData(string uiType) {
        this.uiType = uiType;
        this.elements = new List<UIElement>();
        this.uiAttr = new Dictionary<string, string>();
    }
}