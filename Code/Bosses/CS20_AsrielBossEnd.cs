using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Post-boss cutscene for the Asriel Angel of Death fight.
    /// Asriel, broken and tearful, is comforted by Madeline, Kirby and Badeline Ã¢â‚¬â€
    /// then Els reveals itself as the true villain and begins to transform.
    ///
    /// Dialog key: CH20_ASRIEL_BOSS_END
    ///
    /// Trigger mapping:
    ///   {trigger 0}  madeline and badeline comfort asriel
    ///   {trigger 1}  camera slightly move to els on the right
    ///   {trigger 2}  madeline walk forward and confront els
    ///   {trigger 3}  els start angry with distortion effect
    ///   {trigger 4}  els calm down and disappointed
    ///   {trigger 5}  els start to transform into humanoid form with glitch effects
    ///   {trigger 6}  els faded to black
    /// </summary>
    public class CS20_AsrielBossEnd : CutsceneEntity
    {
        public const string Flag = "ch20_asriel_boss_end";

        private const string DIALOG_KEY = "MAGGYHELPER_CH20_ASRIEL_BOSS_END";
        private const string MUSIC_HIS_THEME = "event:/new_content/music/pusheen/lvl20/musicbox";
        private const string SFX_GLITCH_LONG = "event:/new_content/game/pusheen/19_spaces/glitch_long";
        private const string SFX_POWER = "event:/new_content/char/pusheen/asriel/Asriel_Segapower01";

        private global::Celeste.Player player;
        private AsrielAngelOfDeathBoss asrielBoss;

        // Party NPCs that may already be present in the room
        private Entity madelineNpc;
        private Entity badelineNpc;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CS20_AsrielBossEnd() : base() { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CS20_AsrielBossEnd(global::Celeste.Player player) : base()
        {
            this.player = player;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Cutscene(Level level)
        {
            // Acquire player reference if not supplied via constructor
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                yield return null;
            }

            asrielBoss = level.Entities.FindFirst<AsrielAngelOfDeathBoss>();

            // Resolve party NPCs that should already be in the room
            madelineNpc = level.Entities.FindFirst<Npc20_Madeline>() as Entity;
            badelineNpc = level.Entities.FindFirst<global::Celeste.Entities.BadelineDummy>() as Entity;

            // Lock player during cutscene
            player.StateMachine.State = Player.StDummy; // Player.StDummy
            player.StateMachine.Locked = true;

            // Wait until the player is on the ground
            while (!player.OnGround())
                yield return null;

            yield return 0.5f;

            // Transition to His Theme for the emotional climax
            Audio.SetMusic(MUSIC_HIS_THEME);

            yield return 0.3f;

            // Soft zoom in for the moment
            Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 90f), 1.5f, 0.8f), true));

            yield return Textbox.Say(DIALOG_KEY, new Func<IEnumerator>[]
            {
                MadelineAndBadelineComfortAsriel, // trigger 0
                CameraRevealEls,                  // trigger 1
                MadelineConfrontsEls,             // trigger 2
                ElsAngryDistortion,               // trigger 3
                ElsCalmDown,                      // trigger 4
                ElsTransform,                     // trigger 5
                ElsFadeToBlack                    // trigger 6
            });

            yield return 0.5f;
            EndCutscene(level);
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 0: madeline and badeline comfort asriel Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator MadelineAndBadelineComfortAsriel()
        {
            Level.Shake(0.15f);
            Audio.Play("event:/game/06_reflection/badeline_hug", player.Position);

            Vector2 focus = asrielBoss?.Position ?? player.Position;

            if (madelineNpc != null)
                Add(new Coroutine(WalkEntityTo(madelineNpc, focus.X - 40f), true));
            if (badelineNpc != null)
                Add(new Coroutine(WalkEntityTo(badelineNpc, focus.X + 40f), true));

            yield return 1.0f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 1: camera slightly move to els on the right Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator CameraRevealEls()
        {
            Vector2 target = new Vector2(Level.Camera.X + 80f, Level.Camera.Y);
            yield return CameraTo(target, 0.8f, Ease.SineInOut);
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 2: madeline walk forward and confront els Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator MadelineConfrontsEls()
        {
            if (madelineNpc != null)
                Add(new Coroutine(WalkEntityTo(madelineNpc, player.X + 60f), true));
            yield return 0.6f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 3: els start angry with distortion effect Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator ElsAngryDistortion()
        {
            Glitch.Value = 0.5f;
            Level.Shake(0.5f);
            Audio.Play(SFX_GLITCH_LONG, player.Position);
            Level.Displacement.AddBurst(Level.Camera.Position + new Vector2(240f, 90f), 1.5f, 64f, 256f, 1.5f);
            yield return 0.2f;
            Glitch.Value = 0f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 4: els calm down and disappointed Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator ElsCalmDown()
        {
            Glitch.Value = 0.15f;
            yield return 0.3f;
            Glitch.Value = 0f;
            yield return 0.2f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 5: els transform into humanoid form with glitch effects Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator ElsTransform()
        {
            Vector2 elsPos = Level.Camera.Position + new Vector2(240f, 90f);

            for (float t = 0f; t < 1f; t += Engine.DeltaTime / 1.5f)
            {
                Glitch.Value = MathHelper.Lerp(0f, 0.8f, Ease.CubeIn(t));
                Level.Displacement.AddBurst(elsPos, 0.3f + t * 0.7f, 32f, 128f, 0.5f);
                yield return null;
            }

            Level.Flash(Color.DarkRed * 0.6f, false);
            Level.Shake(1.2f);
            Audio.Play(SFX_POWER, elsPos);

            yield return 0.3f;
            Glitch.Value = 0.2f;
            yield return 0.1f;
            Glitch.Value = 0f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ trigger 6: els faded to black Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator ElsFadeToBlack()
        {
            new FadeWipe(Level, wipeIn: false) { Duration = 1.5f };
            ScreenWipe.WipeColor = Color.Black;
            yield return 1.5f;
        }

        // Ã¢â€â‚¬Ã¢â€â‚¬ Helper: walk entity to a target X position Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

        private IEnumerator WalkEntityTo(Entity entity, float targetX, float speed = 40f)
        {
            if (entity == null) yield break;

            float dir = Math.Sign(targetX - entity.X);
            if (dir == 0) yield break;

            Sprite sprite = entity.Get<Sprite>();
            if (sprite != null)
            {
                sprite.Scale.X = dir;
                if (sprite.Has("walk"))
                    sprite.Play("walk");
            }

            while (Math.Abs(entity.X - targetX) > 1f)
            {
                entity.X = Calc.Approach(entity.X, targetX, speed * Engine.DeltaTime);
                yield return null;
            }

            entity.X = targetX;

            if (sprite != null && sprite.Has("idle"))
                sprite.Play("idle");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnd(Level level)
        {
            Glitch.Value = 0f;

            if (WasSkipped && player != null)
            {
                while (!player.OnGround() && player.Y < (float)level.Bounds.Bottom)
                    player.Y++;
            }

            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal; // Player.StNormal
            }

            level.ResetZoom();
            level.Session.SetFlag(Flag);
        }
    }
}
