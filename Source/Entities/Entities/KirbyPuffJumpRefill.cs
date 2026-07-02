using Celeste;
using Celeste.Entities;
using Celeste.Mod;
using DZ;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Entities
{
    [CustomEntity(
        "DZ/KirbyPuffJumpRefill = KirbyPuffJumpRefill",
        "DZ/KirbyPuffRefill = KirbyPuffJumpRefill")]
    [Tracked]
    [HotReloadable]
    public class KirbyPuffJumpRefill : Actor
    {
        public static ParticleType P_Shatter;
        public static ParticleType P_Regen;
        public static ParticleType P_Glow;

        private Sprite sprite;
        private Sprite flash;
        private Image outline;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private bool oneUse;
        private int puffCount;
        private float respawnTime;
        private bool breakEvenWhenFull;
        private float respawnTimer;
        private string spriteVariant; // "single" or "multi"

        public KirbyPuffJumpRefill(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            puffCount = data.Int("puffCount", 3);
            oneUse = data.Bool("oneUse", false);
            respawnTime = data.Float("respawnTime", 2.5f);
            breakEvenWhenFull = data.Bool("breakEvenWhenFull", false);
            spriteVariant = data.Attr("spriteVariant", "auto"); // "auto", "single", or "multi"

            // Load particles
            LoadParticles();

            // Determine sprite variant based on puff count if auto
            if (spriteVariant == "auto")
            {
                spriteVariant = puffCount > 2 ? "multi" : "single";
            }

            // Create sprites based on variant
            string spriteName = spriteVariant == "multi" ? "puffrefillmulti" : "puffrefill";
            string flashName = spriteVariant == "multi" ? "puffrefillmultiFlash" : "puffrefillFlash";
            string outlinePath = $"objects/DZ/DZ/{spriteName}/outline";

            sprite = CreateAndAddSprite(spriteName, "refill", true);
            flash = CreateAndAddSprite(flashName, "refillFlash", false, false);
            outline = CreateAndAddOutline(outlinePath, "objects/DZ/DZ/refill/outline", false);

            // Add wiggler animation
            Add(wiggler = Wiggler.Create(1f, 4f, v =>
            {
                Vector2 scale = Vector2.One * (1f + v * 0.2f);
                if (sprite != null) sprite.Scale = scale;
                if (flash != null) flash.Scale = scale;
                if (outline != null) outline.Scale = scale;
            }));

            // Add lighting and bloom
            Add(light = new VertexLight(Color.HotPink, 1f, 16, 32));
            Add(bloom = new BloomPoint(0.8f, 16f));

            // Wire collision
            Add(new PlayerCollider(OnPlayer));
        }

        private Sprite CreateAndAddSprite(string id, string fallbackId, bool playIdle, bool visible = true)
        {
            Sprite s = null;
            try
            {
                s = GFX.SpriteBank.Create(id);
            }
            catch
            {
                try
                {
                    s = GFX.SpriteBank.Create(fallbackId);
                }
                catch
                {
                    Logger.Log(LogLevel.Warn, "KirbyPuffJumpRefill", $"Failed to create sprite '{id}'");
                }
            }

            if (s != null)
            {
                if (playIdle && s.Has("idle"))
                {
                    try { s.Play("idle"); } catch { }
                }
                s.Visible = visible;
                Add(s);
            }
            return s;
        }

        private Image CreateAndAddOutline(string path, string fallbackPath, bool visible = false)
        {
            MTexture texture = AtlasPathHelper.TryGetTexture(path) ?? AtlasPathHelper.TryGetTexture(fallbackPath);
            if (texture != null)
            {
                Image image = new Image(texture);
                image.Visible = visible;
                Add(image);
                return image;
            }
            return null;
        }

        private void OnPlayer(Player player)
        {
            // Get K_Player from scene (K_Player is a separate entity from Player)
            K_Player kPlayer = Scene?.Tracker.GetEntity<K_Player>();
            if (kPlayer == null)
                return;

            // Check if we can refill puff jumps
            if (RefillPuffJumps(kPlayer) || breakEvenWhenFull)
            {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;

                Add(new Coroutine(RefillRoutine()));
            }
        }

        /// <summary>
        /// Refills the Kirby player's puff jump count.
        /// Returns true if the refill was applied.
        /// </summary>
        private bool RefillPuffJumps(K_Player kPlayer)
        {
            // Get the max flap count (typically 5 by default)
            int maxFlaps = GetMaxPuffJumps(kPlayer);

            // Get current flap count using reflection
            int currentFlaps = GetCurrentPuffJumps(kPlayer);

            // Only refill if player doesn't have max puff jumps
            if (currentFlaps >= maxFlaps)
                return false;

            // Set to max puff jumps
            SetPuffJumps(kPlayer, maxFlaps);
            return true;
        }

        /// <summary>
        /// Gets the current puff jump count from K_Player.
        /// </summary>
        private int GetCurrentPuffJumps(K_Player kPlayer)
        {
            return kPlayer.KirbyFlapCount;
        }

        /// <summary>
        /// Sets the puff jump count on K_Player.
        /// </summary>
        private void SetPuffJumps(K_Player kPlayer, int count)
        {
            kPlayer.SetKirbyFlapCount(count);
        }

        /// <summary>
        /// Gets the max allowed puff jumps for the player (configurable via DZ settings).
        /// </summary>
        private int GetMaxPuffJumps(K_Player kPlayer)
        {
            // Try to get from DZ settings first
            try
            {
                var settings = DZModule.Settings;
                if (settings != null && settings.KirbyMaxFloatJumps > 0)
                {
                    return settings.KirbyMaxFloatJumps;
                }
            }
            catch { }

            // Fall back to custom puffCount or default
            return puffCount > 0 ? puffCount : 5;
        }

        private IEnumerator RefillRoutine()
        {
            // Flash and particle effects
            if (flash != null && flash.Has("flash"))
            {
                try { flash.Play("flash"); } catch { }
                flash.Visible = true;
            }

            wiggler.Start();

            if (sprite != null)
                sprite.Visible = false;

            if (outline != null)
                outline.Visible = true;

            // Emit particles
            var level = Scene as Level;
            if (P_Glow != null)
                level?.ParticlesFG?.Emit(P_Glow, 9, Position, Vector2.One * 6f);

            yield return 0.05f;

            if (P_Shatter != null)
            {
                level?.ParticlesFG?.Emit(P_Shatter, 5, Position, Vector2.One * 4f);
                level?.ParticlesFG?.Emit(P_Shatter, 5, Position, Vector2.One * 4f);
            }

            if (flash != null)
                flash.Visible = false;

            // Respawn logic for non-one-use refills
            if (!oneUse)
            {
                yield return respawnTime;

                // Regeneration sound and particles
                Audio.Play("event:/game/general/refill_return", Position);

                if (P_Regen != null)
                    level?.ParticlesFG?.Emit(P_Regen, 16, Position, Vector2.One * 2f);

                if (outline != null)
                    outline.Visible = false;

                if (sprite != null)
                {
                    sprite.Visible = true;
                    if (sprite.Has("spin"))
                    {
                        try { sprite.Play("spin"); } catch { }
                    }
                }

                Collidable = true;
                wiggler.Start();
            }
            else
            {
                RemoveSelf();
            }
        }

        public static void LoadParticles()
        {
            // Check if particle texture exists, fallback to default if needed
            MTexture particleTexture = GFX.Game["particles/refill"] ?? GFX.Game["particles/shard"];

            P_Shatter = new ParticleType
            {
                Source = particleTexture,
                Color = Calc.HexToColor("FF69B4"),  // Hot pink for puff jumps
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
                Color = Calc.HexToColor("FF69B4"),
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
                Color = Calc.HexToColor("FF69B4"),
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
