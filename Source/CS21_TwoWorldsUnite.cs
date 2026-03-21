namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Human and monster working together cutscene
    /// Ends with Planet Popstar arriving to Earth
    /// </summary>
    [Tracked]
    public class CS21_TwoWorldsUnite : CutsceneEntity
    {
        private global::Celeste.Player player;
        
        private List<Sprite> humanSprites = new List<Sprite>();
        private List<Sprite> monsterSprites = new List<Sprite>();
        private Sprite planetPopstarSprite;
        
        private Image earthBackground;
        private Image skyBackground;
        
        private float sceneProgress = 0f;
        private float planetApproachProgress = 0f;
        
        private VertexLight harmonyLight;
        private bool planetsUniting = false;
        
        public CS21_TwoWorldsUnite(global::Celeste.Player player) : base(false, true)
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
            
            setupScene();
        }
        
        private void setupScene()
        {
            // Background
            Add(skyBackground = new Image(GFX.Game["bgs/sky"]));
            skyBackground.Position = Vector2.Zero;
            
            Add(earthBackground = new Image(GFX.Game["scenery/earth_surface"]));
            earthBackground.Position = new Vector2(0f, 600f);
            
            // Create human sprites
            for (int i = 0; i < 5; i++)
            {
                var human = new Sprite(GFX.Game, "characters/humans/");
                human.AddLoop("work", "working", 0.1f);
                human.AddLoop("celebrate", "celebrating", 0.1f);
                human.Play("work");
                Add(human);
                humanSprites.Add(human);
            }
            
            // Create monster sprites
            for (int i = 0; i < 5; i++)
            {
                var monster = new Sprite(GFX.Game, "characters/monsters/");
                monster.AddLoop("work", "working", 0.1f);
                monster.AddLoop("celebrate", "celebrating", 0.1f);
                monster.Play("work");
                Add(monster);
                monsterSprites.Add(monster);
            }
            
            // Planet Popstar
            Add(planetPopstarSprite = new Sprite(GFX.Game, "objects/"));
            planetPopstarSprite.AddLoop("approach", "planet_idle", 0.1f);
            planetPopstarSprite.Play("approach");
            planetPopstarSprite.Position = new Vector2(960f, -500f); // Start above screen
            planetPopstarSprite.CenterOrigin();
            planetPopstarSprite.Visible = false;
            
            // Harmony light
            Add(harmonyLight = new VertexLight(Color.LightBlue * 0.6f, 1f, 384, 512));
            harmonyLight.Position = new Vector2(960f, 540f);
            harmonyLight.Alpha = 0f;
        }
        
        public override void OnBegin(Level level)
        {
            Audio.SetAmbience("event:/env/amb/peaceful_world");
            Audio.SetMusic("event:/music/two_worlds_unite");
            
            Add(new Coroutine(cutsceneSequence(level)));
        }
        
        private IEnumerator cutsceneSequence(Level level)
        {
            // Fade in
            yield return 2f;
            
            // Position humans and monsters working together
            positionCharacters();
            
            yield return 1f;
            
            // Dialog: Humans and monsters express hope
            yield return Textbox.Say("CH21_HUMAN_HOPE");
            
            yield return 1f;
            
            yield return Textbox.Say("CH21_MONSTER_UNITY");
            
            yield return 1f;
            
            // Show them working together
            Audio.Play("event:/env/local/construction_sounds");
            
            // Harmony light grows
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                harmonyLight.Alpha = Ease.CubeOut(t / 2f) * 0.6f;
                yield return null;
            }
            
            yield return 2f;
            
            // More dialog
            yield return Textbox.Say("CH21_COOPERATION_SUCCESS");
            
            yield return 1f;
            
            yield return Textbox.Say("CH21_NEW_FUTURE");
            
            yield return 2f;
            
            // Suddenly, something in the sky
            Audio.Play("event:/cutscene/sky_phenomenon");
            
            yield return Textbox.Say("CH21_LOOK_TO_SKY");
            
            // All characters look up
            foreach (var human in humanSprites)
            {
                // Look up animation
            }
            foreach (var monster in monsterSprites)
            {
                // Look up animation
            }
            
            yield return 2f;
            
            // Planet Popstar approaches
            planetPopstarSprite.Visible = true;
            planetsUniting = true;
            
            Audio.Play("event:/cutscene/planet_approach");
            Audio.SetMusic("event:/music/planet_popstar_arrival");
            
            // Slow approach with dark transition
            for (float t = 0f; t < 8f; t += Engine.DeltaTime)
            {
                planetApproachProgress = Ease.SineInOut(t / 8f);
                
                // Move planet into view
                planetPopstarSprite.Position = Vector2.Lerp(
                    new Vector2(960f, -500f),
                    new Vector2(960f, 300f),
                    planetApproachProgress
                );
                
                // Scale up as it gets closer
                float scale = 0.1f + planetApproachProgress * 1.5f;
                planetPopstarSprite.Scale = Vector2.One * scale;
                
                // Dark transition starts
                if (t > 4f)
                {
                    float darkProgress = (t - 4f) / 4f;
                    // Darken screen gradually
                }
                
                yield return null;
            }
            
            yield return 2f;
            
            // Characters react with wonder
            yield return Textbox.Say("CH21_PLANET_WONDER");
            
            foreach (var human in humanSprites)
            {
                human.Play("celebrate");
            }
            foreach (var monster in monsterSprites)
            {
                monster.Play("celebrate");
            }
            
            yield return 2f;
            
            // Final dialog
            yield return Textbox.Say("CH21_TWO_WORLDS_ONE");
            
            yield return 2f;
            
            // Smooth dark transition as worlds merge
            for (float t = 0f; t < 4f; t += Engine.DeltaTime)
            {
                float darkAlpha = Ease.CubeIn(t / 4f);
                yield return null;
            }
            
            // Flash of light
            level.Flash(Color.White, true);
            Audio.Play("event:/cutscene/worlds_merge");
            
            yield return 1f;
            
            // Fade to black
            yield return 2f;
            
            // Set completion flags
            level.Session.SetFlag("two_worlds_unite_complete");
            level.Session.SetFlag("ready_for_dodge_credits");
            
            EndCutscene(level);
        }
        
        private void positionCharacters()
        {
            // Arrange humans and monsters in pairs, working together
            float baseY = 700f;
            float spacing = 180f;
            float startX = 300f;
            
            for (int i = 0; i < humanSprites.Count; i++)
            {
                humanSprites[i].Position = new Vector2(startX + i * spacing, baseY);
                
                if (i < monsterSprites.Count)
                {
                    monsterSprites[i].Position = new Vector2(startX + i * spacing + 50f, baseY);
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            sceneProgress += Engine.DeltaTime * 0.1f;
            
            // Update harmony light pulsing
            if (harmonyLight.Alpha > 0f)
            {
                harmonyLight.StartRadius = 384f + (float)Math.Sin(sceneProgress * 2f) * 64f;
            }
            
            // Rotate planet slightly
            if (planetPopstarSprite.Visible)
            {
                planetPopstarSprite.Rotation += Engine.DeltaTime * 0.1f;
            }
        }
        
        public override void OnEnd(Level level)
        {
            level.Session.SetFlag("two_worlds_unite_complete");
            level.Session.SetFlag("ready_for_dodge_credits");
        }
        
        public override void Render()
        {
            base.Render();
            
            // Dark transition overlay
            if (planetsUniting && planetApproachProgress > 0.5f)
            {
                float darkAlpha = (planetApproachProgress - 0.5f) * 2f * 0.5f;
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * darkAlpha);
            }
        }
    }
}




