using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity("DZ/SansHeh")]
    public class SansHeh : Entity
    {
        private class HehParticle
        {
            public Sprite Sprite;
            public float Percent;
            public float Duration;

            public HehParticle()
            {
                Sprite = new Sprite(GFX.Game, "characters/sans/");
                Sprite.Add("ha", "ha", 0.1f, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Sprite.Play("ha");
                Sprite.JustifyOrigin(0.5f, 1f);
                Duration = Sprite.CurrentAnimationTotalFrames * 0.1f;
            }
        }

        private bool enabled;
        private string ifSet;
        private float timer;
        private int counter;
        private List<HehParticle> hehs = new List<HehParticle>();
        private bool autoTriggerLaughSfx;
        private Vector2 autoTriggerLaughOrigin;
        private string laughSfx;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (!enabled && value)
                {
                    timer = 0f;
                    counter = 0;
                }
                enabled = value;
            }
        }

        public SansHeh(Vector2 position, string ifSet = "", bool triggerLaughSfx = false, Vector2? triggerLaughSfxOrigin = null, string laughSfx = "event:/char/sans/laugh_oneha")
        {
            Depth = -10001;
            Position = position;
            this.ifSet = ifSet;
            this.laughSfx = laughSfx;
            if (triggerLaughSfx)
            {
                autoTriggerLaughSfx = triggerLaughSfx;
                autoTriggerLaughOrigin = triggerLaughSfxOrigin.Value;
            }
        }

        public SansHeh(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("ifset"), data.Bool("triggerLaughSfx"), (data.Nodes.Length != 0) ? (offset + data.Nodes[0]) : Vector2.Zero, data.Attr("laughSfx", "event:/char/sans/laugh_oneha"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(ifSet) && !(Scene as Level).Session.GetFlag(ifSet))
            {
                Enabled = false;
            }
        }

        public override void Update()
        {
            if (Enabled)
            {
                timer -= Engine.DeltaTime;
                if (timer <= 0f)
                {
                    hehs.Add(new HehParticle());
                    counter++;
                    if (counter >= 3)
                    {
                        counter = 0;
                        timer = 1.5f;
                    }
                    else
                    {
                        timer = 0.6f;
                    }
                }
                if (autoTriggerLaughSfx && !string.IsNullOrEmpty(laughSfx) && Scene.OnInterval(0.4f))
                {
                    Audio.Play(laughSfx, autoTriggerLaughOrigin);
                }
            }
            for (int num = hehs.Count - 1; num >= 0; num--)
            {
                if (hehs[num].Percent > 1f)
                {
                    hehs.RemoveAt(num);
                }
                else
                {
                    hehs[num].Sprite.Update();
                    hehs[num].Percent += Engine.DeltaTime / hehs[num].Duration;
                }
            }
            if (!Enabled && !string.IsNullOrEmpty(ifSet) && (Scene as Level).Session.GetFlag(ifSet))
            {
                Enabled = true;
            }
            base.Update();
        }

        public override void Render()
        {
            foreach (HehParticle heh in hehs)
            {
                heh.Sprite.Position = Position + new Vector2(heh.Percent * 60f, -10f + (float)(0.0 - Math.Sin(heh.Percent * 13f)) * 4f + heh.Percent * -16f);
                heh.Sprite.Render();
            }
        }
    }
}
