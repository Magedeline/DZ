using System;
using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC10_Toriel")]
    public class Npc10Toriel : Entity
    {
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public string IdleAnim = "idle";
        public string MoveAnim = "walk";
        public float Maxspeed = 48f;
        public Facings Facing = Facings.Right;

        public Npc10Toriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("toriel"));
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
