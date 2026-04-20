using Celeste.Entities;
using Celeste.NPCs;
using BadelineDummy = Celeste.Entities.BadelineDummy;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 5 Master Suite cutscene - Oshiro's haunted mirror encounter
    /// Features Chara emerging from the mirror while Badeline and Ralsei react
    /// </summary>
    public class CS05_OshiroMasterSuite(NPC oshiro) : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_suite";

        // Audio events
        private const string MusicOshiroTheme = "event:/desolozantas/music/lvl5/oshiro_theme";
        private const string MusicEvilChara = "event:/desolozantas/music/lvl2/evil_chara";
        private const string SfxCharaIntro = "event:/desolozantas/game/05_restore/suite_chara_intro";
        private const string SfxMirrorBreak = "event:/desolozantas/game/05_restore/suite_chara_mirrorbreak";
        private const string SfxBadMoveLeft = "event:/game/03_resort/suite_bad_movestageleft";
        private const string SfxCeilingBreak = "event:/game/03_resort/suite_bad_ceilingbreak";
        private const string SfxBadExit = "event:/game/03_resort/suite_bad_exittop";
        private const string SfxOshiroCollapse = "event:/char/oshiro/chat_collapse";

        // Character offsets
        private static readonly Vector2 BadelineOffset = new(-24f, -16f);
        private static readonly Vector2 RalseiOffset = new(-48f, -16f);

        // Walk limit: player cannot walk past this X during the cutscene
        private const float PlayerWalkLimitOffset = 24f;
        private float playerMaxX;
        private InvisibleBarrier walkBarrier;

        // References
        private readonly NPC oshiro = oshiro;
        private global::Celeste.Player player;
        private CharaDummy chara;
        private BadelineDummy badeline;
        private RalseiDummy ralsei;
        private Entities.ResortMirror mirror;

        public override void OnBegin(Level level)
        {
            mirror = Scene.Entities.FindFirst<Entities.ResortMirror>();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            // Wait for player to be available
            while ((player = Scene.Tracker.GetEntity<global::Celeste.Player>()) == null)
                yield return null;

            // Setup cutscene state
            Audio.SetMusic(null);
            yield return 0.4f;

            LockPlayer();
            InitializeOshiro();

            // Set walk limit barrier so player can't walk past Oshiro
            playerMaxX = oshiro.X - PlayerWalkLimitOffset;

            // Walk player toward Oshiro (clamped by barrier)
            Add(new Coroutine(player.DummyWalkTo(Math.Min(oshiro.X + 32f, playerMaxX))));
            yield return 0.5f;

            // Spawn Badeline with player (Ralsei appears later at trigger 1)
            SpawnBadeline();
            Level.Session.Audio.Music.Event = MusicOshiroTheme;
            Level.Session.Audio.Apply();

            // Main dialogue with event triggers matching dialog order:
            // 0: Badeline looks around the master suite room
            // 1: Ralsei appears and tells Badeline she pushed Oshiro too far
            // 2: Chara appears in the hotel mirror
            // 3: Chara breaks out of the hotel mirror
            // 4: Kirby/player steps closer to Oshiro
            // 5: Everyone jumps back
            yield return Textbox.Say("CH5_OSHIRO_SUITE",
                BadelineWander,
                RalseiAppear,
                CharaAppearInMirror,
                CharaBreakMirror,
                PlayerStepCloser,
                EveryoneJumpBack);

            // After dialogue: Chara dashes to ceiling and breaks the exit path
            yield return CharaBreakCeiling();

            // Chara exits
            if (chara != null)
            {
                chara.Add(new SoundSource(Vector2.Zero, "event:/desolozantas/game/05_restore/suite_bad_exittop"));
                Scene.Remove(chara);
            }

            // Restore lighting
            yield return RestoreLighting(level);

            EndCutscene(level);
        }

        #region Setup Methods

        private void LockPlayer()
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
        }

        private void UnlockPlayer()
        {
            if (player == null) return;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        private void InitializeOshiro()
        {
            oshiro.Sprite.Visible = true;
            oshiro.Sprite.Play("idle");
        }

        private void SpawnBadeline()
        {
            badeline = new BadelineDummy(player.Position + BadelineOffset);
            Scene.Add(badeline);
        }

        private void SpawnRalsei()
        {
            ralsei = new RalseiDummy(player.Position + RalseiOffset);
            Scene.Add(ralsei);
        }

        /// <summary>
        /// Spawns an invisible solid barrier at the player walk limit
        /// so the player physically cannot walk past it during the cutscene.
        /// </summary>

        private void RemoveWalkBarrier()
        {
            if (walkBarrier != null)
            {
                Scene.Remove(walkBarrier);
                walkBarrier = null;
            }
        }

        #endregion

        #region Dialogue Event Handlers

        /// <summary>trigger 0: Badeline lands and looks around the master suite room</summary>
        private IEnumerator BadelineWander()
        {
            yield return 1f;
            yield return this.badeline.FloatTo(this.badeline.Position + new Vector2(0f, 8f), null, true, false, true);
            this.badeline.Floatness = 0f;
            this.badeline.AutoAnimator.Enabled = false;
            this.badeline.Sprite.Play("idle", false, false);
            Audio.Play("event:/char/badeline/landing", this.badeline.Position);

            // Badeline looks up curiously
            badeline.Sprite.Play("lookUp");
            yield return 1f;
            badeline.Sprite.Play("idle");
            yield return 0.4f;

            // Return near player (stay close, don't wander far left)
            yield return badeline.FloatTo(new Vector2(player.X + 8f), null, false);
            yield return 0.5f;

            yield return Level.ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
        }

        /// <summary>trigger 1: Ralsei appears and tells Badeline she pushed Oshiro too far</summary>
        private IEnumerator RalseiAppear()
        {
            SpawnRalsei();
            this.Level.Add(this.ralsei = new RalseiDummy(this.badeline.Position + new Vector2(24f, -8f)));
            this.Level.Displacement.AddBurst(this.ralsei.Center, 0.5f, 8f, 32f, 0.5f, null, null);
            Audio.Play("event:/char/badeline/maddy_split", this.badeline.Position);
            this.ralsei.Sprite.Scale.X = -1f;
            yield return 0.2f;
            yield break;
        }

        /// <summary>trigger 2: Chara appears in the hotel mirror</summary>
        private IEnumerator CharaAppearInMirror()
        {
            if (mirror == null) yield break;

            mirror.EvilAppear();

            // Fade out Oshiro's theme before starting evil Chara music
            Audio.SetMusic(null);
            yield return 0.8f;

            SetEvilMusic();
            Audio.Play("event:/desolozantas/game/05_restore/suite_bad_intro", mirror.Position);

            // Smooth zoom transition to mirror
            yield return SmoothZoomTo(new Vector2(216f, 110f), 2f);
        }

        /// <summary>trigger 3: Chara breaks out of the hotel mirror</summary>
        private IEnumerator CharaBreakMirror()
        {
            if (mirror == null) yield break;

            Audio.Play("event:/desolozantas/game/05_restore/suite_bad_mirrorbreak", mirror.Position);
            yield return mirror.SmashRoutine();

            // Spawn Chara from the broken mirror
            chara = new CharaDummy(mirror.Position + new Vector2(0f, -8f));
            Scene.Add(chara);

            yield return 1.2f;
            oshiro.Sprite.Scale.X = 1f;

            yield return Level.ZoomBack(0.5f);
        }

        /// <summary>trigger 4: Kirby/player steps closer to Oshiro (respects walk limit)</summary>
        private IEnumerator PlayerStepCloser()
        {
            int targetX = (int)Math.Min(oshiro.X - 16, playerMaxX);
            yield return player.DummyWalkToExact(targetX);
        }

        /// <summary>trigger 5: Kirby, Ralsei, and Badeline all jump back in alarm</summary>
        private IEnumerator EveryoneJumpBack()
        {
            // All companions retreat
            Add(new Coroutine(badeline.FloatTo(badeline.Position + new Vector2(-32f, 0f), null, false)));
            if (ralsei != null)
                Add(new Coroutine(ralsei.FloatTo(ralsei.Position + new Vector2(-32f, 0f), null, false)));

            yield return player.DummyWalkToExact((int)oshiro.X - 32, walkBackwards: true);
            yield return 0.8f;
        }

        /// <summary>After dialogue: Chara dashes to the ceiling and breaks the exit path</summary>
    private IEnumerator CharaBreakCeiling()
    {
        yield return SceneAs<Level>().ZoomBack(0.5f);
        chara.Add(new SoundSource(Vector2.Zero, "event:/desolozantas/game/05_restore/suite_bad_movestageleft"));
        yield return chara.FloatTo(new Vector2(Level.Bounds.Left + 96, chara.Y - 16f), 1);
        player.Facing = Facings.Left;
        yield return 0.25f;
        chara.Add(new SoundSource(Vector2.Zero, "event:/desolozantas/game/05_restore/suite_bad_ceilingbreak"));
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        Level.DirectionalShake(-Vector2.UnitY);
        yield return chara.SmashBlock(chara.Position + new Vector2(0f, -32f));
        yield return 0.8f;
    }

        #endregion

        #region Utility Methods

        private IEnumerator SmoothZoomTo(Vector2 target, float speed)
        {
            Vector2 from = Level.ZoomFocusPoint;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * speed)
            {
                Level.ZoomFocusPoint = Vector2.Lerp(from, target, Ease.SineInOut(t));
                yield return null;
            }
            Level.ZoomFocusPoint = target;
        }

        private IEnumerator RestoreLighting(Level level)
        {
            while (level.Lighting.Alpha != level.BaseLightingAlpha)
            {
                level.Lighting.Alpha = Calc.Approach(
                    level.Lighting.Alpha,
                    level.BaseLightingAlpha,
                    Engine.DeltaTime * 0.5f);
                yield return null;
            }
        }

        private void SetEvilMusic()
        {
            if (Level.Session.Area.Mode != AreaMode.Normal) return;

            Level.Session.Audio.Music.Event = MusicEvilChara;
            Level.Session.Audio.Apply();
        }

        #endregion

        #region Cleanup

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                CleanupOnSkip();
            }

            RemoveWalkBarrier();
            oshiro.Talker.Enabled = true;
            UnlockPlayer();

            level.Lighting.Alpha = level.BaseLightingAlpha;
            level.Session.SetFlag(Flag);
            SetEvilMusic();
        }

        private void CleanupOnSkip()
        {
            mirror?.Broken();
            Scene.Entities.FindFirst<CelesteDashBlock>()?.RemoveAndFlagAsGone();
            oshiro.Sprite.Play("idle_ground");
        }

        #endregion
    }
}





