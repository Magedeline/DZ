using Celeste.Mod;

namespace Celeste.Mod.MaggyHelper.Audio;

/// <summary>
/// Minimal EverestModule entry point for the DesoloZantas_Audio content mod.
/// Its only purpose is to make DesoloZantas_Audio appear in Everest.Modules
/// so OverworldMusicManager can resolve its PathDirectory at bank-load time.
/// </summary>
public class DesoloZantasAudioModule : EverestModule
{
    public override void Load() { }
    public override void Unload() { }
}
