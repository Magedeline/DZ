using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [CustomEntity("DZ/MaddyCrystalPedestal")]
    [Tracked]
    public class MaddyCrystalPedestal : Solid
    {
        public Image sprite;
        public bool DroppedMaddy;

        public MaddyCrystalPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, false)
        {
            Add(sprite = new Image(GFX.Game["characters/DZ/maddyCrystal/pedestal"]));
            EnableAssistModeChecks = false;
            sprite.JustifyOrigin(0.5f, 1f);
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = true;
            OnDashCollide = (player, direction) =>
            {
                MaddyCrystal entity = Scene.Tracker.GetEntity<MaddyCrystal>();
                entity.OnPedestal = false;
                entity.Speed = new Vector2(0.0f, -300f);
                DroppedMaddy = true;
                Collidable = false;
                (Scene as Level).Flash(Color.White);
                Celeste.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                Audio.Play("event:/game/05_mirror_temple/crystalmaddy_break_free", entity.Position);
                return DashCollisionResults.Rebound;
            };
            Tag = (int)Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("foundMaddyInCrystal"))
            {
                DroppedMaddy = true;
                Collidable = false;
            }
            else
            {
                MaddyCrystal first = Scene.Entities.FindFirst<MaddyCrystal>();
                if (first == null)
                    return;
                first.Depth = Depth + 1;
            }
        }

        public override void Update()
        {
            MaddyCrystal entity = Scene.Tracker.GetEntity<MaddyCrystal>();
            if (entity != null && !DroppedMaddy)
            {
                entity.Position = Position + new Vector2(0.0f, -32f);
                entity.OnPedestal = true;
            }
            base.Update();
        }
    }
}