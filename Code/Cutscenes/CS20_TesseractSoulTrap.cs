using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Cutscene for Chapter 20: Tesseract Soul trap reveal.
    ///
    /// Plays immediately after the Tesseract Soul boss is defeated.
    /// Kirby thinks he has won and spots what looks like a heartgem â€”
    /// but it is Els's trick. The moment Kirby tries to claim it,
    /// Els breaks the illusion with "GOT YOU, KIRBY!" (SOUL_Maggy_CSide_20_THEEND_A).
    ///
    /// Dialog keys used
    /// â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    /// CH20_TESSERACT_SOUL  (end section â€” no embedded triggers here,
    ///                        all effects fire between the two Textbox.Say calls)
    /// SOUL_Maggy_CSide_20_THEEND_A  â€” Els's gotcha line
    /// </summary>
    [Tracked(true)]
    public class CS20_TesseractSoulTrap : CutsceneEntity
    {
        #region Constants

        private const string DIALOGUE_KEY_KIRBY_DOUBT  = "CH20_TESSERACT_SOUL";
        private const string DIALOGUE_KEY_ELS_GOTCHA    = "SOUL_Maggy_CSide_20_THEEND_A";

        // Session flag written when this cutscene completes
        private const string FLAG_TRAP_DONE = "ch20_tesseract_soul_trap_done";

        // SFX
        private const string SFX_FAKE_HEARTGEM_PULSE   = "event:/pusheen/extra_content/game/20_last_push/fake_heartgem_pulse";
        private const string SFX_FAKE_HEARTGEM_SHATTER = "event:/pusheen/extra_content/game/20_last_push/fake_heartgem_shatter";
        private const string SFX_ELS_LAUGH              = "event:/pusheen/extra_content/game/20_last_push/els_reveal_laugh";

        #endregion

        #region Fields

        private Player player;
        private Level  level;

        /// <summary>Visual stand-in for the fake heartgem.</summary>
        private Entity fakeGemEntity;

        /// <summary>Screen-space position the fake gem is spawned at (set by trigger).</summary>
        private Vector2 fakeGemSpawnOffset = new Vector2(0f, -48f);

        #endregion

        #region Constructor

        public CS20_TesseractSoulTrap(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Static factory â€” call from a trigger or level hook to begin the cutscene.
        /// </summary>
        public static void Start(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            CS20_TesseractSoulTrap cutscene = new CS20_TesseractSoulTrap(player);
            level.Add(cutscene);
        }

        #endregion

        #region Cutscene Lifecycle

        public override void OnBegin(Level level)
        {
            this.level = level;
            Add(new Coroutine(CutsceneSequence()));
        }

        public override void OnEnd(Level level)
        {
            // Clean up dummy gem if the scene was skipped mid-way
            DestroyFakeGem();

            if (player != null && player.StateMachine.State == Player.StDummy)
            {
                player.StateMachine.State  = Player.StNormal;
                player.DummyAutoAnimate    = true;
            }

            Glitch.Value = 0f;
            level.Session.SetFlag(FLAG_TRAP_DONE);
        }

        #endregion

        #region Main Sequence

        private IEnumerator CutsceneSequence()
        {
            // Lock player â€” Kirby stands still while the "victory" illusion unfolds.
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate   = false;

            // â”€â”€ Phase 1: fake heartgem materialises â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            yield return SpawnFakeHeartgem();

            // â”€â”€ Phase 2: Kirby's doubt dialogue â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // "di- did I win? / another heartgem? / wait... no... this isn't right..."
            yield return Textbox.Say(DIALOGUE_KEY_KIRBY_DOUBT);

            // Brief hopeful pause before the trap springs
            yield return 0.4f;

            // â”€â”€ Phase 3: Kirby steps toward the gem â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            yield return KirbyApproachGem();

            // â”€â”€ Phase 4: Els springs the trap â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            yield return SpringTrap();

            // â”€â”€ Phase 5: Els's gotcha line â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // "GOT YOU, KIRBY!"
            yield return Textbox.Say(DIALOGUE_KEY_ELS_GOTCHA);

            // Let the revelation breathe before handing control back
            yield return 1.0f;

            EndCutscene(level);
        }

        #endregion

        #region Helper Sequences

        /// <summary>
        /// Spawns the pulsing fake heartgem above Kirby and plays its ambient hum.
        /// </summary>
        private IEnumerator SpawnFakeHeartgem()
        {
            Vector2 gemPos = player.Position + fakeGemSpawnOffset;

            // Create a minimal entity to act as the gem's visual anchor.
            // Replace with a proper heartgem sprite entity once art is ready.
            fakeGemEntity = new Entity(gemPos);
            level.Add(fakeGemEntity);

            Audio.Play(SFX_FAKE_HEARTGEM_PULSE, gemPos);

            // Gentle screen pulse to sell the "treasure found" feel
            level.Flash(Color.Cyan * 0.15f, drawPlayerOver: true);

            // Let the gem "settle" visually before dialogue starts
            yield return 0.6f;
        }

        /// <summary>
        /// Moves Kirby a few pixels toward the gem, as if about to collect it.
        /// </summary>
        private IEnumerator KirbyApproachGem()
        {
            if (fakeGemEntity == null)
                yield break;

            Vector2 startPos = player.Position;
            // Move Kirby halfway to the gem horizontally over 0.5 s
            Vector2 targetPos = new Vector2(
                player.Position.X,
                player.Position.Y - 20f
            );

            float duration = 0.5f;
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                player.Position = Vector2.Lerp(startPos, targetPos, Ease.SineOut(t / duration));
                yield return null;
            }
            player.Position = targetPos;
        }

        /// <summary>
        /// The fake gem shatters, glitch spikes, and Els's laugh breaks the silence.
        /// </summary>
        private IEnumerator SpringTrap()
        {
            Audio.Play(SFX_FAKE_HEARTGEM_SHATTER,
                fakeGemEntity != null ? fakeGemEntity.Position : player.Position);

            // Violent flash â€” the illusion breaks
            level.Flash(Color.White, drawPlayerOver: false);
            level.Shake(0.5f);

            Glitch.Value = 0.8f;
            yield return 0.1f;

            // Shatter particles from the gem position
            if (fakeGemEntity != null)
            {
                for (int i = 0; i < 32; i++)
                {
                    level.ParticlesFG.Emit(
                        new ParticleType
                        {
                            Color           = Color.Cyan,
                            Color2          = Color.DarkCyan,
                            Size            = 1f,
                            SpeedMin        = 30f,
                            SpeedMax        = 100f,
                            LifeMin         = 0.4f,
                            LifeMax         = 0.9f,
                            DirectionRange  = MathHelper.TwoPi
                        },
                        fakeGemEntity.Position,
                        Calc.Random.NextFloat() * MathHelper.TwoPi
                    );
                }
            }

            DestroyFakeGem();

            Glitch.Value = 0.5f;
            yield return 0.3f;

            Audio.Play(SFX_ELS_LAUGH, player.Position);
            Glitch.Value = 0f;

            yield return 0.5f;
        }

        /// <summary>Safely removes the fake gem entity from the scene.</summary>
        private void DestroyFakeGem()
        {
            if (fakeGemEntity != null)
            {
                fakeGemEntity.RemoveSelf();
                fakeGemEntity = null;
            }
        }

        #endregion
    }
}
