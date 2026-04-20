using System.Reflection.Metadata;
using Celeste.Entities;
using BadelineDummy = Celeste.Entities.BadelineDummy;
using MaggyBreathingGame = Celeste.Entities.TestBreathingGame;

namespace Celeste.Cutscenes
{
    public class CS08_StarJumpEnd : CutsceneEntity
    {
        public const string Flag = "plateaumaggy_2";

        private bool waiting = true;
        private bool shaking;
        private NPC starJumpController;
        private global::Celeste.Player player;
        private Bonfire bonfire;
        private CharaDummy chara;
        private Plateau plateau;
        private MaggyBreathingGame breathing;
        private List<ReflectionTentacles> tentacles = new List<ReflectionTentacles>();
        private Vector2 playerStart;
        private Vector2 cameraStart;
        private float anxietyFade;
        private SineWave anxietySine;
        private float anxietyJitter;
        private bool hidingNorthernLights;
        private bool charactersSpinning;
        private float maddySine;
        private float maddySineTarget;
        private float maddySineAnchorY;
        private SoundSource shakingLoopSfx;
        private bool baddyCircling;
        private HeartGemRumbler rumbler = new HeartGemRumbler();
        private int tentacleIndex;

        public CS08_StarJumpEnd(NPC starJumpController, global::Celeste.Player player, Vector2 playerStart, Vector2 cameraStart)
            : base(true, false)
        {
            base.Depth = 10100;
            this.starJumpController = starJumpController;
            this.player = player;
            this.playerStart = playerStart;
            this.cameraStart = cameraStart;
            base.Add(this.anxietySine = new SineWave(0.3f, 0f));
        }

        public override void Added(Scene scene)
        {
            this.Level = (scene as Level);
            this.bonfire = scene.Entities.FindFirst<Bonfire>();
            this.plateau = scene.Entities.FindFirst<Plateau>();
        }

