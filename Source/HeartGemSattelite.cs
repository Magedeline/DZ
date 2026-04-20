using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/HeartGemSattelite")]
    [Tracked]
    public class HeartGemSattelite : Entity
    {
        private const string UnlockedFlag = "unlocked_azzy_theme";
        private static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>
        {
            {
                "D",
                Calc.HexToColor("0000FF")
            },
            {
                "U",
                Calc.HexToColor("FF0000")
            },
            {
                "L",
                Calc.HexToColor("800080")
            },
            {
                "R",
                Calc.HexToColor("FFFF00")
            }
        };
        public static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>
        {
            {
                "D",
                "event:/desolozantas/game/01_city/console_blue"
            },
            {
                "U",
                "event:/desolozantas/game/01_city/console_red"
            },
            {
                "L",
                "event:/desolozantas/game/01_city/console_purple"
            },
            {
                "R",
                "event:/desolozantas/game/01_city/console_yellow"
            }
        };
        public static readonly Dictionary<string, ParticleType> Particles = new Dictionary<string, ParticleType>
        {
            {
                "D",
                new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Calc.HexToColor("0000FF"),
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.5f,
                    LifeMax = 0.8f,
                    Size = 0.7f,
                    SpeedMin = 10f,
                    SpeedMax = 20f,
                    Direction = (float)Math.PI / 2f,
                    DirectionRange = 0.5f
                }
            },
            {
                "U",
                new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Calc.HexToColor("FF0000"),
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.5f,
                    LifeMax = 0.8f,
                    Size = 0.7f,
                    SpeedMin = 10f,
                    SpeedMax = 20f,
                    Direction = -(float)Math.PI / 2f,
                    DirectionRange = 0.5f
                }
            },
            {
                "L",
                new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Calc.HexToColor("800080"),
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.5f,
                    LifeMax = 0.8f,
                    Size = 0.7f,
                    SpeedMin = 10f,
                    SpeedMax = 20f,
                    Direction = (float)Math.PI,
                    DirectionRange = 0.5f
                }
            },
            {
                "R",
                new ParticleType
                {
                    Source = GFX.Game["particles/blob"],
                    Color = Calc.HexToColor("FFFF00"),
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 0.5f,
                    LifeMax = 0.8f,
                    Size = 0.7f,
                    SpeedMin = 10f,
                    SpeedMax = 20f,
                    Direction = 0f,
                    DirectionRange = 0.5f
                }
            }
        };
        private static readonly string[] Code = new string[6]
        {
            "D",
            "U",
            "D",
            "L",
            "L",
            "R"
        };
        private static List<string> uniqueCodes = new List<string>();
        private bool enabled;
        private List<string> currentInputs = new List<string>();
        private List<CodeBird> birds = new List<CodeBird>();
        private Vector2 gemSpawnPosition;
        private Vector2 birdFlyPosition;
        private Image sprite;
        private Image pulse;
        private Image computer;
        private Image computerScreen;
        private Sprite computerScreenNoise;
        private Image computerScreenShine;
        private BloomPoint pulseBloom;
        private BloomPoint screenBloom;
        private Level level;
        private DashListener dashListener;
        private SoundSource birdFlyingSfx;
        private SoundSource birdThrustSfx;
        private SoundSource birdFinishSfx;
        private SoundSource staticLoopSfx;

        public static Dictionary<string, Color> Colors => colors;

        public HeartGemSattelite(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = new Image(GFX.Game["objects/citysatellite/dish"]));
            Add(pulse = new Image(GFX.Game["objects/citysatellite/light"]));
            Add(computer = new Image(GFX.Game["objects/citysatellite/computer"]));
            Add(computerScreen = new Image(GFX.Game["objects/citysatellite/computerscreen"]));
            Add(computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise"));
            Add(computerScreenShine = new Image(GFX.Game["objects/citysatellite/computerscreenShine"]));
            sprite.JustifyOrigin(0.5f, 1f);
            pulse.JustifyOrigin(0.5f, 1f);
            Add(new Coroutine(PulseRoutine()));
            Add(pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f));
            Add(screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f));
            computerScreenNoise.AddLoop("static", "", 0.05f);
            computerScreenNoise.Play("static");
            computer.Position = computerScreen.Position = computerScreenShine.Position = computerScreenNoise.Position = new Vector2(8f, 8f);
            birdFlyPosition = offset + data.Nodes[0];
            gemSpawnPosition = offset + data.Nodes[1];
            Add(dashListener = new DashListener());
            dashListener.OnDash = dir =>
            {
                string str = "";
                if (dir.Y < 0.0)
                    str = "U";
                else if (dir.Y > 0.0)
                    str = "D";
                if (dir.X < 0.0)
                    str += "L";
                else if (dir.X > 0.0)
                    str += "R";
                currentInputs.Add(str);
                if (currentInputs.Count > HeartGemSattelite.Code.Length)
                    currentInputs.RemoveAt(0);
                if (currentInputs.Count != HeartGemSattelite.Code.Length)
                    return;
                bool flag = true;
                for (int index = 0; index < HeartGemSattelite.Code.Length; ++index)
                {
                    if (!currentInputs[index].Equals(HeartGemSattelite.Code[index]))
                        flag = false;
                }
                if (!flag || level.Camera.Left + 32.0 >= gemSpawnPosition.X || !enabled)
                    return;
                Add(new Coroutine(UnlockGem()));
            };
            foreach (string str in HeartGemSattelite.Code)
            {
                if (!HeartGemSattelite.uniqueCodes.Contains(str))
                    HeartGemSattelite.uniqueCodes.Add(str);
            }
            Depth = 8999;
            Add(staticLoopSfx = new SoundSource());
            staticLoopSfx.Position = computer.Position;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            enabled = !level.Session.HeartGem && !level.Session.GetFlag("unlocked_azzy_theme");
            if (enabled)
            {
                foreach (string uniqueCode in HeartGemSattelite.uniqueCodes)
                {
                    CodeBird codeBird = new CodeBird(birdFlyPosition, uniqueCode);
                    birds.Add(codeBird);
                    level.Add(codeBird);
                }
                Add(birdFlyingSfx = new SoundSource());
                Add(birdFinishSfx = new SoundSource());
                Add(birdThrustSfx = new SoundSource());
                birdFlyingSfx.Position = birdFlyPosition - Position;
                birdFlyingSfx.Play("event:/game/01_forsaken_city/birdbros_fly_loop");
            }
            else
                staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
            if (level.Session.HeartGem || !level.Session.GetFlag("unlocked_azzy_theme"))
                return;
            level.Add(new HeartGem(gemSpawnPosition));
        }

        public override void Update()
        {
            base.Update();
            computerScreenNoise.Visible = !pulse.Visible;
            computerScreen.Visible = pulse.Visible;
            screenBloom.Visible = pulseBloom.Visible;
        }

        private IEnumerator PulseRoutine()
        {
            HeartGemSattelite heartGemSattelite = this;
            heartGemSattelite.pulseBloom.Visible = heartGemSattelite.pulse.Visible = false;
            while (heartGemSattelite.enabled)
            {
                yield return 2f;
                for (int i = 0; i < HeartGemSattelite.Code.Length && heartGemSattelite.enabled; ++i)
                {
                    heartGemSattelite.pulse.Color = heartGemSattelite.computerScreen.Color = HeartGemSattelite.Colors[HeartGemSattelite.Code[i]];
                    heartGemSattelite.pulseBloom.Visible = heartGemSattelite.pulse.Visible = true;
                    Audio.Play(HeartGemSattelite.Sounds[HeartGemSattelite.Code[i]], heartGemSattelite.Position + heartGemSattelite.computer.Position);
                    yield return 0.5f;
                    heartGemSattelite.pulseBloom.Visible = heartGemSattelite.pulse.Visible = false;
                    Audio.Play(i < HeartGemSattelite.Code.Length - 1 ? "event:/desolozantas/game/01_city/console_static_short" : "event:/desolozantas/game/01_city/console_static_long", heartGemSattelite.Position + heartGemSattelite.computer.Position);
                    yield return 0.2f;
                }
                // ISSUE: reference to a compiler-generated method
                heartGemSattelite.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
                {
                        if (enabled)
                        {
                                birdThrustSfx.Position = birdFlyPosition - Position;
                                birdThrustSfx.Play("event:/desolozantas/game/01_city/birdbros_thrust");
                        }
                }, 1.1f, true));
                heartGemSattelite.birds.Shuffle();
                foreach (CodeBird bird in heartGemSattelite.birds)
                {
                    if (heartGemSattelite.enabled)
                    {
                        bird.Dash();
                        yield return 0.02f;
                    }
                }
            }
            heartGemSattelite.pulseBloom.Visible = heartGemSattelite.pulse.Visible = false;
        }

        private IEnumerator UnlockGem()
        {
            HeartGemSattelite heartGemSattelite = this;
            heartGemSattelite.level.Session.SetFlag("unlocked_azzy_theme");
            heartGemSattelite.birdFinishSfx.Position = heartGemSattelite.birdFlyPosition - heartGemSattelite.Position;
            heartGemSattelite.birdFinishSfx.Play("event:/desolozantas/game/01_city/birdbros_finish");
            heartGemSattelite.staticLoopSfx.Play("event:/desolozantas/game/01_city/console_static_loop");
            heartGemSattelite.enabled = false;
            yield return 0.25f;
            heartGemSattelite.level.Displacement.Clear();
            yield return null;
            heartGemSattelite.birdFlyingSfx.Stop();
            heartGemSattelite.level.Frozen = true;
            heartGemSattelite.Tag = (int) Tags.FrozenUpdate;
            BloomPoint bloom = new BloomPoint(heartGemSattelite.birdFlyPosition - heartGemSattelite.Position, 0.0f, 32f);
            heartGemSattelite.Add(bloom);
            foreach (CodeBird bird in heartGemSattelite.birds)
                bird.Transform(3f);
            while (bloom.Alpha < 1.0)
            {
                bloom.Alpha += Engine.DeltaTime / 3f;
                yield return null;
            }
            yield return 0.25f;
            foreach (Entity bird in heartGemSattelite.birds)
                bird.RemoveSelf();
            ParticleSystem particles = new ParticleSystem(-10000, 100);
            particles.Tag = (int) Tags.FrozenUpdate;
            particles.Emit(BirdNPC.P_Feather, 24, heartGemSattelite.birdFlyPosition, new Vector2(4f, 4f));
            heartGemSattelite.level.Add(particles);
            HeartGem gem = new HeartGem(heartGemSattelite.birdFlyPosition);
            gem.Tag = (int) Tags.FrozenUpdate;
            heartGemSattelite.level.Add(gem);
            yield return null;
            gem.ScaleWiggler.Start();
            yield return 0.85f;
            SimpleCurve curve = new SimpleCurve(gem.Position, heartGemSattelite.gemSpawnPosition, (gem.Position + heartGemSattelite.gemSpawnPosition) / 2f + new Vector2(0.0f, -64f));
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime)
            {
                yield return null;
                gem.Position = curve.GetPoint(Ease.CubeInOut(t));
            }
            yield return 0.5f;
            particles.RemoveSelf();
            heartGemSattelite.Remove(bloom);
            heartGemSattelite.level.Frozen = false;
        }

        private class CodeBird : Entity
        {
            private Sprite sprite;
            private Coroutine routine;
            private float timer = Calc.Random.NextFloat();
            private Vector2 speed;
            private Image heartImage;
            private readonly string code;
            private readonly Vector2 origin;
            private readonly Vector2 dash;

            public CodeBird(Vector2 origin, string code)
                : base(origin)
            {
                this.code = code;
                this.origin = origin;
                Add(sprite = new Sprite(GFX.Game, "scenery/flutterbird/"));
                sprite.AddLoop("fly", "flap", 0.08f);
                sprite.Play("fly");
                sprite.CenterOrigin();
                if (HeartGemSattelite.Colors.TryGetValue(code, out var color))
                    sprite.Color = color;
                else
                    sprite.Color = Color.White;
                dash = (Vector2.Zero with
                {
                    X = (code.Contains('L') ? -1f : (code.Contains('R') ? 1f : 0.0f)),
                    Y = (code.Contains('U') ? -1f : (code.Contains('D') ? 1f : 0.0f))
                }).SafeNormalize();
                Add(routine = new Coroutine(AimlessFlightRoutine()));
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                sprite.Y = (float) Math.Sin(timer * 2.0);
                base.Update();
            }

            public void Dash() => routine.Replace(DashRoutine());

            public void Transform(float duration)
            {
                Tag = (int) Tags.FrozenUpdate;
                routine.Replace(TransformRoutine(duration));
            }

            private IEnumerator AimlessFlightRoutine()
            {
                CodeBird codeBird = this;
                codeBird.speed = Vector2.Zero;
                while (true)
                {
                    Vector2 target = codeBird.origin + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 16f + Calc.Random.NextFloat(40f));
                    float reset = 0.0f;
                    while (reset < 1.0 && (target - codeBird.Position).Length() > 8.0)
                    {
                        Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
                        codeBird.speed += vector2 * 420f * Engine.DeltaTime;
                        if (codeBird.speed.Length() > 90.0)
                            codeBird.speed = codeBird.speed.SafeNormalize(90f);
                        codeBird.Position += codeBird.speed * Engine.DeltaTime;
                        reset += Engine.DeltaTime;
                        if (Math.Sign(vector2.X) != 0)
                            codeBird.sprite.Scale.X = Math.Sign(vector2.X);
                        yield return null;
                    }
                    target = new Vector2();
                }
            }

            private IEnumerator DashRoutine()
            {
                CodeBird codeBird = this;
                float t;
                for (t = 0.25f; t > 0.0; t -= Engine.DeltaTime)
                {
                    codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 200f * Engine.DeltaTime);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    yield return null;
                }
                Vector2 from = codeBird.Position;
                Vector2 to = codeBird.origin + codeBird.dash * 8f;
                if (Math.Sign(to.X - from.X) != 0)
                    codeBird.sprite.Scale.X = Math.Sign(to.X - from.X);
                for (t = 0.0f; t < 1.0; t += Engine.DeltaTime * 1.5f)
                {
                    codeBird.Position = from + (to - from) * Ease.CubeInOut(t);
                    yield return null;
                }
                codeBird.Position = to;
                yield return 0.2f;
                from = new Vector2();
                to = new Vector2();
                if (codeBird.dash.X != 0.0)
                    codeBird.sprite.Scale.X = Math.Sign(codeBird.dash.X);
                (codeBird.Scene as Level).Displacement.AddBurst(codeBird.Position, 0.25f, 4f, 24f, 0.4f);
                codeBird.speed = codeBird.dash * 300f;
                for (t = 0.4f; t > 0.0; t -= Engine.DeltaTime)
                {
                    if (t > 0.10000000149011612 && codeBird.Scene.OnInterval(0.02f))
                    {
                        if (HeartGemSattelite.Particles.TryGetValue(codeBird.code, out var particleType))
                            codeBird.SceneAs<Level>().ParticlesBG.Emit(particleType, 1, codeBird.Position, Vector2.One * 2f, codeBird.dash.Angle());
                    }
                    codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 800f * Engine.DeltaTime);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    yield return null;
                }
                yield return 0.4f;
                codeBird.routine.Replace(codeBird.AimlessFlightRoutine());
            }

            private IEnumerator TransformRoutine(float duration)
            {
                CodeBird codeBird = this;
                Color colorFrom = codeBird.sprite.Color;
                Color colorTo = Color.White;
                Vector2 target = codeBird.origin;
                codeBird.Add(codeBird.heartImage = new Image(GFX.Game["collectables/heartGem/shape"]));
                codeBird.heartImage.CenterOrigin();
                codeBird.heartImage.Scale = Vector2.Zero;
                for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / duration)
                {
                    Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
                    codeBird.speed += 400f * vector2 * Engine.DeltaTime;
                    float length = Math.Max(20f, (float) ((1.0 - t) * 200.0));
                    if (codeBird.speed.Length() > (double) length)
                        codeBird.speed = codeBird.speed.SafeNormalize(length);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    codeBird.sprite.Color = Color.Lerp(colorFrom, colorTo, t);
                    codeBird.heartImage.Scale = Vector2.One * Math.Max(0.0f, (float) ((t - 0.75) * 4.0));
                    if (vector2.X != 0.0)
                        codeBird.sprite.Scale.X = Math.Abs(codeBird.sprite.Scale.X) * Math.Sign(vector2.X);
                    codeBird.sprite.Scale.X = Math.Sign(codeBird.sprite.Scale.X) * (1f - codeBird.heartImage.Scale.X);
                    codeBird.sprite.Scale.Y = 1f - codeBird.heartImage.Scale.X;
                    yield return null;
                }
            }
        }
    }
}