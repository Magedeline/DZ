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

        // Idle animation for placeholder circles
        private SineWave idleSine = null!;

        // Boss placeholder colours (matches KirbyFinalBattleScene.ZeroAuraColors + ELS Termina)
        private static readonly Color[] BossPlaceholderColors = new Color[]
        {
            Calc.HexToColor("8800ff"), // Siamo Zero
            Calc.HexToColor("0044ff"), // Zero 3
            Calc.HexToColor("ff0044"), // Contra Void
            Calc.HexToColor("00ffcc"), // Tesseract Soul
            Calc.HexToColor("888800"), // Hyper Meta Morpho Knight
            Calc.HexToColor("440044"), // Nollus Nova
        };
        private static readonly Color ELSColor = Calc.HexToColor("ff00ff"); // ELS Termina
        private static readonly Color NightmareColor = Calc.HexToColor("9900ff"); // NightmareSequenceBoss

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

            idleSine = new SineWave(0.6f).Randomize();
            Add(idleSine);
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

        // ── Render ────────────────────────────────────────────────────────────
        // Draws coloured placeholder circles at boss positions, matching the
        // WarpStarLaunchPad placeholder style (filled inner circle + outer outline).
        public override void Render()
        {
            base.Render();

            float r = 14f + idleSine.Value * 2f;

            // ── SiamoZeroFinalBoss entities (Tracked) ───────────────────────
            string[] zeroNames = { "Siamo Zero", "Zero 3", "Contra Void",
                "Tesseract Soul", "Hyper Meta Morpho", "Nollus Nova" };
            int zeroIdx = 0;
            foreach (Entity entity in Scene.Tracker.GetEntities<SiamoZeroFinalBoss>())
            {
                if (entity == null || !entity.Visible) continue;

                Color color = zeroIdx < BossPlaceholderColors.Length
                    ? BossPlaceholderColors[zeroIdx] : Color.White;

                Vector2 pos = entity.Position + new Vector2(0f, idleSine.Value * 2f);
                Draw.Circle(pos, r, color, 5);
                Draw.Circle(pos, r + 3f, color * 0.4f, 3);

                string label = zeroIdx < zeroNames.Length ? zeroNames[zeroIdx] : "Zero";
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont, label,
                    pos + new Vector2(-20f, -r - 14f),
                    color, 0f, Vector2.Zero, 0.4f,
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
                zeroIdx++;
            }

            // ── ELSTerminaFinalBoss + NightmareSequenceBoss (not Tracked — scan by index) ──
            int entityCount = Scene.Entities.Count;
            for (int i = 0; i < entityCount; i++)
            {
                Entity e = Scene.Entities[i];
                if (e == null) continue;

                if (e is ELSTerminaFinalBoss els && e.Visible)
                {
                    Vector2 pos = e.Position + new Vector2(0f, idleSine.Value * 2f);
                    Draw.Circle(pos, r + 4f, ELSColor, 6);
                    Draw.Circle(pos, r + 7f, ELSColor * 0.4f, 3);
                    Draw.SpriteBatch.DrawString(
                        Draw.DefaultFont, "ELS Termina",
                        pos + new Vector2(-28f, -r - 18f),
                        ELSColor, 0f, Vector2.Zero, 0.4f,
                        Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
                }
                else if (e is NightmareSequenceBoss nm && e.Visible)
                {
                    Vector2 pos = e.Position + new Vector2(0f, idleSine.Value * 2f);
                    Draw.Circle(pos, r + 2f, NightmareColor, 5);
                    Draw.Circle(pos, r + 5f, NightmareColor * 0.4f, 3);
                    Draw.SpriteBatch.DrawString(
                        Draw.DefaultFont, "Nightmare",
                        pos + new Vector2(-24f, -r - 14f),
                        NightmareColor, 0f, Vector2.Zero, 0.4f,
                        Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
                }
            }
        }
    }
}
