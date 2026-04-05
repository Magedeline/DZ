using MonoMod.Utils;

namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/AdvancedRefill")]
    [Monocle.Tracked]
    [HotReloadable]
    public class AdvancedRefill : Actor
    {
        public static ParticleType P_Shatter;
        public static ParticleType P_Regen;
        public static ParticleType P_Glow;

        private Monocle.Sprite sprite;
        private Monocle.Sprite flash;
        private Image outline;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private bool oneUse;
        private bool respawnTimer;
        private int dashCount;
        private bool respectInventoryLimits;

        public AdvancedRefill(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Monocle.Hitbox(16f, 16f, -8f, -8f);
            dashCount = data.Int("dashCount", 1);
            oneUse = data.Bool("oneUse", false);
            respectInventoryLimits = data.Bool("respectInventoryLimits", true);

            // Determine sprite based on dash count
            string spriteName = GetSpriteNameForDashCount(dashCount);
            string flashSpriteName = GetFlashSpriteNameForDashCount(dashCount);

            // Create main sprite with fallback
            try
            {
                sprite = GFX.SpriteBank.Create(spriteName);
                Add(sprite);
            }
            catch (Exception)
            {
                // Fallback to the base refill sprite if the requested sprite is missing
                try
                {
                    sprite = GFX.SpriteBank.Create("refill");
                    Add(sprite);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "AdvancedRefill", $"Failed to create refill sprite '{spriteName}': {ex.Message}");
                }
            }

            // Create flash sprite with fallback
            try
            {
                flash = GFX.SpriteBank.Create(flashSpriteName);
                Add(flash);
            }
            catch (Exception)
            {
                // Fallback to regular refill flash
                try
                {
                    flash = GFX.SpriteBank.Create("refillFlash");
                    Add(flash);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "AdvancedRefill", $"Failed to create flash sprite '{flashSpriteName}': {ex.Message}");
                }
            }

            // Outline texture with fallback
            var outlineTexture = AtlasPathHelper.TryGetTexture(GetOutlineTextureForDashCount(dashCount))
                ?? AtlasPathHelper.TryGetTexture("objects/refill/outline");
            
            if (outlineTexture != null)
            {
                Add(outline = new Image(outlineTexture));
                outline.Visible = false;
            }

            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                if (sprite != null)
                    sprite.Scale = Vector2.One * (1f + v * 0.2f);
                if (flash != null)
                    flash.Scale = Vector2.One * (1f + v * 0.2f);
                if (outline != null)
                    outline.Scale = Vector2.One * (1f + v * 0.2f);
            }));

            Add(new PlayerCollider(OnPlayer));
            Add(light = new VertexLight(GetColorForDashCount(dashCount), 1f, 16, 32));
            Add(bloom = new BloomPoint(0.8f, 16f));
        }

        private static string GetFlashSpriteNameForDashCount(int dashCount)
        {
            return dashCount switch
            {
                2 => "refillTwoFlash",
                3 => "solarrefillFlash",
                4 => "lunarrefillFlash",
                5 => "blackholerefillFlash",
                >= 10 => "savestarrefillFlash",
                _ => "refillFlash"
            };
        }

        private static string GetSpriteNameForDashCount(int dashCount)
        {
            return dashCount switch
            {
                2 => "refillTwo",
                3 => "solarrefill",
                4 => "lunarrefill",
                5 => "blackholerefill",
                >= 10 => "savestarrefill",
                _ => "refill"
            };
        }

        private static string GetOutlineTextureForDashCount(int dashCount)
        {
            return dashCount switch
            {
                2 => "objects/refillTwo/outline",
                3 => "objects/solarrefill/outline",
                4 => "objects/lunarrefill/outline",
                5 => "objects/blackholerefill/outline",
                >= 10 => "objects/savestarrefill/outline",
                _ => "objects/refill/outline"
            };
        }

        private static Color GetColorForDashCount(int dashCount)
        {
            return dashCount switch
            {
                2 => Color.Pink,
                3 => Color.Orange,
                4 => Color.LightBlue,
                5 => Color.Purple,
                >= 10 => Color.Gold,
                _ => Color.White
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            respawnTimer = false;
        }

        public override void Update()
        {
            base.Update();

            if (respawnTimer)
            {
                // Only set animation frames if the animations exist on the sprites
                if (flash != null && flash.Has("flash"))
                {
                    try { flash.SetAnimationFrame(0); } catch { }
                }
                if (sprite != null && sprite.CurrentAnimationID != null)
                {
                    try { sprite.SetAnimationFrame(0); } catch { }
                }

                if (sprite == null || sprite.CurrentAnimationID != "spin")
                {
                    respawnTimer = false;
                }
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            var level = Scene as Level;
            global::Celeste.PlayerInventory? inventory = level?.Session.Inventory;

            // Check if this refill should work with current inventory
            if (respectInventoryLimits && inventory.HasValue)
            {
                // Don't refill if NoRefills is active and player is on the ground
                if (inventory.Value.NoRefills && player.OnGround())
                {
                    return;
                }
            }

            // Determine how many dashes to give
            int dashesToGive = CalculateDashesToGive(player, inventory);

            if (player.Dashes >= dashesToGive)
                return; // Player already has enough dashes

            // Apply the refill using LessDasheline-compatible method for extended dashes (3-10)
            SetPlayerDashes(player, dashesToGive);

            // Play appropriate sound
            Audio.Play(GetSoundForDashCount(dashCount), Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;

            Add(new Coroutine(RefillRoutine(player)));
        }

        /// <summary>
        /// Sets the player's dashes using LessDasheline-compatible DynData method.
        /// This properly handles extended dash counts (3-10) for compatibility with LessDasheline/MoreDasheline mods.
        /// </summary>
        private static void SetPlayerDashes(global::Celeste.Player player, int dashes)
        {
            // Store the previous dash count for LessDasheline compatibility
            int previousDashes = player.Dashes;
            
            // Use DynData to set LessDasheline tracking fields
            using (var dynData = new DynData<global::Celeste.Player>(player))
            {
                // Set the dash count
                player.Dashes = dashes;
                
                // For extended dashes (3-10), set LessDasheline-compatible fields
                if (dashes >= 3)
                {
                    // Set recharge tracking fields for smooth color transitions
                    dynData.Set("LessDasheline/rechargeAt", previousDashes);
                    dynData.Set("LessDasheline/rechargeInto", dashes);
                    dynData.Set("LessDasheline/rechargeTimer", 0.12f);
                    
                    // Also set startDashCount for trail color compatibility
                    dynData.Set("LessDasheline/startDashCount", dashes);
                }
            }
        }

        private int CalculateDashesToGive(global::Celeste.Player player, global::Celeste.PlayerInventory? inventory)
        {
            // For extended dashes (3+), always give the full dash count - don't limit by MaxDashes
            // This is needed for LessDasheline/MoreDasheline compatibility
            if (dashCount >= 3)
            {
                return dashCount;
            }

            if (inventory.HasValue && respectInventoryLimits)
            {
                // For Heart mode (NoRefills), limit dashes appropriately
                if (inventory.Value.NoRefills)
                {
                    // In Heart mode, typically limited to 1 dash
                    int maxDashes = 1;
                    if (dashCount == -1)
                    {
                        return maxDashes;
                    }
                    return Math.Min(dashCount, maxDashes);
                }
                
                // For standard inventories with limits, use the configured dash count capped by MaxDashes
                if (dashCount == -1) // Special value for "max dashes"
                {
                    return player.MaxDashes;
                }
                return Math.Min(dashCount, player.MaxDashes);
            }

            // No inventory limits or no inventory - use the configured dash count directly
            return dashCount == -1 ? player.MaxDashes : dashCount;
        }

        private static string GetSoundForDashCount(int dashCount)
        {
            return dashCount switch
            {
                2 => "event:/game/general/refill_two_get",
                3 => "event:/desolozantas/game/general/reddiamond_touch",
                4 => "event:/desolozantas/game/general/cyandiamond_touch",
                5 => "event:/desolozantas/final_content/game/19_the_end/gigadiamond_touch",
                >= 10 => "event:/desolozantas/final_content/game/20_last_push/savediamond_touch",
                _ => "event:/game/general/refill_get"
            };
        }

        private IEnumerator RefillRoutine(global::Celeste.Player player)
        {
            // Ensure particles are loaded
            if (P_Shatter == null || P_Regen == null || P_Glow == null)
            {
                LoadParticles();
            }

            // Particle effects with color based on dash count
            var level = Scene as Level;
            Color particleColor = GetColorForDashCount(dashCount);

            if (P_Shatter != null)
                level?.ParticlesFG?.Emit(P_Shatter, 5, Position, Vector2.One * 4f);
            
            if (P_Glow != null)
                level?.ParticlesFG?.Emit(P_Glow, 9, Position, Vector2.One * 6f);

            // Flash and wiggle - with null checks
            if (flash != null && flash.Has("flash"))
            {
                flash.Play("flash");
                flash.Visible = true;
            }
            
            wiggler.Start();
            
            if (sprite != null)
                sprite.Visible = false;
            
            if (outline != null)
                outline.Visible = true;

            yield return 0.05f;

            if (P_Shatter != null)
            {
                level?.ParticlesFG?.Emit(P_Shatter, 5, Position, Vector2.One * 4f);
                level?.ParticlesFG?.Emit(P_Shatter, 5, Position, Vector2.One * 4f);
            }

            if (flash != null)
                flash.Visible = false;

            if (!oneUse)
            {
                yield return 2.5f;

                // Regeneration
                Audio.Play("event:/game/general/refill_return", Position);
                
                if (P_Regen != null)
                    level?.ParticlesFG?.Emit(P_Regen, 16, Position, Vector2.One * 2f);

                if (outline != null)
                    outline.Visible = false;
                
                if (sprite != null)
                {
                    sprite.Visible = true;
                    
                    // Play spin animation if available
                    if (sprite.Has("spin"))
                    {
                        try { sprite.Play("spin"); } catch { }
                    }
                }
                
                Collidable = true;
                wiggler.Start();
                respawnTimer = true;
            }
            else
            {
                RemoveSelf();
            }
        }

        public static void LoadParticles()
        {
            // Check if particle texture exists
            MTexture particleTexture = GFX.Game["particles/refill"];
            if (particleTexture == null)
            {
                Logger.Log(LogLevel.Warn, "AdvancedRefill", "particles/refill texture not found, using default particle");
                particleTexture = GFX.Game["particles/shard"]; // Fallback to a default particle
            }

            P_Shatter = new ParticleType
            {
                Source = particleTexture,
                Color = Calc.HexToColor("5FCDE4"),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Static,
                RotationMode = ParticleType.RotationModes.SameAsDirection,
                Size = 1f,
                SizeRange = 0.5f,
                LifeMin = 0.6f,
                LifeMax = 1.0f,
                SpeedMin = 40f,
                SpeedMax = 50f,
                DirectionRange = (float)Math.PI * 2f,
                FadeMode = ParticleType.FadeModes.Late
            };

            P_Regen = new ParticleType
            {
                Color = Calc.HexToColor("5FCDE4"),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                Size = 1f,
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                SpeedMin = 8f,
                SpeedMax = 16f,
                DirectionRange = (float)Math.PI * 2f
            };

            P_Glow = new ParticleType
            {
                Color = Calc.HexToColor("5FCDE4"),
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                Size = 1f,
                LifeMin = 0.4f,
                LifeMax = 0.6f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f
            };
        }
    }
}



