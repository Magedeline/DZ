using System;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A barrier that requires Soul Fragments to unlock
    /// Used in Chapter 22 and other void-themed areas
    /// </summary>
    [CustomEntity(ids: "DesoloZantas/SoulBarrier,MaggyHelper/SoulBarrier")]
    [Tracked]
    public class SoulBarrier : Solid
    {
        private int fragmentsRequired;
        private string barrierId;
        private float dissolveTime;
        
        private int fragmentsCollected = 0;
        private bool dissolving = false;
        private float dissolveProgress = 0f;
        
        private Color barrierColor;
        private float pulseTimer = 0f;
        
        public SoulBarrier(EntityData data, Vector2 offset) 
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            fragmentsRequired = data.Int("fragmentsRequired", 3);
            barrierId = data.Attr("barrierId", "");
            dissolveTime = data.Float("dissolveTime", 1.0f);
            
            barrierColor = Color.HotPink;
            Collidable = true;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Restore persistent progress so fragments can be collected across rooms/sessions.
            if (!string.IsNullOrEmpty(barrierId))
            {
                fragmentsCollected = global::Celeste.Mod.MaggyHelper.MaggyHelperModule.SaveData?.GetCollectedSoulFragmentsForBarrier(barrierId) ?? 0;
            }
            
            // Register with the session for fragment tracking
            if (!string.IsNullOrEmpty(barrierId))
            {
                var session = (scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"SoulBarrier_{barrierId}_collected";
                    if (session.GetFlag(key))
                    {
                        // Already dissolved in this session
                        RemoveSelf();
                        return;
                    }
                }
            }

            if (fragmentsCollected >= fragmentsRequired)
            {
                StartDissolve();
            }
        }
        
        public void CollectFragment()
        {
            fragmentsCollected++;
            
            Audio.Play("guid://{3070bcdd-928f-4ffb-ba75-d43dbb2c2864}", Position);
            
            if (fragmentsCollected >= fragmentsRequired)
            {
                StartDissolve();
            }
        }
        
        private void StartDissolve()
        {
            dissolving = true;
            Collidable = false;
            
            Audio.Play("guid://{92f75010-4e90-428e-834c-1315ddbd9460}", Position);
            
            // Set session flag
            if (!string.IsNullOrEmpty(barrierId))
            {
                var session = (Scene as Level)?.Session;
                if (session != null)
                {
                    string key = $"SoulBarrier_{barrierId}_collected";
                    session.SetFlag(key, true);
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            pulseTimer += Engine.DeltaTime * 2f;
            
            if (dissolving)
            {
                dissolveProgress += Engine.DeltaTime / dissolveTime;
                
                if (dissolveProgress >= 1f)
                {
                    // Spawn particles
                    Level level = Scene as Level;
                    if (level != null)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            level.Particles.Emit(
                                ParticleTypes.SparkyDust, 
                                Position + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height)),
                                barrierColor
                            );
                        }
                    }
                    
                    RemoveSelf();
                }
            }
        }
        
        public override void Render()
        {
            if (dissolving)
            {
                float alpha = 1f - dissolveProgress;
                Draw.Rect(X, Y, Width, Height, barrierColor * alpha);
            }
            else
            {
                float pulse = 0.7f + (float)Math.Sin(pulseTimer) * 0.3f;
                Draw.Rect(X, Y, Width, Height, barrierColor * pulse);
                
                // Draw progress indicator
                string text = $"{fragmentsCollected}/{fragmentsRequired}";
                ActiveFont.DrawOutline(
                    text,
                    Center,
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.8f,
                    Color.White,
                    2f,
                    Color.Black
                );
            }
        }
        
        public string BarrierId => barrierId;
    }
    
    /// <summary>
    /// Collectible fragment that powers Soul Barriers
    /// </summary>
    [CustomEntity(ids: "DesoloZantas/SoulFragment,MaggyHelper/SoulFragment")]
    [Tracked]
    public class SoulFragment : Entity
    {
        private string barrierId;
        private string color;
        private float floatOffset = 0f;
        private bool collected = false;
        
        private static readonly Dictionary<string, Color> ColorMap = new Dictionary<string, Color>
        {
            { "red", new Color(255, 50, 50) },
            { "orange", new Color(255, 150, 50) },
            { "yellow", new Color(255, 255, 50) },
            { "green", new Color(50, 255, 50) },
            { "cyan", new Color(50, 255, 255) },
            { "blue", new Color(50, 50, 255) },
            { "purple", new Color(150, 50, 255) }
        };
        
        public SoulFragment(EntityData data, Vector2 offset) 
            : base(data.Position + offset)
        {
            barrierId = data.Attr("barrierId", "");
            color = data.Attr("color", "red").ToLower();
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            
            Add(new PlayerCollider(p => OnPlayer(p)));
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Check if already collected in this session
            var session = (scene as Level)?.Session;
            if (session != null && !string.IsNullOrEmpty(barrierId))
            {
                string key = $"SoulFragment_{barrierId}_{Position.X}_{Position.Y}_collected";
                if (session.GetFlag(key))
                {
                    RemoveSelf();
                }
            }
        }
        
        private void OnPlayer(global::Celeste.Player player)
        {
            if (collected) return;
            
            collected = true;
            
            // Play collection sound
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", Position);
            
            // Mark as collected in session
            var session = (Scene as Level)?.Session;
            if (session != null && !string.IsNullOrEmpty(barrierId))
            {
                string key = $"SoulFragment_{barrierId}_{Position.X}_{Position.Y}_collected";
                session.SetFlag(key, true);

                global::Celeste.Mod.MaggyHelper.MaggyHelperModule.SaveData?.CollectSoulFragment(key, barrierId);
            }
            
            // Find and notify the linked barrier
            foreach (SoulBarrier barrier in Scene.Tracker.GetEntities<SoulBarrier>())
            {
                if (barrier.BarrierId == barrierId)
                {
                    barrier.CollectFragment();
                    break;
                }
            }
            
            // Spawn particles and remove
            Level level = Scene as Level;
            if (level != null)
            {
                Color fragmentColor = ColorMap.ContainsKey(color) ? ColorMap[color] : Color.Red;
                for (int i = 0; i < 10; i++)
                {
                    level.Particles.Emit(ParticleTypes.SparkyDust, Position, fragmentColor);
                }
            }
            
            RemoveSelf();
        }
        
        public override void Update()
        {
            base.Update();
            floatOffset += Engine.DeltaTime * 2f;
        }
        
        public override void Render()
        {
            Color fragmentColor = ColorMap.ContainsKey(color) ? ColorMap[color] : Color.Red;
            
            float yOffset = (float)Math.Sin(floatOffset) * 4f;
            Vector2 pos = Position + new Vector2(0, yOffset);
            
            // Draw glow
            Draw.Circle(pos, 12f, fragmentColor * 0.3f, 12);
            
            // Draw core
            Draw.Circle(pos, 6f, fragmentColor, 12);
            Draw.Circle(pos, 4f, Color.White * 0.8f, 8);
        }
    }
}

