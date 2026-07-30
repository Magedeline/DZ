using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [CustomEntity("DZ/NPC00_Theo")]
    [Tracked(false)]
    public class NPC00_Theo : Entity
    {
        private bool talking;

        public Sprite Sprite;

        public SoundSource Hahaha;

        public Level Level;

        public Session Session => Level?.Session;

        public NPC00_Theo(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Play("idle");
            Add(Hahaha = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = scene as Level;
            if (Level.Session.GetFlag("theo"))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            Player entity = Level.Tracker.GetEntity<Player>();
            if (entity != null && !Session.GetFlag("theo") && !talking)
            {
                int num = Level.Bounds.Left + 96;
                if (entity.OnGround() && entity.X >= (double)num && entity.X <= X + 16.0 && Math.Abs(entity.Y - Y) < 4.0 && entity.Facing == (Facings)Math.Sign(X - entity.X))
                {
                    talking = true;
                    Scene.Add(new CS00_Theo(this, entity));
                }
            }
        }
    }
}