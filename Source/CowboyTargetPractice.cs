using System;
using System.Collections;
using System.Collections.Generic;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Cowboy Target Practice Minigame
    /// Player must shoot targets with accuracy and speed
    /// Used in CH11 with Starlo's gun practice
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/CowboyTargetPractice")]
    [Tracked]
    [HotReloadable]
    public class CowboyTargetPractice : Entity
    {
        // Configuration
        private readonly int requiredTargets;
        private readonly float timeLimit;
        private readonly bool movingTargets;
        private readonly string practiceType; // "A", "B", "C", "passed", "truely_passed"
        
        // Game state
        private bool gameActive = false;
        private bool gameComplete = false;
        private int targetsHit = 0;
        private int totalTargets = 0;
        private float gameTimer = 0f;
        private int ammoCount = 6;
        private const int MAX_AMMO = 6;
        
        // Scoring
        private float accuracy = 0f;
        private int shotsFired = 0;
        private string resultRank = "";
        
        // Targets
        private List<ShootingTarget> targets = new List<ShootingTarget>();
        
        // Player reference
        private global::Celeste.Player player;
        
        // UI Elements
        private float uiAlpha = 0f;
        private Sprite gunsightSprite;
        private Vector2 aimPosition;
        
        // Audio
        private SoundSource gunSound;
        private SoundSource targetHitSound;
        
        public CowboyTargetPractice(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            requiredTargets = data.Int("requiredTargets", 10);
            timeLimit = data.Float("timeLimit", 60f);
            movingTargets = data.Bool("movingTargets", false);
            practiceType = data.Attr("practiceType", "A");
            
            Tag = Tags.TransitionUpdate;
            Depth = -10000;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            player = scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Setup UI
            setupUI();
            
            // Spawn targets
            spawnTargets();
            
            // Start game
            Add(new Coroutine(startGame()));
        }
        
        private void setupUI()
        {
            // Create gunsight sprite
            gunsightSprite = new Sprite(GFX.Game, "objects/");
            gunsightSprite.Add("aim", "gunsight", 0.1f);
            gunsightSprite.Play("aim");
            gunsightSprite.CenterOrigin();
            Add(gunsightSprite);
            
            // Audio sources
            Add(gunSound = new SoundSource());
            Add(targetHitSound = new SoundSource());
        }
        
        private void spawnTargets()
        {
            var level = Scene as Level;
            if (level == null) return;
            
            // Spawn targets based on configuration
            int targetCount = requiredTargets + 5; // Spawn extra targets
            totalTargets = targetCount;
            
            for (int i = 0; i < targetCount; i++)
            {
                Vector2 targetPos = new Vector2(
                    level.Bounds.Left + 100 + (i % 5) * 150,
                    level.Bounds.Top + 100 + (i / 5) * 120
                );
                
                ShootingTarget target = new ShootingTarget(targetPos, movingTargets);
                targets.Add(target);
                Scene.Add(target);
            }
        }
        
        private IEnumerator startGame()
        {
            var level = Scene as Level;
            
            // Fade in UI
            while (uiAlpha < 1f)
            {
                uiAlpha += Engine.DeltaTime * 2f;
                yield return null;
            }
            
            // Countdown
            Audio.Play("event:/ui/game/minigame_countdown");
            yield return 1f;
            
            // Start game
            gameActive = true;
            Audio.Play("event:/ui/game/minigame_start");
            
            // Run game timer
            while (gameActive && gameTimer < timeLimit)
            {
                gameTimer += Engine.DeltaTime;
                
                // Check if enough targets hit
                if (targetsHit >= requiredTargets)
                {
                    gameActive = false;
                    gameComplete = true;
                }
                
                yield return null;
            }
            
            // Game complete
            if (!gameComplete)
            {
                gameActive = false;
            }
            
            yield return evaluatePerformance();
        }
        
        private IEnumerator evaluatePerformance()
        {
            yield return 1f;
            
            // Calculate accuracy
            accuracy = shotsFired > 0 ? (float)targetsHit / shotsFired * 100f : 0f;
            
            // Determine rank based on practice type and accuracy
            if (accuracy >= 90f && targetsHit >= requiredTargets)
            {
                resultRank = "truely_passed";
                setSessionFlag("CH11_COWBOY_GUN_PRACTICES_TRUELY_PASSED");
                Audio.Play("event:/ui/game/perfect_complete");
            }
            else if (accuracy >= 70f && targetsHit >= requiredTargets)
            {
                resultRank = "passed";
                setSessionFlag("CH11_COWBOY_GUN_PRACTICES_PASSED");
                Audio.Play("event:/ui/game/complete");
            }
            else if (accuracy >= 50f)
            {
                resultRank = "B";
                setSessionFlag($"CH11_COWBOY_GUN_PRACTICES_TRY_AGAIN_B");
            }
            else if (accuracy >= 30f)
            {
                resultRank = "C";
                setSessionFlag($"CH11_COWBOY_GUN_PRACTICES_TRY_AGAIN_C");
            }
            else
            {
                resultRank = "A";
                setSessionFlag($"CH11_COWBOY_GUN_PRACTICES_TRY_AGAIN_A");
            }
            
            yield return 2f;
            
            // Show results dialog
            showResultDialog();
            
            yield return 3f;
            
            // Fade out
            while (uiAlpha > 0f)
            {
                uiAlpha -= Engine.DeltaTime * 2f;
                yield return null;
            }
            
            RemoveSelf();
        }
        
        private void setSessionFlag(string flag)
        {
            var level = Scene as Level;
            if (level != null)
            {
                level.Session.SetFlag(flag, true);
            }
        }
        
        private void showResultDialog()
        {
            var level = Scene as Level;
            if (level == null) return;
            
            // Trigger dialog based on result
            string dialogKey = $"CH11_COWBOY_GUN_PRACTICES_{resultRank.ToUpper()}";
            
            // This would normally trigger a cutscene/dialog
            // For now, just set the flag
            level.Session.SetFlag($"target_practice_{resultRank}", true);
        }
        
        public override void Update()
        {
            base.Update();
            
            if (!gameActive) return;
            
            var level = Scene as Level;
            if (level == null || player == null) return;
            
            // Update aim position (follow player aim direction)
            aimPosition = player.Center + Input.Aim.Value * 100f;
            gunsightSprite.Position = aimPosition - Position;
            
            // Handle shooting
            if (Input.Dash.Pressed && ammoCount > 0)
            {
                shoot();
            }
            
            // Handle reload
            if (Input.Jump.Pressed && ammoCount < MAX_AMMO)
            {
                reload();
            }
        }
        
        private void shoot()
        {
            shotsFired++;
            ammoCount--;
            
            // Play gun sound
            Audio.Play("event:/game/general/revolver_shoot", Position);
            
            // Check if any targets were hit
            foreach (var target in targets)
            {
                if (!target.IsHit && target.Collider != null && target.Collider.Collide(new Hitbox(10, 10, aimPosition.X - 5, aimPosition.Y - 5)))
                {
                    target.Hit();
                    targetsHit++;
                    Audio.Play("event:/game/general/target_hit", target.Position);
                    break;
                }
            }
            
            // Automatically reload if empty
            if (ammoCount <= 0)
            {
                Add(new Coroutine(autoReload()));
            }
        }
        
        private void reload()
        {
            Audio.Play("event:/game/general/revolver_reload", Position);
            ammoCount = MAX_AMMO;
        }
        
        private IEnumerator autoReload()
        {
            yield return 0.5f;
            reload();
        }
        
        public override void Render()
        {
            base.Render();
            
            if (uiAlpha <= 0f) return;
            
            var level = Scene as Level;
            if (level == null) return;
            
            // Draw UI
            Vector2 topLeft = level.Camera.Position + new Vector2(32, 32);
            
            // Draw timer
            string timerText = $"Time: {(timeLimit - gameTimer):F1}s";
            ActiveFont.Draw(timerText, topLeft, Vector2.Zero, Vector2.One, Color.White * uiAlpha);
            
            // Draw targets hit
            string targetsText = $"Targets: {targetsHit}/{requiredTargets}";
            ActiveFont.Draw(targetsText, topLeft + new Vector2(0, 32), Vector2.Zero, Vector2.One, Color.White * uiAlpha);
            
            // Draw ammo count
            string ammoText = $"Ammo: {ammoCount}/{MAX_AMMO}";
            ActiveFont.Draw(ammoText, topLeft + new Vector2(0, 64), Vector2.Zero, Vector2.One, Color.White * uiAlpha);
            
            // Draw accuracy (if game complete)
            if (gameComplete || !gameActive)
            {
                string accuracyText = $"Accuracy: {accuracy:F1}%";
                ActiveFont.Draw(accuracyText, topLeft + new Vector2(0, 96), Vector2.Zero, Vector2.One, Color.Yellow * uiAlpha);
            }
        }
        
        // Shooting Target Entity
        private class ShootingTarget : Entity
        {
            private Sprite sprite;
            private bool isHit = false;
            private bool moving;
            private Vector2 startPos;
            private float moveTimer = 0f;
            
            public bool IsHit => isHit;
            
            public ShootingTarget(Vector2 position, bool moving) : base(position)
            {
                this.moving = moving;
                this.startPos = position;
                
                Depth = -9000;
                Collider = new Hitbox(32, 32, -16, -16);
                
                // Setup sprite
                sprite = new Sprite(GFX.Game, "objects/");
                sprite.Add("idle", "target_idle", 0.1f);
                sprite.Add("hit", "target_hit", 0.1f, "destroyed");
                sprite.Add("destroyed", "target_destroyed", 0.1f);
                sprite.Play("idle");
                sprite.CenterOrigin();
                Add(sprite);
            }
            
            public void Hit()
            {
                if (isHit) return;
                
                isHit = true;
                sprite.Play("hit");
                
                // Add particles
                var level = Scene as Level;
                if (level != null)
                {
                    level.ParticlesFG.Emit(Refill.P_Shatter, 10, Position, Vector2.One * 8);
                }
            }
            
            public override void Update()
            {
                base.Update();
                
                if (isHit) return;
                
                // Moving target logic
                if (moving)
                {
                    moveTimer += Engine.DeltaTime;
                    Position = startPos + new Vector2(
                        (float)Math.Sin(moveTimer * 2f) * 50f,
                        (float)Math.Cos(moveTimer * 1.5f) * 30f
                    );
                }
            }
        }
    }
}