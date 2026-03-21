using System;
using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC10_Madeline")]
    public class Npc10Madeline : Entity
    {
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }

        public Npc10Madeline(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupSprite();
            Depth = 100;
        }

        private void setupSprite()
        {
            Add(sprite = GFX.SpriteBank.Create("madeline"));
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
