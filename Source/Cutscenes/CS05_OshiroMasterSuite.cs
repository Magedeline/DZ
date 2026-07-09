#pragma warning disable CS0436
using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;
using Celeste.NPCs;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes
{
    public class CS05_OshiroMasterSuite : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_suite";

        private global::Celeste.Player player;

        private NPC oshiro;

        private global::Celeste.Entities.BadelineDummy badeline;

        private RalseiDummy ralsei;

        private global::Celeste.Entities.ResortMirror mirror;

        public CS05_OshiroMasterSuite(NPC oshiro)
        {
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level)
        {
            mirror = base.Scene.Entities.FindFirst<global::Celeste.Entities.ResortMirror>();
            Add(new Coroutine(Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    break;
                }
                yield return null;
            }
            Audio.SetMusic(null);
            yield return 0.4f;
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            if (oshiro != null)
            {
                Add(new Coroutine(player.DummyWalkTo(oshiro.X + 2f, false, 1f, false), true));
            }
            yield return 1f;
            badeline = new global::Celeste.Entities.BadelineDummy(player.Position + new Vector2(-24f, -16f));
            Scene.Add(badeline);
            Audio.SetMusic("event:/DZ/music/lvl5/oshiro_theme");
            yield return Textbox.Say("DZ_CH5_OSHIRO_SUITE", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(BadelineLookAround),
                new Func<IEnumerator>(RalseiAppear),
                new Func<IEnumerator>(CharaAppearInMirror),
                new Func<IEnumerator>(CharaBreakMirror),
                new Func<IEnumerator>(EveryoneStepCloser),
                new Func<IEnumerator>(EveryoneJumpBack),
                new Func<IEnumerator>(SuiteCharaCeiling)
            });
            yield return SceneAs<Level>().ZoomBack(0.5f);
            yield return badeline.FloatTo(new Vector2(badeline.X, (float)(level.Bounds.Top - 0.8f)), null, true, false, false);
            Scene.Remove(badeline);
            while (level.Lighting.Alpha != level.BaseLightingAlpha)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, level.BaseLightingAlpha, Engine.DeltaTime * 0.5f);
                yield return null;
            }
            EndCutscene(level, true);
            yield break;
        }

        private IEnumerator BadelineLookAround()
        {
            yield return 1f;
            yield return badeline.FloatTo(badeline.Position + new Vector2(0f, 8f), null, true, false, true);
            badeline.Floatness = 0f;
            yield return 0.5f;
            badeline.Sprite.Play("idle", false, false);
            Audio.Play("event:/char/badeline/landing", badeline.Position);
            yield return 1f;
            yield return badeline.WalkTo(player.X - 0f, 6f);
            badeline.Sprite.Scale.X = 1f;
            yield return 0.2f;
            Audio.Play("event:/char/badeline/duck", badeline.Position);
            yield return 0.3f;
            badeline.Sprite.Play("duck", false, false);
            yield return 1f;
            badeline.Sprite.Play("lookUp", false, false);
            yield return 1f;
            badeline.Sprite.Play("idle", false, false);
            yield return 0.4f;
            badeline.Sprite.Scale.X = 1f;
            yield return badeline.FloatTo(new Vector2(player.X - 2f, badeline.Y), null, false, false, false);
            yield return 0.5f;
            Level level = SceneAs<Level>();
            yield return level.ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
            yield break;
        }

        private IEnumerator RalseiAppear()
        {
            ralsei = new RalseiDummy(badeline.Position + new Vector2(24f, -8f));
            Scene.Add(ralsei);
            Level.Displacement.AddBurst(ralsei.Center, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("event:/char/badeline/maddy_split", badeline.Position);
            ralsei.Sprite.Scale.X = -1f;
            yield return 0.2f;
            yield break;
        }

        private IEnumerator CharaAppearInMirror()
        {
            if (mirror != null)
            {
                mirror.EvilAppear();
                SetEvilMusic();
                Audio.Play("event:/DZ/game/05_restore/suiteDZ_CHara_intro", mirror.Position);
                Vector2 from = Level.ZoomFocusPoint;
                Vector2 to = new Vector2(216f, 110f);
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
                {
                    Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                    yield return null;
                }
                yield return null;
            }
            yield break;
        }

        private IEnumerator CharaBreakMirror()
        {
            if (mirror != null)
            {
                Audio.Play("event:/DZ/game/05_restore/suite_bad_mirrorbreak", mirror.Position);
                yield return mirror.SmashRoutine();
                yield return 1.2f;
                if (oshiro != null && oshiro.Sprite != null)
                {
                    oshiro.Sprite.Scale.X = 1f;
                }
                yield return Level.ZoomBack(0.5f);
            }
            yield break;
        }

        private IEnumerator EveryoneStepCloser()
        {
            if (oshiro != null)
            {
                yield return player.DummyWalkToExact((int)oshiro.X - 16, false);
                yield return ralsei.DummyWalkToExact((int)oshiro.X - 8, false);
                yield return badeline.DummyWalkToExact((int)oshiro.X, 4, false);
            }
            yield break;
        }

        private IEnumerator EveryoneJumpBack()
        {
            if (oshiro != null)
            {
                yield return player.DummyWalkToExact((int)oshiro.X - 24, true);
                yield return ralsei.DummyWalkToExact((int)oshiro.X - 16, 4, true);
                yield return badeline.DummyWalkToExact((int)oshiro.X - 8, 4, true);
            }
            yield return 0.8f;
            yield break;
        }

        private IEnumerator SuiteCharaCeiling()
        {
            yield return SceneAs<Level>().ZoomBack(0.5f);
            ralsei.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_movestageleft"));
            yield return ralsei.FloatTo(new Vector2(Level.Bounds.Left + 96, ralsei.Y - 16f), 1);
            player.Facing = Facings.Left;
            yield return 0.25f;
            ralsei.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_ceilingbreak"));
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Level.DirectionalShake(-Vector2.UnitY);
            yield return ralsei.SmashBlock(ralsei.Position + new Vector2(0.0f, -32f));
            yield return 0.8f;
        }

        private void SetEvilMusic()
        {
            if (Level.Session.Area.Mode == AreaMode.Normal)
            {
                Level level = base.Scene as Level;
                level.Session.Audio.Music.Event = "event:/DZ/music/lvl2/evilDZ_CHara";
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (badeline != null)
                {
                    base.Scene.Remove(badeline);
                }
                if (mirror != null)
                {
                    mirror.Broken();
                }
                base.Scene.Entities.FindFirst<DashBlock>()?.RemoveAndFlagAsGone();
                if (oshiro != null && oshiro.Sprite != null)
                {
                    oshiro.Sprite.Play("idle_ground", false, false);
                }
            }
            if (oshiro != null && oshiro.Talker != null)
            {
                oshiro.Talker.Enabled = true;
            }
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }
            level.Lighting.Alpha = level.BaseLightingAlpha;
            level.Session.SetFlag("oshiro_resort_suite");
            SetEvilMusic();
        }
    }
}

