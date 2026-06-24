namespace Celeste
{
    [HotReloadable]
    public class FinalTitanHeightDisplayMod : Entity
    {
        private bool drawText => index >= 0 && ease > 0f && !string.IsNullOrEmpty(text);

        public FinalTitanHeightDisplayMod(int index)
        {
            Tag = (Tags.HUD | Tags.Persistent);
            this.index = index;

            int displayValue = Calc.Clamp(index + 1, 1, 12);
            int keyIndex = Calc.Clamp(index, 0, 12);
            string name = "MAGGYHELPER_CH20_HEIGHT_" + keyIndex;

            if (Dialog.Has(name, null))
            {
                text = Dialog.Get(name, null).ToUpper();
                height = displayValue;
                approach = displayValue - 1;

                int tokenIndex = text.IndexOf("{X}", StringComparison.Ordinal);
                if (tokenIndex >= 0)
                {
                    leftText = text.Substring(0, tokenIndex);
                    rightText = text.Substring(tokenIndex + 3);
                }
                else
                {
                    leftText = string.Empty;
                    rightText = text;
                }

                leftSize = ActiveFont.Measure(leftText).X;
                numberSize = ActiveFont.Measure(height.ToString()).X;
                rightSize = ActiveFont.Measure(rightText).X;
                size = ActiveFont.Measure(leftText + height + rightText);
            }

            Add(new Coroutine(routine(), true));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            spawnedLevel = (scene as Level).Session.Level;
        }

        private IEnumerator routine()
        {
            global::Celeste.Player player;
            for (;;)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && (Scene as Level).Session.Level != spawnedLevel)
                    break;

                yield return null;
            }

            stepAudioProgression();
            easingCamera = false;
            yield return 0.1f;
            Add(new Coroutine(cameraUp(), true));

            if (!string.IsNullOrEmpty(text) && index >= 0)
                Audio.Play("event:/game/pusheen/09_beyondsummit/altitude_count");

            while ((ease += Engine.DeltaTime / 0.15f) < 1f)
                yield return null;

            while (approach < height && !player.OnGround(1))
                yield return null;

            approach = height;
            pulse = 1f;
            while ((pulse -= Engine.DeltaTime * 4f) > 0f)
                yield return null;

            pulse = 0f;
            yield return 1f;

            while ((ease -= Engine.DeltaTime / 0.15f) > 0f)
                yield return null;

            RemoveSelf();
        }

        private void stepAudioProgression()
        {
            Session session = (Scene as Level).Session;
            if (setAudioProgression || index < 0 || session.Area.Mode != AreaMode.Normal)
                return;

            setAudioProgression = true;
            int progress = Calc.Clamp(index + 1, 1, 12);

            if (progress < 12)
            {
                session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl20/climb";
                session.Audio.Music.Progress = progress;
            }
            else
            {
                session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl20/final_titan";
            }

            session.Audio.Apply(false);
        }

        private IEnumerator cameraUp()
        {
            easingCamera = true;
            Level level = Scene as Level;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)
            {
                level.Camera.Y = (level.Bounds.Bottom - 180) + 64f * (1f - Ease.CubeOut(p));
                yield return null;
            }
        }

        public override void Update()
        {
            if (index >= 0 && ease > 0f)
            {
                if (height - approach > 2f)
                    approach += 20f * Engine.DeltaTime;
                else if (height - approach > 1f)
                    approach += 8f * Engine.DeltaTime;
                else if (height - approach > 0f)
                    approach += 4f * Engine.DeltaTime;
                else
                    approach = height;
            }

            Level level = Scene as Level;
            if (!easingCamera)
                level.Camera.Y = level.Bounds.Bottom - 180 + 64;

            base.Update();
        }

        public override void Render()
        {
            if (Scene.Paused)
                return;

            if (drawText)
            {
                Vector2 center = new Vector2(1920f, 1080f) / 2f;
                float scaleFactor = 1.2f + pulse * 0.2f;
                Vector2 displaySize = size * scaleFactor;
                float alpha = Ease.SineInOut(ease);
                Vector2 squash = new Vector2(1f, alpha);
                Draw.Rect(center.X - (displaySize.X + 64f) * 0.5f * squash.X, center.Y - (displaySize.Y + 32f) * 0.5f * squash.Y, (displaySize.X + 64f) * squash.X, (displaySize.Y + 32f) * squash.Y, Color.Black);

                Vector2 textPos = center + new Vector2(-displaySize.X * 0.5f, 0f);
                Vector2 scale = squash * scaleFactor;
                Color color = Color.White * alpha;

                ActiveFont.Draw(leftText, textPos, new Vector2(0f, 0.5f), scale, color);
                ActiveFont.Draw(rightText, textPos + Vector2.UnitX * (leftSize + numberSize) * scaleFactor, new Vector2(0f, 0.5f), scale, color);

                int currentApproach = (int)approach;
                if (currentApproach != _lastCachedApproach)
                {
                    _lastCachedApproach = currentApproach;
                    _cachedApproachString = currentApproach.ToString();
                }

                ActiveFont.Draw(_cachedApproachString, textPos + Vector2.UnitX * (leftSize + numberSize * 0.5f) * scaleFactor, new Vector2(0.5f, 0.5f), scale, color);
            }
        }

        public override void Removed(Scene scene)
        {
            stepAudioProgression();
            base.Removed(scene);
        }

        private int index;
        private string text = string.Empty;
        private string leftText = string.Empty;
        private string rightText = string.Empty;
        private float leftSize;
        private float rightSize;
        private float numberSize;
        private Vector2 size;
        private int height;
        private float approach;
        private float ease;
        private float pulse;
        private string spawnedLevel;
        private bool setAudioProgression;
        private bool easingCamera = true;

        // Cache number text to avoid allocations each frame.
        private int _lastCachedApproach = -1;
        private string _cachedApproachString = "0";
    }
}

