using System.Runtime.CompilerServices;
using Celeste.Entities;
using DZ;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene after HyperGoner attack - Asriel is impressed by Kirby and Maddy/Baddy,
    /// mocks them, then uses a fraction of his real power before transforming
    /// into the Angel of Death form (Phase 2 transition)
    /// </summary>
    public class CS20_BossEnd : CutsceneEntity
    {
        public const string Flag = "asriel_boss_end_hypergoner";

        private global::Celeste.Player player;
        private AsrielGodBoss asriel;
        private Level level;

        public CS20_BossEnd() : base()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            this.level = level;
            Add(new Coroutine(Cutscene(level)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Cutscene(Level level)
        {
            // Wait for player to be available
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                yield return null;
            }

            // Find Asriel boss entity
            asriel = level.Entities.FindFirst<AsrielGodBoss>();

            // Lock player in dummy state
            player.StateMachine.State = Player.StDummy; // Player.StDummy
            player.StateMachine.Locked = true;

            // Wait for player to land if in air
            while (!player.OnGround())
            {
                yield return null;
            }

            // Stop Asriel's attacks during cutscene
            if (asriel != null)
            {
                asriel.Moving = false;
            }

            // Brief pause after HyperGoner ends
            yield return 1.0f;

            // Screen effects - recovery from HyperGoner
            level.Shake(0.3f);

            // Zoom in for dramatic effect
            yield return level.ZoomTo(new Vector2(160f, 90f), 2.5f, 0.6f);

            yield return 0.5f;

            // Part 1: Asriel impressed
            yield return Textbox.Say("DZ_CH20_ASRIEL_HYPERGONER_IMPRESSED");

            yield return 0.3f;

            // Part 2: Asriel mocks them
            yield return Textbox.Say("DZ_CH20_ASRIEL_MOCKING_HEROES");

            yield return 0.5f;

            // Part 3: Asriel reveals fraction of real power
            // Dramatic screen shake and flash
            level.Shake(1.5f);
            level.Flash(Color.Purple * 0.6f, false);
            Audio.Play("event:/DZ/new_content/char/bosses/asriel/Asriel_Segapower01", asriel?.Position ?? player.Position);

            yield return 0.4f;

            // Displacement burst for power reveal
            level.Displacement.AddBurst(asriel?.Position ?? (player.Position + new Vector2(0, -100f)), 2f, 192f, 384f, 2.5f);

            yield return Textbox.Say("DZ_CH20_ASRIEL_FRACTION_POWER");

            yield return 0.5f;

            // Begin transformation sequence
            level.Shake(2.0f);
            level.Flash(Color.Gold * 0.8f, false);
            Audio.Play("event:/DZ/new_content/char/bosses/asriel/Asriel_Segapower02", asriel?.Position ?? player.Position);

            // Multiple bursts for transformation
            for (int i = 0; i < 3; i++)
            {
                level.Displacement.AddBurst(asriel?.Position ?? player.Position, 1.0f + i * 0.5f, 128f + i * 64f, 256f + i * 128f, 1.5f);
                yield return 0.3f;
            }

            // Final transformation dialog
            yield return Textbox.Say("DZ_CH20_ASRIEL_ANGEL_TRANSFORMATION");

            // Zoom out for the transformation
            yield return level.ZoomBack(0.8f);

            // Massive screen flash for transformation
            level.Flash(Color.White, true);
            level.Shake(3.0f);
            Audio.Play("event:/DZ/new_content/char/bosses/asriel/Asriel_BarrierShatter", asriel?.Position ?? player.Position);

            yield return 1.0f;

            // End cutscene and transition to Angel of Death boss
            EndCutscene(level);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnEnd(Level level)
        {
            // Handle skipped cutscene - make sure player is on ground
            if (WasSkipped && player != null)
            {
                while (!player.OnGround() && player.Y < (float)level.Bounds.Bottom)
                {
                    player.Y++;
                }
            }

            // Restore player control
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal; // Player.StNormal
            }

            // Remove Asriel God Boss and spawn Angel of Death Boss
            if (asriel != null)
            {
                Vector2 asrielPosition = asriel.Position;
                asriel.RemoveSelf();
                
                // Spawn the Angel of Death boss at the same position
                var angelBoss = new AsrielAngelOfDeathBoss(
                    new EntityData { Position = asrielPosition },
                    Vector2.Zero
                );
                level.Add(angelBoss);
            }

            // Set flags to prevent re-triggering and mark phase transition
            level.Session.SetFlag(Flag);
            level.Session.SetFlag("asriel_angel_phase_started");
            
            // Update music for Angel of Death phase
            level.Session.Audio.Music.Event = "event:/DZ/new_content/music/lvl20/burn_in_despair";
            level.Session.Audio.Apply();
        }
    }
}

