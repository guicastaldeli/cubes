namespace App.Root.Screen;
using System.Collections.Generic;

class ScreenData {
    public string screenType;
    public List<ScreenElement> elements;
    public Dictionary<string, string> screenAttr;
    
    public ScreenData(string screenType) {
        this.screenType = screenType;
        this.elements = new List<ScreenElement>();
        this.screenAttr = new Dictionary<string, string>();
    }
}