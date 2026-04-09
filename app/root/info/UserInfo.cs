namespace App.Root.Info;

class UserInfo {
    /**

        Store

        */

    private readonly Store store;

    public UserInfo(Store store) {
        this.store = store;
    }

    // Get Id
    public string getId() {
        return store.get(ID);
    }

    // Username
    public string getUsername() {
        return store.get(USERNAME);
    }

    public void setUsername(string val) {
        store.set(USERNAME, val);
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