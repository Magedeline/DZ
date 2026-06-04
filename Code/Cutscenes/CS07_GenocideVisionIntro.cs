using Celeste.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS07_GenocideVisionIntro : CutsceneEntity
    {
        private readonly global::Celeste.Player player;
        private MainCharaVisionActor mainChara;
        private FloweyNPC flowey;
        private GenocideVisionProp realKnife;
        private GenocideVisionProp heartLocket;
        private bool knifeCollected;
        private bool locketCollected;

        public CS07_GenocideVisionIntro(global::Celeste.Player player)
            : base(true, false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag(CH7GenocideMirrorState.VisionIntroPlayedFlag))
            {
                RemoveSelf();
                return;
            }

            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.StateMachine.Locked = true;
            player.Visible = false;
            player.ForceCameraUpdate = true;

            Vector2 origin = player.Position;

            mainChara = new MainCharaVisionActor(origin);
            realKnife = new GenocideVisionProp(origin + new Vector2(28f, -2f), GenocideVisionProp.PropKind.RealKnife);
            heartLocket = new GenocideVisionProp(origin + new Vector2(56f, -2f), GenocideVisionProp.PropKind.HeartLocket);
            flowey = new FloweyNPC(origin + new Vector2(92f, 0f), dialogId: null, startHidden: false);

            level.Add(mainChara);
            level.Add(realKnife);
            level.Add(heartLocket);
            level.Add(flowey);

            flowey.PlayExpression("idle");
            flowey.FaceTarget(mainChara.Position);

            yield return 0.3f;
            yield return level.ZoomTo(new Vector2(160f, 90f), 2f, 0.4f);
            yield return Textbox.Say("MAGGYHELPER_CH7_GENO_VORTEX_START");

            mainChara.EnablePlayerControl(true);

            while (!knifeCollected || !locketCollected)
            {
                if (!knifeCollected && mainChara.ConfirmPressed && Vector2.Distance(mainChara.Position, realKnife.Position) <= 16f)
                {
                    yield return CollectProp(level, realKnife, "MAGGYHELPER_CH7_GENO_KNIFE_PICKUP", () => knifeCollected = true);
                }
                else if (!locketCollected && mainChara.ConfirmPressed && Vector2.Distance(mainChara.Position, heartLocket.Position) <= 16f)
                {
                    yield return CollectProp(level, heartLocket, "MAGGYHELPER_CH7_GENO_LOCKET_PICKUP", () =>
                    {
                        locketCollected = true;
                        level.Session.SetFlag(CH7GenocideMirrorState.EquipmentTakenFlag);
                    });
                }

                yield return null;
            }

            mainChara.EnablePlayerControl(false);

            flowey.PlayExpression("creepy");
            flowey.FaceTarget(mainChara.Position);
            level.Session.SetFlag(CH7GenocideMirrorState.FloweySeenFlag);
            yield return 0.15f;
            yield return Textbox.Say("MAGGYHELPER_CH7_GENO_FLOWEY_WRONGNESS");

            level.Session.SetFlag(CH7GenocideMirrorState.VisionIntroPlayedFlag);
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.ResetZoom();
            player.Visible = true;
            player.StateMachine.Locked = false;
            player.StateMachine.State = global::Celeste.Player.StNormal;
            player.ForceCameraUpdate = false;

            if (!level.Session.GetFlag(CH7GenocideMirrorState.VisionIntroPlayedFlag) || level.Session.GetFlag(CH7GenocideMirrorState.VisionFinalePlayedFlag))
            {
                mainChara?.RemoveSelf();
                realKnife?.RemoveSelf();
                heartLocket?.RemoveSelf();
                flowey?.RemoveSelf();
                return;
            }

            bool loadExecutionRoom = CH7GenocideMirrorState.HasRoom(level, CH7GenocideMirrorState.ExecutionRoom);

            if (!loadExecutionRoom)
            {
                level.OnEndOfFrame += () =>
                {
                    global::Celeste.Player currentPlayer = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (currentPlayer != null && !level.Session.GetFlag(CH7GenocideMirrorState.VisionFinalePlayedFlag))
                    {
                        level.Add(new CS07_GenocideVisionFinale(currentPlayer));
                    }
                };
            }
            else
            {
                level.OnEndOfFrame += () =>
                {
                    Leader.StoreStrawberries(player.Leader);
                    level.Remove(player);
                    level.UnloadLevel();
                    level.Session.Level = CH7GenocideMirrorState.ExecutionRoom;
                    level.Session.RespawnPoint = level.GetSpawnPoint(CH7GenocideMirrorState.GetRespawnProbe(level, CH7GenocideMirrorState.ExecutionRoom));
                    level.LoadLevel(global::Celeste.Player.IntroTypes.WakeUp);

                    global::Celeste.Player loadedPlayer = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (loadedPlayer != null)
                    {
                        level.Add(new CS07_GenocideVisionFinale(loadedPlayer));
                        Leader.RestoreStrawberries(loadedPlayer.Leader);
                    }
                };
            }

            mainChara?.RemoveSelf();
            realKnife?.RemoveSelf();
            heartLocket?.RemoveSelf();
            flowey?.RemoveSelf();
        }

        private IEnumerator CollectProp(Level level, GenocideVisionProp prop, string dialogId, Action onCollected)
        {
            mainChara.EnablePlayerControl(false);
            mainChara.PlayPickup();
            Audio.Play("guid://{7a436113-5059-4565-a239-927c4876b345}", prop.Position);
            prop.Collect();
            onCollected?.Invoke();
            yield return 0.25f;
            yield return Textbox.Say(dialogId);
            mainChara.EnablePlayerControl(true);
        }
    }
}

