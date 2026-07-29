namespace App.Root.Info;

class UserValue {
    /**
     *
     * Id
     *
     */
    public static string Id() {
        string val = Guid.NewGuid().ToString();
        return val;
    }

    /**
     *
     * Username
     *
     */
    public static string Username() {
        int min = 1000;
        int max = 9999;
        string val = $"User_{Random.Shared.Next(min, max)}";

        return val;
    }
}