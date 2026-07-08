namespace Celeste.Helpers;

/// <summary>
/// Handles migration of save data between mod versions.
/// </summary>
public static class DZSaveDataMigration
{
    public static void Run()
    {
        // Migration logic for save data between mod versions
        var saveData = global::Celeste.Mod.DZ.DZModule.SaveData;
        if (saveData == null)
            return;

        // Future migration steps go here
    }
}
