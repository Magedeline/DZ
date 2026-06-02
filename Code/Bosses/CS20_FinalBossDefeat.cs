namespace Celeste.Cutscenes
{
    /// <summary>
    /// Post-final boss cutscene: Reality restoration and heart revival
    /// </summary>
    [Tracked]
    public class CS20_FinalBossDefeat : CutsceneEntity
    {
        private global::Celeste.Player player;
        private Vector2 madelinePos;
        private Vector2 badelinePos;
        private Vector2 asrielPos;
        
        private Sprite madelineSprite;
        private Sprite badelineSprite;
        private Sprite asrielSprite;
        
        private VertexLight restorationLight;
        private float healingProgress = 0f;
        private float overlayAlpha;
        private Color overlayColor = Color.Transparent;
        
        public CS20_FinalBossDefeat(global::Celeste.Player player) : base(false, true)
        {
            this.player = player;
        }
        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                
            var level = scene as Level;
            if (level != null)
            {
                level.TimerStopped = true;
                level.TimerHidden = true;
                level.SaveQuitDisabled = true;
                level.PauseLock = true;
            }
            
            setupSprites();
        }

        private void setOverlay(Color color, float alpha)
        {
            overlayColor = color;
            overlayAlpha = Calc.Clamp(alpha, 0f, 1f);
        }
        
        private void setupSprites()
        {
            // Create sprites for the characters
            madelineSprite = GFX.SpriteBank.Create("madeline");
            madelineSprite.Play("sleep");
            Add(madelineSprite);
            
            badelineSprite = GFX.SpriteBank.Create("maggy_badeline");
            badelineSprite.Play("sleep");
            Add(badelineSprite);
            
            asrielSprite = new Sprite(GFX.Game, "characters/asriel/");
            asrielSprite.AddLoop("idle", "idle", 0.1f);
            asrielSprite.AddLoop("kneeling", "kneel", 0.1f);
            asrielSprite.Play("kneeling");
            Add(asrielSprite);
            
            // Restoration light effect
            Add(restorationLight = new VertexLight(Color.Gold * 1.5f, 1f, 256, 384));
            restorationLight.Alpha = 0f;
        }
        
        public override void OnBegin(Level level)
        {
            if (level == null)
            {
                EndCutscene(level);
                return;
            }

            Audio.SetAmbience(null);
            Audio.SetMusic("event:/cutscene/final_boss_aftermath");
            
            Add(new Coroutine(cutsceneSequence(level)));
        }
        
        private IEnumerator cutsceneSequence(Level level)
        {
            // Fade in from battle
            yield return Fade(1f, 0f, 2f);
            
            // Position characters
            madelinePos = new Vector2(level.Camera.X + 120f, level.Camera.Bottom - 100f);
            badelinePos = new Vector2(level.Camera.X + 180f, level.Camera.Bottom - 100f);
            asrielPos = new Vector2(level.Camera.X + 280f, level.Camera.Bottom - 100f);
            
            madelineSprite.Position = madelinePos;
            badelineSprite.Position = badelinePos;
            asrielSprite.Position = asrielPos;
            
            yield return 1f;
            
            // Reality begins to restore itself
            Audio.Play("event:/cutscene/reality_restoration");
            
            // Visual effect: reality healing
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                healingProgress = t / 3f;
                
                // Particle effects
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 2, 
                    new Vector2(level.Camera.X + level.Camera.Viewport.Width / 2f, level.Camera.Y + level.Camera.Viewport.Height / 2f),
                    Vector2.One * 100f);
                
                yield return null;
            }
            
            yield return 1f;
            
            // Asriel approaches Madeline and Badeline
            yield return Textbox.Say("CH20_ASRIEL_RESTORATION_START");
            
            // Asriel kneels beside them
            Audio.Play("event:/cutscene/asriel_kneels");
            asrielSprite.Play("kneeling");
            
            yield return 1f;
            
            // Asriel gives them life back with beating heart
            yield return Textbox.Say("CH20_ASRIEL_GIVES_LIFE");
            
            // Healing light appears
            restorationLight.Position = asrielPos + new Vector2(0f, -32f);
            
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                restorationLight.Alpha = Ease.CubeOut(t / 2f);
                yield return null;
            }
            
            Audio.Play("event:/cutscene/heart_revival");
            
            // Pulse effect
            for (int i = 0; i < 3; i++)
            {
                level.Shake(0.3f);
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 10, madelinePos, Vector2.One * 16f);
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 10, badelinePos, Vector2.One * 16f);
                
                yield return 0.5f;
            }
            
            // Hearts begin beating
            Audio.Play("event:/cutscene/heartbeat");
            
            yield return 1f;
            
            // Madeline and Badeline begin to stir
            yield return Textbox.Say("CH20_MADELINE_AWAKENING");
            
            madelineSprite.Play("idle");
            badelineSprite.Play("idle");
            
            // They slowly sit up
            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                float progress = Ease.CubeOut(t / 1.5f);
                madelineSprite.Position = Vector2.Lerp(madelinePos, madelinePos + new Vector2(0f, -24f), progress);
                badelineSprite.Position = Vector2.Lerp(badelinePos, badelinePos + new Vector2(0f, -24f), progress);
                yield return null;
            }
            
            yield return 0.5f;
            
            // They look at each other, then at Asriel
            yield return Textbox.Say("CH20_MADELINE_BADELINE_GRATEFUL");
            
            // Asriel stands
            asrielSprite.Play("idle");
            
            yield return Textbox.Say("CH20_ASRIEL_RESPONSE");
            
            yield return 1f;
            
            // Begin waking up sequence
            Audio.Play("event:/cutscene/waking_up");
            
            // Fade to white
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                float fade = Ease.CubeIn(t / 2f);
                setOverlay(Color.White, fade);
                yield return null;
            }

            setOverlay(Color.White, 1f);
            
            yield return 1f;
            
            // Set flags
            level.Session.SetFlag("final_boss_defeated");
            level.Session.SetFlag("hearts_restored");
            level.Session.SetFlag("reality_restored");
            
            // End cutscene
            EndCutscene(level);
        }
        
        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                float progress = t / duration;
                float alpha = MathHelper.Lerp(from, to, progress);

                setOverlay(Color.Black, alpha);
                
                yield return null;
            }

            setOverlay(Color.Black, to);
        }
        
        public override void OnEnd(Level level)
        {
            // Clean up
            if (level != null)
            {
                level.Session.SetFlag("final_boss_defeated");
                level.Session.SetFlag("hearts_restored");
                level.TimerStopped = false;
                level.TimerHidden = false;
                level.SaveQuitDisabled = false;
                level.PauseLock = false;
            }

            setOverlay(Color.Transparent, 0f);
            
            // Progress to next cutscene
            level?.Session.SetFlag("ready_for_rainbow_tree");
        }
        
        public override void Render()
        {
            base.Render();

            Level level = Scene as Level;
            if (level == null)
                return;
            
            // Draw restoration effect
            if (healingProgress > 0f)
            {
                float alpha = (float)Math.Sin(healingProgress * MathHelper.Pi) * 0.3f;
                Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, Color.LightGoldenrodYellow * alpha);
            }

            if (overlayAlpha > 0f)
            {
                Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, overlayColor * overlayAlpha);
            }
        }
    }
}




