#nullable enable
using System;
using System.Collections;
using Celeste.Bosses;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Kirby Flying Final Battle — Warp Star Launch Pad
    ///
    /// Place this entity in the pre-battle room (any room in 21_LastLevel.bin).
    /// When both Kirby and Madeline are declared ready, the player can walk up
    /// and press the Interact / Jump key to board the Warp Star.
    ///
    /// Mount sequence
    /// ──────────────
    ///  1. Player approaches and presses Grab/Talk while inside the interaction radius.
    ///  2. A "ready?" prompt is shown.  If ReadyConditionFlag is set (or auto-ready), boarding begins.
    ///  3. Launch SFX + gold flash.  Player is frozen (StDummy).
    ///  4. Warp Star bob animation plays on the player while the room fades out.
    ///  5. Room teleport:
    ///       level.Session.Level = FinalBattleRoom  (default "ch21-final-battle")
    ///       level.Session.RespawnPoint = spawn in that room
    ///       level.LoadLevel(IntroTypes.None)
    ///     → KirbyFinalBattleScene.StartBattle() fires automatically in Awake().
    ///
    /// Return teleport (called by KirbyFinalBattleScene after Phase 1 + Phase 2 void done)
    /// ──────────────────────────────────────────────────────────────────────────────────────
    ///  • KirbyFinalBattleScene sets the session flag ReturnFlag and sets session.Level
    ///    back to ReturnRoom, then calls LoadLevel.
    ///  • Phase 2 (ELS Termina boss fight) then starts in the return room because
    ///    the FinalBattlePhaseShiftTriggers detect ReturnFlag.
    ///
    /// Level designer knobs (EntityData properties)
    /// ──────────────────────────────────────────────
    ///   finalBattleRoom     string  "ch21-final-battle"
    ///                         — session.Level name of the room containing KirbyFinalBattleScene
    ///   returnRoom          string  ""
    ///                         — session.Level to return to after phases; empty = stay in battle room
    ///   returnSpawnX        int     0
    ///   returnSpawnY        int     0
    ///                         — pixel position of the spawn point in the return room
    ///   kirbyReadyFlag      string  "ch21_kirby_ready"
    ///                         — session flag; if set, Kirby is ready (shown as lit indicator)
    ///   madelineReadyFlag   string  "ch21_madeline_ready"
    ///                         — session flag; if set, Madeline is ready
    ///   autoReady           bool    true
    ///                         — if true, both indicators are auto-lit (skip manual flag checks)
    ///   requireBothReady    bool    true
    ///                         — if false, boarding is allowed even if readiness flags aren't set
    ///   onlyOnce            bool    true
    ///                         — remove self after first use
    ///   launchSfx           string  "event:/pusheen/new_content/game/21_desolo_zantas/warpstar_launch"
    ///   rideSfx             string  "event:/pusheen/new_content/music/lvl21/warpstar_ride"
    ///   bobAmplitude        float   3.0
    ///   bobSpeed            float   2.5
    ///   preLaunchDelay      float   1.2   — seconds of bob before the teleport fires
    ///   interactRadius      float   24.0  — pixel radius within which Grab triggers boarding
    /// </summary>
    [CustomEntity("DZ/WarpStarLaunchPad")]
    [Tracked(true)]
    [HotReloadable]
    public class WarpStarLaunchPad : Entity
    {
        // ── Config ─────────────────────────────────────────────────────────────
        private readonly string finalBattleRoom;
        private readonly string returnRoom;
        private readonly int    returnSpawnX;
        private readonly int    returnSpawnY;
        private readonly string kirbyReadyFlag;
        private readonly string madelineReadyFlag;
        private readonly bool   autoReady;
        private readonly bool   requireBothReady;
        private readonly bool   onlyOnce;
        private readonly string launchSfx;
        private readonly string rideSfx;
        private readonly float  bobAmplitude;
        private readonly float  bobSpeed;
        private readonly float  preLaunchDelay;
        private readonly float  interactRadius;

        // ── Runtime ───────────────────────────────────────────────────────────
        private Level  level = null!;
        private Player? player;
        private bool   launching;
        private bool   used;

        // Visuals
        private Image?       starImg;
        private VertexLight  starLight;
        private BloomPoint   starBloom;
        private SineWave     idleSine;
        private Wiggler      collectWiggle;
        private Image?       kirbyIndicator;
        private Image?       madelineIndicator;
        private TalkComponent? talkPrompt;

        // Bob state
        private float bobTimer;
        private float bobAnchorY;
        private bool  bobAnchorSet;

        // Static particle types (reuse existing WarpStar ones where possible)
        private static readonly ParticleType P_Launch = new ParticleType
        {
            Color          = Calc.HexToColor("FFD700"),
            Color2         = Calc.HexToColor("ffffff"),
            ColorMode      = ParticleType.ColorModes.Blink,
            FadeMode       = ParticleType.FadeModes.Late,
            Size           = 1f,
            LifeMin        = 0.5f,
            LifeMax        = 1.2f,
            SpeedMin       = 40f,
            SpeedMax       = 120f,
            DirectionRange = MathHelper.TwoPi
        };

        // ── Constructor ───────────────────────────────────────────────────────
        public WarpStarLaunchPad(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            finalBattleRoom   = data.Attr("finalBattleRoom",   "ch21-final-battle");
            returnRoom        = data.Attr("returnRoom",        "");
            returnSpawnX      = data.Int("returnSpawnX",       0);
            returnSpawnY      = data.Int("returnSpawnY",       0);
            kirbyReadyFlag    = data.Attr("kirbyReadyFlag",    "ch21_kirby_ready");
            madelineReadyFlag = data.Attr("madelineReadyFlag", "ch21_madeline_ready");
            autoReady         = data.Bool("autoReady",         true);
            requireBothReady  = data.Bool("requireBothReady",  true);
            onlyOnce          = data.Bool("onlyOnce",          true);
            launchSfx         = data.Attr("launchSfx",
                "event:/pusheen/new_content/game/21_desolo_zantas/warpstar_launch");
            rideSfx           = data.Attr("rideSfx",
                "event:/pusheen/new_content/music/lvl21/warpstar_ride");
            bobAmplitude      = data.Float("bobAmplitude",     3f);
            bobSpeed          = data.Float("bobSpeed",         2.5f);
            preLaunchDelay    = data.Float("preLaunchDelay",   1.2f);
            interactRadius    = data.Float("interactRadius",   24f);

            Depth  = -100;
            Collider = new Circle(interactRadius);
        }

        // ── Added ─────────────────────────────────────────────────────────────
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();

            // ── Warp Star visuals ─────────────────────────────────────────────
            bool hasStarSprite = GFX.SpriteBank.SpriteData.ContainsKey("warpstars");

            if (hasStarSprite)
            {
                var spr = GFX.SpriteBank.Create("warpstars");
                spr.CenterOrigin();
                Add(spr);
            }
            else if (GFX.Game.Has("objects/DZ/DZ/warpstars/idle00"))
            {
                starImg = new Image(GFX.Game["objects/DZ/DZ/warpstars/idle00"]);
                starImg.CenterOrigin();
                Add(starImg);
            }
            else
            {
                // Placeholder: bright yellow circle rendered in Render()
            }

            idleSine     = new SineWave(0.6f).Randomize();
            collectWiggle = Wiggler.Create(1f, 4f);
            starLight    = new VertexLight(Calc.HexToColor("FFD700"), 1f, 32, 80);
            starBloom    = new BloomPoint(0.6f, 28f);
            Add(idleSine, collectWiggle, starLight, starBloom);

            // ── Readiness indicators ──────────────────────────────────────────
            // Small coloured dots: Kirby (pink) left, Madeline (blue) right
            bool kiIndicator = GFX.Game.Has("util/pixel");
            if (kiIndicator)
            {
                kirbyIndicator = new Image(GFX.Game["util/pixel"]);
                kirbyIndicator.Scale  = new Vector2(6f, 6f);
                kirbyIndicator.Origin = new Vector2(0.5f, 0.5f);
                kirbyIndicator.Position = new Vector2(-12f, -24f);
                Add(kirbyIndicator);

                madelineIndicator = new Image(GFX.Game["util/pixel"]);
                madelineIndicator.Scale  = new Vector2(6f, 6f);
                madelineIndicator.Origin = new Vector2(0.5f, 0.5f);
                madelineIndicator.Position = new Vector2(12f, -24f);
                Add(madelineIndicator);
            }

            // ── Talk / interact prompt ────────────────────────────────────────
            talkPrompt = new TalkComponent(
                new Rectangle(-16, -8, 32, 8),
                new Vector2(0f, -28f),
                OnInteract);
            Add(talkPrompt);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = scene.Tracker.GetEntity<Player>();
        }

        // ── Interact callback ─────────────────────────────────────────────────
        private void OnInteract(Player p)
        {
            if (launching || used) return;
            if (requireBothReady && !autoReady)
            {
                bool kirbyOk   = level.Session.GetFlag(kirbyReadyFlag);
                bool madelineOk = level.Session.GetFlag(madelineReadyFlag);
                if (!kirbyOk || !madelineOk) return;
            }
            launching = true;
            player    = p;
            Add(new Coroutine(LaunchRoutine(p)));
        }

        // ── Launch coroutine ──────────────────────────────────────────────────
        private IEnumerator LaunchRoutine(Player p)
        {
            // Disable the talk prompt so it can't be triggered twice
            if (talkPrompt != null) talkPrompt.Enabled = false;

            // Freeze the player
            p.StateMachine.State  = Player.StDummy;
            p.DummyAutoAnimate     = true;
            p.EnforceLevelBounds   = false;
            level.StartCutscene(OnCutsceneEnd);

            // Set session flags
            level.Session.SetFlag(kirbyReadyFlag);
            level.Session.SetFlag(madelineReadyFlag);
            level.Session.SetFlag("ch21_warpstar_launched");

            // ── Visual burst + SFX ────────────────────────────────────────────
            level.Shake(0.8f);
            Audio.Play(launchSfx, Position);
            level.Flash(Calc.HexToColor("FFD700") * 0.9f, true);
            level.ParticlesFG.Emit(P_Launch, 32, Position, Vector2.One * 12f);

            collectWiggle.Start();

            // ── Bob animation on the player ───────────────────────────────────
            bobAnchorSet = false;
            float elapsed = 0f;
            while (elapsed < preLaunchDelay)
            {
                elapsed += Engine.DeltaTime;
                bobTimer += Engine.DeltaTime * bobSpeed;

                if (!bobAnchorSet)
                {
                    bobAnchorY  = p.Y;
                    bobAnchorSet = true;
                }
                p.Y = bobAnchorY + (float)Math.Sin(bobTimer) * bobAmplitude;

                // Emit trail particles
                level.ParticlesFG.Emit(WarpStar.P_Boost, 2, p.Center, Vector2.One * 4f);
                yield return null;
            }

            // ── Screen flash → teleport ───────────────────────────────────────
            level.Flash(Color.White, true);
            yield return 0.1f;

            Audio.SetMusic(rideSfx);

            // Store strawberries / inventory
            Leader.StoreStrawberries(p.Leader);
            level.Remove(p);

            // Unload current room
            level.UnloadLevel();

            // Set destination room
            level.Session.Level         = finalBattleRoom;
            level.Session.RespawnPoint  = level.GetSpawnPoint(
                new Vector2(level.Bounds.Left, level.Bounds.Top));
            level.Session.FirstLevel    = false;

            // Load the final battle room — KirbyFinalBattleScene.Awake() will call StartBattle()
            level.LoadLevel(Player.IntroTypes.None);

            // Mark as used
            if (onlyOnce) used = true;
        }

        // Called if cutscene is skipped
        private void OnCutsceneEnd(Level l)
        {
            launching = false;
            player    = l.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.EnforceLevelBounds = true;
            }
        }

        // ── Update ────────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            if (used)
            {
                Visible   = false;
                Collidable = false;
                return;
            }

            // Idle float on the star sprite / placeholder
            if (starImg != null)
                starImg.Y = idleSine.Value * 2f;

            // Update readiness indicators
            if (kirbyIndicator != null)
            {
                bool ready = autoReady || level.Session.GetFlag(kirbyReadyFlag);
                kirbyIndicator.Color = ready
                    ? Calc.HexToColor("ff99cc")         // Kirby pink — lit
                    : Calc.HexToColor("ff99cc") * 0.3f; // dim
            }
            if (madelineIndicator != null)
            {
                bool ready = autoReady || level.Session.GetFlag(madelineReadyFlag);
                madelineIndicator.Color = ready
                    ? Calc.HexToColor("4488ff")         // Madeline blue — lit
                    : Calc.HexToColor("4488ff") * 0.3f; // dim
            }

            // Light pulse
            starLight.Alpha = 0.7f + idleSine.Value * 0.3f;
            starBloom.Alpha = starLight.Alpha * 0.8f;

            // Hide talk prompt if not both ready and requireBothReady is on
            if (talkPrompt != null && !launching)
            {
                bool canBoard = !requireBothReady || autoReady ||
                    (level.Session.GetFlag(kirbyReadyFlag) && level.Session.GetFlag(madelineReadyFlag));
                talkPrompt.Enabled = canBoard;
            }
        }

        // ── Render ────────────────────────────────────────────────────────────
        public override void Render()
        {
            base.Render();
            if (used) return;

            // Fallback placeholder: yellow star circle with outline
            if (starImg == null && !GFX.SpriteBank.SpriteData.ContainsKey("warpstars"))
            {
                float r = 10f + collectWiggle.Value * 3f + idleSine.Value * 1.5f;
                Draw.Circle(Position + new Vector2(0f, idleSine.Value * 2f),
                    r, Calc.HexToColor("FFD700"), 5);
                Draw.Circle(Position + new Vector2(0f, idleSine.Value * 2f),
                    r + 2f, Calc.HexToColor("FFD700") * 0.4f, 3);
            }

            // Labels above indicators
            if (kirbyIndicator != null)
            {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont, "K",
                    Position + kirbyIndicator.Position + new Vector2(-3f, -12f),
                    Calc.HexToColor("ff99cc"), 0f, Vector2.Zero, 0.45f,
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            }
            if (madelineIndicator != null)
            {
                Draw.SpriteBatch.DrawString(
                    Draw.DefaultFont, "M",
                    Position + madelineIndicator.Position + new Vector2(-3f, -12f),
                    Calc.HexToColor("4488ff"), 0f, Vector2.Zero, 0.45f,
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RETURN CONTROLLER
    //  Placed inside the finalBattleRoom (alongside KirbyFinalBattleScene).
    //  After both Phase 1 and Phase 2 void are complete, teleports the player
    //  back to the original room and sets the Phase2 activation flag so the
    //  ELS Termina boss fight starts.
    // ══════════════════════════════════════════════════════════════════════════

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
