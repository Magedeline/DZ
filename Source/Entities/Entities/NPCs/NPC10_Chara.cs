using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.NPCs
{
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC10_Chara")]
    public class Npc10Chara : Entity
    {
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public string IdleAnim = "idle";

        public Npc10Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("maggy_chara"));
            sprite.Play("idle");
        }

        public IEnumerator MoveTo(Vector2 target)
        {
            while (Math.Abs(Position.X - target.X) > 1f || Math.Abs(Position.Y - target.Y) > 1f)
            {
                Vector2 direction = (target - Position).SafeNormalize();
                Position += direction * 48f * Engine.DeltaTime;
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
