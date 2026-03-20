namespace App.Root;

interface DataEntry {
    string getId();
    Dictionary<string, object> serialize();
}