/**
    
    Attribute Converter class.
    
    */
namespace App.Root.Utils;

/**

    Converter Attribute

    */
[AttributeUsage(AttributeTargets.Field)]
public class ConvertAttribute : Attribute {
    public string Converter {
        get;
    }

    public ConvertAttribute(string converter) {
        this.Converter = converter;
    }
}

/**

    Converter Key

    */
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class ConverterKey : Attribute {
    public string Key {
        get;
    }

    public ConverterKey(string key) {
        this.Key = key;
    }
}