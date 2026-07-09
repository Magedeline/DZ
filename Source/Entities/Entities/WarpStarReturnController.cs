#nullable enable
using System.Collections;
using Celeste.Bosses;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Placed in the final battle room.  Listens for KirbyFinalBattleScene
    /// completing phases 1+2 (void), then teleports the player back to the
    /// ground room and sets the phase2 activation flag.
    ///
    /// Level designer knobs
    /// ─────────────────────
    ///   returnRoom         string  — session.Level of the ground arena room
    ///   returnSpawnX       int     — spawn X in that room
    ///   returnSpawnY       int     — spawn Y in that room
    ///   phase2ActivateFlag string  "ch21_els_termina_phase2_active"
    ///                              — set before loading the return room so
    ///                                FinalBattlePhaseShiftTrigger / boss intro
    ///                                know phase 2 is live.
    ///   autoListen         bool    true — subscribe to KirbyFinalBattleScene.OnPhaseChanged
    /// </summary>
    [CustomEntity("DZ/WarpStarReturnController")]
    [Tracked(true)]
    [HotReloadable]
    public class WarpStarReturnController : Entity
    {
        private readonly string returnRoom;
        private readonly int    returnSpawnX;
        private readonly int    returnSpawnY;
        private readonly string phase2ActivateFlag;
        private readonly bool   autoListen;

        private Level level = null!;
        private bool  hasSubscribed;
        private bool  returning;

        public WarpStarReturnController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            returnRoom         = data.Attr("returnRoom",         "");
            returnSpawnX       = data.Int("returnSpawnX",        0);
            returnSpawnY       = data.Int("returnSpawnY",        0);
            phase2ActivateFlag = data.Attr("phase2ActivateFlag", "ch21_els_termina_phase2_active");
            autoListen         = data.Bool("autoListen",         true);
            Depth = -10001;
            Tag   = Tags.Persistent;
        }

        public static WarpStarReturnController? Get(Scene scene) =>
            scene.Tracker.GetEntity<WarpStarReturnController>();

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!autoListen || hasSubscribed) return;

            var battle = KirbyFinalBattleScene.Get(scene);
            if (battle != null)
            {
                battle.OnPhaseChanged += OnBattlePhaseChanged;
                hasSubscribed = true;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            var battle = KirbyFinalBattleScene.Get(scene);
            if (battle != null && hasSubscribed)
                battle.OnPhaseChanged -= OnBattlePhaseChanged;
        }

        private void OnBattlePhaseChanged(KirbyFinalBattleScene.BattlePhase phase)
        {
            // After Phase2Scroll begins, void flight is over — return to ground
            if (phase == KirbyFinalBattleScene.BattlePhase.Phase2Scroll && !returning)
            {
                returning = true;
                Add(new Coroutine(ReturnRoutine()));
            }
        }

        /// <summary>
        /// Can also be called manually (e.g. from EventTrigger "ch21_warpstar_return").
        /// </summary>
        public void TriggerReturn()
        {
            if (returning) return;
            returning = true;
            Add(new Coroutine(ReturnRoutine()));
        }

        private IEnumerator ReturnRoutine()
        {
            // Short dramatic pause + flash before returning
            level.Flash(Color.White * 0.8f, true);
            yield return 0.4f;

            if (string.IsNullOrEmpty(returnRoom))
            {
                // Stay in same room — just set the flag and let the boss trigger activate
                level.Session.SetFlag(phase2ActivateFlag);
                returning = false;
                yield break;
            }

            var player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                Leader.StoreStrawberries(player.Leader);
                level.Remove(player);
            }

            // Set the phase2 flag BEFORE loading the room so triggers there see it immediately
            level.Session.SetFlag(phase2ActivateFlag);

            level.UnloadLevel();
            level.Session.Level        = returnRoom;
            level.Session.RespawnPoint = new Vector2(returnSpawnX, returnSpawnY);
            level.Session.FirstLevel   = false;

            level.LoadLevel(Player.IntroTypes.None);
        }
    }
}
