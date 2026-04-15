namespace App.Root.Info;

class UserInfo {
    /**

        Store

        */
    private readonly Store store;
    private string? tempId = null;

    public UserInfo(Store store) {
        this.store = store;
    }

    // Id
    public string getId() {
        return tempId ?? store.get(ID);
    }

    public void switchTempId() {
        tempId = UserValue.id();
    }

    public void clearTempId() {
        tempId = null;
    }

    public bool hasTempId() {
        return tempId != null;
    }

    // Username
    public string getUsername() {
        return store.get(USERNAME);
    }

    public void setUsername(string val) {
        store.set(USERNAME, val);
        store.save();
    }

    // Ensure Defaults
    public void ensureDefaults() {
        if(!store.has(ID.key)) store.set(ID, ID.defaultValue());
        if(!store.has(USERNAME.key)) store.set(USERNAME, USERNAME.defaultValue());

        store.save();
    }

    /**

        Fields

        */

    // Id
    public static readonly InfoField<string> ID = new(
        key: "id",
        defaultValue: () => UserValue.id()
    );

    // Username
    public static readonly InfoField<string> USERNAME = new(
        key: "username",
        defaultValue: () => UserValue.username()
    );
}