using Celeste;
using Monocle;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Stub entity for MultiCharacterCutscene referenced in level entities.
    /// Original implementation was part of removed systems.
    /// This minimal stub allows the level to load without errors.
    /// </summary>
    [CustomEntity("DesoloZantas/MultiCharacterCutscene")]
    public class MultiCharacterCutscene : Entity
    {
        public MultiCharacterCutscene(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 100;
            Visible = false;
            Active = false;
            Logger.Log(LogLevel.Debug, "DZ/MultiCharacterCutscene", "MultiCharacterCutscene spawned (stub implementation)");
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
