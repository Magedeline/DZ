namespace Celeste.Effects
{
    /// <summary>
    /// Additional elemental effects: Earth, Wind, Dark, and Light
    /// </summary>
    public static class AdditionalElementalEffects
    {
        #region Earth Effects
        
        // Earth particle types
        public static ParticleType P_Stone;
        public static ParticleType P_Dust;
        public static ParticleType P_Boulder;
        public static ParticleType P_Crystals;
        public static ParticleType P_Earthquake;

        // Earth audio events
        public const string SFX_EARTH_RUMBLE = "event:/pusheen/earth/rumble";
        public const string SFX_EARTH_STONE_BREAK = "event:/pusheen/earth/stone_break";
        public const string SFX_EARTH_BOULDER = "event:/pusheen/earth/boulder";

        #endregion

        #region Wind Effects

        // Wind particle types
        public static ParticleType P_Wind;
        public static ParticleType P_Tornado;
        public static ParticleType P_Leaves;
        public static ParticleType P_AirCurrent;
        public static ParticleType P_Gust;

        // Wind audio events
        public const string SFX_WIND_GUST = "event:/pusheen/wind/gust";
        public const string SFX_WIND_TORNADO = "event:/pusheen/wind/tornado";
        public const string SFX_WIND_WHOOSH = "event:/pusheen/wind/whoosh";

        #endregion

        #region Dark Effects

        // Dark particle types
        public static ParticleType P_Shadow;
        public static ParticleType P_Void;
        public static ParticleType P_DarkEnergy;
        public static ParticleType P_Corruption;
        public static ParticleType P_Shadow;

        // Dark audio events
        public const string SFX_DARK_WHISPER = "event:/pusheen/dark/whisper";
        public const string SFX_DARK_VOID = "event:/pusheen/dark/void";
        public const string SFX_DARK_CORRUPTION = "event:/pusheen/dark/corruption";

        #endregion

        #region Light Effects

        // Light particle types
        public static ParticleType P_Light;
        public static ParticleType P_Holy;
        public static ParticleType P_Sparkle;
        public static ParticleType P_Radiance;
        public static ParticleType P_Healing;

        // Light audio events
        public const string SFX_LIGHT_BEAM = "event:/pusheen/light/beam";
        public const string SFX_LIGHT_HEAL = "event:/pusheen/light/heal";
        public const string SFX_LIGHT_PURIFY = "event:/pusheen/light/purify";

        #endregion

        static AdditionalElementalEffects()
        {
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            InitializeEarthParticles();
            InitializeWindParticles();
            InitializeDarkParticles();
            InitializeLightParticles();
        }

        #region Earth Particle Initialization

        private static void InitializeEarthParticles()
        {
            P_Stone = new ParticleType
            {
                Size = 1.2f,
                Color = Color.SaddleBrown,
                Color2 = Color.Gray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 3.0f,
                SpeedMin = 15f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 40f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            P_Dust = new ParticleType
            {
                Size = 0.8f,
                Color = Color.BurlyWood,
                Color2 = Color.Tan,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 5f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -5f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_Boulder = new ParticleType
            {
                Size = 2f,
                Color = Color.DarkGray,
                Color2 = Color.Gray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 20f,
                SpeedMax = 50f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 60f),
                SpinMin = -3f,
                SpinMax = 3f
            };

            P_Crystals = new ParticleType
            {
                Size = 1f,
                Color = Color.Purple,
                Color2 = Color.Magenta,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 20f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_Earthquake = new ParticleType
            {
                Size = 1.5f,
                Color = Color.Brown,
                Color2 = Color.DarkGoldenrod,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 30f,
                SpeedMax = 60f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 30f),
                SpinMin = -4f,
                SpinMax = 4f
            };
        }

        #endregion

        #region Wind Particle Initialization

        private static void InitializeWindParticles()
        {
            P_Wind = new ParticleType
            {
                Size = 1f,
                Color = Color.LightGray * 0.7f,
                Color2 = Color.White * 0.5f,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 25f,
                SpeedMax = 50f,
                DirectionRange = (float)Math.PI * 0.5f,
                Acceleration = new Vector2(10f, -5f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            P_Tornado = new ParticleType
            {
                Size = 1.2f,
                Color = Color.LightGray,
                Color2 = Color.DarkGray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -8f,
                SpinMax = 8f
            };

            P_Leaves = new ParticleType
            {
                Size = 0.8f,
                Color = Color.Green,
                Color2 = Color.DarkGreen,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 15f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(5f, 15f),
                SpinMin = -3f,
                SpinMax = 3f
            };

            P_AirCurrent = new ParticleType
            {
                Size = 1.5f,
                Color = Color.CornflowerBlue * 0.6f,
                Color2 = Color.LightBlue * 0.4f,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 3.0f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 0.3f,
                Acceleration = new Vector2(0f, -10f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_Gust = new ParticleType
            {
                Size = 2f,
                Color = Color.White * 0.8f,
                Color2 = Color.LightGray * 0.6f,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.5f,
                LifeMax = 1.0f,
                SpeedMin = 60f,
                SpeedMax = 120f,
                DirectionRange = (float)Math.PI * 0.2f,
                Acceleration = Vector2.Zero,
                SpinMin = -5f,
                SpinMax = 5f
            };
        }

        #endregion

        #region Dark Particle Initialization

        private static void InitializeDarkParticles()
        {
            P_Shadow = new ParticleType
            {
                Size = 1f,
                Color = Color.Black,
                Color2 = Color.DarkGray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 3.0f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -10f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            P_Void = new ParticleType
            {
                Size = 1.5f,
                Color = Color.Purple * 0.8f,
                Color2 = Color.Black,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_DarkEnergy = new ParticleType
            {
                Size = 1.2f,
                Color = Color.DarkViolet,
                Color2 = Color.MediumPurple,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -15f),
                SpinMin = -3f,
                SpinMax = 3f
            };

            P_Corruption = new ParticleType
            {
                Size = 0.8f,
                Color = Color.DarkRed,
                Color2 = Color.Black,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.5f,
                LifeMax = 5.0f,
                SpeedMin = 8f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 5f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            P_Shadow = new ParticleType
            {
                Size = 2f,
                Color = Color.Black,
                Color2 = Color.DarkSlateGray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 15f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -4f,
                SpinMax = 4f
            };
        }

        #endregion

        #region Light Particle Initialization

        private static void InitializeLightParticles()
        {
            P_Light = new ParticleType
            {
                Size = 1f,
                Color = Color.White,
                Color2 = Color.LightYellow,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 15f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -20f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            P_Holy = new ParticleType
            {
                Size = 1.2f,
                Color = Color.Gold,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 3.0f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -15f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_Sparkle = new ParticleType
            {
                Size = 0.5f,
                Color = Color.White,
                Color2 = Color.LightBlue,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -5f,
                SpinMax = 5f
            };

            P_Radiance = new ParticleType
            {
                Size = 1.8f,
                Color = Color.LightGoldenrodYellow,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -10f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            P_Healing = new ParticleType
            {
                Size = 1f,
                Color = Color.LightGreen,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 2.5f,
                SpeedMin = 8f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -25f),
                SpinMin = -2f,
                SpinMax = 2f
            };
        }

        #endregion

        #region Earth Effects

        /// <summary>
        /// Create an earthquake effect
        /// </summary>
        public static void CreateEarthquake(Level level, Vector2 epicenter, float magnitude = 1f, float duration = 3f)
        {
            Audio.Play(SFX_EARTH_RUMBLE, epicenter);
            
            // Screen shake based on magnitude
            level.Shake(magnitude * 0.5f);
            
            // Earthquake particles
            level.ParticlesFG.Emit(P_Earthquake, (int)(magnitude * 30), epicenter, Vector2.One * magnitude * 32f);
            level.ParticlesBG.Emit(P_Dust, (int)(magnitude * 20), epicenter, Vector2.One * magnitude * 48f);
            
            // Create earth spikes in random locations
            for (int i = 0; i < magnitude * 5; i++)
            {
                Vector2 spikePos = epicenter + new Vector2(
                    Calc.Random.Range(-magnitude * 64f, magnitude * 64f),
                    Calc.Random.Range(-magnitude * 32f, magnitude * 32f)
                );
                
                CreateEarthSpike(level, spikePos);
            }
        }

        /// <summary>
        /// Create an earth spike at a position
        /// </summary>
        public static void CreateEarthSpike(Level level, Vector2 position)
        {
            Audio.Play(SFX_EARTH_STONE_BREAK, position);
            
            level.ParticlesFG.Emit(P_Stone, 10, position, Vector2.One * 8f);
            level.ParticlesFG.Emit(P_Dust, 5, position, Vector2.One * 12f);
            
            // Damage entities at position
            DamageEntitiesAt(level, position, 16f, "earth");
        }

        /// <summary>
        /// Create boulder throw effect
        /// </summary>
        public static void CreateBoulderThrow(Level level, Vector2 start, Vector2 end)
        {
            Audio.Play(SFX_EARTH_BOULDER, start);
            
            // Create boulder trajectory
            Vector2 direction = (end - start);
            int segments = (int)(direction.Length() / 16f);
            Vector2 step = direction / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 position = start + step * i;
                level.ParticlesFG.Emit(P_Boulder, 2, position, Vector2.One * 4f);
                
                if (i % 3 == 0)
                {
                    level.ParticlesBG.Emit(P_Dust, 1, position, Vector2.One * 6f);
                }
            }
            
            // Impact at end
            level.ParticlesFG.Emit(P_Stone, 15, end, Vector2.One * 12f);
            DamageEntitiesAt(level, end, 24f, "earth");
        }

        /// <summary>
        /// Create crystal formation
        /// </summary>
        public static void CreateCrystalFormation(Level level, Vector2 center, int crystalCount = 8)
        {
            float angleStep = (float)(Math.PI * 2) / crystalCount;
            
            for (int i = 0; i < crystalCount; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 24f;
                Vector2 position = center + offset;
                
                level.ParticlesFG.Emit(P_Crystals, 5, position, Vector2.One * 6f);
            }
        }

        #endregion

        #region Wind Effects

        /// <summary>
        /// Create tornado effect
        /// </summary>
        public static void CreateTornado(Level level, Vector2 center, float radius = 48f, float duration = 5f)
        {
            Audio.Play(SFX_WIND_TORNADO, center);
            
            // Create the tornado entity
            level.Add(new TornadoEntity(center, radius, duration));
            
            // Initial tornado particles
            level.ParticlesFG.Emit(P_Tornado, 40, center, Vector2.One * radius);
            level.ParticlesBG.Emit(P_Wind, 30, center, Vector2.One * radius * 1.2f);
            level.ParticlesFG.Emit(P_Leaves, 20, center, Vector2.One * radius * 0.8f);
        }

        /// <summary>
        /// Create wind gust effect
        /// </summary>
        public static void CreateWindGust(Level level, Vector2 origin, Vector2 direction, float force = 200f)
        {
            Audio.Play(SFX_WIND_GUST, origin);
            
            // Wind gust particles
            level.ParticlesFG.Emit(P_Gust, 15, origin, Vector2.One * 16f);
            level.ParticlesBG.Emit(P_Wind, 25, origin, Vector2.One * 20f);
            
            // Push entities in direction
            PushEntitiesInDirection(level, origin, direction, force, 64f);
        }

        /// <summary>
        /// Create air current for lifting entities
        /// </summary>
        public static void CreateAirCurrent(Level level, Vector2 position, float height = 64f)
        {
            Audio.Play(SFX_WIND_WHOOSH, position);
            
            // Vertical air current
            for (int i = 0; i < height / 8f; i++)
            {
                Vector2 currentPos = position + Vector2.UnitY * (-i * 8f);
                level.ParticlesFG.Emit(P_AirCurrent, 3, currentPos, Vector2.One * 4f);
            }
            
            // Lift entities in the current
            LiftEntitiesInArea(level, position, 16f, height);
        }

        #endregion

        #region Dark Effects

        /// <summary>
        /// Create shadow wave effect
        /// </summary>
        public static void CreateShadowWave(Level level, Vector2 center, float radius = 64f)
        {
            Audio.Play(SFX_DARK_WHISPER, center);
            
            // Expanding shadow wave
            level.ParticlesFG.Emit(P_Shadow, 30, center, Vector2.One * radius);
            level.ParticlesBG.Emit(P_Void, 20, center, Vector2.One * radius * 1.2f);
            
            // Dark screen effect
            level.Flash(Color.Black * 0.7f, true);
            
            // Corrupt entities in range
            CorruptEntitiesInRange(level, center, radius);
        }

        /// <summary>
        /// Create void portal effect
        /// </summary>
        public static void CreateVoidPortal(Level level, Vector2 position, float duration = 4f)
        {
            Audio.Play(SFX_DARK_VOID, position);
            
            // Create void portal entity
            level.Add(new VoidPortalEntity(position, duration));
            
            // Initial void particles
            level.ParticlesFG.Emit(P_Void, 25, position, Vector2.One * 32f);
            level.ParticlesFG.Emit(P_DarkEnergy, 15, position, Vector2.One * 24f);
        }

        /// <summary>
        /// Create corruption spread effect
        /// </summary>
        public static void CreateCorruption(Level level, Vector2 origin, float spreadRadius = 48f)
        {
            Audio.Play(SFX_DARK_CORRUPTION, origin);
            
            // Corruption particles spreading outward
            level.ParticlesFG.Emit(P_Corruption, 35, origin, Vector2.One * spreadRadius);
            level.ParticlesBG.Emit(P_Shadow, 20, origin, Vector2.One * spreadRadius * 1.5f);
            
            // Apply corruption to entities
            CorruptEntitiesInRange(level, origin, spreadRadius);
        }

        #endregion

        #region Light Effects

        /// <summary>
        /// Create light beam effect
        /// </summary>
        public static void CreateLightBeam(Level level, Vector2 start, Vector2 end)
        {
            Audio.Play(SFX_LIGHT_BEAM, start);
            
            // Light beam particles
            Vector2 direction = (end - start);
            int segments = (int)(direction.Length() / 8f);
            Vector2 step = direction / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 position = start + step * i;
                level.ParticlesFG.Emit(P_Light, 3, position, Vector2.One * 6f);
                
                if (i % 2 == 0)
                {
                    level.ParticlesFG.Emit(P_Sparkle, 2, position, Vector2.One * 4f);
                }
            }
            
            // Purify entities along the beam
            PurifyEntitiesAlongLine(level, start, end, 12f);
        }

        /// <summary>
        /// Create healing light effect
        /// </summary>
        public static void CreateHealingLight(Level level, Vector2 center, float radius = 32f)
        {
            Audio.Play(SFX_LIGHT_HEAL, center);
            
            // Healing particles
            level.ParticlesFG.Emit(P_Healing, 25, center, Vector2.One * radius);
            level.ParticlesFG.Emit(P_Light, 15, center, Vector2.One * radius * 0.8f);
            level.ParticlesFG.Emit(P_Sparkle, 30, center, Vector2.One * radius * 1.2f);
            
            // Heal entities in range
            HealEntitiesInRange(level, center, radius);
        }

        /// <summary>
        /// Create radiance burst effect
        /// </summary>
        public static void CreateRadianceBurst(Level level, Vector2 center, float intensity = 1f)
        {
            Audio.Play(SFX_LIGHT_PURIFY, center);
            
            // Intense light burst
            level.ParticlesFG.Emit(P_Radiance, (int)(intensity * 30), center, Vector2.One * intensity * 48f);
            level.ParticlesFG.Emit(P_Holy, (int)(intensity * 20), center, Vector2.One * intensity * 32f);
            level.ParticlesFG.Emit(P_Sparkle, (int)(intensity * 40), center, Vector2.One * intensity * 64f);
            
            // Bright flash
            level.Flash(Color.White * intensity * 0.8f, true);
            
            // Purify large area
            PurifyEntitiesInRange(level, center, intensity * 80f);
        }

        #endregion

        #region Helper Methods

        private static void DamageEntitiesAt(Level level, Vector2 position, float radius, string damageType)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, position) <= radius)
                {
                    ApplyElementalDamage(actor, position, damageType);
                }
            }
        }

        private static void PushEntitiesInDirection(Level level, Vector2 origin, Vector2 direction, float force, float range)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, origin) <= range)
                {
                    // Apply wind force (this would need entity-specific implementation)
                    if (actor is global::Celeste.Player player)
                    {
                        // Simple knockback for player
                        Vector2 knockback = direction.SafeNormalize() * force * 0.5f;
                        // This would need proper player state handling
                    }
                }
            }
        }

        private static void LiftEntitiesInArea(Level level, Vector2 position, float radius, float height)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, position) <= radius)
                {
                    // Apply upward force
                    level.ParticlesFG.Emit(P_AirCurrent, 3, actor.Position, Vector2.One * 6f);
                }
            }
        }

        private static void CorruptEntitiesInRange(Level level, Vector2 center, float radius)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, center) <= radius)
                {
                    // Apply corruption effect
                    level.ParticlesFG.Emit(P_Corruption, 3, actor.Position, Vector2.One * 6f);
                }
            }
        }

        private static void PurifyEntitiesAlongLine(Level level, Vector2 start, Vector2 end, float width)
        {
            // Optimize: iterate once over actors and solids instead of multiple times in a loop
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (IsPointNearLine(actor.Position, start, end, width))
                {
                    level.ParticlesFG.Emit(P_Light, 2, actor.Position, Vector2.One * 4f);
                }
            }
        }

        private static bool IsPointNearLine(Vector2 point, Vector2 start, Vector2 end, float width)
        {
            float lengthSq = Vector2.DistanceSquared(start, end);
            if (lengthSq == 0) return Vector2.Distance(point, start) <= width;
            
            float t = MathHelper.Clamp(Vector2.Dot(point - start, end - start) / lengthSq, 0, 1);
            Vector2 projection = start + t * (end - start);
            return Vector2.Distance(point, projection) <= width;
        }

        private static void PurifyEntitiesInRange(Level level, Vector2 center, float radius)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, center) <= radius)
                {
                    // Remove negative effects, heal if applicable
                    level.ParticlesFG.Emit(P_Light, 2, actor.Position, Vector2.One * 4f);
                }
            }
        }

        private static void HealEntitiesInRange(Level level, Vector2 center, float radius)
        {
            foreach (Actor actor in level.Tracker.GetEntities<Actor>())
            {
                if (Vector2.Distance(actor.Position, center) <= radius)
                {
                    if (actor is global::Celeste.Player)
                    {
                        // Heal player (would need proper health system integration)
                        level.ParticlesFG.Emit(P_Healing, 5, actor.Position, Vector2.One * 8f);
                    }
                }
            }
        }

        private static void ApplyElementalDamage(Entity entity, Vector2 damageCenter, string damageType)
        {
            if (entity is global::Celeste.Player player)
            {
                Vector2 knockback = (player.Position - damageCenter).SafeNormalize() * 100f;
                
                switch (damageType)
                {
                    case "earth":
                        // Stun briefly then damage
                        player.Die(knockback);
                        break;
                    default:
                        player.Die(knockback);
                        break;
                }
            }
        }

        #endregion
    }

    #region Supporting Entities

    /// <summary>
    /// Tornado entity that pulls in nearby objects
    /// </summary>
    public class TornadoEntity : Entity
    {
        private float radius;
        private float duration;
        private float timer;
        private float pullForce = 150f;

        public TornadoEntity(Vector2 position, float radius, float duration) : base(position)
        {
            this.radius = radius;
            this.duration = duration;
            
            Collider = new Circle(radius);
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            // Emit tornado particles
            if (Scene.OnInterval(0.1f))
            {
                (Scene as Level)?.ParticlesFG.Emit(AdditionalElementalEffects.P_Tornado, 5, Position, Vector2.One * radius);
                (Scene as Level)?.ParticlesBG.Emit(AdditionalElementalEffects.P_Wind, 3, Position, Vector2.One * radius * 1.2f);
            }
            
            // Pull entities toward center
            PullEntities();
            
            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        private void PullEntities()
        {
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
            {
                float distance = Vector2.Distance(actor.Position, Position);
                if (distance <= radius)
                {
                    Vector2 pullDirection = (Position - actor.Position).SafeNormalize();
                    float pullStrength = (1f - distance / radius) * pullForce;
                    
                    // Apply pull force (would need entity-specific implementation)
                    if (Scene is Level level)
                    {
                        level.ParticlesFG.Emit(AdditionalElementalEffects.P_Wind, 1, actor.Position, Vector2.One * 4f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Void portal that consumes entities
    /// </summary>
    public class VoidPortalEntity : Entity
    {
        private float duration;
        private float timer;
        private float consumeRadius = 24f;

        public VoidPortalEntity(Vector2 position, float duration) : base(position)
        {
            this.duration = duration;
            Collider = new Circle(consumeRadius);
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            // Emit void particles
            if (Scene.OnInterval(0.15f))
            {
                (Scene as Level)?.ParticlesFG.Emit(AdditionalElementalEffects.P_Void, 3, Position, Vector2.One * consumeRadius);
                (Scene as Level)?.ParticlesFG.Emit(AdditionalElementalEffects.P_DarkEnergy, 2, Position, Vector2.One * consumeRadius * 0.8f);
            }
            
            // Consume nearby entities
            ConsumeEntities();
            
            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        private void ConsumeEntities()
        {
            // Only consume actors and solids to avoid iterating over every single entity in the scene
            // This significantly improves performance in large rooms.
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>())
            {
                if (!(actor is global::Celeste.Player) && Vector2.Distance(actor.Position, Position) <= consumeRadius)
                {
                    actor.RemoveSelf();
                    (Scene as Level)?.ParticlesFG.Emit(AdditionalElementalEffects.P_Void, 5, actor.Position, Vector2.One * 8f);
                }
            }
            
            foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
            {
                if (Vector2.Distance(solid.Position, Position) <= consumeRadius)
                {
                    solid.RemoveSelf();
                    (Scene as Level)?.ParticlesFG.Emit(AdditionalElementalEffects.P_Void, 5, solid.Position, Vector2.One * 8f);
                }
            }
        }
    }

    #endregion
}