        public override void Update()
        {
            base.Update();
            if (this.waiting && this.player.Y <= (float)(this.Level.Bounds.Top + 160))
            {
                this.waiting = false;
                base.Start();
            }
            if (this.shaking)
            {
                this.Level.Shake(0.2f);
            }
            if (this.Level != null && this.Level.OnInterval(0.1f))
            {
                this.anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
            }
            Distort.Anxiety = this.anxietyFade * Math.Max(0f, 0f + this.anxietyJitter + this.anxietySine.Value * 0.6f);
            this.maddySine = Calc.Approach(this.maddySine, this.maddySineTarget, 12f * Engine.DeltaTime);
            if (this.maddySine > 0f)
            {
                this.player.Y = this.maddySineAnchorY + (float)Math.Sin((double)(this.Level.TimeActive * 2f)) * 3f * this.maddySine;
            }
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            StarJumpController controller = level.Entities.FindFirst<StarJumpController>();
            if (controller != null)
            {
                controller.RemoveSelf();
            }
            foreach (CelesteStarJumpBlock block in level.Entities.FindAll<CelesteStarJumpBlock>())
            {
                block.Collidable = false;
            }
            int center = level.Bounds.X + 160;
            Vector2 cutsceneCenter = new Vector2((float)center, (float)(level.Bounds.Top + 150));
            NorthernLights bg = level.Background.Get<NorthernLights>();
            level.CameraOffset.Y = -30f;
            base.Add(new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -70f), 1.5f, Ease.CubeOut, 0f), true));
            base.Add(new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -120f), 2f, Ease.CubeInOut, 1.5f), true));
            Tween.Set(this, Tween.TweenMode.Oneshot, 3f, Ease.CubeInOut, delegate(Tween t)
            {
                bg.OffsetY = t.Eased * 32f;
            }, null);
            if (this.player.StateMachine.State == 19)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            this.player.Dashes = 0;
            this.player.StateMachine.State = 11;
            this.player.DummyGravity = false;
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("fallSlow", false, false);
            this.player.Dashes = 1;
            this.player.Speed = new Vector2(0f, -80f);
            this.player.Facing = Facings.Right;
            this.player.ForceCameraUpdate = false;
            while (this.player.Speed.Length() > 0f || this.player.Position != cutsceneCenter)
            {
                this.player.Speed = Calc.Approach(this.player.Speed, Vector2.Zero, 200f * Engine.DeltaTime);
                this.player.Position = Calc.Approach(this.player.Position, cutsceneCenter, 64f * Engine.DeltaTime);
                yield return null;
            }
            this.player.Sprite.Play("spin", false, false);
            yield return 3.5f;
            this.player.Facing = Facings.Right;
            
            // Chara splits from Madeline instead of Badeline
            level.Add(this.chara = new CharaDummy(this.player.Position));
            level.Displacement.AddBurst(this.player.Position, 0.5f, 8f, 48f, 0.5f, null, null);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            this.player.CreateSplitParticles();
            Audio.Play("event:/char/chara/appear", this.player.Position);
            this.chara.Sprite.Scale.X = -1f;
            
            // Madeline and Chara split apart
            Vector2 start = this.player.Position;
            Vector2 target = cutsceneCenter + new Vector2(-30f, 0f);
            this.maddySineAnchorY = cutsceneCenter.Y;
            for (float p = 0f; p <= 1f; p += 2f * Engine.DeltaTime)
            {
                yield return null;
                if (p > 1f)
                {
                    p = 1f;
                }
                this.player.Position = Vector2.Lerp(start, target, Ease.CubeOut(p));
                this.chara.Position = new Vector2((float)center + ((float)center - this.player.X), this.player.Y);
            }
            start = default(Vector2);
            target = default(Vector2);
            
            // Madeline and Chara spin/circle around each other
            this.charactersSpinning = true;
            base.Add(new Coroutine(this.SpinMaddyAndChara(), true));
            this.SetMusicLayer(2);
            yield return 2f;

            yield return Textbox.Say("CH8_TRUTH_DREAMING", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(this.VinesAppear),           // trigger 0: Vines Appear
                new Func<IEnumerator>(this.VinesAppearMore),       // trigger 1: Vines Appear More
                new Func<IEnumerator>(this.VinesAppearMore2),      // trigger 2: Vines Appear More
                new Func<IEnumerator>(this.VinesAppearEvenMore),   // trigger 3: Vines Appear EVEN MORE
                new Func<IEnumerator>(this.VinesAppearEvenEvenMore),   // trigger 4: Vines Appear EVEN EVEN MORE
                new Func<IEnumerator>(this.VinesGrabMadeline),     // trigger 5: Vines grab Madeline
                new Func<IEnumerator>(this.CharaStartCircling),    // trigger 6: chara start circling the madeline
                new Func<IEnumerator>(this.HeartgemMinigame),      // trigger 7: heartgem minigame
                new Func<IEnumerator>(this.BreakHeartMinigame),     // trigger 8: Break Heart minigame
                new Func<IEnumerator>(this.CharaFlyDown)     // trigger 9: Chara Fly Down
            });
            Audio.Play("event:/desolozantas/game/08_truth/chara_pull_whooshdown");
            base.Add(new Coroutine(this.CharaFlyDown(), true));
            yield return 0.7f;
            foreach (FlyFeather feather in level.Entities.FindAll<FlyFeather>())
            {
                feather.RemoveSelf();
            }
            foreach (CelesteStarJumpBlock block2 in level.Entities.FindAll<CelesteStarJumpBlock>())
            {
                block2.RemoveSelf();
            }
            foreach (CelesteJumpThru jumpThru in level.Entities.FindAll<CelesteJumpThru>())
            {
                jumpThru.RemoveSelf();
            }
            level.Shake(0.3f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            level.CameraOffset.Y = 0f;
            this.player.Sprite.Play("tentacle_pull", false, false);
            this.player.Speed.Y = 160f;
            FallEffects.Show(true);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 3f)
            {
                global::Celeste.Player player = this.player;
                player.Speed.Y = player.Speed.Y + Engine.DeltaTime * 100f;
                if (this.player.X < (float)(level.Bounds.X + 32))
                {
                    this.player.X = (float)(level.Bounds.X + 32);
                }
                if (this.player.X > (float)(level.Bounds.Right - 32))
                {
                    this.player.X = (float)(level.Bounds.Right - 32);
                }
                if (p > 0.7f)
                {
                    Level level2 = level;
                    level2.CameraOffset.Y = level2.CameraOffset.Y - 100f * Engine.DeltaTime;
                }
                foreach (ReflectionTentacles tentacle in this.tentacles)
                {
                    tentacle.Nodes[0] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 300f);
                    tentacle.Nodes[1] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 600f);
                }
                FallEffects.SpeedMultiplier += Engine.DeltaTime * 0.75f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                yield return null;
            }
            Audio.Play("event:/desolozantas/game/08_truth/chara_pull_impact");
            FallEffects.Show(false);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White, false);
            level.Session.Dreaming = false;
            level.CameraOffset.Y = 0f;
            level.Camera.Position = this.cameraStart;
            this.SetBloom(0f);
            this.bonfire.SetMode(Bonfire.Mode.Smoking);
            this.plateau.Depth = this.player.Depth + 10;
            this.plateau.Remove(this.plateau.Occluder);
            this.player.Position = this.playerStart + new Vector2(0f, 8f);
            this.player.Speed = Vector2.Zero;
            this.player.Sprite.Play("tentacle_dangling", false, false);
            this.player.Facing = Facings.Left;
            if (this.starJumpController != null)
            {
                NPC npc = this.starJumpController;
                npc.Position.X = npc.Position.X - 24f;
            }
            foreach (ReflectionTentacles tentacle in this.tentacles)
            {
                tentacle.Index = 0;
                tentacle.Nodes[0] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 32f);
                tentacle.Nodes[1] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 400f);
                tentacle.SnapTentacles();
            }
            this.shaking = true;
            base.Add(this.shakingLoopSfx = new SoundSource());
            this.shakingLoopSfx.Play("event:/desolozantas/game/08_truth/chara_pull_rumble_loop", null, 0f);
            yield return Textbox.Say("CH8_WATCHOUT", new Func<IEnumerator>[0]);
            Audio.Play("event:/desolozantas/game/08_truth/chara_pull_cliffbreak");
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
            this.shakingLoopSfx.Stop(true);
            this.shaking = false;
            int num = 0;
            while ((float)num < this.plateau.Width)
            {
                level.Add(Engine.Pooler.Create<Debris>().Init(this.plateau.Position + new Vector2((float)num + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3', true).BlastFrom(this.plateau.Center + new Vector2(0f, 8f)));
                level.Add(Engine.Pooler.Create<Debris>().Init(this.plateau.Position + new Vector2((float)num + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3', true).BlastFrom(this.plateau.Center + new Vector2(0f, 8f)));
                num += 8;
            }
            this.plateau.RemoveSelf();
            this.bonfire.RemoveSelf();
            level.Shake(0.3f);
            this.player.Speed.Y = 160f;
            this.player.Sprite.Play("tentacle_pull", false, false);
            this.player.ForceCameraUpdate = false;
            FadeWipe wipe = new FadeWipe(level, false, delegate()
            {
                this.EndCutscene(level, true);
            });
            wipe.Duration = 3f;
            target = level.Camera.Position;
            start = level.Camera.Position + new Vector2(0f, 400f);
            while (wipe.Percent < 1f)
            {
                level.Camera.Position = Vector2.Lerp(target, start, Ease.CubeIn(wipe.Percent));
                global::Celeste.Player player = this.player;
                this.player.Speed.Y = player.Speed.Y + 400f * Engine.DeltaTime;
                foreach (ReflectionTentacles tentacle in this.tentacles)
                {
                    tentacle.Nodes[0] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 300f);
                    tentacle.Nodes[1] = new Vector2((float)level.Bounds.Center.X, this.player.Y + 600f);
                }
                yield return null;
            }
            wipe = null;
            target = default(Vector2);
            start = default(Vector2);
            yield break;
        }

        private void SetMusicLayer(int index)
        {
            for (int i = 1; i <= 3; i++)
            {
                this.Level.Session.Audio.Music.Layer(i, index == i);
            }
            this.Level.Session.Audio.Apply(false);
        }

        // trigger 0: Vines Appear
        private IEnumerator VinesAppear()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_1");
            
            if (!this.hidingNorthernLights)
            {
                base.Add(new Coroutine(this.NorthernLightsDown(), true));
                this.hidingNorthernLights = true;
            }
            this.Level.Shake(0.3f);
            this.anxietyFade += 0.1f;
            this.SetMusicLayer(3);
            
            this.AddTentacle();
            this.charactersSpinning = false;
            this.maddySineTarget = 1f;
            yield return null;
            yield break;
        }

        // trigger 1: Vines Appear More
        private IEnumerator VinesAppearMore()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_2");
            this.Level.Shake(0.3f);
            this.anxietyFade += 0.1f;
            
            this.AddTentacle();
            yield return null;
            yield break;
        }

        // trigger 2: Vines Appear More (second wave)
        private IEnumerator VinesAppearMore2()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_3");
            this.Level.Shake(0.4f);
            this.anxietyFade += 0.1f;
            
            this.AddTentacle();
            yield return null;
            yield break;
        }

        // trigger 3: Vines Appear EVEN MORE
        private IEnumerator VinesAppearEvenMore()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_4");
            this.Level.Shake(0.5f);
            this.anxietyFade += 0.15f;
            
            this.AddTentacle();
            yield return null;
            yield break;
        }

                // trigger 4: Vines Appear EVEN EVEN MORE
        private IEnumerator VinesAppearEvenEvenMore()
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_5");
            this.Level.Shake(1f);
            this.anxietyFade += 0.2f;
            
            if (!this.hidingNorthernLights)
            {
                base.Add(new Coroutine(this.NorthernLightsDown(), true));
                this.hidingNorthernLights = true;
            }
            
            this.AddTentacle();
            yield return null;
            yield break;
        }

        private void AddTentacle()
        {
            int num = 400;
            int num2 = 140;
            List<Vector2> list = new List<Vector2>();
            list.Add(new Vector2(this.Level.Camera.X + 160f, this.Level.Camera.Y + (float)num));
            list.Add(new Vector2(this.Level.Camera.X + 160f, this.Level.Camera.Y + (float)num + 200f));
            ReflectionTentacles tentacle = new ReflectionTentacles();
            tentacle.Create(0f, 0, this.tentacles.Count, list);
            tentacle.Nodes[0] = new Vector2(tentacle.Nodes[0].X, this.Level.Camera.Y + (float)num2);
            this.Level.Add(tentacle);
            this.tentacles.Add(tentacle);
            this.tentacleIndex++;
        }

        // trigger 4: Vines grab Madeline
        private IEnumerator VinesGrabMadeline()
        {
            this.maddySineTarget = 0f;
            Audio.Play("event:/desolozantas/game/08_truth/chara_freakout_6");
            this.player.Sprite.Play("tentacle_grab", true, true);
            yield return 0.1f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            this.Level.Shake(0.5f);
            this.Level.Add(this.rumbler);
            yield break;
        }

        // trigger 7: chara start circling the madeline
        private IEnumerator CharaStartCircling()
        {
            // Chara starts circling around Madeline menacingly
            base.Add(new Coroutine(this.CharaCirclePlayer(), true));
            Vector2 from = this.player.Position;
            Vector2 to = new Vector2((float)this.Level.Bounds.Center.X, this.player.Y);
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.CubeOut, delegate(Tween t)
            {
                this.player.Position = Vector2.Lerp(from, to, t.Eased);
            }, null);
            yield return null;
            yield break;
        }

        private IEnumerator CharaCirclePlayer()
        {
            float offset = 0f;
            float dist = (this.chara.Position - this.player.Position).Length();
            this.baddyCircling = true;
            while (this.baddyCircling)
            {
                offset -= Engine.DeltaTime * 5f;  // Faster, more aggressive circling
                dist = Calc.Approach(dist, 28f, Engine.DeltaTime * 40f);
                this.chara.Position = this.player.Position + Calc.AngleToVector(offset, dist);
                int num = Math.Sign(this.player.X - this.chara.X);
                if (num != 0)
                {
                    this.chara.Sprite.Scale.X = (float)num;
                }
                if (this.Level.OnInterval(0.08f))
                {
                    TrailManager.Add(this.chara, Color.Red * 0.7f, 1f, false, false);
                }
                yield return null;
            }
            this.chara.Sprite.Scale.X = -1f;
            yield break;
        }

        // trigger 9: heartgem minigame
        private IEnumerator HeartgemMinigame()
        {
            // Start the breathing/heartgem minigame (cutscene manages player, so freezePlayer=false)
            this.breathing = new MaggyBreathingGame(freezePlayer: false);
            this.rumbler.TrackBreathingGame(this.breathing);
            this.Level.Add(this.breathing);
            while (!this.breathing.Completed)
            {
                yield return null;
            }
            yield break;
        }

        // trigger 10: Break Heart minigame
        private IEnumerator BreakHeartMinigame()
        {
            // Chara interrupts and breaks the heartgem minigame
            this.baddyCircling = false;
            
            // Dramatic interruption
            this.Level.Shake(0.4f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/", "slices", 1f);
            
            this.Level.Flash(Color.Red * 0.3f, false);
            
            if (this.breathing != null)
            {
                this.breathing.RemoveSelf();
            }
            
            yield return 0.3f;
            
            // Chara delivers final threat
            if (this.chara != null)
            {
                this.chara.Sprite.Play("fallSlow", false, false);
            }
            
            this.breathing = null;
            yield break;
        }

        private IEnumerator CharaFlyDown()
        {
            this.chara.Sprite.Play("fallSlow", false, false);
            this.chara.FloatSpeed = 600f;
            this.chara.FloatAccel = 1200f;
            yield return this.chara.FloatTo(new Vector2(this.chara.X, this.Level.Camera.Y + 200f), null, true, true, false);
            this.chara.RemoveSelf();
            yield break;
        }

        private IEnumerator NorthernLightsDown()
        {
            NorthernLights bg = this.Level.Background.Get<NorthernLights>();
            if (bg != null)
            {
                while (bg.NorthernLightsAlpha > 0f)
                {
                    bg.NorthernLightsAlpha -= Engine.DeltaTime * 0.5f;
                    yield return null;
                }
            }
            yield break;
        }
        
        private IEnumerator SpinMaddyAndChara()
        {
            Vector2 maddyStart = this.player.Position;
            Vector2 charaStart = this.chara.Position;
            Vector2 center = (maddyStart + charaStart) / 2f;
            float dist = Math.Abs(maddyStart.X - center.X);
            float timer = 1.5707964f;
            this.player.Sprite.Play("spin", false, false);
            this.chara.Sprite.Play("spin", false, false); // Chara uses idle or appropriate animation
            this.chara.Sprite.Scale.X = 1f;
            
            while (this.charactersSpinning)
            {
                int num = (int)(timer / 6.2831855f * 14f + 10f);
                this.player.Sprite.SetAnimationFrame(num);
                
                float sinVal = (float)Math.Sin((double)timer);
                float cosVal = (float)Math.Cos((double)timer);
                this.player.Position = center - new Vector2(sinVal * dist, cosVal * 8f);
                this.chara.Position = center + new Vector2(sinVal * dist, cosVal * 8f);
                
                // Update Chara's facing based on position relative to Madeline
                int charaFacing = Math.Sign(this.player.X - this.chara.X);
                if (charaFacing != 0)
                {
                    this.chara.Sprite.Scale.X = (float)charaFacing;
                }
                
                // Add trail effect for dramatic spinning
                if (this.Level.OnInterval(0.1f))
                {
                    TrailManager.Add(this.chara, Color.Red * 0.5f, 0.5f, false, false);
                    TrailManager.Add(this.player, Color.LightBlue * 0.5f, 0.5f, false, false);
                }
                
                timer += Engine.DeltaTime * 2.5f; // Slightly faster spinning
                yield return null;
            }
            
            // Stop spinning - settle into positions
            this.player.Facing = Facings.Right;
            this.player.Sprite.Play("fallSlow", false, false);
            this.chara.Sprite.Scale.X = -1f;
            this.chara.Sprite.Play("fallSlow", false, false);
            
            Vector2 maddyFrom = this.player.Position;
            Vector2 charaFrom = this.chara.Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 3f)
            {
                this.player.Position = Vector2.Lerp(maddyFrom, maddyStart, Ease.CubeOut(p));
                this.chara.Position = Vector2.Lerp(charaFrom, charaStart, Ease.CubeOut(p));
                yield return null;
            }
            yield break;
        }

        public override void OnEnd(Level level)
        {
            if (this.rumbler != null)
            {
                this.rumbler.RemoveSelf();
                this.rumbler = null;
            }
            if (this.breathing != null)
            {
                this.breathing.RemoveSelf();
            }
            this.SetBloom(0f);
            level.Session.Audio.Music.Event = null;
            level.Session.Audio.Apply(false);
            level.Remove(this.player);
            level.UnloadLevel();
            level.EndCutscene();
            level.Session.SetFlag("plateaumaggy_2", true);
            level.SnapColorGrade(AreaData.Get(level).ColorGrade);
            level.Session.Dreaming = false;
            level.Session.FirstLevel = false;
            if (this.WasSkipped)
            {
                level.OnEndOfFrame += delegate()
                {
                    level.Session.Level = "a-00";
                    level.Session.RespawnPoint = new Vector2?(level.GetSpawnPoint(new Vector2((float)level.Bounds.Left, (float)level.Bounds.Bottom)));
                    level.LoadLevel(global::Celeste.Player.IntroTypes.None, false);
                    FallEffects.Show(false);
                    level.Session.Audio.Music.Event = "event:/desolozantas/Music/lvl8/main";
                    level.Session.Audio.Apply(false);
                };
                return;
            }
            // Use the factory method which handles level loading properly
            // This avoids stale level reference issues and mod render crashes
            Engine.Scene = MaggyOverworldReflectionFall.CreateFor08TruthAlt1(level);
        }

        private void SetBloom(float add)
        {
            this.Level.Session.BloomBaseAdd = add;
            this.Level.Bloom.Base = AreaData.Get(this.Level).BloomBase + add;
        }
    }
}



