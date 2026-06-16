using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "DZ/NPC10_Wispy")]
    public class Npc10Wispy : Entity
    {
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public string IdleAnim = "idle";

        public Npc10Wispy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("wispy_woods"));
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
