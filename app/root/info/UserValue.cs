namespace App.Root.Info;

class UserValue {
    // Id
    public static string id() {
        string val = Guid.NewGuid().ToString();
        return val;
    }

    // Username
    public static string username() {
        int min = 1000;
        int max = 9999;
        string val = $"User_{Random.Shared.Next(min, max)}";

        return val;
    }
}