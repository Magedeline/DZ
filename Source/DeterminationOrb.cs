using System;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Grants temporary determination boost (extra dashes, speed)
    /// Inspired by Undertale's red soul power
    /// </summary>
    [HotReloadable]
    public class DeterminationOrb : Entity
    {
        private int dashBoost;
        private float speedMultiplier;
        private float duration;
        private bool oneUse;
        
        private float floatTimer = 0f;
        private bool collected = false;
        private float respawnTimer = 0f;
        private const float RESPAWN_TIME = 5f;
        
        private Color orbColor;
        private Color coreColor;
        
        public DeterminationOrb(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            dashBoost = data.Int("dashBoost", 1);
            speedMultiplier = data.Float("speedMultiplier", 1.2f);
            duration = data.Float("duration", 10f);
            oneUse = data.Bool("oneUse", true);
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(p => OnPlayer(p)));
            
            orbColor = new Color(255, 50, 50);
            coreColor = new Color(255, 150, 150);
            
            Depth = -100;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected (one-time only)
            if (oneUse)
            {
                var session = (scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"DeterminationOrb_{Position.X}_{Position.Y}_collected";
                    if (session.GetFlag(key))
                    {
                        RemoveSelf();
                    }
                }
            }
        }
        
        private void OnPlayer(Celeste.Player player)
        {
            if (collected) return;
            
            collected = true;
            
            // Apply determination boost
            ApplyBoost(player);
            
            Audio.Play("event:/game/general/diamond_return", Position);
            
            // Mark as collected if one-time
            if (oneUse)
            {
                var session = (Scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"DeterminationOrb_{Position.X}_{Position.Y}_collected";
                    session.SetFlag(key, true);
                }
                
                SpawnParticles();
                RemoveSelf();
            }
            else
            {
                respawnTimer = RESPAWN_TIME;
            }
        }
        
        private void ApplyBoost(Celeste.Player player)
        {
            // Store original values and apply boost
            // This would integrate with your existing player modification system
            
            // For now, we'll just refill dashes and add a visual effect
            player.RefillDash();
            if (dashBoost > 1)
            {
                player.Dashes = Math.Min(player.Dashes + dashBoost - 1, player.MaxDashes + dashBoost);
            }
            
            // Add a speed boost tween
            // In a full implementation, this would modify player speed for the duration
            
            // Visual feedback
            Level level = Scene as Level;
            if (level != null)
            {
                level.Displacement.AddBurst(Position, 0.5f, 8f, 32f, 0.5f);
            }
            
            // Show message
            // Dialog.Clean("ENTITY_DETERMINATION_ORB_COLLECTED") would be used here
        }
        
        private void SpawnParticles()
        {
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 15; i++)
                {
                    float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                    Vector2 dir = Calc.AngleToVector(angle, Calc.Random.NextFloat(16f) + 8f);
                    level.Particles.Emit(ParticleTypes.SparkyDust, Position + dir, orbColor);
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            floatTimer += Engine.DeltaTime;
            
            if (collected && !oneUse)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    collected = false;
                    Audio.Play("event:/game/general/diamond_return", Position);
                }
            }
        }
        
        public override void Render()
        {
            if (collected && !oneUse)
            {
                // Draw respawn progress
                float progress = 1f - (respawnTimer / RESPAWN_TIME);
                Draw.Circle(Position, 8f * progress, orbColor * 0.3f, 12);
                return;
            }
            
            float yOffset = (float)Math.Sin(floatTimer * 2f) * 3f;
            Vector2 pos = Position + new Vector2(0, yOffset);
            
            // Draw glow
            float pulse = 0.6f + (float)Math.Sin(floatTimer * 3f) * 0.4f;
            Draw.Circle(pos, 12f * pulse, orbColor * 0.3f, 16);
            
            // Draw orb
            Draw.Circle(pos, 8f, orbColor, 12);
            Draw.Circle(pos, 5f, coreColor, 8);
            Draw.Circle(pos, 2f, Color.White, 4);
            
            // Draw determination symbol (stylized heart)
            float scale = 0.8f + (float)Math.Sin(floatTimer * 4f) * 0.1f;
            // In full implementation, draw a heart sprite here
        }
    }
}
