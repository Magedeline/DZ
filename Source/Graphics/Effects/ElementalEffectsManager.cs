namespace Celeste.Effects
{
    /// <summary>
    /// Central manager for all elemental effects in the game
    /// </summary>
    [HotReloadable]
    public static class ElementalEffectsManager
    {
        private static readonly Dictionary<string, EffectDefinition> registeredEffects = new Dictionary<string, EffectDefinition>();
        private static readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();
        
        /// <summary>
        /// Initialize all elemental effects
        /// </summary>
        public static void Initialize()
        {
            RegisterFireEffects();
            RegisterIceEffects();
            RegisterLightningEffects();
            RegisterEarthEffects();
            RegisterWindEffects();
            RegisterDarkEffects();
            RegisterLightEffects();
            
            IngesteLogger.Info("ElementalEffectsManager: Initialized with " + registeredEffects.Count + " effects");
        }

        /// <summary>
        /// Register a new effect definition
        /// </summary>
        public static void RegisterEffect(string effectId, EffectDefinition definition)
        {
            registeredEffects[effectId] = definition;
        }

        /// <summary>
        /// Create and play an effect by ID
        /// </summary>
        public static void PlayEffect(string effectId, Level level, Vector2 position, Dictionary<string, object> parameters = null)
        {
            if (!registeredEffects.ContainsKey(effectId))
            {
                IngesteLogger.Info($"ElementalEffectsManager: Effect '{effectId}' not found");
                return;
            }

            var definition = registeredEffects[effectId];
            var activeEffect = new ActiveEffect(definition, level, position, parameters ?? new Dictionary<string, object>());
            
            activeEffects.Add(activeEffect);
            activeEffect.Start();
            
            IngesteLogger.Debug($"ElementalEffectsManager: Started effect '{effectId}' at {position}");
        }

        /// <summary>
        /// Update all active effects
        /// </summary>
        public static void Update()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].Update())
                {
                    activeEffects.RemoveAt(i);
                }
            }
            
            // Update specific effect systems
            IceEffects.UpdateFrozenEntities();
            LightningEffects.UpdateChargedEntities();
        }

        /// <summary>
        /// Stop all active effects
        /// </summary>
        public static void StopAllEffects()
        {
            foreach (var effect in activeEffects)
            {
                effect.Stop();
            }
            activeEffects.Clear();
        }

        /// <summary>
        /// Get all registered effect IDs
        /// </summary>
        public static IEnumerable<string> GetRegisteredEffectIds()
        {
            return registeredEffects.Keys;
        }

        #region Effect Registration Methods

        private static void RegisterFireEffects()
        {
            RegisterEffect("fire_burst", new EffectDefinition
            {
                Name = "Fire Burst",
                ElementType = ElementType.Fire,
                Duration = 1f,
                Action = (level, pos, param) => FireEffects.CreateFireBurst(level, pos, 
                    param.ContainsKey("particleCount") ? (int)param["particleCount"] : 20)
            });

            RegisterEffect("fire_explosion", new EffectDefinition
            {
                Name = "Fire Explosion",
                ElementType = ElementType.Fire,
                Duration = 2f,
                Action = (level, pos, param) => FireEffects.CreateFireExplosion(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 32f)
            });

            RegisterEffect("fire_pillar", new EffectDefinition
            {
                Name = "Fire Pillar",
                ElementType = ElementType.Fire,
                Duration = 2f,
                Action = (level, pos, param) => FireEffects.CreateFirePillar(level, pos,
                    param.ContainsKey("height") ? (float)param["height"] : 64f,
                    param.ContainsKey("duration") ? (float)param["duration"] : 2f)
            });

            RegisterEffect("fire_trail", new EffectDefinition
            {
                Name = "Fire Trail",
                ElementType = ElementType.Fire,
                Duration = 1f,
                Action = (level, pos, param) => {
                    Vector2 end = param.ContainsKey("end") ? (Vector2)param["end"] : pos + Vector2.UnitX * 64f;
                    FireEffects.CreateFireTrail(level, pos, end);
                }
            });

            RegisterEffect("magical_fire", new EffectDefinition
            {
                Name = "Magical Fire",
                ElementType = ElementType.Fire,
                Duration = 1.5f,
                Action = (level, pos, param) => {
                    FireType fireType = param.ContainsKey("fireType") ? (FireType)param["fireType"] : FireType.Normal;
                    FireEffects.CreateMagicalFire(level, pos, fireType);
                }
            });
        }

        private static void RegisterIceEffects()
        {
            RegisterEffect("ice_burst", new EffectDefinition
            {
                Name = "Ice Burst",
                ElementType = ElementType.Ice,
                Duration = 1f,
                Action = (level, pos, param) => IceEffects.CreateIceBurst(level, pos,
                    param.ContainsKey("particleCount") ? (int)param["particleCount"] : 20)
            });

            RegisterEffect("freeze_explosion", new EffectDefinition
            {
                Name = "Freeze Explosion",
                ElementType = ElementType.Ice,
                Duration = 2f,
                Action = (level, pos, param) => IceEffects.CreateFreezeExplosion(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 32f)
            });

            RegisterEffect("blizzard", new EffectDefinition
            {
                Name = "Blizzard",
                ElementType = ElementType.Ice,
                Duration = 3f,
                Action = (level, pos, param) => IceEffects.CreateBlizzard(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 64f,
                    param.ContainsKey("duration") ? (float)param["duration"] : 3f)
            });

            RegisterEffect("ice_wall", new EffectDefinition
            {
                Name = "Ice Wall",
                ElementType = ElementType.Ice,
                Duration = 1f,
                Action = (level, pos, param) => {
                    Vector2 end = param.ContainsKey("end") ? (Vector2)param["end"] : pos + Vector2.UnitX * 64f;
                    IceEffects.CreateIceWall(level, pos, end);
                }
            });

            RegisterEffect("icicle_spikes", new EffectDefinition
            {
                Name = "Icicle Spikes",
                ElementType = ElementType.Ice,
                Duration = 1.5f,
                Action = (level, pos, param) => {
                    Vector2 direction = param.ContainsKey("direction") ? (Vector2)param["direction"] : Vector2.UnitX;
                    IceEffects.CreateIcicleSpikes(level, pos, direction);
                }
            });
        }

        private static void RegisterLightningEffects()
        {
            RegisterEffect("lightning_strike", new EffectDefinition
            {
                Name = "Lightning Strike",
                ElementType = ElementType.Lightning,
                Duration = 0.5f,
                Action = (level, pos, param) => {
                    Vector2 end = param.ContainsKey("end") ? (Vector2)param["end"] : pos + Vector2.UnitY * 100f;
                    LightningEffects.CreateLightningStrike(level, pos, end);
                }
            });

            RegisterEffect("chain_lightning", new EffectDefinition
            {
                Name = "Chain Lightning",
                ElementType = ElementType.Lightning,
                Duration = 1f,
                Action = (level, pos, param) => LightningEffects.CreateChainLightning(level, pos,
                    param.ContainsKey("maxRange") ? (float)param["maxRange"] : 64f,
                    param.ContainsKey("maxChains") ? (int)param["maxChains"] : 5)
            });

            RegisterEffect("electric_field", new EffectDefinition
            {
                Name = "Electric Field",
                ElementType = ElementType.Lightning,
                Duration = 3f,
                Action = (level, pos, param) => LightningEffects.CreateElectricField(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 48f,
                    param.ContainsKey("duration") ? (float)param["duration"] : 3f)
            });

            RegisterEffect("plasma_ball", new EffectDefinition
            {
                Name = "Plasma Ball",
                ElementType = ElementType.Lightning,
                Duration = 1f,
                Action = (level, pos, param) => LightningEffects.CreatePlasmaBall(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 24f)
            });

            RegisterEffect("emp", new EffectDefinition
            {
                Name = "Electromagnetic Pulse",
                ElementType = ElementType.Lightning,
                Duration = 1.5f,
                Action = (level, pos, param) => LightningEffects.CreateEMP(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 80f)
            });
        }

        private static void RegisterEarthEffects()
        {
            RegisterEffect("earthquake", new EffectDefinition
            {
                Name = "Earthquake",
                ElementType = ElementType.Earth,
                Duration = 3f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateEarthquake(level, pos,
                    param.ContainsKey("magnitude") ? (float)param["magnitude"] : 1f,
                    param.ContainsKey("duration") ? (float)param["duration"] : 3f)
            });

            RegisterEffect("earth_spike", new EffectDefinition
            {
                Name = "Earth Spike",
                ElementType = ElementType.Earth,
                Duration = 1f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateEarthSpike(level, pos)
            });

            RegisterEffect("boulder_throw", new EffectDefinition
            {
                Name = "Boulder Throw",
                ElementType = ElementType.Earth,
                Duration = 2f,
                Action = (level, pos, param) => {
                    Vector2 end = param.ContainsKey("end") ? (Vector2)param["end"] : pos + Vector2.UnitX * 96f;
                    AdditionalElementalEffects.CreateBoulderThrow(level, pos, end);
                }
            });

            RegisterEffect("crystal_formation", new EffectDefinition
            {
                Name = "Crystal Formation",
                ElementType = ElementType.Earth,
                Duration = 1f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateCrystalFormation(level, pos,
                    param.ContainsKey("crystalCount") ? (int)param["crystalCount"] : 8)
            });
        }

        private static void RegisterWindEffects()
        {
            RegisterEffect("tornado", new EffectDefinition
            {
                Name = "Tornado",
                ElementType = ElementType.Wind,
                Duration = 5f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateTornado(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 48f,
                    param.ContainsKey("duration") ? (float)param["duration"] : 5f)
            });

            RegisterEffect("wind_gust", new EffectDefinition
            {
                Name = "Wind Gust",
                ElementType = ElementType.Wind,
                Duration = 1f,
                Action = (level, pos, param) => {
                    Vector2 direction = param.ContainsKey("direction") ? (Vector2)param["direction"] : Vector2.UnitX;
                    AdditionalElementalEffects.CreateWindGust(level, pos, direction);
                }
            });

            RegisterEffect("air_current", new EffectDefinition
            {
                Name = "Air Current",
                ElementType = ElementType.Wind,
                Duration = 2f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateAirCurrent(level, pos,
                    param.ContainsKey("height") ? (float)param["height"] : 64f)
            });
        }

        private static void RegisterDarkEffects()
        {
            RegisterEffect("shadow_wave", new EffectDefinition
            {
                Name = "Shadow Wave",
                ElementType = ElementType.Dark,
                Duration = 2f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateShadowWave(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 64f)
            });

            RegisterEffect("void_portal", new EffectDefinition
            {
                Name = "Void Portal",
                ElementType = ElementType.Dark,
                Duration = 4f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateVoidPortal(level, pos,
                    param.ContainsKey("duration") ? (float)param["duration"] : 4f)
            });

            RegisterEffect("corruption", new EffectDefinition
            {
                Name = "Corruption",
                ElementType = ElementType.Dark,
                Duration = 2f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateCorruption(level, pos,
                    param.ContainsKey("spreadRadius") ? (float)param["spreadRadius"] : 48f)
            });
        }

        private static void RegisterLightEffects()
        {
            RegisterEffect("light_beam", new EffectDefinition
            {
                Name = "Light Beam",
                ElementType = ElementType.Light,
                Duration = 1f,
                Action = (level, pos, param) => {
                    Vector2 end = param.ContainsKey("end") ? (Vector2)param["end"] : pos + Vector2.UnitX * 64f;
                    AdditionalElementalEffects.CreateLightBeam(level, pos, end);
                }
            });

            RegisterEffect("healing_light", new EffectDefinition
            {
                Name = "Healing Light",
                ElementType = ElementType.Light,
                Duration = 2f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateHealingLight(level, pos,
                    param.ContainsKey("radius") ? (float)param["radius"] : 32f)
            });

            RegisterEffect("radiance_burst", new EffectDefinition
            {
                Name = "Radiance Burst",
                ElementType = ElementType.Light,
                Duration = 1.5f,
                Action = (level, pos, param) => AdditionalElementalEffects.CreateRadianceBurst(level, pos,
                    param.ContainsKey("intensity") ? (float)param["intensity"] : 1f)
            });
        }

        #endregion

        /// <summary>
        /// Create elemental combination effects
        /// </summary>
        public static void PlayComboEffect(ElementType element1, ElementType element2, Level level, Vector2 position)
        {
            string comboId = GetComboEffectId(element1, element2);
            if (!string.IsNullOrEmpty(comboId))
            {
                PlayEffect(comboId, level, position);
            }
            else
            {
                // Play both effects simultaneously for a combo effect
                PlayElementalEffect(element1, level, position);
                PlayElementalEffect(element2, level, position + new Vector2(8f, 0f));
            }
        }

        /// <summary>
        /// Play a basic elemental effect based on element type
        /// </summary>
        public static void PlayElementalEffect(ElementType elementType, Level level, Vector2 position)
        {
            string effectId = elementType switch
            {
                ElementType.Fire => "fire_burst",
                ElementType.Ice => "ice_burst",
                ElementType.Lightning => "chain_lightning",
                ElementType.Earth => "earth_spike",
                ElementType.Wind => "wind_gust",
                ElementType.Dark => "shadow_wave",
                ElementType.Light => "healing_light",
                _ => "fire_burst"
            };

            PlayEffect(effectId, level, position);
        }

        private static string GetComboEffectId(ElementType element1, ElementType element2)
        {
            // Define combination effects
            var combos = new Dictionary<(ElementType, ElementType), string>
            {
                { (ElementType.Fire, ElementType.Ice), "steam_explosion" },
                { (ElementType.Ice, ElementType.Fire), "steam_explosion" },
                { (ElementType.Lightning, ElementType.Ice), "ice_lightning" },
                { (ElementType.Ice, ElementType.Lightning), "ice_lightning" },
                { (ElementType.Fire, ElementType.Earth), "lava_eruption" },
                { (ElementType.Earth, ElementType.Fire), "lava_eruption" },
                { (ElementType.Wind, ElementType.Fire), "fire_tornado" },
                { (ElementType.Fire, ElementType.Wind), "fire_tornado" },
                { (ElementType.Light, ElementType.Dark), "twilight_blast" },
                { (ElementType.Dark, ElementType.Light), "twilight_blast" },
            };

            return combos.ContainsKey((element1, element2)) ? combos[(element1, element2)] : null;
        }
    }

    /// <summary>
    /// Enumeration for all element types
    /// </summary>
    public enum ElementType
    {
        Fire,
        Ice,
        Lightning,
        Earth,
        Wind,
        Dark,
        Light
    }

    /// <summary>
    /// Definition of an elemental effect
    /// </summary>
    public class EffectDefinition
    {
        public string Name { get; set; }
        public ElementType ElementType { get; set; }
        public float Duration { get; set; }
        public Action<Level, Vector2, Dictionary<string, object>> Action { get; set; }
        public string Description { get; set; }
        public bool RequiresTarget { get; set; }
        public float Cooldown { get; set; }
        public int ManaCost { get; set; }
    }

    /// <summary>
    /// Represents an active effect instance
    /// </summary>
    public class ActiveEffect
    {
        public EffectDefinition Definition { get; }
        public Level Level { get; }
        public Vector2 Position { get; }
        public Dictionary<string, object> Parameters { get; }
        
        private float timer;
        private bool isComplete;

        public ActiveEffect(EffectDefinition definition, Level level, Vector2 position, Dictionary<string, object> parameters)
        {
            Definition = definition;
            Level = level;
            Position = position;
            Parameters = parameters;
        }

        public void Start()
        {
            try
            {
                Definition.Action?.Invoke(Level, Position, Parameters);
            }
            catch (Exception ex)
            {
                IngesteLogger.Info($"Error starting effect '{Definition.Name}': {ex.Message}");
                isComplete = true;
            }
        }

        public bool Update()
        {
            if (isComplete) return true;
            
            timer += Engine.DeltaTime;
            
            if (timer >= Definition.Duration)
            {
                isComplete = true;
                return true;
            }
            
            return false;
        }

        public void Stop()
        {
            isComplete = true;
        }
    }

    /// <summary>
    /// Utility class for creating effect parameter dictionaries
    /// </summary>
    public static class EffectParams
    {
        public static Dictionary<string, object> Create()
        {
            return new Dictionary<string, object>();
        }

        public static Dictionary<string, object> WithRadius(float radius)
        {
            return new Dictionary<string, object> { { "radius", radius } };
        }

        public static Dictionary<string, object> WithDirection(Vector2 direction)
        {
            return new Dictionary<string, object> { { "direction", direction } };
        }

        public static Dictionary<string, object> WithEnd(Vector2 end)
        {
            return new Dictionary<string, object> { { "end", end } };
        }

        public static Dictionary<string, object> WithDuration(float duration)
        {
            return new Dictionary<string, object> { { "duration", duration } };
        }

        public static Dictionary<string, object> WithIntensity(float intensity)
        {
            return new Dictionary<string, object> { { "intensity", intensity } };
        }

        public static Dictionary<string, object> WithCount(int count)
        {
            return new Dictionary<string, object> { { "particleCount", count } };
        }
    }
}



