namespace App.Root.Info;

class UserInfo {
    private readonly Store store;
    private string? tempId = null;

    public UserInfo(Store store) {
        this.store = store;
    }

    // Ensure Defaults
    public void ensureDefaults() {
        if(!store.has(ID.key)) store.set(ID, ID.defaultValue());
        if(!store.has(USERNAME.key)) store.set(USERNAME, USERNAME.defaultValue());

        store.save();
    }

    /**
     *
     * Id
     *
     */
    // Get Id
    public string getId() {
        return tempId ?? store.get(ID);
    }

    // Switch Temp Id
    public void switchTempId() {
        tempId = UserValue.Id();
    }

    // Clear Temp Id
    public void clearTempId() {
        tempId = null;
    }

    // Has Temp Id
    public bool hasTempId() {
        return tempId != null;
    }

    /**
     *
     * Username
     *
     */
    // Get Username
    public string getUsername() {
        return store.get(USERNAME);
    }

    // Set Username
    public void setUsername(string val) {
        store.set(USERNAME, val);
        store.save();
    }

    /**
     * 
     * Fields
     *
     */
    // Id
    public static readonly InfoField<string> ID = new(
        key: "id",
        defaultValue: () => UserValue.Id()
    );

    // Username
    public static readonly InfoField<string> USERNAME = new(
        key: "username",
        defaultValue: () => UserValue.Username()
    );
}