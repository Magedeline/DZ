using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Cutscenes;
using DZ;

namespace Celeste.NPCs
{
    [Tracked]
    [CustomEntity("DZ/NPC06_Magolor")]
    public class NPC06_Magolor : NPC
    {
        public const string CutsceneFlag = "ch6_gondola_cutscene";

        private bool started;

        public NPC06_Magolor(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
        }

        public NPC06_Magolor(Vector2 position) : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            IdleAnim = "idle";
            MoveAnim = "walk";
            Visible = false;
            Maxspeed = 48f;
        }

        public override void Update()
        {
            base.Update();
            if (started)
                return;

            if (Scene is not Level level || level.Session.GetFlag(CutsceneFlag))
                return;

            GondolaMod gondola = Scene.Entities.FindFirst<GondolaMod>();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (gondola == null || player == null || player.X <= gondola.Left - 16f)
                return;

            started = true;
            level.Session.SetFlag(CutsceneFlag, true);
            Scene.Add(new CS06_Gondola(this, gondola, player));
        }
    }
}
