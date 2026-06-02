namespace Celeste.Entities
{
    /// <summary>
    /// Dodge credit minigame - Final outro vignette
    /// Player must dodge credits with infinite HP
    /// Perfect dodge = special ending
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DodgeCreditMinigame")]
    [Tracked]
    [HotReloadable]
    public class DodgeCreditMinigame : Entity
    {
        private global::Celeste.Player player;
        private List<CreditProjectile> creditProjectiles = new List<CreditProjectile>();
        
        private bool gameActive = false;
        private bool gameComplete = false;
        private bool perfectDodge = true; // true if never hit
        private int hitCount = 0;
        
        private float gameTimer = 0f;
        private const float GAME_DURATION = 60f; // 60 seconds of dodging
        
        private float spawnTimer = 0f;
        private float spawnInterval = 1f;
        
        private Sprite madelineSprite;
        private VertexLight playerLight;
        
        // UI
        private float completionAlpha = 0f;
        private string completionMessage = "";
        
        private class CreditProjectile : Entity
        {
            public string CreditText;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public bool HasHit;
            
            public CreditProjectile(Vector2 position, Vector2 velocity, string text) : base(position)
            {
                CreditText = text;
                Velocity = velocity;
                Rotation = Calc.Random.NextFloat(MathHelper.TwoPi);
                RotationSpeed = Calc.Random.Range(-2f, 2f);
                HasHit = false;
                
                Collider = new Hitbox(80f, 30f, -40f, -15f);
                Depth = -10000;
            }
            
            public override void Update()
            {
                base.Update();
                
                Position += Velocity * Engine.DeltaTime;
                Rotation += RotationSpeed * Engine.DeltaTime;
                
                // Remove if off screen
                var level = Scene as Level;
                if (level != null)
                {
                    if (X < level.Camera.Left - 200f || X > level.Camera.Right + 200f ||
                        Y < level.Camera.Top - 200f || Y > level.Camera.Bottom + 200f)
                    {
                        RemoveSelf();
                    }
                }
            }
            
            public override void Render()
            {
                base.Render();
                
                // Draw credit text
                Vector2 renderPos = Position;
                
                // Rotate text
                Matrix transform = Matrix.CreateRotationZ(Rotation) * 
                                 Matrix.CreateTranslation(renderPos.X, renderPos.Y, 0f);
                
                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    transform
                );
                
                ActiveFont.DrawOutline(
                    CreditText,
                    Vector2.Zero,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.8f,
                    Color.Cyan,
                    2f,
                    Color.Black
                );
                
                Draw.SpriteBatch.End();
            }
        }
        
        private List<string> creditTexts = new List<string>
        {
            "DesoloZatnas Team",
            "Lead Developer: Magedeline",
            "Story & Design",
            "Programming",
            "Art & Graphics",
            "Music & Audio",
            "Level Design",
            "Boss Design",
            "Cutscene Direction",
            "Special Thanks",
            "Celeste Community",
            "Modding Community",
            "Playtesters",
            "Beta Testers",
            "Supporters",
            "Everest Team",
            "Celeste by Maddy Thorson",
            "Thank You For Playing!",
            "Your Journey is Complete",
            "Two Worlds United",
            "Madeline & Badeline",
            "Kirby",
            "Asriel",
            "Dream Friends",
            "All Allies",
            "Final DLC",
            "Post Epilogue",
            "Chapter 21 Complete",
            "The End... Or Is It?",
            "Congratulations!"
        };
        
        public DodgeCreditMinigame(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -1000000;
            
            setupPlayer();
        }
        
        private void setupPlayer()
        {
            // Create Madeline sprite
            Add(madelineSprite = new Sprite(GFX.Game, "characters/player/"));
            madelineSprite.AddLoop("idle", "idle", 0.1f);
            madelineSprite.AddLoop("run", "runFast", 0.08f);
            madelineSprite.AddLoop("dash", "dash", 0.08f);
            madelineSprite.Play("idle");
            madelineSprite.CenterOrigin();
            
            // Player light
            Add(playerLight = new VertexLight(Color.LightBlue * 0.8f, 1f, 64, 96));
            playerLight.Position = Vector2.Zero;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            player = scene.Tracker.GetEntity<global::Celeste.Player>();
            
            if (player != null)
            {
                // Position minigame at player location
                Position = player.Position;
            }
            
            Add(new Coroutine(startMinigame()));
        }
        
        private IEnumerator startMinigame()
        {
            var level = Scene as Level;
            
            // Introduction
            yield return 1f;
            
            Audio.Play("event:/ui/game/minigame_start");
            
            // Instructions
            yield return 2f;
            
            // Start game
            gameActive = true;
            Audio.SetMusic("event:/music/dodge_credits_theme");
            
            // Spawn credits for duration
            while (gameTimer < GAME_DURATION && gameActive)
            {
                yield return null;
            }
            
            // Game complete
            gameComplete = true;
            gameActive = false;
            
            yield return 1f;
            
            // Show results
            if (perfectDodge && hitCount == 0)
            {
                // Perfect! Show victory vignette
                completionMessage = "PERFECT! FINAL OUTRO VIGNETTE UNLOCKED!";
                Audio.Play("event:/ui/game/perfect_complete");
                
                yield return 3f;
                
                // Trigger victory vignette
                level?.Session.SetFlag("final_outro_vignette_unlocked");
                yield return showVictoryVignette();
            }
            else
            {
                // Completed but hit
                completionMessage = $"COMPLETE! Hits: {hitCount}";
                Audio.Play("event:/ui/game/minigame_complete");
                
                yield return 3f;
                
                // Skip to end
                level?.Session.SetFlag("dodge_credits_complete");
            }
            
            yield return 2f;
            
            // Thank you message
            yield return showThankYouMessage();
        }
        
        public override void Update()
        {
            base.Update();
            
            if (!gameActive || gameComplete) return;
            
            // Update game timer
            gameTimer += Engine.DeltaTime;
            
            // Update spawn timer
            spawnTimer += Engine.DeltaTime;
            
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                spawnCreditProjectile();
                
                // Increase difficulty over time
                spawnInterval = Math.Max(0.3f, 1f - (gameTimer / GAME_DURATION) * 0.7f);
            }
            
            // Check collisions with player (infinite HP, just count hits)
            if (player != null && !player.Dead)
            {
                foreach (var credit in creditProjectiles)
                {
                    if (!credit.HasHit && credit.CollideCheck(player))
                    {
                        credit.HasHit = true;
                        hitCount++;
                        perfectDodge = false;
                        
                        Audio.Play("event:/game/general/hit_deflect", Position);
                        
                        var level = Scene as Level;
                        level?.Displacement.AddBurst(player.Position, 0.3f, 16f, 32f, 0.3f);
                        
                        // Flash player
                        playerLight.Alpha = 1.5f;
                    }
                }
            }
            
            // Update player light
            playerLight.Alpha = Calc.Approach(playerLight.Alpha, 0.8f, Engine.DeltaTime * 2f);
            
            // Update player sprite
            if (player != null)
            {
                madelineSprite.Position = player.Position - Position;
                
                if (player.Speed.Length() > 50f)
                {
                    madelineSprite.Play("run");
                }
                else
                {
                    madelineSprite.Play("idle");
                }
            }
        }
        
        private void spawnCreditProjectile()
        {
            var level = Scene as Level;
            if (level == null) return;
            
            // Random spawn position on screen edge
            Vector2 spawnPos;
            Vector2 velocity;
            
            int edge = Calc.Random.Next(4);
            
            switch (edge)
            {
                case 0: // Top
                    spawnPos = new Vector2(
                        Calc.Random.Range(level.Camera.Left, level.Camera.Right),
                        level.Camera.Top - 50f
                    );
                    velocity = new Vector2(
                        Calc.Random.Range(-100f, 100f),
                        Calc.Random.Range(100f, 200f)
                    );
                    break;
                case 1: // Bottom
                    spawnPos = new Vector2(
                        Calc.Random.Range(level.Camera.Left, level.Camera.Right),
                        level.Camera.Bottom + 50f
                    );
                    velocity = new Vector2(
                        Calc.Random.Range(-100f, 100f),
                        Calc.Random.Range(-200f, -100f)
                    );
                    break;
                case 2: // Left
                    spawnPos = new Vector2(
                        level.Camera.Left - 50f,
                        Calc.Random.Range(level.Camera.Top, level.Camera.Bottom)
                    );
                    velocity = new Vector2(
                        Calc.Random.Range(100f, 200f),
                        Calc.Random.Range(-100f, 100f)
                    );
                    break;
                default: // Right
                    spawnPos = new Vector2(
                        level.Camera.Right + 50f,
                        Calc.Random.Range(level.Camera.Top, level.Camera.Bottom)
                    );
                    velocity = new Vector2(
                        Calc.Random.Range(-200f, -100f),
                        Calc.Random.Range(-100f, 100f)
                    );
                    break;
            }
            
            // Pick random credit text
            string creditText = creditTexts[Calc.Random.Next(creditTexts.Count)];
            
            var projectile = new CreditProjectile(spawnPos, velocity, creditText);
            Scene.Add(projectile);
            creditProjectiles.Add(projectile);
        }
        
        private IEnumerator showVictoryVignette()
        {
            var level = Scene as Level;
            
            // Epic victory sequence
            Audio.Play("event:/cutscene/final_outro_vignette");
            
            // Screen flash
            level?.Flash(Color.Gold, true);
            
            yield return 2f;
            
            // Show special ending
            completionAlpha = 1f;
            
            yield return 5f;
        }
        
        private IEnumerator showThankYouMessage()
        {
            // Final thank you
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                completionAlpha = Ease.CubeOut(t / 3f);
                yield return null;
            }
            
            yield return 5f;
            
            // Fade out
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                completionAlpha = 1f - Ease.CubeIn(t / 3f);
                yield return null;
            }
            
            // Complete
            var level = Scene as Level;
            level?.Session.SetFlag("game_complete");
            level?.Session.SetFlag("true_ending_seen");
        }
        
        public override void Render()
        {
            base.Render();
            
            if (gameActive)
            {
                // Draw timer
                string timerText = $"Time: {(int)(GAME_DURATION - gameTimer)}s";
                ActiveFont.DrawOutline(
                    timerText,
                    new Vector2(100f, 50f),
                    new Vector2(0f, 0f),
                    Vector2.One,
                    Color.White,
                    2f,
                    Color.Black
                );
                
                // Draw hit counter
                string hitText = $"Hits: {hitCount}";
                ActiveFont.DrawOutline(
                    hitText,
                    new Vector2(100f, 90f),
                    new Vector2(0f, 0f),
                    Vector2.One,
                    perfectDodge ? Color.Green : Color.Red,
                    2f,
                    Color.Black
                );
            }
            
            if (completionAlpha > 0f)
            {
                // Draw completion message
                Vector2 center = new Vector2(960f, 540f);
                
                ActiveFont.DrawOutline(
                    completionMessage,
                    center,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 1.5f,
                    Color.Gold * completionAlpha,
                    2f,
                    Color.Black * completionAlpha
                );
                
                // Thank you message
                if (gameComplete)
                {
                    ActiveFont.DrawOutline(
                        "Thank you for playing DesoloZatnas!",
                        center + new Vector2(0f, 100f),
                        new Vector2(0.5f, 0.5f),
                        Vector2.One,
                        Color.White * completionAlpha,
                        2f,
                        Color.Black * completionAlpha
                    );
                }
            }
        }
    }
}




