namespace Celeste.Mod.DZ;

public class OverworldHelperSettings : EverestModuleSettings
{
    [SettingNeedsRelaunch]
    [SettingName("DZ_OverworldHelper_Settings_Enabled")]
    public bool Enabled { get; set; } = true;
}