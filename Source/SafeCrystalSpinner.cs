using System;
using System.Collections.Generic;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// A safe crystal spinner that doesn't conflict with IsaGrabBag's DreamSpinnerRenderer.
    /// Use this instead of CrystalStaticSpinner when you need spinners that won't cause 
    /// null reference crashes during level transitions or cutscenes.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/SafeCrystalSpinner")]
    [Tracked]
    public class SafeCrystalSpinner : Entity
    {
        public static ParticleType P_Move;
        
        private const float MinFlingSpeed = 220f;
        private const float MinFlingSpeedSq = MinFlingSpeed * MinFlingSpeed;
        
        private Sprite sprite;
        private bool canHurt = true;
        private float offset;
        private bool expanded;
        private int randomSeed;
        private CrystalColor color;

        public enum CrystalColor
        {
            Blue,
            Red,
            Purple,
            Rainbow
        }

        static SafeCrystalSpinner()
        {
            P_Move = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = Calc.HexToColor("5FCDE4"),
                FadeMode = ParticleType.FadeModes.Late,
                Size = 0.5f,
                Acceleration = new Vector2(0f, 4f),
                Direction = -(float)Math.PI / 2f,
                DirectionRange = 0.5f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.2f,
                LifeMin = 0.3f,
                LifeMax = 0.5f
            };
        }

        public SafeCrystalSpinner(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("attachToSolid"), data.Enum("color", CrystalColor.Blue))
        {
        }

        public SafeCrystalSpinner(Vector2 position, bool attachToSolid, CrystalColor color)
            : base(position)
        {
            this.color = color;
            Tag = Tags.TransitionUpdate;
            Collider = new ColliderList(
                new Circle(6f, 0f, 0f),
                new Hitbox(16f, 4f, -8f, -3f)
            );
            Visible = false;
            Add(new PlayerCollider(player => OnPlayer((Celeste.Player)player)));
            Add(new HoldableCollider(h => OnHoldable((Celeste.Holdable)h)));
            Add(new LedgeBlocker());
            Depth = -8500;
            
            offset = Calc.Random.NextFloat();
            randomSeed = Calc.Random.Next();
            
            if (attachToSolid)
            {
                Add(new StaticMover
                {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = base.RemoveSelf
                });
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            // Create sprite based on color
            string spritePath = color switch
            {
                CrystalColor.Red => "danger/crystal/fg_red",
                CrystalColor.Purple => "danger/crystal/fg_purple",
                CrystalColor.Rainbow => "danger/crystal/fg_rainbow",
                _ => "danger/crystal/fg_blue"
            };
            
            // Use random seed for consistent appearance
            Calc.PushRandom(randomSeed);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(spritePath);
            MTexture texture = Calc.Random.Choose(atlasSubtextures);
            Calc.PopRandom();
            
            sprite = new Sprite(GFX.Game, spritePath);
            sprite.CenterOrigin();
            sprite.Color = GetColorForType(color);
            
            // Use image instead of complex sprite for simplicity
            Image image = new Image(texture);
            image.CenterOrigin();
            image.Color = GetColorForType(color);
            Add(image);
            
            Visible = true;
            expanded = true;
        }

        private Color GetColorForType(CrystalColor type)
        {
            return type switch
            {
                CrystalColor.Red => Calc.HexToColor("FF4040"),
                CrystalColor.Purple => Calc.HexToColor("9040FF"),
                CrystalColor.Rainbow => GetRainbowColor(),
                _ => Color.White
            };
        }

        private Color GetRainbowColor()
        {
            float hue = (Scene?.TimeActive ?? 0f) * 0.5f % 1f;
            return Calc.HsvToColor(hue, 0.8f, 1f);
        }

        public override void Update()
        {
            base.Update();
            
            // Update rainbow color if applicable
            if (color == CrystalColor.Rainbow)
            {
                foreach (Component component in Components)
                {
                    if (component is Image img)
                    {
                        img.Color = GetRainbowColor();
                    }
                }
            }
            
            // Sway animation
            if (Scene.OnInterval(0.25f, offset) && expanded)
            {
                Position += new Vector2(Calc.Random.Range(-0.5f, 0.5f), Calc.Random.Range(-0.5f, 0.5f));
            }
        }

        private void OnShake(Vector2 amount)
        {
            Position += amount;
        }

        private bool IsRiding(Solid solid)
        {
            return CollideCheck(solid);
        }

        private void OnPlayer(Celeste.Player player)
        {
            if (canHurt && player != null && !player.Dead)
            {
                if (player.StateMachine.State == Celeste.Player.StDash || 
                    player.Speed.LengthSquared() >= MinFlingSpeedSq)
                {
                    // Player is dashing or moving fast - don't hurt
                    return;
                }
                
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        private void OnHoldable(Holdable h)
        {
            h.HitSpinner(this);
        }

        /// <summary>
        /// Force this spinner to be visible and active immediately.
        /// Safe alternative to CrystalStaticSpinner.ForceInstantiate() that won't crash IsaGrabBag.
        /// </summary>
        public void ForceInstantiate()
        {
            Visible = true;
            expanded = true;
            canHurt = true;
        }

        /// <summary>
        /// Safely force instantiate all SafeCrystalSpinners in a level.
        /// </summary>
        public static void ForceInstantiateAll(Level level)
        {
            if (level?.Tracker == null) return;
            
            foreach (SafeCrystalSpinner spinner in level.Tracker.GetEntities<SafeCrystalSpinner>())
            {
                spinner?.ForceInstantiate();
            }
        }

        /// <summary>
        /// Safely try to instantiate vanilla CrystalStaticSpinners with error handling.
        /// Returns true if successful, false if there was an error.
        /// </summary>
        public static bool TrySafeInstantiateVanillaSpinners(Level level)
        {
            if (level?.Tracker == null) return false;
            
            try
            {
                foreach (CrystalStaticSpinner spinner in level.Tracker.GetEntities<CrystalStaticSpinner>())
                {
                    spinner?.ForceInstantiate();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/SafeCrystalSpinner", 
                    $"Failed to instantiate vanilla spinners (likely IsaGrabBag conflict): {ex.Message}");
                return false;
            }
        }
    }
}
