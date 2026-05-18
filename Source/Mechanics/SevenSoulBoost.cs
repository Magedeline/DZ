using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

#pragma warning disable CS0618 // Engine.TimeRate is obsolete but needed for vanilla behavior

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Seven Soul Boost - Combines all seven soul abilities
    /// Red (Determination), Orange (Bravery), Yellow (Justice), 
    /// Green (Kindness), Cyan (Patience), Blue (Integrity), Purple (Perseverance)
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/SevenSoulBoost")]
    [Tracked]
    public class SevenSoulBoost : Entity
    {
        private static readonly Vector2[] DefaultNode = new Vector2[] { Vector2.Zero };

        private static Vector2[] NormalizeNodes(Vector2[] input)
        {
            return input != null && input.Length > 0 ? input : DefaultNode;
        }

        // Soul colors from Undertale
        public static readonly Color[] SoulColors = new Color[]
        {
            Calc.HexToColor("ff0000"), // Red - Determination
            Calc.HexToColor("ff8000"), // Orange - Bravery
            Calc.HexToColor("ffff00"), // Yellow - Justice
            Calc.HexToColor("00ff00"), // Green - Kindness
            Calc.HexToColor("00ffff"), // Cyan - Patience
            Calc.HexToColor("0000ff"), // Blue - Integrity
            Calc.HexToColor("ff00ff")  // Purple - Perseverance
        };

        public static readonly string[] SoulNames = new string[]
        {
            "Determination", "Bravery", "Justice", "Kindness",
            "Patience", "Integrity", "Perseverance"
        };

        // Particle types
        protected ParticleType P_Ambience;
        protected ParticleType P_Move;
        protected ParticleType P_Burst;

        // Components
        protected Sprite sprite;
        protected Image stretch;
        protected List<Image> soulImages = new List<Image>();
        protected Wiggler wiggler;
        protected VertexLight light;
        protected BloomPoint bloom;
        protected SoundSource relocateSfx;

        // Movement and state
        protected Vector2[] nodes;
        protected int nodeIndex;
        protected bool travelling;
        protected States state;
        protected Vector2 spriteOffset = new Vector2(0f, 8f);

        // Boost mechanics
        protected global::Celeste.Player holding;
        protected bool lockCamera;
        protected bool canSkip;
        protected bool oneUse;
        protected float boostSpeed;
        protected bool refillDashes;
        protected bool refillStamina;
        protected int dashCount;

        // Ch20 Final boost fields
        private bool finalCh21Boost;
        private bool finalCh21GoldenBoost;
        private string finalCh21Dialog;
        public FMOD.Studio.EventInstance Ch21FinalBoostSfx;

        // Soul orbit animation
        private float soulOrbitAngle = 0f;
        private const float SOUL_ORBIT_RADIUS = 20f;
        private const float SOUL_ORBIT_SPEED = 2f;

        protected enum States
        {
            Wait,
            Fling,
            Move,
            Leaving
        }

        public SevenSoulBoost(EntityData data, Vector2 offset)
            : this(
                data.NodesWithPosition(offset),
                data.Bool("lockCamera", true),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f),
                data.Bool("refillDashes", true),
                data.Bool("refillStamina", true),
                data.Int("dashCount", 10),
                data.Bool("finalCh21Boost"),
                data.Bool("finalCh21GoldenBoost"),
                data.Attr("finalCh21Dialog")
            )
        {
        }

        public SevenSoulBoost(
            Vector2[] nodes,
            bool lockCamera = true,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            bool refillDashes = true,
            bool refillStamina = true,
            int dashCount = 10,
            bool finalCh21Boost = false,
            bool finalCh21GoldenBoost = false,
            string finalCh21Dialog = null
        ) : base(NormalizeNodes(nodes)[0])
        {
            Depth = -1000000;
            this.nodes = NormalizeNodes(nodes);
            this.lockCamera = lockCamera;
            this.canSkip = canSkip;
            this.oneUse = oneUse;
            this.boostSpeed = boostSpeed;
            this.refillDashes = refillDashes;
            this.refillStamina = refillStamina;
            this.dashCount = dashCount;
            this.finalCh21Boost = finalCh21Boost;
            this.finalCh21GoldenBoost = finalCh21GoldenBoost;
            this.finalCh21Dialog = finalCh21Dialog;

            Collider = new Circle(20f);
            Add(new PlayerCollider(player => OnPlayer(player)));

            // Create particles
            CreateParticles();

            // Sprite setup - use custom vessel soul sprite
            Add(sprite = new Sprite(GFX.Game, "characters/soul/soul/"));
            sprite.AddLoop("madelineboost", "vessel_soulA", 0.08f);
            sprite.Play("madelineboost");
            sprite.CenterOrigin();
            sprite.Scale.X = -1f;
            sprite.Position = spriteOffset;
            sprite.Color = Color.White;

            // Stretch image for travel
            Add(stretch = new Image(GFX.Game["objects/madelinesevensoulboost/stretch"]));
            stretch.Visible = false;
            stretch.CenterOrigin();
            stretch.Color = Color.White;

            // Create seven soul images
            for (int i = 0; i < 7; i++)
            {
                try
                {
                    char soulSuffix = (char)('A' + i);
                    var soulImage = new Image(GFX.Game[$"characters/soul/soul/vessel_soul{soulSuffix}"]);
                    soulImage.CenterOrigin();
                    soulImages.Add(soulImage);
                    Add(soulImage);
                }
                catch
                {
                    // Fallback: create a simple colored circle representation
                    var soulImage = new Image(GFX.Game["particles/shard"]);
                    soulImage.CenterOrigin();
                    soulImage.Color = SoulColors[i];
                    soulImage.Scale = new Vector2(2f, 2f);
                    soulImages.Add(soulImage);
                    Add(soulImage);
                }
            }

            // Lighting - white/rainbow light
            Add(light = new VertexLight(Color.White, 1f, 24, 48));
            Add(bloom = new BloomPoint(0.8f, 24f));

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
                Color = Color.White,
                Color2 = Calc.HexToColor("ffff00"),
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
                Color = Color.White,
                Color2 = Calc.HexToColor("00ffff"),
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
                Color = Color.White,
                Color2 = Calc.HexToColor("ff00ff"),
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

        public override void Update()
        {
            base.Update();

            // Update soul orbit animation
            soulOrbitAngle += Engine.DeltaTime * SOUL_ORBIT_SPEED;

            // Position orbiting souls
            for (int i = 0; i < soulImages.Count && i < 7; i++)
            {
                float angle = soulOrbitAngle + (i / 7f) * (float)Math.PI * 2f;
                float radius = SOUL_ORBIT_RADIUS;
                
                // Add wave motion
                float waveOffset = (float)Math.Sin(soulOrbitAngle * 3f + i) * 4f;
                
                soulImages[i].Position = spriteOffset + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius + waveOffset
                );
                
                // Pulse the souls
                float pulse = (float)Math.Sin(soulOrbitAngle * 4f + i * 0.5f) * 0.2f + 1f;
                soulImages[i].Scale = Vector2.One * pulse * 0.8f;
            }

            // Ambient particles - cycle through colors
            if (sprite.Visible && Scene.OnInterval(0.08f))
            {
                int colorIndex = (int)((soulOrbitAngle * 2f) % 7f);
                P_Ambience.Color = SoulColors[colorIndex];
                P_Ambience.Color2 = SoulColors[(colorIndex + 1) % 7];
                (Scene as Level)?.ParticlesBG.Emit(P_Ambience, 1, Center, Vector2.One * 12f);
            }

            // Cycle light color
            int lightColorIndex = (int)((soulOrbitAngle * 1.5f) % 7f);
            light.Color = Color.Lerp(SoulColors[lightColorIndex], SoulColors[(lightColorIndex + 1) % 7], (soulOrbitAngle * 1.5f) % 1f);

            // State machine
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
            }

            // Update holding player
            if (holding != null)
            {
                holding.Speed = Vector2.Zero;
            }
        }

        protected void OnPlayer(global::Celeste.Player player)
        {
            if (state != States.Wait)
                return;

            state = States.Fling;
            Add(new Coroutine(BoostRoutine(player)));
        }

        protected virtual IEnumerator BoostRoutine(global::Celeste.Player player)
        {
        if (player == null || Scene == null)
        {
            yield break;
        }

        holding = player;
        travelling = true;
        nodeIndex++;
        sprite.Visible = false;
        sprite.Position = Vector2.Zero;
        Collidable = false;
        bool finalBoost = nodeIndex >= nodes.Length;
        Level level = Scene as Level;
            if (level == null)
            {
                state = States.Wait;
                travelling = false;
                sprite.Visible = true;
                Collidable = true;
                holding = null;
                yield break;
            }

            // Hide soul images during boost
            foreach (var soul in soulImages)
            {
                soul.Visible = false;
            }
            bool endLevel;
            if (finalBoost && finalCh21GoldenBoost)
            {
                endLevel = true;
            }
            else
            {
                bool flag = false;
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry { Golden: false })
                    {
                        flag = true;
                        break;
                    }
                }
                endLevel = finalBoost && finalCh21Boost && !flag;
            }
            Stopwatch sw = Stopwatch.StartNew();
            if (finalCh21Boost)
            {
                Audio.Play("event:/desolozantas/final_content/char/madeline/finalfinalfinalultra_part1", Position);
            }
            else if (!finalBoost)
            {
                Audio.Play("event:/desolozantas/final_content/char/madeline/ready", Position);
            }
            else
            {
                Audio.Play("event:/desolozantas/final_content/char/madeline/launch", Position);
            }
            if (player.Holding != null)
            {
                player.Holding = null;
            }
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            if (player.Inventory.Dashes > 10)
            {
                player.Dashes = 10;
            }
            else
            {
                player.RefillDash();
            }
            player.RefillStamina();
            player.Speed = Vector2.Zero;
            int num = Math.Sign(player.X - X);
            if (num == 0)
            {
                num = -1;
            }
            MadelineGodDummy madelinegod = new MadelineGodDummy(Position);
            Scene.Add(madelinegod);
            player.Facing = (Facings)(-num);
            madelinegod.Sprite.Scale.X = num;
            Vector2 playerFrom = player.Position;
            Vector2 playerTo = Position + new Vector2(num * 4, -3f);
            Vector2 madelinegodFrom = madelinegod.Position;
            Vector2 madelinegodTo = Position + new Vector2(-num * 4, 3f);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
            {
                Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
                if (player.Scene != null)
                {
                    player.MoveToX(vector.X);
                }
                if (player.Scene != null)
                {
                    player.MoveToY(vector.Y);
                }
                madelinegod.Position = Vector2.Lerp(madelinegodFrom, madelinegodTo, p);
                yield return null;
            }
            if (finalBoost)
            {
                Vector2 screenSpaceFocusPoint = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
                Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
                Engine.TimeRate = 0.5f;
            }
            else
            {
                Audio.Play("event:/char/badeline/booster_throw", Position);
            }
            if (madelinegod.Sprite.Has("boost"))
            {
                madelinegod.Sprite.Play("boost");
            }
            yield return 0.1f;
            if (!player.Dead)
            {
                player.MoveV(5f);
            }
            yield return 0.1f;
            if (endLevel)
            {
                level.TimerStopped = true;
                level.RegisterAreaComplete();
            }
            if (finalBoost && finalCh21Boost)
            {
                Scene.Add(new CS20_ElsDeathTrueLastUltraLaunch(player, this, finalCh21Dialog));
                player.Active = false;
                madelinegod.Active = false;
                Active = false;
                yield return null;
                player.Active = true;
                madelinegod.Active = true;
            }
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
            {
                if (player.Dashes < player.Inventory.Dashes)
                {
                    player.Dashes++;
                }
                Scene.Remove(madelinegod);
                (Scene as Level)?.Displacement.AddBurst(madelinegod.Position, 0.25f, 8f, 32f, 0.5f);
            }, 0.15f, start: true));
            (Scene as Level)?.Shake();
            holding = null;
            if (!finalBoost)
            {
                player.BadelineBoostLaunch(CenterX);
                Vector2 from = Position;
                Vector2 to = nodes[nodeIndex];
                float val = Vector2.Distance(from, to) / boostSpeed;
                val = Math.Min(3f, val);
                stretch.Visible = true;
                stretch.Rotation = (to - from).Angle();
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
                tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
                {
                    Position = Vector2.Lerp(from, to, t.Eased);
                    stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
                    stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
                    if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
                    {
                        int colorIdx = (int)((t.Eased * 7f) % 7f);
                        TrailManager.Add(this, SoulColors[colorIdx], 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                        
                        P_Move.Color = SoulColors[colorIdx];
                        level?.ParticlesFG.Emit(P_Move, 1, Center, Vector2.One * 4f);
                    }
                };
                tween.OnComplete = (Tween t) =>
                {
                    if (X >= (float)level.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                    else
                    {
                        travelling = false;
                        stretch.Visible = false;
                        sprite.Visible = true;

                        foreach (var soul in soulImages)
                        {
                            soul.Visible = true;
                        }

                        Collidable = true;
                        state = States.Wait;
                        Audio.Play("event:/desolozantas/final_content/char/madeline/ready", Position);
                    }
                };
                Add(tween);
                relocateSfx.Play("event:/char/badeline/booster_relocate");
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(-Vector2.UnitY);
                level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
            }
            else
            {
                if (finalCh21Boost)
                {
                    Ch21FinalBoostSfx = Audio.Play("event:/desolozantas/final_content/char/madeline/finalfinalfinalultra_part2", Position);
                }
                Engine.FreezeTimer = 0.1f;
                yield return true;
                if (endLevel)
                {
                    level.TimerHidden = true;
                }
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level.Flash(Color.White * 0.5f, drawPlayerOver: true);
                level.DirectionalShake(-Vector2.UnitY, 0.6f);
                level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
                level.ResetZoom();
                yield return ApplyAllSoulsStart(player);
                yield return ApplyAllSoulsEnd(player);
                player.SummitLaunch(X);
                Engine.TimeRate = 1f;
            }
        }

        protected virtual IEnumerator ApplyAllSoulsStart(global::Celeste.Player player)
        {
            Level level = Scene as Level;

            // Massive rainbow burst effect
            for (int i = 0; i < 7; i++)
            {
                float angle = (i / 7f) * (float)Math.PI * 2f;
                Vector2 offset = Calc.AngleToVector(angle, 32f);
                
                level?.ParticlesFG.Emit(
                    new ParticleType
                    {
                        Source = GFX.Game["particles/shard"],
                        Color = SoulColors[i],
                        Color2 = Color.White,
                        ColorMode = ParticleType.ColorModes.Blink,
                        Size = 1.5f,
                        LifeMin = 0.5f,
                        LifeMax = 1f,
                        SpeedMin = 40f,
                        SpeedMax = 100f,
                        DirectionRange = (float)Math.PI / 4f
                    },
                    8,
                    player.Center + offset,
                    Vector2.One * 4f
                );
            }

            yield return 0.2f;
        }

        protected virtual IEnumerator ApplyAllSoulsEnd(global::Celeste.Player player)
        {
            // Apply combined buff with all seven soul abilities
            player.Add(new SevenSoulBuff(5f, this));

            // Grant extra dashes (Determination)
            player.Dashes = Math.Min(player.Dashes + 2, 3);
            
            // Refill stamina (Perseverance)
            player.RefillStamina();
            
            // Increased launch power (Determination + Integrity)
            player.Speed *= 1.75f;

            yield break;
        }

        protected void Skip()
        {
            if (nodeIndex + 1 >= nodes.Length)
                return;

            travelling = true;
            nodeIndex++;
            Collidable = false;

            Level level = SceneAs<Level>();
            if (level == null)
                return;

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
                    int colorIdx = (int)((t.Eased * 7f) % 7f);
                    TrailManager.Add(this, SoulColors[colorIdx], 0.5f);
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
                    
                    foreach (var soul in soulImages)
                    {
                        soul.Visible = true;
                    }
                    
                    Collidable = true;
                    state = States.Wait;
                    Audio.Play("event:/desolozantas/final_content/char/madeline/ready", Position);
                }
            };

            Add(tween);
            relocateSfx.Play("event:/char/badeline/booster_relocate");
            level?.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
        }

        protected void Finish()
        {
            Level level = SceneAs<Level>();
            level?.Displacement.AddBurst(Center, 0.8f, 32f, 128f, 0.4f);
            
            // Final rainbow burst
            for (int i = 0; i < 7; i++)
            {
                P_Burst.Color = SoulColors[i];
                P_Burst.Color2 = SoulColors[(i + 1) % 7];
                level?.Particles.Emit(P_Burst, 6, Center, Vector2.One * 8f);
            }
            
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();

            // Rainbow glow effect
            if (!travelling && sprite.Visible)
            {
                int colorIndex = (int)((soulOrbitAngle * 2f) % 7f);
                Color glowColor = Color.Lerp(SoulColors[colorIndex], SoulColors[(colorIndex + 1) % 7], (soulOrbitAngle * 2f) % 1f);
                
                Draw.Circle(Center, 24f, glowColor * 0.15f, 16);
                Draw.Circle(Center, 16f, glowColor * 0.25f, 12);
                Draw.Circle(Center, 8f, Color.White * 0.3f, 8);
            }
        }

        /// <summary>
        /// Combined buff from all seven souls
        /// </summary>
        private class SevenSoulBuff : Component
        {
            private float duration;
            private float timer;
            private int shieldHits;
            private SevenSoulBoost parent;

            public SevenSoulBuff(float duration, SevenSoulBoost parent) : base(true, true)
            {
                this.duration = duration;
                this.timer = duration;
                this.shieldHits = 3; // Kindness shield
                this.parent = parent;
            }

            public override void Update()
            {
                base.Update();
                timer -= Engine.DeltaTime;
                
                if (timer <= 0f)
                {
                    RemoveSelf();
                    return;
                }

                Player player = Entity as Player;
                if (player != null)
                {
                    // Perseverance - Infinite stamina
                    player.Stamina = 110f;
                    
                    // Kindness - Shield from spikes
                    if (shieldHits > 0 && player.CollideCheck<Spikes>())
                    {
                        shieldHits--;
                        player.Speed *= -0.8f;
                        
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("00ff00"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 1.5f,
                                LifeMin = 0.5f,
                                LifeMax = 1f,
                                SpeedMin = 40f,
                                SpeedMax = 80f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            20,
                            player.Center,
                            Vector2.One * 8f
                        );
                    }

                    // Rainbow color cycling effect
                    float cyclePosition = (duration - timer) * 3f;
                    int colorIndex = (int)(cyclePosition % 7f);
                    float lerp = cyclePosition % 1f;
                    Color currentColor = Color.Lerp(SoulColors[colorIndex], SoulColors[(colorIndex + 1) % 7], lerp);
                    
                    float alpha = (float)Math.Sin(timer * 8f) * 0.2f + 0.4f;
                    player.Sprite.Color = Color.Lerp(Color.White, currentColor, alpha);

                    // Emit rainbow particles
                    if (Scene.OnInterval(0.08f))
                    {
                        int particleColorIdx = Calc.Random.Next(7);
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = SoulColors[particleColorIdx],
                                Color2 = SoulColors[(particleColorIdx + 1) % 7],
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 0.8f,
                                LifeMin = 0.3f,
                                LifeMax = 0.6f,
                                SpeedMin = 10f,
                                SpeedMax = 30f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            1,
                            player.Center,
                            Vector2.One * 6f
                        );
                    }

                    // Trail effect
                    if (Math.Abs(player.Speed.X) > 50f && Scene.OnInterval(0.05f))
                    {
                        TrailManager.Add(player, currentColor, 0.3f);
                    }
                }

                if (parent.sprite.Visible && base.Scene.OnInterval(0.05f))
                {
                    SceneAs<Level>().ParticlesBG.Emit(parent.P_Ambience, 1, parent.Center, Vector2.One * 3f);
                }
                if (parent.holding != null)
                {
                    parent.holding.Speed = Vector2.Zero;
                }
                if (!parent.travelling)
                {
                    global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        float num = Calc.ClampedMap((entity.Center - parent.Position).Length(), 16f, 64f, 12f, 0f);
                        Vector2 vector = (entity.Center - parent.Position).SafeNormalize();
                        parent.sprite.Position = Calc.Approach(parent.sprite.Position, parent.spriteOffset + vector * num, 32f * Engine.DeltaTime);
                        if (parent.canSkip && entity.Position.X - parent.X >= 100f && parent.nodeIndex + 1 < parent.nodes.Length)
                        {
                            parent.Skip();
                        }
                    }
                }
                parent.light.Visible = (parent.bloom.Visible = parent.sprite.Visible || parent.stretch.Visible);
            }

            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                
                Player player = entity as Player;
                if (player != null)
                {
                    player.Sprite.Color = Color.White;
                }
            }
        }
    }
}
