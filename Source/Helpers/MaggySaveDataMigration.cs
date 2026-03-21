namespace MaggyHelper.Helpers
{
    /// <summary>
    /// Handles migration of save data between mod versions.
    /// </summary>
    public static class MaggySaveDataMigration
    {
        public static void Run()
        {
            // Migration logic for save data between mod versions
            var saveData = Celeste.Mod.MaggyHelper.MaggyHelperModule.SaveData;
            if (saveData == null)
                return;

            // Future migration steps go here
        }
    }
}
