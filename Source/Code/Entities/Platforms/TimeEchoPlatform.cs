using System;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A platform that phases between timelines
    /// Player can sync with it by dashing at the right moment
    /// </summary>
    public class TimeEchoPlatform : CelesteJumpThru
    {
        private float phaseTime;
        private float offset;
        private bool syncOnDash;
        
        private float timer;
        private bool isPhased = false;
        private float syncedTimer = 0f;
        private bool playerSynced = false;
        
        private Color platformColor;
        private Color ghostColor;
        
        public TimeEchoPlatform(EntityData data, Vector2 pos) 
            : base(data.Position + pos, data.Width, false)
        {
            phaseTime = data.Float("phaseTime", 2.0f);
            offset = data.Float("offset", 0f);
            syncOnDash = data.Bool("syncOnDash", true);
            
            timer = offset * phaseTime;
            
            platformColor = new Color(50, 200, 255);
            ghostColor = new Color(50, 200, 255, 100);
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Listen for player dash if sync enabled
            if (syncOnDash)
            {
                Add(new TransitionListener
                {
                    OnOutBegin = () => { playerSynced = false; }
                });
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            if (timer >= phaseTime)
            {
                timer -= phaseTime;
                isPhased = !isPhased;
                
                if (!playerSynced)
                {
                    Collidable = !isPhased;
                }
            }
            
            // Check for player dash to sync
            if (syncOnDash && !playerSynced)
            {
                Player player = Scene?.Tracker.GetEntity<Player>();
                if (player != null && player.StateMachine.State == Player.StDash)
                {
                    // Check if player is near and platform is fading
                    float distSq = Vector2.DistanceSquared(player.Center, Center);
                    if (distSq < 80 * 80)
                    {
                        float phaseProgress = timer / phaseTime;
                        // Sync if dashing during transition window (40-60%)
                        if (phaseProgress > 0.4f && phaseProgress < 0.6f)
                        {
                            SyncToPlayer();
                        }
                    }
                }
            }
            
            if (playerSynced)
            {
                syncedTimer -= Engine.DeltaTime;
                if (syncedTimer <= 0f)
                {
                    playerSynced = false;
                    Collidable = !isPhased;
                }
            }
        }
        
        private void SyncToPlayer()
        {
            playerSynced = true;
            Collidable = true;
            syncedTimer = 3f; // Platform stays solid for 3 seconds
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            // Visual feedback
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    level.Particles.Emit(
                        ParticleTypes.SparkyDust,
                        Position + new Vector2(Calc.Random.NextFloat(Width), 0),
                        platformColor
                    );
                }
            }
        }
        
        public override void Render()
        {
            float alpha = isPhased && !playerSynced ? 0.3f : 1f;
            
            if (playerSynced)
            {
                // Synced state - solid cyan
                Draw.Rect(X, Y, Width, 8f, platformColor);
                Draw.Rect(X, Y, Width, 2f, Color.White);
            }
            else if (isPhased)
            {
                // Ghost state - semi-transparent
                Draw.Rect(X, Y, Width, 8f, ghostColor);
            }
            else
            {
                // Solid state
                Draw.Rect(X, Y, Width, 8f, platformColor * alpha);
                Draw.Rect(X, Y, Width, 2f, Color.White * alpha);
            }
            
            // Phase indicator
            float phaseProgress = timer / phaseTime;
            float indicatorWidth = Width * phaseProgress;
            Draw.Rect(X, Y + 6f, indicatorWidth, 2f, Color.White * 0.5f);
        }
    }
}

