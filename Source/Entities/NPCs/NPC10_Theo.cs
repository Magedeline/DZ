using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC10_Theo")]
    public class Npc10Theo : Entity
    {
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public string IdleAnim = "idle";
        public string MoveAnim = "walk";
        public float Maxspeed = 48f;

        public Npc10Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("theo"));
            sprite.Play("idle");
        }

        public IEnumerator MoveTo(Vector2 target)
        {
            while (Math.Abs(Position.X - target.X) > 1f || Math.Abs(Position.Y - target.Y) > 1f)
            {
                Vector2 direction = (target - Position).SafeNormalize();
                Position += direction * Maxspeed * Engine.DeltaTime;
                yield return null;
            }
            Position = target;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
