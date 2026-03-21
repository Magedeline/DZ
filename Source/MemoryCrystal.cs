using System;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Collectible that reveals lore through a flashback vision
    /// Used throughout the mod to tell the story of the Seven Heroes,
    /// Cornelius Vane, and the history of Desolo Zantas
    /// </summary>
    public class MemoryCrystal : Entity
    {
        private string memoryId;
        private bool oneTime;
        private float flashbackDuration;
        
        private float floatTimer = 0f;
        private bool collected = false;
        
        private Color crystalColor;
        
        public MemoryCrystal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            memoryId = data.Attr("memoryId", "");
            oneTime = data.Bool("oneTime", true);
            flashbackDuration = data.Float("flashbackDuration", 3f);
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            
            crystalColor = new Color(50, 200, 220);
            
            Depth = -100;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected
            if (oneTime && !string.IsNullOrEmpty(memoryId))
            {
                var session = (scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"MemoryCrystal_{memoryId}_collected";
                    if (session.GetFlag(key))
                    {
                        RemoveSelf();
                    }
                }
            }
        }
        
        private void OnPlayer(Player player)
        {
            if (collected) return;
            
            collected = true;
            
            Audio.Play("event:/game/general/cassette_get", Position);
            
            // Mark as collected
            if (oneTime && !string.IsNullOrEmpty(memoryId))
            {
                var session = (Scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"MemoryCrystal_{memoryId}_collected";
                    session.SetFlag(key, true);
                }
            }
            
            // Trigger flashback/dialog
            TriggerMemory(player);
            
            // Particles
            SpawnParticles();
            
            if (oneTime)
            {
                RemoveSelf();
            }
            else
            {
                // Become dormant
                Visible = false;
                Collidable = false;
            }
        }
        
        private void TriggerMemory(Player player)
        {
            Level level = Scene as Level;
            if (level == null) return;
            
            // Create flashback effect
            level.Displacement.AddBurst(Position, 0.5f, 8f, 64f, 1f);
            
            // Show dialog if memoryId is set
            if (!string.IsNullOrEmpty(memoryId))
            {
                string dialogKey = $"MEMORY_CRYSTAL_{memoryId.ToUpper()}";
                
                // Try to show the dialog
                // This would trigger a textbox with the memory content
                Scene.Add(new CS_MemoryFlashback(memoryId, flashbackDuration));
            }
        }
        
        private void SpawnParticles()
        {
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                    Vector2 dir = Calc.AngleToVector(angle, Calc.Random.NextFloat(24f) + 8f);
                    level.Particles.Emit(ParticleTypes.SparkyDust, Position + dir, crystalColor);
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            floatTimer += Engine.DeltaTime;
        }
        
        public override void Render()
        {
            if (collected) return;
            
            float yOffset = (float)Math.Sin(floatTimer * 1.5f) * 4f;
            float rotation = floatTimer * 0.5f;
            Vector2 pos = Position + new Vector2(0, yOffset);
            
            // Draw glow
            float pulse = 0.5f + (float)Math.Sin(floatTimer * 2f) * 0.3f;
            Draw.Circle(pos, 16f * pulse, crystalColor * 0.3f, 16);
            
            // Draw crystal (diamond shape)
            float size = 10f;
            Vector2 top = pos + new Vector2(0, -size);
            Vector2 bottom = pos + new Vector2(0, size);
            Vector2 left = pos + new Vector2(-size * 0.7f, 0);
            Vector2 right = pos + new Vector2(size * 0.7f, 0);
            
            // Rotate points
            top = RotatePoint(top, pos, rotation);
            bottom = RotatePoint(bottom, pos, rotation);
            left = RotatePoint(left, pos, rotation);
            right = RotatePoint(right, pos, rotation);
            
            // Draw crystal faces
            Draw.Line(top, right, crystalColor, 2f);
            Draw.Line(right, bottom, crystalColor, 2f);
            Draw.Line(bottom, left, crystalColor, 2f);
            Draw.Line(left, top, crystalColor, 2f);
            
            // Draw inner glow
            Draw.Circle(pos, 4f, Color.White * 0.6f, 8);
        }
        
        private Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            Vector2 offset = point - center;
            return new Vector2(
                center.X + offset.X * cos - offset.Y * sin,
                center.Y + offset.X * sin + offset.Y * cos
            );
        }
    }
    
    /// <summary>
    /// Simple cutscene for memory flashbacks
    /// </summary>
    public class CS_MemoryFlashback : CutsceneEntity
    {
        private string memoryId;
        private float duration;
        
        public CS_MemoryFlashback(string memoryId, float duration)
        {
            this.memoryId = memoryId;
            this.duration = duration;
        }
        
        public override void OnBegin(Level level)
        {
            // Darken screen, show memory dialog
            Add(new Coroutine(FlashbackRoutine(level)));
        }
        
        private System.Collections.IEnumerator FlashbackRoutine(Level level)
        {
            // Pause player
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
            }
            
            // Show dialog
            string dialogKey = $"MEMORY_CRYSTAL_{memoryId.ToUpper()}";
            if (Dialog.Has(dialogKey))
            {
                yield return Textbox.Say(dialogKey);
            }
            
            // Resume player
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            
            EndCutscene(level);
        }
        
        public override void OnEnd(Level level)
        {
            // Cleanup
        }
    }
}
