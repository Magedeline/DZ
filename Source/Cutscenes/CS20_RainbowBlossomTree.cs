namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 20 cinematic ending on rainbow blossom tree
    /// Includes skip option
    /// </summary>
    [Tracked]
    public class CS20_RainbowBlossomTree : CutsceneEntity
    {
        private global::Celeste.Player player;
        private bool canSkip = false;
        private bool hasSkipped = false;
        
        private Sprite madelineSprite;
        private Sprite badelineSprite;
        private Sprite kirbySprite;
        private Sprite asrielSprite;
        
        private Image rainbowTreeBackground;
        private Image mountainBackground;
        
        private VertexLight sunlight;
        private float cinematicProgress = 0f;
        
        // Skip UI
        private float skipPromptAlpha = 0f;
        private const string SKIP_TEXT = "Press [Confirm] to Skip";
        
        public CS20_RainbowBlossomTree(global::Celeste.Player player) : base(false, true)
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
                level.PauseLock = false; // Allow skipping
            }
            
            setupScene();
        }
        
        private void setupScene()
        {
            // Background images
            Add(mountainBackground = new Image(GFX.Game["scenery/mountain_vista"]));
            mountainBackground.Position = Vector2.Zero;
            
            Add(rainbowTreeBackground = new Image(GFX.Game["scenery/rainbow_blossom_tree"]));
            rainbowTreeBackground.Position = new Vector2(960f, 200f);
            rainbowTreeBackground.CenterOrigin();
            
            // Character sprites
            madelineSprite = GFX.SpriteBank.Create("madeline");
            madelineSprite.Play("idle");
            Add(madelineSprite);
            
            badelineSprite = GFX.SpriteBank.Create("DZ_badeline");
            badelineSprite.Play("idle");
            Add(badelineSprite);
            
            kirbySprite = new Sprite(GFX.Game, "characters/kirby/");
            kirbySprite.AddLoop("sit", "idle", 0.1f);
            kirbySprite.Play("sit");
            Add(kirbySprite);
            
            asrielSprite = new Sprite(GFX.Game, "characters/asriel/");
            asrielSprite.AddLoop("sit", "idle", 0.1f);
            asrielSprite.Play("sit");
            Add(asrielSprite);
            
            // Sunlight
            Add(sunlight = new VertexLight(Color.Gold * 0.8f, 1f, 512, 768));
            sunlight.Position = new Vector2(960f, 100f);
        }
        
        public override void OnBegin(Level level)
        {
            Audio.SetAmbience("event:/env/amb/mountain_wind");
            Audio.SetMusic("event:/music/ch20_rainbow_tree_theme");
            
            level.AutoSave();
            
            Add(new Coroutine(cinematicSequence(level)));
        }
        
        private IEnumerator cinematicSequence(Level level)
        {
            // Fade in
            yield return 1f;
            
            // Enable skip after 2 seconds
            canSkip = true;
            
            // Position characters at tree top
            Vector2 treeTop = new Vector2(level.Camera.X + level.Camera.Viewport.Width / 2f, level.Camera.Top + 150f);
            
            madelineSprite.Position = treeTop + new Vector2(-40f, 0f);
            badelineSprite.Position = treeTop + new Vector2(-10f, 0f);
            kirbySprite.Position = treeTop + new Vector2(20f, 0f);
            asrielSprite.Position = treeTop + new Vector2(50f, 0f);
            
            // Camera pan to tree
            yield return CameraTo(treeTop, 3f, Ease.CubeInOut);
            
            if (hasSkipped) yield break;
            
            yield return 1f;
            
            // Character dialog - reflecting on journey
            yield return Textbox.Say("DZ_CH20_TREE_MADELINE_REFLECTS");
            
            if (hasSkipped) yield break;
            
            yield return Textbox.Say("DZ_CH20_TREE_BADELINE_RESPONDS");
            
            if (hasSkipped) yield break;
            
            yield return Textbox.Say("DZ_CH20_TREE_KIRBY_SPEAKS");
            
            if (hasSkipped) yield break;
            
            yield return Textbox.Say("DZ_CH20_TREE_ASRIEL_WISDOM");
            
            if (hasSkipped) yield break;
            
            yield return 2f;
            
            // Peaceful moment
            Audio.Play("event:/env/local/mountain_wind_gust");
            
            // Petals falling
            for (int i = 0; i < 20; i++)
            {
                Vector2 petalPos = new Vector2(
                    Calc.Random.Range(level.Camera.Left, level.Camera.Right),
                    level.Camera.Top
                );
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashA, petalPos);
            }
            
            yield return 3f;
            
            if (hasSkipped) yield break;
            
            // Madeline stands
            yield return Textbox.Say("DZ_CH20_TREE_MADELINE_DETERMINATION");
            
            if (hasSkipped) yield break;
            
            yield return 1f;
            
            // Everyone stands together
            yield return Textbox.Say("DZ_CH20_TREE_GROUP_UNITY");
            
            if (hasSkipped) yield break;
            
            yield return 2f;
            
            // Camera zooms out to show full mountain and tree
            yield return CameraTo(new Vector2(level.Camera.X + level.Camera.Viewport.Width / 2f, level.Camera.Y + level.Camera.Viewport.Height / 2f - 100f), 5f, Ease.SineInOut);
            
            if (hasSkipped) yield break;
            
            // Rainbow light effect
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                cinematicProgress = t / 3f;
                sunlight.Color = getRainbowColor(cinematicProgress) * 0.8f;
                yield return null;
            }
            
            if (hasSkipped) yield break;
            
            yield return 2f;
            
            // Final message
            yield return Textbox.Say("DZ_CH20_TREE_FINAL_MESSAGE");
            
            // Fade out
            yield return Fade(0f, 1f, 3f);
            
            // Set completion flag
            level.Session.SetFlag("rainbow_tree_ending_complete");
            
            canSkip = false;
            EndCutscene(level);
        }
        
        private Color getRainbowColor(float progress)
        {
            // Cycle through rainbow colors
            float hue = progress * 6f % 1f;
            
            if (hue < 1f / 6f) return Color.Lerp(Color.Red, Color.Orange, hue * 6f);
            if (hue < 2f / 6f) return Color.Lerp(Color.Orange, Color.Yellow, (hue - 1f / 6f) * 6f);
            if (hue < 3f / 6f) return Color.Lerp(Color.Yellow, Color.Green, (hue - 2f / 6f) * 6f);
            if (hue < 4f / 6f) return Color.Lerp(Color.Green, Color.Blue, (hue - 3f / 6f) * 6f);
            if (hue < 5f / 6f) return Color.Lerp(Color.Blue, Color.Indigo, (hue - 4f / 6f) * 6f);
            return Color.Lerp(Color.Indigo, Color.Violet, (hue - 5f / 6f) * 6f);
        }
        
        private IEnumerator CameraTo(Vector2 target, float duration, Ease.Easer ease = null)
        {
            if (ease == null) ease = Ease.Linear;
            
            var level = Scene as Level;
            Vector2 start = new Vector2(level.Camera.X, level.Camera.Y);
            
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                if (hasSkipped) yield break;
                
                float progress = ease(t / duration);
                Vector2 current = Vector2.Lerp(start, target, progress);
                level.Camera.Position = current;
                
                yield return null;
            }
            
            level.Camera.Position = target;
        }
        
        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                if (hasSkipped) yield break;
                
                float progress = t / duration;
                float alpha = MathHelper.Lerp(from, to, progress);
                
                yield return null;
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // Handle skip input
            if (canSkip && !hasSkipped)
            {
                skipPromptAlpha = Calc.Approach(skipPromptAlpha, 1f, Engine.DeltaTime * 2f);
                
                if (Input.MenuConfirm.Pressed || Input.Dash.Pressed)
                {
                    hasSkipped = true;
                    Audio.Play("event:/ui/main/button_select");
                    
                    // Jump to end
                    var level = Scene as Level;
                    level.Session.SetFlag("rainbow_tree_ending_complete");
                    EndCutscene(level);
                }
            }
            else
            {
                skipPromptAlpha = Calc.Approach(skipPromptAlpha, 0f, Engine.DeltaTime * 2f);
            }
        }
        
        public override void OnEnd(Level level)
        {
            level.Session.SetFlag("rainbow_tree_ending_complete");
            level.Session.SetFlag("ready_for_epilogue");
        }
        
        public override void Render()
        {
            base.Render();
            
            // Render skip prompt
            if (skipPromptAlpha > 0f)
            {
                Vector2 promptPos = new Vector2(1920f / 2f, 1080f - 100f);
                ActiveFont.DrawOutline(
                    SKIP_TEXT,
                    promptPos,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One,
                    Color.White * skipPromptAlpha,
                    2f,
                    Color.Black * skipPromptAlpha
                );
            }
        }
    }
}





