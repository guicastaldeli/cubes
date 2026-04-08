/*

    Global controller for main
    configurations of the program
    instances, etc...

    */
namespace App.Root;

using System.Reflection;
using System.Runtime.Serialization;

public enum Instance {
    [EnumMember(Value = "debug")] 
    DEBUG,
    [EnumMember(Value = "dev")] 
    DEV,
    [EnumMember(Value = "prod")] 
    PROD
}

class Controller {
    // Get Instance
    public static Dictionary<string, Instance> getInstance() {
        return Enum.GetValues<Instance>()
            .ToDictionary(i => 
                i.GetType()
                .GetField(i.ToString())!
                .GetCustomAttribute<EnumMemberAttribute>()!
                .Value!, i => i
            );
    }
}