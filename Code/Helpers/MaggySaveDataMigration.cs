namespace Celeste.Helpers
{
    /// <summary>
    /// Handles migration of save data between mod versions.
    /// </summary>
    public static class KIRBY_CELESTESaveDataMigration
    {
        public static void Run()
        {
            // Migration logic for save data between mod versions
            var saveData = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.SaveData;
            if (saveData == null)
                return;

            // Future migration steps go here
        }
    }
}
