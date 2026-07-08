using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// A global UI entity that displays hot reload notifications.
    /// </summary>
    [Tracked]
    public class HotReloadUI : Entity
    {
        private static HotReloadUI Instance;

        private float alpha = 0f;
        private string message = "";
        private float displayTimer = 0f;
        private Color textColor = Color.White;

        public HotReloadUI()
        {
            Tag = Tags.HUD | Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate;
            Instance = this;
        }

        public static void Show(string msg, Color color, float duration = 3f)
        {
            if (Engine.Scene == null) return;

            if (Instance == null || Instance.Scene == null)
            {
                Instance = new HotReloadUI();
                Engine.Scene.Add(Instance);
            }

            Instance.message = msg;
            Instance.textColor = color;
            Instance.displayTimer = duration;
            Instance.alpha = 1f;
        }

        public override void Update()
        {
            base.Update();

            if (displayTimer > 0)
            {
                displayTimer -= Engine.RawDeltaTime;
                if (displayTimer <= 0.5f)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.RawDeltaTime * 2f);
                }
            }
            else
            {
                alpha = Calc.Approach(alpha, 0f, Engine.RawDeltaTime * 2f);
            }

            if (alpha <= 0 && displayTimer <= 0 && Instance == this)
            {
                RemoveSelf();
                Instance = null;
            }
        }

        public override void Render()
        {
            if (alpha <= 0 || string.IsNullOrEmpty(message)) return;

            Vector2 position = new Vector2(1920f / 2f, 100f);
            Vector2 size = ActiveFont.Measure(message);

            // Draw background bar
            Draw.Rect(position.X - size.X / 2f - 20f, position.Y - 10f, size.X + 40f, size.Y + 20f, Color.Black * alpha * 0.7f);

            // Draw text
            ActiveFont.Draw(message, position, new Vector2(0.5f, 0f), Vector2.One, textColor * alpha);
        }
    }
}
