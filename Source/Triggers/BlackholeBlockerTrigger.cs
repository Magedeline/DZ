namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that stops player movement when entering, creating a "pulled into blackhole" effect
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/BlackholeBlockerTrigger")]
    public class BlackholeBlockerTrigger : Trigger
    {
        public static ParticleType P_Rainbow;

        private float stopDuration;
        private bool oneUse;
        private bool hasTriggered;
        private float pullStrength;
        private Vector2 pullDirection;
        private bool killPlayer;
        private bool visualEffect;
        private string releaseFlag;

        public BlackholeBlockerTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            stopDuration = data.Float("stopDuration", 1.5f);
            oneUse = data.Bool("oneUse", false);
            pullStrength = data.Float("pullStrength", 200f);
            killPlayer = data.Bool("killPlayer", false);
            visualEffect = data.Bool("visualEffect", true);
            releaseFlag = data.Attr("releaseFlag", "");
            
            // Parse pull direction from string
            string pullDirStr = data.Attr("pullDirection", "Center");
            switch (pullDirStr.ToLower())
            {
                case "center":
                    pullDirection = Vector2.Zero; // Pull to center of trigger
                    break;
                case "up":
                    pullDirection = -Vector2.UnitY;
                    break;
                case "down":
                    pullDirection = Vector2.UnitY;
                    break;
                case "left":
                    pullDirection = -Vector2.UnitX;
                    break;
                case "right":
                    pullDirection = Vector2.UnitX;
                    break;
                default:
                    pullDirection = Vector2.Zero;
                    break;
            }

            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            if (P_Rainbow == null)
            {
                P_Rainbow = new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Color.Purple,
                    Color2 = Color.Cyan,
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.InAndOut,
                    LifeMin = 0.8f,
                    LifeMax = 1.5f,
                    Size = 1.2f,
                    SizeRange = 0.6f,
                    SpeedMin = 30f,
                    SpeedMax = 60f,
                    DirectionRange = (float)Math.PI * 2f,
                    SpinMin = -4f,
                    SpinMax = 4f
                };
            }
        }

        public override void OnStay(global::Celeste.Player player)
        {
            base.OnStay(player);
            
            // Check release flag
            if (!string.IsNullOrEmpty(releaseFlag))
            {
                Level level = Scene as Level;
                if (level != null && level.Session.GetFlag(releaseFlag))
                {
                    return; // Don't block if release flag is set
                }
            }

            if (oneUse && hasTriggered)
                return;

            if (!hasTriggered)
            {
                hasTriggered = true;
                Add(new Coroutine(BlockPlayerRoutine(player)));
            }
        }

        private System.Collections.IEnumerator BlockPlayerRoutine(global::Celeste.Player player)
        {
            Level level = Scene as Level;
            
            // Store original state
            int originalState = player.StateMachine.State;
            Vector2 originalSpeed = player.Speed;
            
            // Play sound
            Audio.Play("event:/game/general/thing_booped", Position);
            
            if (visualEffect && level != null)
            {
                level.Shake(0.4f);
            }

            // Calculate pull target
            Vector2 pullTarget;
            if (pullDirection == Vector2.Zero)
            {
                // Pull to center of trigger
                pullTarget = Center;
            }
            else
            {
                // Pull in direction
                pullTarget = player.Position + pullDirection * pullStrength;
            }

            // Stop player movement
            player.StateMachine.State = 11; // Dummy state
            player.Speed = Vector2.Zero;
            
            // Pull player gradually
            float timer = 0f;
            while (timer < stopDuration)
            {
                timer += Engine.DeltaTime;
                
                // Apply pull force
                if (pullDirection == Vector2.Zero)
                {
                    Vector2 toPullTarget = (pullTarget - player.Position).SafeNormalize();
                    player.Position += toPullTarget * pullStrength * Engine.DeltaTime;
                }
                else
                {
                    player.Position += pullDirection * pullStrength * Engine.DeltaTime;
                }

                // Visual effects
                if (visualEffect && level != null && Scene.OnInterval(0.1f))
                {
                    Color rainbowColor = GetRainbowColor(timer);
                    ParticleType rainbow = new ParticleType(P_Rainbow)
                    {
                        Color = rainbowColor,
                        Color2 = Color.Black
                    };
                    level.ParticlesFG.Emit(rainbow, player.Center, (float)Math.PI * 2f);
                }

                // Check release flag during blocking
                if (!string.IsNullOrEmpty(releaseFlag) && level != null)
                {
                    if (level.Session.GetFlag(releaseFlag))
                    {
                        break; // Release early
                    }
                }

                yield return null;
            }

            // Kill player or release
            if (killPlayer)
            {
                Vector2 deathDirection = (pullTarget - player.Position).SafeNormalize() * 100f;
                player.Die(deathDirection);
                
                if (visualEffect && level != null)
                {
                    // Death explosion
                    for (int i = 0; i < 20; i++)
                    {
                        Color explosionColor = GetRainbowColor(i * 0.1f);
                        ParticleType explosion = new ParticleType(P_Rainbow)
                        {
                            Color = explosionColor,
                            Color2 = Color.Black
                        };
                        level.ParticlesFG.Emit(explosion, player.Center, (float)Math.PI * 2f);
                    }
                }
            }
            else
            {
                // Release player
                player.StateMachine.State = originalState;
                
                // Apply release velocity (opposite to pull)
                if (pullDirection != Vector2.Zero)
                {
                    player.Speed = -pullDirection * 100f;
                }
            }

            yield break;
        }

        private Color GetRainbowColor(float time)
        {
            float hue = (time * 5f) % 1f;
            return HSVToRGB(hue, 1f, 1f);
        }

        private Color HSVToRGB(float h, float s, float v)
        {
            int i = (int)Math.Floor(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return new Color(v, t, p);
                case 1: return new Color(q, v, p);
                case 2: return new Color(p, v, t);
                case 3: return new Color(p, q, v);
                case 4: return new Color(t, p, v);
                case 5: return new Color(v, p, q);
                default: return Color.White;
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Optional: visualize trigger in debug mode
            if (visualEffect && Engine.Commands != null && Engine.Commands.Open)
            {
                Draw.HollowRect(Collider, Color.Purple * 0.5f);
            }
        }
    }
}




