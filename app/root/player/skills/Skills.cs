namespace App.Root.Player.Skills;
using App.Root.Utils;
using App.Root.Input;
using System.Reflection;
using NLua;

/**

    Skills Data

    */
[ActionConverter]
[DataInput]
[DataOutput(Path: "player_storage.ps")]
class SkillsData {
    public class Skill {
        [Convert("int32")] [ConverterKey("id")] public int Id { get; set; } = 0;
        [Convert("string")] [ConverterKey("name")] public string Name { get; set; } = "";
        [Convert("string")] [ConverterKey("movement")] public string? Movement { get; set; }
        [Convert("string")] [ConverterKey("audio")] public string? Audio { get; set; }
        [Convert("string")] [ConverterKey("particles")] public string? Particles { get; set; }

        public Skill() {}
    }

    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "player/skills/SkillsData.lua");

    private static Lua data = null!;
    private static List<Skill> skills = null!;

    private static Skills skill = null!;

    private static readonly Dictionary<string, MethodInfo> converters = typeof(Converter)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            m => m.GetCustomAttribute<ConverterKey>()!.Key,
            m => m
        );
    
    private static readonly Dictionary<string, PropertyInfo> skillProps = typeof(Skill)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(s => s.GetCustomAttribute<ConverterKey>() != null)
        .ToDictionary(
            s => s.GetCustomAttribute<ConverterKey>()!.Key,
            s => s
        );

    // Has Data
    public static bool HasData() {
        bool val = skills != null && skills.Count > 0;
        return val;
    }

    // Get Skills
    public static List<Skill> GetSkills() {
        if(skills == null) ExtractData();

        List<Skill> val = skills ?? new List<Skill>();
        return val;
    }

    // Get Skill
    public static Skill GetSkill(int id) {
        Skill val = GetSkills().FirstOrDefault(s => s.Id == id)!;
        return val;
    }

    public static Skill GetSkill(string name) {
        Skill val = GetSkills().FirstOrDefault(s => s.Name == name)!;
        return val;
    }

    // Handle Mouse Click
    [GlobalInput]
    public static void HandleMouseClick(int skillId) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[SkillsData] Applying skill: {skillId}");
        Console.ResetColor();

        Apply(skillId);
    }

    /**
     *
     * Apply
     *
     */
    private static void Apply(int id) {
        var data = GetSkill(id);

        if(data != null) {
            if(skill != null) {
                skill.applySkill(data);
            } else {
                Console.WriteLine("err skill null");    
            }
        } else {
            Console.WriteLine("err skill data null");
        }
    }

    /**
     *
     * Extract Data
     *
     */
    public static object ExtractData() {
        if(skills != null) return skills;

        Load();
        skills = new List<Skill>();
        if(data == null) return skills;

        try {
            var skillsData = data["Skills"] as LuaTable;
            if(skillsData == null) {
                Console.WriteLine("[SkillsData] Skills table not found!");
                return skills;
            }

            foreach(var key in skillsData.Keys) {
                var skillData = skillsData[key] as LuaTable;
                if(skillData != null) {
                    var skill = new Skill();

                    foreach(var field in skillProps) {
                        var fieldKey = field.Key;
                        var propInfo = field.Value;

                        if(skillData[fieldKey] != null) {
                            var val = skillData[fieldKey];
                            var attr = propInfo.GetCustomAttribute<ConvertAttribute>();

                            if(attr != null) {
                                string name = attr.Converter;

                                if(converters.TryGetValue(name, out var converter)) {
                                    try {
                                        var converted = converter.Invoke(null, new[] { val });
                                        propInfo.SetValue(skill, converted);
                                    } catch(Exception err) {
                                        Console.WriteLine($"[SkillsData] Converter error for {fieldKey}: {err.Message}");
                                    }
                                }
                            }
                        }
                    }

                    skills.Add(skill);
                }
            }

            Console.WriteLine($"ExtractData() -- [SkillsData] Extracted {skills.Count}");
        } catch(Exception err) {
            throw new Exception($"ExtractData() -- [SkillsData] Error: {err.Message}");
        }

        return skills;
    }

    /**
     *
     * Save Data
     *
     */
    public static void SaveData(object data) {
        if(data is List<Skill> s) {
            skills = s;
            Console.WriteLine($"SaveData() -- [SkillsData] Saved {skills?.Count ?? 0} skills");
        }
    }

    /**
     *
     * Load
     *
     */
    private static void Load() {
        if(data != null) return;

        if(!File.Exists(DATA_PATH)) {
            Console.WriteLine($"[SkillsData] File not found: {DATA_PATH}");
            return;
        }

        data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        try {
            var result = data.DoFile(DATA_PATH);
            
            if(result != null && result.Length > 0) {
                var returnTable = result[0] as LuaTable;
                if(returnTable != null) {
                    var skillsTable = returnTable["Skills"];
                    if(skillsTable != null) {
                        data["Skills"] = skillsTable;
                        Console.WriteLine("[SkillsData] Successfully loaded skills from Lua");
                    } else {
                        Console.WriteLine("[SkillsData] No 'Skills' key in returned table");
                    }
                }
            }
        } catch(Exception err) {
            Console.WriteLine($"[SkillsData] Error loading Lua file: {err.Message}");
            Console.WriteLine(err.StackTrace);
        }
        
        Directory.SetCurrentDirectory(originalDir);
    }

    /**
     *
     * Init
     *
     */
    public static void Init(Skills skill) {
        SkillsData.skill = skill;
    }
}

/**

    Skills main class

    */
class Skills {
    private Window window;
    private Tick tick;

    public Skills(Window window, Tick tick) {
        this.window = window;
        this.tick = tick;

        SkillsData.Init(this);
    }

    /**
     * 
     * Apply
     *
     */
    // Apply Skill
    public void applySkill(SkillsData.Skill data) {
        if(data == null) {
            Console.WriteLine("[Platform] Cannot apply null theme");
            return;
        }

        Console.WriteLine($"[Platform] Applying skill: {data.Name} (ID: {data.Id})");

        applyData(data);

        Console.WriteLine($"[Skill] Skill applied successfully!");
    }

    // Apply Data
    private void applyData(SkillsData.Skill data) {
        window.queueOnRenderThread(() => {
            updateMovement(data.Movement);
        });
    }

    /**
     *
     * Update
     *
     */
    // Update
    public void update() {
        
    }

    // Update Movement
    private void updateMovement(string? data) {
        if(string.IsNullOrEmpty(data)) return;

        var config = PlayerMovement.Convert(data);
        if(config == null) return;

        PlayerMovement.Apply(config);
    }

    // Update Audio

    // Update Particles
}