/**
    
    Attribute injection class.
    
    */
namespace App.Root.Utils;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public class InjectAttribute : Attribute {}