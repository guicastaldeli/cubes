using System.Runtime.CompilerServices;

namespace App.Root.Utils;

public static class WordInflector {
    // Is Vowel
    private static bool IsVowel(char c) {
        HashSet<char> v = new () { 
            'a', 'e', 'i', 'o', 'u',
            'A', 'E', 'I', 'O', 'U' 
        };

        bool val = v.Contains(c);
        return val;
    }

    /**
     *
     * To Singular
     *
     */
    public static string ToSingular(string word) {
        if(string.IsNullOrEmpty(word)) return word;

        var lower = word.ToLower();
        
        if(lower.EndsWith("ies")) return word.Substring(0, word.Length - 3) + "y";
        if(lower.EndsWith("es")) return word.Substring(0, word.Length - 2);
        if(lower.EndsWith("s")) return word.Substring(0, word.Length - 1);

        return word;
    }

    /**
     *
     * To Plural
     *
     */
    public static string ToPlural(string word) {
        if(string.IsNullOrEmpty(word)) return word;

        var lower = word.ToLower();

        if(lower.EndsWith("y") && !IsVowel(lower[word.Length - 2])) return word.Substring(0, word.Length - 1) + "ies";
        if(lower.EndsWith("s") || lower.EndsWith("x") || lower.EndsWith("z") || lower.EndsWith("ch") || lower.EndsWith("ch")) return word + "es";

        string val = word + "s";
        return val;
    }
}