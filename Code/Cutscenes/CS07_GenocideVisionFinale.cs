using System;
using FMOD.Studio;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS07_GenocideVisionFinale : CutsceneEntity
    {
        private const string MainCharaRevealMusicEvent = "event:/music/pusheen/lvl7/him";
        private const string MainCharaRevealParam = "him";

        private readonly global::Celeste.Player player;
        private MainCharaVisionActor mainChara;
        private BadelineDummy badeline;
        private Fader fader;
        private bool loadWakeupRoom;
        private bool followMainCharaCamera;
        private EventInstance mainCharaRevealMusic;
        private bool mainCharaRevealMusicStarted;

        public CS07_GenocideVisionFinale(global::Celeste.Player player)
            : base(true, false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag(CH7GenocideMirrorState.VisionFinalePlayedFlag))
            {
                RemoveSelf();
                return;
            }

            level.Add(fader = new Fader());
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.StateMachine.Locked = true;
            player.ForceCameraUpdate = true;

            Vector2 kirbyStart = player.Position;
            player.Facing = Facings.Right;

            badeline = new BadelineDummy(kirbyStart + new Vector2(20f, -6f));
            level.Add(badeline);
            badeline.Appear(level, true);
            badeline.Sprite.Scale.X = 1f;

            mainChara = new MainCharaVisionActor(kirbyStart + new Vector2(-48f, 0f));
            mainChara.Face(MainCharaVisionActor.FacingDirection.Right);
            level.Add(mainChara);

            yield return 0.2f;
            yield return level.ZoomTo(new Vector2(160f, 90f), 2f, 0.35f);
            yield return Textbox.Say("MAGGYHELPER_CH7_GENO_KIRBY_BADELINE_PANIC");

            Vector2 kirbyTarget = kirbyStart + new Vector2(64f, 0f);
            Vector2 badelineTarget = kirbyTarget + new Vector2(20f, -6f);

            Add(new Coroutine(badeline.FloatTo(badelineTarget, faceDirection: true, quickEnd: true)));
            yield return player.DummyWalkToExact((int)kirbyTarget.X, true);

            yield return 0.2f;
            player.ForceCameraUpdate = false;
            followMainCharaCamera = true;
            TriggerMainCharaRevealMusic();
            Add(new Coroutine(FollowMainCharaCamera(level)));
            yield return mainChara.MoveTo(kirbyTarget + new Vector2(10f, 0f), 88f);
            followMainCharaCamera = false;
            yield return CutsceneEntity.CameraTo(player.CameraTarget, 0.3f, Ease.CubeOut);
            level.Camera.Position = player.CameraTarget;
            player.ForceCameraUpdate = true;
            mainChara.PlayAttack();
            Audio.Play("event:/game/pusheen/08_edge/chara_heartgem_slice", kirbyTarget);

            fader.Target = 1f;
            Glitch.Value = 0.15f;
            yield return 0.65f;
            Glitch.Value = 0f;

            level.Session.SetFlag(CH7GenocideMirrorState.VisionFinalePlayedFlag);
            level.Session.SetFlag(CH7GenocideMirrorState.CompletedFlag);
            loadWakeupRoom = CH7GenocideMirrorState.HasRoom(level, CH7GenocideMirrorState.WakeupRoom);
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.ResetZoom();
            followMainCharaCamera = false;
            player.StateMachine.Locked = false;
            player.StateMachine.State = global::Celeste.Player.StNormal;
            player.ForceCameraUpdate = false;

            if (mainCharaRevealMusicStarted)
            {
                Audio.Stop(mainCharaRevealMusic);
                mainCharaRevealMusicStarted = false;
            }

            mainChara?.RemoveSelf();
            badeline?.RemoveSelf();

            if (!loadWakeupRoom)
            {
                if (fader != null)
                {
                    fader.Target = 0f;
                    fader.Ended = true;
                }
                return;
            }

            level.OnEndOfFrame += () =>
            {
                Glitch.Value = 0f;
                Distort.Anxiety = 0f;
                level.Session.ColorGrade = null;
                level.CanRetry = true;
                Leader.StoreStrawberries(player.Leader);
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = CH7GenocideMirrorState.WakeupRoom;
                level.Session.RespawnPoint = level.GetSpawnPoint(CH7GenocideMirrorState.GetRespawnProbe(level, CH7GenocideMirrorState.WakeupRoom));
                level.LoadLevel(global::Celeste.Player.IntroTypes.WakeUp);

                global::Celeste.Player loadedPlayer = level.Tracker.GetEntity<global::Celeste.Player>();
                if (loadedPlayer != null)
                {
                    Leader.RestoreStrawberries(loadedPlayer.Leader);
                }
            };
        }

        private IEnumerator FollowMainCharaCamera(Level level)
        {
            while (followMainCharaCamera && mainChara != null)
            {
                level.Camera.Position += (mainChara.CameraTarget - level.Camera.Position) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));
                yield return null;
            }
        }

        private void TriggerMainCharaRevealMusic()
        {
            if (mainCharaRevealMusicStarted)
            {
                return;
            }

            mainCharaRevealMusic = Audio.Play(MainCharaRevealMusicEvent);
            Audio.SetParameter(mainCharaRevealMusic, MainCharaRevealParam, 1f);
            mainCharaRevealMusicStarted = true;
        }

        private class Fader : Entity
        {
            public float Target;
            public bool Ended;
            private float fade;

            public Fader()
            {
                Depth = -1000000;
            }

            public override void Update()
            {
                fade = Calc.Approach(fade, Target, Engine.DeltaTime * 2f);
                if (Ended && fade <= 0f)
                {
                    RemoveSelf();
                }
                base.Update();
            }

            public override void Render()
            {
                Camera camera = (Scene as Level)?.Camera;
                if (camera != null && fade > 0f)
                {
                    Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
                }
            }
        }
    }
}

