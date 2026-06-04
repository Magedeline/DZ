using Celeste;
using Monocle;
using MonoMod.Core;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Stub entity for BirdNPCMod referenced in Prologue map.
    /// Original implementation was part of removed PCG/development systems.
    /// This minimal stub allows the level to load without errors.
    /// </summary>
    [CustomEntity("MaggyHelper/BirdNPCMod")]
    public class BirdNPCMod : Actor
    {
        public BirdNPCMod(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 100;
            Visible = false;
            Active = false;
            Logger.Log(LogLevel.Debug, "MaggyHelper/BirdNPCMod", "BirdNPCMod spawned (stub implementation)");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
    }
}
