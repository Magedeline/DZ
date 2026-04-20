using System.Runtime.CompilerServices;
using Celeste.Entities;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Mid-boss cutscene for Asriel God of Hyperdeath fight
    /// Triggers at the midpoint of the battle (azzyboss-25)
    /// Features emotional dialogue and temporary ceasefire
    /// </summary>
    public class CS20_BossMid : CutsceneEntity
    {
        public const string Flag = "asriel_boss_mid";

        private global::Celeste.Player player;
        private AsrielGodBoss asriel;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
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
            player.StateMachine.State = 11; // Player.StDummy
            player.StateMachine.Locked = true;

            // Wait for player to land if in air
            while (!player.OnGround())
            {
                yield return null;
            }

            // Stop Asriel's attacks during cutscene
            if (asriel != null)
            {
                // Stop any ongoing attack sequences
                asriel.Moving = false;
            }

            // Small walk animation for dramatic effect
            yield return player.DummyWalkToExact((int)player.X + 20);

            // Zoom in for emotional moment
            yield return level.ZoomTo(new Vector2(160f, 90f), 2f, 0.5f);

            // Brief pause for tension
            yield return 0.3f;

            // Main dialogue - Asriel mid-battle emotional moment
            // This should reference a dialog entry in English.txt
            yield return Textbox.Say("ch20_asriel_boss_middle");

            // Small pause after dialogue
            yield return 0.2f;

            // Zoom back out
            yield return level.ZoomBack(0.5f);

            // End cutscene
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
                player.StateMachine.State = 0; // Player.StNormal
            }

            // Resume Asriel's attack patterns
            if (asriel != null)
            {
                // Trigger start hit to resume attacks
                asriel.TriggerStartHit();
            }

            // Set flag to prevent re-triggering
            level.Session.SetFlag(Flag);
        }
    }
}
