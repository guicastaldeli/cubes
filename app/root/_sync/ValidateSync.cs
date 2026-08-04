namespace App.Root._Sync;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using App.Root._Crypto;

public class ValidateSync {
    public enum ValidationType {
        REQUIRED,
        RANGE,
        PATTERN
    }

    public class ValidationResult {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void AddError(string error) => Errors.Add(error);        
        public void AddWarning(string warning) => Warnings.Add(warning);
    }

    public class ValidationRule {
        public string FieldName { get; set; } = "";
        public ValidationType Type { get; set; }
        public object? MinValue { get; set; }
        public object? MaxValue { get; set; }
        public string? Pattern { get; set; }
        public string? CustomValidator { get; set; }
        public bool Required { get; set; }
    }

    private Dictionary<string, List<ValidationRule>> rules = new();

    // Get Rules
    private List<ValidationRule> GetRules(Type type) {
        string key = type.Name;
        if(rules.TryGetValue(key, out var cachedRules)) return cachedRules;

        var newRules = new List<ValidationRule>();
        
        var attr = type.GetCustomAttribute<DataSyncAttribute>();
        if(attr != null && attr.ValidateAlways) {
            foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var syncAttr = prop.GetCustomAttribute<SyncFieldAttribute>();
                if(syncAttr != null && !syncAttr.Ignore) {
                    var rule = new ValidationRule {
                        FieldName = prop.Name,
                        Type = ValidationType.REQUIRED,
                        Required = true
                    };

                    newRules.Add(rule);
                }
            }  
        }

        rules[key] = newRules;
        return newRules;
    }
    
    /**
     *
     * Validate
     *
     */
    // Validate
    public ValidationResult Validate(object data, string? userId = null) {
        var result = new ValidationResult();
        var type = data.GetType();

        var rules = GetRules(type);
        if(rules.Count == 0) {
            result.IsValid = true;
            return result;
        }

        foreach(var rule in rules) ValidateField(data, rule, result, userId);
        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    // Validate Packet
    public bool ValidatePacket(PacketSync packet) {
        if(packet == null) return false;
        if(string.IsNullOrEmpty(packet.DataId)) return false;
        if(string.IsNullOrEmpty(packet.Action)) return false;
        if(packet.Timestamp <= 0) return false;
        if(packet.Payload == null || packet.Payload.Length == 0) return false;
        if(packet.Checksum != null && packet.Checksum.Length > 0) {
            if(!CryptoProvider.VerifyHash(packet.Payload, packet.Checksum)) {
                Console.WriteLine($"[SyncValidator] Invalid checksum for {packet.DataId}");
                return false;
            }
        }
        
        return true; 
    }

    // Validate Field
    private void ValidateField(object data, ValidationRule rule, ValidationResult result, object? userId) {
        var type = data.GetType();
        var prop = type.GetProperty(rule.FieldName);
        var field = type.GetField(rule.FieldName);

        object? value = null;
        if(prop != null) value = prop.GetValue(data);
        else if(field != null) value = field.GetValue(data);

        switch(rule.Type) {
            case ValidationType.REQUIRED:
                if(value == null) result.AddError($"{rule.FieldName} is required");
                break;
            case ValidationType.RANGE:
                if(value is IComparable comparable) {
                    if(rule.MinValue != null && comparable.CompareTo(rule.MinValue) < 0) result.AddError($"{rule.FieldName} is below minimum value");
                    if(rule.MaxValue != null && comparable.CompareTo(rule.MaxValue) > 0) result.AddError($"{rule.FieldName} is above maximum value");
                }
                break;
            case ValidationType.PATTERN:
                if(value is string str && !string.IsNullOrEmpty(rule.Pattern)) {
                    if(!Regex.IsMatch(str, rule.Pattern)) {
                        result.AddError($"{rule.FieldName} does not match pattern");
                    }
                }
                break;
        }
    }
}