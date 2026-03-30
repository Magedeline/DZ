using MaggyHelper.Entities;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Cutscene for the final room of Chapter 19, featuring interaction with Chara
    /// </summary>
    public class Cs19BigFinalRoom : CutsceneEntity
    {
        private global::Celeste.Player player;
        private CharaDummy chara;
        private bool first;

        /// <summary>
        /// Creates a new CS19 Big Final Room cutscene
        /// </summary>
        /// <param name="player">The player entity</param>
        /// <param name="first">True if this is the first time entering the room</param>
        public Cs19BigFinalRoom(global::Celeste.Player player, bool first) : base()
        {
            this.Depth = -8500;
            this.player = player;
            this.first = first;
        }

        /// <summary>
        /// Starts the cutscene when it begins
        /// </summary>
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene(level)));
        }

        /// <summary>
        /// Main cutscene coroutine that handles the sequence of events
        /// </summary>
        private IEnumerator cutscene(Level level)
        {
            if (player == null || level == null)
            {
                EndCutscene(level);
                yield break;
            }

            // Put player in cutscene state
            player.StateMachine.State = 11;
            
            if (first)
            {
                // First time: player walks forward
                yield return player.DummyWalkToExact((int)(player.X + 16.0));
                yield return 0.5f;
            }
            else
            {
                // Return visit: player is already sitting
                player.DummyAutoAnimate = false;
                player.Sprite.Play("sitDown");
                player.Sprite.SetAnimationFrame(player.Sprite.CurrentAnimationTotalFrames - 1);
                yield return 1.25f;
            }
            
            // Show Chara
            yield return charaAppears();
            
            // Display appropriate dialog
            if (first)
                yield return Textbox.Say("CH19_LAST_ROOM");
            else
                yield return Textbox.Say("CH19_LAST_ROOM_ALT");
            
            // Remove Chara and end cutscene
            yield return charaVanishes();
            EndCutscene(level);
        }

        /// <summary>
        /// Handles Chara's appearance with appropriate effects
        /// </summary>
        private IEnumerator charaAppears()
        {
            if (player == null || Level == null) yield break;

            // Create and position Chara
            chara = new CharaDummy(player.Position + new Vector2(18f, -8f));
            Level.Add(chara);
            
            // Add appearance effects
            Level.Displacement?.AddBurst(chara.Center, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("event:/char/badeline/maddy_split", chara.Position);
            
            // Set Chara to face left (towards player)
            if (chara.Sprite != null)
                chara.Sprite.Scale.X = -1f;
                
            yield return null;
        }

        /// <summary>
        /// Handles Chara's disappearance with appropriate effects
        /// </summary>
        private IEnumerator charaVanishes()
        {
            yield return 0.2f;
            
            // Use Chara's built-in vanish method
            chara?.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            chara = null;
            
            yield return 0.5f;
            
            // Make player face right after Chara leaves
            if (player != null)
                player.Facing = Facings.Right;
        }

        /// <summary>
        /// Cleanup method called when the cutscene ends
        /// </summary>
        public override void OnEnd(Level level)
        {
            if (Level?.Session?.Inventory != null)
                Level.Session.Inventory.Dashes = 1;
                
            if (player != null)
            {
                player.StateMachine.State = 0;
                
                // Play stand animation if player was sitting and cutscene wasn't skipped
                if (!first && !WasSkipped)
                    Audio.Play("event:/desolozantas/char/kirby/stand", player.Position);
            }
            
            // Ensure Chara is properly removed if still present
            if (chara != null)
            {
                chara.RemoveSelf();
                chara = null;
            }
        }
    }
}



