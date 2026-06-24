using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC10_Madeline")]
    public class Npc10Madeline : Entity
    {
        private MadelineDummy dummy;
        public Sprite sprite;
        public Sprite Sprite { get => sprite; set => sprite = value; }

        public Npc10Madeline(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            setupDummy();
            Depth = 100;
        }

        private void setupDummy()
        {
            dummy = new MadelineDummy(Vector2.Zero);
            sprite = dummy.Sprite;
            Add(sprite);
            sprite.Play("idle");
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
