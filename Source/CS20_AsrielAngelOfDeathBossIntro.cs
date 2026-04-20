using System.Runtime.CompilerServices;
using Celeste.Entities;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Intro cutscene for Asriel Angel of Death boss fight
    /// Triggers when Kirby enters the angel form boss room for the first time
    /// Shows the dramatic transformation and sets up the final battle
    /// </summary>
    public class CS20_AsrielAngelOfDeathBossIntro : CutsceneEntity
    {
        public const string Flag = "asriel_angel_boss_intro";

        private global::Celeste.Player player;
        private AsrielAngelOfDeathBoss asrielAngel;
        private string roomId;

        public CS20_AsrielAngelOfDeathBossIntro(string roomId = null) : base()
        {
            this.roomId = roomId;
        }

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

            // Find Asriel Angel boss entity
            asrielAngel = level.Entities.FindFirst<AsrielAngelOfDeathBoss>();

            // Lock player in dummy state
            player.StateMachine.State = 11; // Player.StDummy
            player.StateMachine.Locked = true;

            // Wait for player to land if in air
            while (!player.OnGround())
            {
                yield return null;
            }

            // Brief pause for tension
            yield return 0.8f;

            // Intense screen shake for angel transformation
            level.Shake(1.0f);

            // Displacement burst for dramatic effect
            level.Displacement.AddBurst(player.Position + new Vector2(0, -100f), 1.5f, 128f, 256f, 2f);

            // Zoom in for the confrontation
            yield return level.ZoomTo(new Vector2(160f, 90f), 2.5f, 0.6f);

            // Flash effect for divine presence
            level.Flash(Color.Gold * 0.6f, false);

            // Small pause
            yield return 0.4f;

            // Main dialogue - Kirby confronts Asriel in Angel of Death form
            // This should reference a dialog entry in English.txt
            yield return Textbox.Say("ch20_asriel_angel_boss_intro");

            // Small pause after dialogue
            yield return 0.3f;

            // Zoom back out
            yield return level.ZoomBack(0.6f);

            // Final dramatic effect
            level.Shake(0.5f);
            level.Flash(Color.White * 0.3f, false);

            yield return 0.5f;

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

            // Set flag to prevent re-triggering (use room-specific flag if roomId provided)
            string flagToSet = !string.IsNullOrEmpty(roomId) ? $"{Flag}_{roomId}" : Flag;
            level.Session.SetFlag(flagToSet);
        }
    }
}
