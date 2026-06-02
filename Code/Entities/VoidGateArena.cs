using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Void Gate Arena - Manages the arena challenge where player must defeat enemies
    /// to open the void gate. Spawns enemies and tracks progress.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/VoidGateArena")]
    [Tracked]
    [HotReloadable]
    public class VoidGateArena : Entity
    {
        #region Fields
        
        // Arena configuration
        private int requiredEnemyKills;
        private int currentKills;
        private bool arenaActive;
        private bool arenaCompleted;
        
        // Enemy spawning
        private List<Vector2> enemySpawnPoints;
        private List<Entity> activeEnemies;
        private bool hasSpawnedBoss;
        private int waveCount;
        private int currentWave;
        
        // Gate reference
        private VoidGate gate;
        
        // Level reference
        private Level level;
        
        // UI
        private float uiAlpha;
        private string arenaText;
        private Color textColor;
        
        // Session flag
        private string completionFlag;
        
        // Configuration
        private bool spawnBoss;
        private int enemiesPerWave;
        private int totalWaves;
        
        #endregion
        
        #region Constructor
        
        public VoidGateArena(EntityData data, Vector2 offset) 
            : base(data.Position + offset)
        {
            requiredEnemyKills = data.Int("requiredKills", 10);
            spawnBoss = data.Bool("spawnBoss", true);
            enemiesPerWave = data.Int("enemiesPerWave", 3);
            totalWaves = data.Int("totalWaves", 3);
            completionFlag = data.Attr("completionFlag", "void_gate_arena_complete");
            
            currentKills = 0;
            arenaActive = false;
            arenaCompleted = false;
            hasSpawnedBoss = false;
            currentWave = 0;
            waveCount = totalWaves;
            
            activeEnemies = new List<Entity>();
            enemySpawnPoints = new List<Vector2>();
            
            // Get spawn points from nodes
            if (data.Nodes != null && data.Nodes.Length > 0)
            {
                foreach (var node in data.Nodes)
                {
                    enemySpawnPoints.Add(node + data.Position);
                }
            }
            else
            {
                // Default spawn points around arena center
                Vector2 center = Position;
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * (float)Math.PI * 2f;
                    enemySpawnPoints.Add(center + Calc.AngleToVector(angle, 150f));
                }
            }
            
            Depth = -10000;
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Check if already completed
            if (level.Session.GetFlag(completionFlag))
            {
                arenaCompleted = true;
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update UI alpha
            if (arenaActive && !arenaCompleted)
            {
                uiAlpha = Calc.Approach(uiAlpha, 1f, Engine.DeltaTime * 2f);
            }
            else
            {
                uiAlpha = Calc.Approach(uiAlpha, 0f, Engine.DeltaTime * 2f);
            }
            
            // Update arena state
            if (arenaActive && !arenaCompleted)
            {
                UpdateArena();
            }
        }
        
        #endregion
        
        #region Arena Management
        
        public void RegisterGate(VoidGate voidGate)
        {
            gate = voidGate;
        }
        
        public void OnGateClosed()
        {
            if (arenaCompleted)
                return;
            
            arenaActive = true;
            currentKills = 0;
            currentWave = 0;
            
            // Start first wave
            SpawnWave();
            
            // UI notification
            arenaText = $"Defeat {requiredEnemyKills} enemies!";
            textColor = Color.Red;
            
            Audio.Play("event:/ui/main/button_lowkey", Position);
        }
        
        public void OnGateOpened()
        {
            arenaActive = false;
            arenaCompleted = true;
            
            // Set completion flag
            if (level != null && !string.IsNullOrEmpty(completionFlag))
            {
                level.Session.SetFlag(completionFlag, true);
            }
            
            // Clear remaining enemies - avoid ToList() allocation
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy != null && enemy.Scene != null)
                {
                    enemy.RemoveSelf();
                }
            }
            activeEnemies.Clear();
            
            // Success notification
            arenaText = "Gate opened!";
            textColor = Color.Green;
            
            Audio.Play("event:/ui/main/button_select", Position);
        }
        
        private void UpdateArena()
        {
            // Clean up dead enemies from list - avoid lambda allocation
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null || activeEnemies[i].Scene == null)
                {
                    activeEnemies.RemoveAt(i);
                }
            }
            
            // Check if wave is complete
            if (activeEnemies.Count == 0 && currentWave < waveCount)
            {
                SpawnWave();
            }
            
            // Check if should spawn boss
            if (spawnBoss && !hasSpawnedBoss && currentKills >= requiredEnemyKills * 0.7f)
            {
                SpawnBoss();
                hasSpawnedBoss = true;
            }
            
            // Update UI text
            int remaining = Math.Max(0, requiredEnemyKills - currentKills);
            if (remaining > 0)
            {
                arenaText = $"Enemies remaining: {remaining}";
                textColor = Color.Lerp(Color.Red, Color.Yellow, 1f - (remaining / (float)requiredEnemyKills));
            }
        }
        
        #endregion
        
        #region Enemy Spawning
        
        private void SpawnWave()
        {
            if (currentWave >= waveCount)
                return;
            
            currentWave++;
            
            // Spawn enemies at random spawn points
            for (int i = 0; i < enemiesPerWave; i++)
            {
                Vector2 spawnPos = enemySpawnPoints[Calc.Random.Next(enemySpawnPoints.Count)];
                SpawnEnemy(spawnPos);
            }
            
            // Wave announcement
            if (level != null)
            {
                level.Shake(0.3f);
                Audio.Play("event:/char/badeline/appear", Position);
            }
        }
        
        private void SpawnEnemy(Vector2 position)
        {
            // Create enemy entity data
            var enemyData = new EntityData
            {
                Position = position,
                Values = new Dictionary<string, object>
                {
                    { "health", Calc.Random.Range(8, 15) },
                    { "minDamage", Calc.Random.Range(2, 4) },
                    { "maxDamage", Calc.Random.Range(5, 7) },
                    { "patrolRadius", 64f }
                }
            };
            
            var enemy = new DarkMatterEnemy(enemyData, Vector2.Zero);
            Scene.Add(enemy);
            activeEnemies.Add(enemy);
            
            // Spawn effect
            SpawnEffect(position);
        }
        
        private void SpawnBoss()
        {
            // Find center spawn point or use arena center
            Vector2 bossSpawn = Position;
            if (enemySpawnPoints.Count > 0)
            {
                // Use average of all spawn points
                Vector2 sum = Vector2.Zero;
                foreach (var point in enemySpawnPoints)
                {
                    sum += point;
                }
                bossSpawn = sum / enemySpawnPoints.Count;
            }
            
            // Create boss entity data
            var bossData = new EntityData
            {
                Position = bossSpawn,
                Values = new Dictionary<string, object>
                {
                    { "health", 80 }
                }
            };
            
            var boss = new DarkMatterMidBoss(bossData, Vector2.Zero);
            Scene.Add(boss);
            activeEnemies.Add(boss);
            
            // Boss spawn effect
            if (level != null)
            {
                level.Shake(1f);
                level.Flash(Color.Purple);
                Audio.Play("event:/char/badeline/boss_revive", bossSpawn);
            }
            
            SpawnEffect(bossSpawn, isLarge: true);
            
            // Boss announcement
            arenaText = "Dark Matter Lord has appeared!";
            textColor = Color.Purple;
        }
        
        private void SpawnEffect(Vector2 position, bool isLarge = false)
        {
            if (level == null)
                return;
            
            int particleCount = isLarge ? 40 : 20;
            float radius = isLarge ? 40f : 20f;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = (i / (float)particleCount) * (float)Math.PI * 2f;
                Vector2 offset = Calc.AngleToVector(angle, radius);
                
                var particle = new ParticleType
                {
                    Color = Color.Purple,
                    Color2 = Color.DarkViolet,
                    ColorMode = ParticleType.ColorModes.Blink,
                    Size = isLarge ? 2f : 1f,
                    LifeMin = 0.5f,
                    LifeMax = 1f,
                    SpeedMin = 20f,
                    SpeedMax = 40f,
                    DirectionRange = (float)Math.PI * 2f
                };
                
                level.ParticlesFG.Emit(particle, position + offset);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        public void OnEnemyDefeated(Entity enemy)
        {
            if (!arenaActive || arenaCompleted)
                return;
            
            currentKills++;
            
            // Remove from active list
            activeEnemies.Remove(enemy);
            
            // Audio feedback
            Audio.Play("event:/ui/game/increment", Position);
            
            // Check if arena is complete
            if (currentKills >= requiredEnemyKills)
            {
                CompleteArena();
            }
        }
        
        public void OnBossDefeated(DarkMatterMidBoss boss)
        {
            if (!arenaActive || arenaCompleted)
                return;
            
            // Boss counts as multiple kills
            currentKills += 5;
            
            // Remove from active list
            activeEnemies.Remove(boss);
            
            // Audio feedback
            Audio.Play("event:/ui/game/summit_checkpoint_confetti", Position);
            
            // Check if arena is complete
            if (currentKills >= requiredEnemyKills)
            {
                CompleteArena();
            }
        }
        
        private void CompleteArena()
        {
            if (gate != null && gate.IsClosed)
            {
                gate.StartOpening();
            }
            
            // Success effects
            if (level != null)
            {
                level.Shake(0.5f);
                level.Flash(Color.LightGreen);
            }
        }
        
        #endregion
        
        #region UI Rendering
        
        public override void Render()
        {
            base.Render();
            
            if (uiAlpha <= 0f)
                return;
            
            // Render progress UI
            Vector2 uiPosition = new Vector2(960f, 100f); // Top center of screen
            
            // Background
            float boxWidth = 400f;
            float boxHeight = 60f;
            Draw.Rect(
                uiPosition.X - boxWidth / 2,
                uiPosition.Y - boxHeight / 2,
                boxWidth,
                boxHeight,
                Color.Black * 0.7f * uiAlpha
            );
            
            // Border
            Draw.HollowRect(
                uiPosition.X - boxWidth / 2,
                uiPosition.Y - boxHeight / 2,
                boxWidth,
                boxHeight,
                textColor * uiAlpha
            );
            
            // Text
            Vector2 textSize = ActiveFont.Measure(arenaText);
            ActiveFont.DrawOutline(
                arenaText,
                uiPosition - textSize / 2,
                new Vector2(0.5f, 0.5f),
                Vector2.One,
                Color.White * uiAlpha,
                2f,
                Color.Black * uiAlpha
            );
            
            // Progress bar
            if (!arenaCompleted && requiredEnemyKills > 0)
            {
                Vector2 barPos = uiPosition + new Vector2(-boxWidth / 2 + 20f, 20f);
                float barWidth = boxWidth - 40f;
                float barHeight = 10f;
                float progress = Math.Min(1f, currentKills / (float)requiredEnemyKills);
                
                // Background
                Draw.Rect(barPos, barWidth, barHeight, Color.DarkGray * uiAlpha);
                
                // Progress
                Draw.Rect(barPos, barWidth * progress, barHeight, textColor * uiAlpha);
                
                // Border
                Draw.HollowRect(barPos, barWidth, barHeight, Color.White * uiAlpha);
                
                // Kill count text
                string countText = $"{currentKills}/{requiredEnemyKills}";
                Vector2 countSize = ActiveFont.Measure(countText) * 0.5f;
                ActiveFont.DrawOutline(
                    countText,
                    barPos + new Vector2(barWidth / 2 - countSize.X / 2, -15f),
                    Vector2.Zero,
                    Vector2.One * 0.5f,
                    Color.White * uiAlpha,
                    1f,
                    Color.Black * uiAlpha
                );
            }
        }
        
        #endregion
        
        #region Debug
        
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            
            // Draw spawn points
            foreach (var spawnPoint in enemySpawnPoints)
            {
                Draw.Circle(spawnPoint, 8f, Color.Yellow, 8);
            }
            
            // Draw arena center
            Draw.Circle(Position, 16f, Color.Red, 8);
            
            // Draw status text
            string status = $"Active: {arenaActive}, Kills: {currentKills}/{requiredEnemyKills}, Wave: {currentWave}/{waveCount}";
            Draw.SpriteBatch.DrawString(
                Draw.DefaultFont,
                status,
                new Vector2(Position.X - 100, Position.Y - 40),
                Color.White
            );
        }
        
        #endregion
    }
}
