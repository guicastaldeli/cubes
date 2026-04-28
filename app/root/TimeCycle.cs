/**

    Main Time Cycle class to
    control general time.

    */
namespace App.Root;
using NLua;

/**

    Period helper

    */
class Period {
    private static string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "utils/TimePeriod.lua");

    private static TimeCycle timeCycle = null!;

    public static Lua data = null!;
    public static LuaTable? periods = null!;
    public static LuaTable? currentPeriod;

    // Get Current
    public static LuaTable? getCurrent() {
        LuaFunction? current = data["getCurrent"] as LuaFunction;
        if(current == null) return null;

        object[] res = current.Call(timeCycle.getHour());
        return res[0] as LuaTable;
    }

    // Get Name
    public static string getName(LuaTable period) {
        string val = period["name"] as string ?? "UNKNOWN";
        return val;
    }

    // Get Start
    public static int getStart(LuaTable period) {
        int val = Convert.ToInt32(period["s"]);
        return val;
    }

    // Get End
    public static int getEnd(LuaTable period) {
        int val = Convert.ToInt32(period["e"]);
        return val;
    }

    /**

        Init
    
        */
    public static void init(TimeCycle timeCycle) {
        Period.timeCycle = timeCycle;
        data = new Lua();
        data.DoFile(DATA_PATH);
    }

    /**

        Update
    
        */
    public static void update() {
        periods = data["Period"] as LuaTable;
        currentPeriod = getCurrent();
    }

    public static void updatePeriod() {
        LuaTable? newPeriod = getCurrent();
        if(newPeriod != null && currentPeriod != null) {
            if(getName(newPeriod) != getName(currentPeriod)) {
                currentPeriod = newPeriod;
                //Console.WriteLine($"Period changed to: {getName(currentPeriod)}");
            } 
        }
    }
}

/**

    Main Time Cycle class.
    
    */
class TimeCycle {
    private const float DAY_DURATION = 120.0f;
    private const float HOUR_DURATION = DAY_DURATION / 24.0f;

    private Tick tick;

    private float currentTime = 6.0f * HOUR_DURATION;
    private float timeSpeed = 10.0f;
    private float timeDayPercentage = 0.25f;

    private float hourDiv = 24.0f;
    private float minDiv = 60.0f;

    public TimeCycle(Tick tick) {
        this.tick = tick;

        Period.init(this);
        Period.update();
        
        setTime(7, 0);
        updateTime();
    }

    // Get Hour
    public float getHour() {
        float hour = (currentTime / DAY_DURATION) * hourDiv;
        return hour;
    }

    // Get Minute
    public int getMinute() {
        float hourFraction = getHour();
        float minFraction = hourFraction - (int)hourFraction;
        
        return (int)(minFraction * minDiv);
    }

    // Get Formatted Time
    public string getFormattedTime() {
        int hour = (int)getHour();
        int min = getMinute();

        string val = $"{hour:D2}:{min:D2}";
        return val;
    }

    // Get Time of Day Percentage
    public float getTimeOfDayPercentage() {
        return timeDayPercentage;
    }

    // Set Pause
    public void setPause(bool paused) {
        float f = 0.0f;
        timeSpeed = paused ? f : minDiv;
    }

    /**
    
        Time
    
        */
    public void setTime(int hour, int min) {
        float totalHours = hour + (min / minDiv);
        currentTime = (totalHours / hourDiv) * DAY_DURATION;
        
        updateTime();
        Period.updatePeriod();
    }

    public void setTimeSpeed(float speed) {
        timeSpeed = Math.Max(0.0f, speed);
    }

    public float getTimeSpeed() {
        return timeSpeed;
    }

    public string getTime() {
        if(Period.currentPeriod == null) return "";
        
        string name = Period.getName(Period.currentPeriod);
        string time = getFormattedTime();
        
        string val = $"TIME: {time} ({name})";
        return val;
    }

    /**
    
        Update
    
        */
    public void update() {
        currentTime += tick.getDeltaTime() * timeSpeed;
        if(currentTime >= DAY_DURATION) {
            currentTime -= DAY_DURATION;
        } else if(currentTime < 0) {
            currentTime += DAY_DURATION;
        }

        updateTime();
        Period.updatePeriod();
    }

    private void updateTime() {
        timeDayPercentage = currentTime / DAY_DURATION;
    }

    /**
    
        Cleanup
    
        */
    public void cleanup() {
        Period.data.Dispose();
    }
}