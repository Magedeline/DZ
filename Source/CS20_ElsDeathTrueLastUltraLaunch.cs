using Celeste.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.Entities.SoulBoosts;

// Kirby Heaven Ascend Manager for the final ascension
using Microsoft.Xna.Framework;
using Monocle;
using BirdNPC = Celeste.Entities.BirdNPC;
using System.Linq;

#pragma warning disable CS0618 // Engine.TimeRate is obsolete but needed for vanilla behavior

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Final ascension cutscene after Els' defeat: Kirby, Madeline, and Badeline
    /// ascend together while the live Els backdrop surges and destroys Els for good.
    /// Based on CS10_FinalLaunch structure.
    /// </summary>
    [Tracked]
    public class CS20_ElsDeathTrueLastUltraLaunch : CutsceneEntity
    {
        private Player player;

        private SevenSoulBoost boost;

        private BirdNPC bird;

        private SiamoZeroFinalBoss els;

        private ElsTrueFinalBackdrop backdrop;

        private float fadeToWhite;

        private Vector2 birdScreenPosition;

        private KirbyHeavenAscendManager heavenManager;

        private Vector2 cameraWaveOffset;

        private Vector2 cameraOffset;

        private float timer;

        private Coroutine wave;

        private bool hasGolden;

        private string dialog;

        // Seven goner birds that transform into souls
        private readonly List<BirdNPC> soulBirds = new List<BirdNPC>(7);
        private readonly Vector2[] soulBirdScreenPositions = new Vector2[7];
        private readonly List<VertexLight> soulLights = new List<VertexLight>(7);
        private bool soulBirdsTransformed = false;

        private static readonly Color[] SoulColors = new Color[]
        {
            Calc.HexToColor("ff0000"), // Red - Determination
            Calc.HexToColor("ff8000"), // Orange - Bravery
            Calc.HexToColor("ffff00"), // Yellow - Justice
            Calc.HexToColor("00ff00"), // Green - Kindness
            Calc.HexToColor("00ffff"), // Cyan - Patience
            Calc.HexToColor("0000ff"), // Blue - Integrity
            Calc.HexToColor("ff00ff")  // Purple - Perseverance
        };

        private static readonly string[] SoulNames = new string[]
        {
            "Determination", "Bravery", "Justice", "Kindness",
            "Patience", "Integrity", "Perseverance"
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CS20_ElsDeathTrueLastUltraLaunch(global::Celeste.Player player, SevenSoulBoost boost, string dialog = null)
            : base(fadeInOnSkip: false)
        {
            this.player = player;
            this.boost = boost;
            this.dialog = dialog;
            base.Depth = 10010;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            if (player == null || boost == null)
            {
                EndCutscene(level);
                return;
            }

            Audio.SetMusic(null);
            ScreenWipe.WipeColor = Color.White;
            foreach (var _ in player.Leader.Followers.Where(follower => follower.Entity is Strawberry { Golden: not false }).Select(follower => new { }))
            {
                hasGolden = true;
                break;
            }

            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator Cutscene()
        {
            if (Level == null) yield break;

            Engine.TimeRate = 1f;
            if (boost != null) boost.Active = false;
            yield return null;

            // Find Els and the live backdrop used by the room
            els = Level.Entities.FindFirst<SiamoZeroFinalBoss>();
            backdrop = Level.Background.Get<ElsTrueFinalBackdrop>();

            if (!string.IsNullOrEmpty(dialog))
            {
                yield return Textbox.Say(dialog);
            }
            else
            {
                yield return 0.152f;
            }

            cameraOffset = new Vector2(0f, -20f);
            if (boost != null) boost.Active = true;
            if (player != null) player.EnforceLevelBounds = false;
            yield return null;

            // === Destroy Els with backdrop burst ===
            if (backdrop != null)
            {
                backdrop.TriggerBurst();
            }

            if (els != null)
            {
                Vector2 elsCenter = els.Center;
                Audio.Play("event:/game/06_reflection/boss_spikes_burst", elsCenter);
                Level.Displacement.AddBurst(elsCenter, 4f, 128f, 512f, 2f);
                Level.Shake(1.2f);
                Level.Flash(Color.White, true);

                // Massive particle burst for Els destruction
                for (int i = 0; i < 360; i += 10)
                {
                    float angle = MathHelper.ToRadians(i);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 60f;
                    Level.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 4, elsCenter + offset, Vector2.One * 8f);
                }

                // Els is destroyed for good
                els.Visible = false;
                els.Collidable = false;
            }

            // === Ascension sequence (only Kirby, Madeline, Badeline) ===
            if (backdrop != null)
            {
                backdrop.Intensity = 5.5f;
                backdrop.Speed = 3.5f;
                backdrop.VoidRadius = 100f;
                backdrop.RainbowEdgeIntensity = 2.5f;
                backdrop.GridExpansionSpeed = 1.0f;
                backdrop.RainbowSpeed = 3.0f;
                backdrop.CorruptionSpeed = 2.0f;
            }

            Add(wave = new Coroutine(WaveCamera()));
            Add(new Coroutine(BirdRoutine(0.8f)));
            Add(new Coroutine(SevenSoulBirdsRoutine(1.2f)));

            // Spawn the heaven ascend manager with gold/pink/rainbow stars + Deltarune symbols
            if (heavenManager == null || heavenManager.Scene != Level)
            {
                heavenManager = new KirbyHeavenAscendManager();
                Level.Add(heavenManager);
            }
            heavenManager.Activate();
            Add(new Coroutine(heavenManager.FadeIn(3f)));

            float p;
            for (p = 0f; p < 1f; p += Engine.DeltaTime / 12f)
            {
                fadeToWhite = p;
                foreach (ElsTrueFinalBackdrop item in Level.Background.Backdrops.OfType<ElsTrueFinalBackdrop>())
                {
                    item.FadeAlphaMultiplier = 1f - p;
                }
                yield return null;
            }

            while (bird != null)
            {
                yield return null;
            }

            FadeWipe wipe = new FadeWipe(Level, wipeIn: false)
            {
                Duration = 4f
            };
            ScreenWipe.WipeColor = Color.White;

            if (!hasGolden)
            {
                Audio.SetMusic("event:/desolozantas/final_content/music/lvl20/saved", startPlaying: true, allowFadeOut: true);
            }

            p = cameraOffset.Y;
            int to = 180;
            for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 2f)
            {
                cameraOffset.Y = p + ((float)to - p) * Ease.BigBackIn(p2);
                yield return null;
            }

            while (wipe.Percent < 1f)
            {
                yield return null;
            }

            EndCutscene(Level);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnd(Level level)
        {
            if (WasSkipped && boost != null && boost.Ch20FinalBoostSfx != null)
            {
                boost.Ch20FinalBoostSfx.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                boost.Ch20FinalBoostSfx.release();
            }

            // Clean up heaven manager and all its sub-entities
            if (heavenManager != null && heavenManager.Scene != null)
                heavenManager.RemoveSelf();
            heavenManager = null;

            // Clean up any remaining soul birds
            foreach (BirdNPC b in soulBirds)
            {
                if (b != null && b.Scene != null)
                    b.RemoveSelf();
            }
            soulBirds.Clear();

            // Clean up main bird
            if (bird != null && bird.Scene != null)
                bird.RemoveSelf();
            bird = null;

            // Restore boost visibility so it can be reused or cleaned up
            if (boost != null)
                boost.Active = false;

            string nextLevelName = "end-saved";
            global::Celeste.Player.IntroTypes nextLevelIntro = global::Celeste.Player.IntroTypes.Transition;
            if (hasGolden)
            {
                nextLevelName = "end-golden";
                nextLevelIntro = global::Celeste.Player.IntroTypes.Jump;
            }

            Engine.TimeRate = 1f;

            if (player != null)
            {
                player.Active = true;
                player.Speed = Vector2.Zero;
                player.EnforceLevelBounds = true;
                player.StateMachine.State = 0;
                player.DummyFriction = true;
                player.DummyGravity = true;
                player.DummyAutoAnimate = true;
                player.ForceCameraUpdate = false;
            }

            Level.OnEndOfFrame += [MethodImpl(MethodImplOptions.NoInlining)] () =>
            {
                Level.TeleportTo(player, nextLevelName, nextLevelIntro);
                if (hasGolden)
                {
                    if (Level.Wipe != null)
                    {
                        Level.Wipe.Cancel();
                    }
                    Level.SnapColorGrade("golden");
                    new FadeWipe(level, wipeIn: true).Duration = 2f;
                    ScreenWipe.WipeColor = Color.White;
                }
            };
        }

        private IEnumerator WaveCamera()
        {
            float timer = 0f;
            while (true)
            {
                cameraWaveOffset.X = (float)Math.Sin(timer) * 16f;
                cameraWaveOffset.Y = (float)Math.Sin(timer * 0.5f) * 16f + (float)Math.Sin(timer * 0.25f) * 8f;
                timer += Engine.DeltaTime * 2f;
                yield return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator BirdRoutine(float delay)
        {
            yield return delay;
            Level.Add(bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
            bird.Sprite.Play("flyupIdle");
            Vector2 vector = new Vector2(320f, 180f) / 2f;
            Vector2 topCenter = new Vector2(vector.X, 0f);
            Vector2 vector2 = new Vector2(vector.X, 180f);
            Vector2 from = vector2 + new Vector2(40f, 40f);
            Vector2 to = vector + new Vector2(-32f, -24f);
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 4f)
            {
                birdScreenPosition = from + (to - from) * Ease.BackOut(t);
                yield return null;
            }
            bird.Sprite.Play("flyupRoll");
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 2f)
            {
                birdScreenPosition = to + new Vector2(64f, 0f) * Ease.CubeInOut(t);
                yield return null;
            }
            to = birdScreenPosition;
            from = topCenter + new Vector2(-40f, -100f);
            bool playedAnim = false;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 4f)
            {
                if (t >= 0.35f && !playedAnim)
                {
                    bird.Sprite.Play("flyupRoll");
                    playedAnim = true;
                }
                birdScreenPosition = to + (from - to) * Ease.BigBackIn(t);
                birdScreenPosition.X += t * 32f;
                yield return null;
            }
            bird.RemoveSelf();
            bird = null;
        }

        /// <summary>
        /// Seven goner birds appear one by one during ascension, circle the player,
        /// then transform into seven colored soul lights before departing upward.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator SevenSoulBirdsRoutine(float delay)
        {
            yield return delay;

            Vector2 screenCenter = new Vector2(320f, 180f) / 2f;

            // Spawn seven goner birds one by one from different directions
            for (int i = 0; i < 7; i++)
            {
                float spawnAngle = (i / 7f) * MathHelper.TwoPi;
                Vector2 spawnFrom = screenCenter + new Vector2(
                    (float)Math.Cos(spawnAngle) * 200f,
                    (float)Math.Sin(spawnAngle) * 200f
                );

                BirdNPC soulBird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None);
                Level.Add(soulBird);
                soulBird.Sprite.Play("flyupIdle");
                soulBird.Sprite.Color = Color.Lerp(SoulColors[i], Color.White, 0.3f);
                soulBirds.Add(soulBird);
                soulBirdScreenPositions[i] = spawnFrom;

                Audio.Play("event:/game/general/diamond_touch", Level.Camera.Position + screenCenter);
                yield return 0.3f;
            }

            // Birds circle inward to orbit positions around the player
            float orbitRadius = 60f;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 2f)
            {
                if (Scene == null) yield break;
                float eased = Ease.CubeOut(t);
                for (int i = 0; i < 7; i++)
                {
                    float angle = (i / 7f) * MathHelper.TwoPi + t * 3f;
                    Vector2 orbitTarget = screenCenter + new Vector2(
                        (float)Math.Cos(angle) * orbitRadius,
                        (float)Math.Sin(angle) * orbitRadius - 20f
                    );
                    Vector2 spawnAngleVec = screenCenter + new Vector2(
                        (float)Math.Cos((i / 7f) * MathHelper.TwoPi) * 200f,
                        (float)Math.Sin((i / 7f) * MathHelper.TwoPi) * 200f
                    );
                    soulBirdScreenPositions[i] = Vector2.Lerp(spawnAngleVec, orbitTarget, eased);
                }
                yield return null;
            }

            // Birds orbit for a while, getting tighter
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                if (Scene == null) yield break;
                float shrink = MathHelper.Lerp(orbitRadius, 24f, Ease.CubeIn(t / 3f));
                float spin = t * 4f;
                for (int i = 0; i < 7; i++)
                {
                    float angle = (i / 7f) * MathHelper.TwoPi + spin;
                    soulBirdScreenPositions[i] = screenCenter + new Vector2(
                        (float)Math.Cos(angle) * shrink,
                        (float)Math.Sin(angle) * shrink - 20f
                    );
                }
                yield return null;
            }

            // === Transform: birds flash and become soul lights ===
            Audio.Play("event:/game/general/crystalheart_pulse", Level.Camera.Position + screenCenter);
            Level.Flash(Color.White * 0.6f, false);
            Level.Shake(0.3f);

            soulBirdsTransformed = true;

            // Remove birds, spawn colored soul lights in their place
            for (int i = 0; i < 7; i++)
            {
                if (i < soulBirds.Count && soulBirds[i] != null && soulBirds[i].Scene != null)
                {
                    Vector2 birdWorldPos = soulBirds[i].Position;
                    soulBirds[i].RemoveSelf();

                    // Spawn soul light
                    VertexLight soulLight = new VertexLight(SoulColors[i] * 1.4f, 1f, 64, 128)
                    {
                        Position = birdWorldPos
                    };
                    soulLights.Add(soulLight);
                    Add(soulLight);

                    // Soul reveal particles
                    Level.ParticlesFG.Emit(
                        new ParticleType
                        {
                            Source = GFX.Game["particles/shard"],
                            Color = SoulColors[i],
                            Color2 = Color.White,
                            ColorMode = ParticleType.ColorModes.Blink,
                            Size = 1.5f,
                            LifeMin = 0.6f,
                            LifeMax = 1.2f,
                            SpeedMin = 60f,
                            SpeedMax = 140f,
                            DirectionRange = MathHelper.TwoPi
                        },
                        12,
                        birdWorldPos,
                        Vector2.One * 8f
                    );

                    Level.Displacement.AddBurst(birdWorldPos, 0.5f, 16f, 64f, 0.4f);
                }
            }
            soulBirds.Clear();

            yield return 0.5f;

            // Soul lights orbit briefly, then streak upward and vanish
            for (float t = 0f; t < 2f; t += Engine.DeltaTime)
            {
                if (Scene == null) yield break;
                float spin = t * 6f;
                float radius = MathHelper.Lerp(24f, 8f, Ease.CubeIn(t / 2f));
                Vector2 center = Level.Camera.Position + screenCenter + new Vector2(0f, -20f);
                for (int i = 0; i < soulLights.Count; i++)
                {
                    float angle = (i / 7f) * MathHelper.TwoPi + spin;
                    soulLights[i].Position = center + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius
                    );
                    soulLights[i].Alpha = 0.7f + 0.3f * (float)Math.Sin(t * 8f + i);

                    // Trailing particles
                    if (Scene.OnInterval(0.1f))
                    {
                        Level.ParticlesFG.Emit(
                            global::Celeste.Player.P_DashA,
                            1,
                            soulLights[i].Position,
                            Vector2.One * 4f
                        );
                    }
                }
                yield return null;
            }

            // Final flash and souls streak upward
            Audio.Play("event:/game/06_reflection/badeline_dash", Level.Camera.Position + screenCenter);
            Level.Displacement.AddBurst(Level.Camera.Position + screenCenter, 1f, 32f, 128f, 0.6f);

            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 1.5f)
            {
                if (Scene == null) yield break;
                float eased = Ease.CubeIn(t);
                Vector2 center = Level.Camera.Position + screenCenter + new Vector2(0f, -20f);
                for (int i = 0; i < soulLights.Count; i++)
                {
                    float angle = (i / 7f) * MathHelper.TwoPi;
                    Vector2 spreadDir = new Vector2((float)Math.Cos(angle) * 40f, -200f * eased);
                    soulLights[i].Position = center + spreadDir;
                    soulLights[i].Alpha = 1f - eased;

                    if (Scene.OnInterval(0.05f))
                    {
                        Level.ParticlesFG.Emit(
                            global::Celeste.Player.P_DashB,
                            2,
                            soulLights[i].Position,
                            Vector2.One * 6f
                        );
                    }
                }
                yield return null;
            }

            // Clean up soul lights
            foreach (VertexLight light in soulLights)
            {
                Remove(light);
            }
            soulLights.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            if (Level == null) return;
            timer += Engine.DeltaTime;
            if (bird != null)
            {
                bird.Position = Level.Camera.Position + birdScreenPosition;
                bird.Position.X += (float)Math.Sin(timer) * 4f;
                bird.Position.Y += (float)Math.Sin(timer * 0.1f) * 4f + (float)Math.Sin(timer * 0.25f) * 4f;
            }

            // Update soul bird positions (before transformation)
            if (!soulBirdsTransformed)
            {
                for (int i = 0; i < soulBirds.Count; i++)
                {
                    if (soulBirds[i] != null && soulBirds[i].Scene != null)
                    {
                        soulBirds[i].Position = Level.Camera.Position + soulBirdScreenPositions[i];
                        soulBirds[i].Position.X += (float)Math.Sin(timer * 1.5f + i * 0.9f) * 3f;
                        soulBirds[i].Position.Y += (float)Math.Sin(timer * 0.8f + i * 1.3f) * 3f;
                    }
                }
            }

            Level.CameraOffset = cameraOffset + cameraWaveOffset;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            if (Level == null) return;
            Camera camera = Level.Camera;
            Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
        }
    }
}

