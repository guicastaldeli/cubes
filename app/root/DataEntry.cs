/**
    
    Main Data entry interface

    */
namespace App.Root;

interface DataEntry {
    string? getId() => null;
    Dictionary<string, object> serialize();
}