using System;
using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities.SoulBoosts
{
    /// <summary>
    /// Base class for soul boost entities that combines FlingBird's waiting behavior
    /// with BadelineBoost's launch mechanics. Each soul type provides unique abilities.
    /// </summary>
    public abstract class SoulBoostBase : Entity
    {
        // Soul type enumeration
        public enum SoulType
        {
            Determination = 0,  // Red
            Patience = 1,       // Cyan
            Bravery = 2,        // Orange
            Integrity = 3,      // Blue
            Perseverance = 4,   // Purple
            Kindness = 5,       // Green
            Justice = 6         // Yellow
        }

        // Soul colors from Undertale
        public static readonly Color[] SoulColors = new Color[]
        {
            Calc.HexToColor("ff0000"), // Red - Determination
            Calc.HexToColor("00ffff"), // Cyan - Patience
            Calc.HexToColor("ff8000"), // Orange - Bravery
            Calc.HexToColor("0000ff"), // Blue - Integrity
            Calc.HexToColor("ff00ff"), // Purple - Perseverance
            Calc.HexToColor("00ff00"), // Green - Kindness
            Calc.HexToColor("ffff00")  // Yellow - Justice
        };

        // Particle types
        protected ParticleType P_Ambience;
        protected ParticleType P_Move;
        protected ParticleType P_Burst;

        // Components
        protected Sprite sprite;
        protected Image stretch;
        protected Image soulImage;
        protected Wiggler wiggler;
        protected VertexLight light;
        protected BloomPoint bloom;
        protected SoundSource relocateSfx;
        protected readonly TimeRateModifier timeRateModifier;

        // Movement and state (FlingBird-style)
        protected Vector2[] nodes;
        protected int nodeIndex;
        protected bool travelling;
        protected States state;
        protected Vector2 spriteOffset = new Vector2(0f, 8f);

        // Boost mechanics (BadelineBoost-style)
        protected Player holding;
        protected bool canSkip;
        protected bool oneUse;
        protected float boostSpeed;
        protected float soulRotation;

        // Soul-specific properties
        public abstract SoulType Soul { get; }
        public Color SoulColor => SoulColors[(int)Soul];
        public abstract string SoulName { get; }
        protected abstract float AbilityDuration { get; }

        protected enum States
        {
            Wait,
            Fling,
            Move,
            Leaving
        }

        protected SoulBoostBase(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f
        ) : base(nodes[0])
        {
            Depth = -1000000;
            this.nodes = nodes;
            this.canSkip = canSkip;
            this.oneUse = oneUse;
            this.boostSpeed = boostSpeed;

            Collider = new Circle(16f);
            Add(new PlayerCollider(OnPlayer));

            // Create particles
            CreateParticles();

            // Sprite setup - use custom vessel soul sprite
            Add(sprite = new Sprite(GFX.Game, "objects/MaggyHelper/sevensoulboost/"));
            sprite.AddLoop("vessel_soul00", "soul", 0.08f);
            sprite.Play("vessel_soul00");
            sprite.CenterOrigin();
            sprite.Scale.X = -1f;
            sprite.Position = spriteOffset;
            sprite.Color = SoulColor;

            // Stretch image for travel
            Add(stretch = new Image(GFX.Game["objects/badelineboost/stretch"]));
            stretch.Visible = false;
            stretch.CenterOrigin();
            stretch.Color = SoulColor;

            // Lighting
            Add(light = new VertexLight(SoulColor, 0.8f, 16, 32));
            Add(bloom = new BloomPoint(0.6f, 16f));
            Add(timeRateModifier = new TimeRateModifier(1f, false));

            // Wiggler for pulse
            Add(wiggler = Wiggler.Create(0.4f, 3f, f =>
            {
                sprite.Scale = new Vector2(sprite.Scale.X < 0 ? -1f : 1f, 1f) * (1f + wiggler.Value * 0.4f);
            }));

            Add(relocateSfx = new SoundSource());

            state = States.Wait;
        }

        protected virtual void CreateParticles()
        {
            P_Ambience = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = SoulColor,
                Color2 = Color.Lerp(SoulColor, Color.White, 0.5f),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                Size = 1f,
                SpeedMin = 4f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f
            };

            P_Move = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = SoulColor,
                Color2 = Color.Lerp(SoulColor, Color.White, 0.3f),
                ColorMode = ParticleType.ColorModes.Blink,
                DirectionRange = 0.6981317f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                SpeedMultiplier = 0.2f,
                LifeMin = 0.6f,
                LifeMax = 1.2f
            };

            P_Burst = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = SoulColor,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                Size = 1.2f,
                SpeedMin = 30f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI * 2f
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Try to load soul-specific sprite
            try
            {
                int soulIndex = (int)Soul;
                soulImage = new Image(GFX.Game[$"objects/MaggyHelper/sevensoulboost/soul/vessel_soul00{soulIndex}"]);
                soulImage.CenterOrigin();
                Add(soulImage);
            }
            catch
            {
                // Fallback: no soul image
            }
        }

        public override void Update()
        {
            base.Update();

            // Ambient particles
            if (sprite.Visible && Scene.OnInterval(0.08f))
            {
                (Scene as Level)?.ParticlesBG.Emit(P_Ambience, 1, Center, Vector2.One * 8f);
            }

            // Rotate soul image
            if (soulImage != null && !travelling)
            {
                soulRotation += Engine.DeltaTime * 3f;
                soulImage.Position = spriteOffset + Calc.AngleToVector(soulRotation, 6f);
            }

            // State machine (FlingBird-style)
            switch (state)
            {
                case States.Wait:
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        // Skip if player passes by
                        if (canSkip && entity.X - X >= 100f)
                        {
                            Skip();
                            break;
                        }

                        // Hover toward player
                        float distance = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0f);
                        sprite.Position = Calc.Approach(sprite.Position, spriteOffset + (entity.Center - Position).SafeNormalize() * distance, 32f * Engine.DeltaTime);
                    }
                    break;

                case States.Fling:
                    // Handled in coroutine
                    break;

                case States.Move:
                    // Handled in coroutine
                    break;
            }

            // Update holding player
            if (holding != null)
            {
                holding.Speed = Vector2.Zero;
            }
        }

        protected void OnPlayer(Player player)
        {
            if (state != States.Wait || !CanBoostPlayer(player))
                return;

            state = States.Fling;
            Add(new Coroutine(BoostRoutine(player)));
        }

        protected virtual bool CanBoostPlayer(Player player)
        {
            return true;
        }

        protected virtual IEnumerator BoostRoutine(Player player)
        {
            holding = player;
            travelling = true;
            nodeIndex++;
            sprite.Visible = false;
            if (soulImage != null) soulImage.Visible = false;
            Collidable = false;

            Level level = Scene as Level;
            bool finalBoost = nodeIndex >= nodes.Length;

            // Play boost sound
            Audio.Play("event:/char/badeline/booster_begin", Position);

            // Drop held items
            if (player.Holding != null)
                player.Drop();

            // Set player state
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            player.Speed = Vector2.Zero;

            // Refill
            player.RefillDash();
            player.RefillStamina();

            // Determine facing
            int facing = Math.Sign(player.X - X);
            if (facing == 0) facing = -1;
            player.Facing = (Facings)(-facing);

            // Move player to boost position
            Vector2 playerFrom = player.Position;
            Vector2 playerTo = Position + new Vector2(facing * 4, -3f);

            for (float t = 0f; t < 0.2f; t += Engine.DeltaTime)
            {
                float progress = t / 0.2f;
                Vector2 newPos = Vector2.Lerp(playerFrom, playerTo, progress);
                player.MoveToX(newPos.X);
                player.MoveToY(newPos.Y);
                yield return null;
            }

            // Gather animation
            yield return 0.1f;

            // Apply soul ability start
            yield return ApplyAbilityStart(player);

            // Throw animation
            Audio.Play("event:/char/badeline/booster_throw", Position);
            yield return 0.1f;

            if (!player.Dead)
                player.MoveV(5f);

            yield return 0.1f;

            // Shake and effects
            level?.Shake();
            level?.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
            level?.ParticlesFG.Emit(P_Burst, 16, Center, Vector2.One * 8f);

            holding = null;

            if (!finalBoost)
            {
                // Launch player
                player.BadelineBoostLaunch(CenterX);

                // Move to next node
                Vector2 from = Position;
                Vector2 to = nodes[nodeIndex];
                float duration = Math.Min(3f, Vector2.Distance(from, to) / boostSpeed);

                stretch.Visible = true;
                stretch.Rotation = (to - from).Angle();

                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
                tween.OnUpdate = t =>
                {
                    Position = Vector2.Lerp(from, to, t.Eased);
                    stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
                    stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;

                    if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
                    {
                        TrailManager.Add(this, SoulColor, 0.5f);
                        level?.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
                    }
                };

                tween.OnComplete = t =>
                {
                    if (X >= level.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                    else
                    {
                        travelling = false;
                        stretch.Visible = false;
                        sprite.Visible = true;
                        if (soulImage != null) soulImage.Visible = true;
                        Collidable = true;
                        state = States.Wait;
                        Audio.Play("event:/char/badeline/booster_reappear", Position);
                    }
                };

                Add(tween);
                relocateSfx.Play("event:/char/badeline/booster_relocate");
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level?.DirectionalShake(-Vector2.UnitY);
            }
            else
            {
                // Final boost
                Audio.Play("event:/char/badeline/booster_final", Position);
                Engine.FreezeTimer = 0.1f;
                yield return null;

                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level?.Flash(Color.White * 0.5f, true);
                level?.DirectionalShake(-Vector2.UnitY, 0.6f);
                level?.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);

                // Apply final ability
                yield return ApplyAbilityEnd(player);

                player.SummitLaunch(X);
                timeRateModifier.ResetTimeRateMultiplier();

                Finish();
            }
        }

        protected void Skip()
        {
            if (nodeIndex + 1 >= nodes.Length)
                return;

            travelling = true;
            nodeIndex++;
            Collidable = false;

            Level level = SceneAs<Level>();
            Vector2 from = Position;
            Vector2 to = nodes[nodeIndex];
            float duration = Math.Min(3f, Vector2.Distance(from, to) / boostSpeed);

            stretch.Visible = true;
            stretch.Rotation = (to - from).Angle();

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween.OnUpdate = t =>
            {
                Position = Vector2.Lerp(from, to, t.Eased);
                stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
                stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;

                if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
                {
                    TrailManager.Add(this, SoulColor, 0.5f);
                    level?.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
                }
            };

            tween.OnComplete = t =>
            {
                if (X >= level.Bounds.Right)
                {
                    RemoveSelf();
                }
                else
                {
                    travelling = false;
                    stretch.Visible = false;
                    sprite.Visible = true;
                    if (soulImage != null) soulImage.Visible = true;
                    Collidable = true;
                    state = States.Wait;
                    Audio.Play("event:/char/badeline/booster_reappear", Position);
                }
            };

            Add(tween);
            relocateSfx.Play("event:/char/badeline/booster_relocate");
            level?.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
        }

        protected void Finish()
        {
            SceneAs<Level>()?.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            SceneAs<Level>()?.Particles.Emit(P_Burst, 12, Center, Vector2.One * 6f);
            RemoveSelf();
        }

        // Override these in derived classes for soul-specific abilities
        protected virtual IEnumerator ApplyAbilityStart(Player player) { yield break; }
        protected virtual IEnumerator ApplyAbilityEnd(Player player) { yield break; }

        public override void Render()
        {
            base.Render();

            // Glow effect
            if (!travelling && sprite.Visible)
            {
                Draw.Circle(Center, 12f, SoulColor * 0.2f, 12);
                Draw.Circle(Center, 8f, SoulColor * 0.3f, 8);
            }
        }
    }
}
