using Celeste.Entities;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for the final room of Chapter 19, featuring interaction with Chara
    /// and the final approach toward the edge of the universe.
    /// </summary>
    public class Cs19BigFinalRoom : CutsceneEntity
    {
        private readonly global::Celeste.Player player;
        private CharaDummy chara;
        private readonly bool first;

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
            // Reapply generator-driven blackhole weakening when entering this cutscene room.
            PowerGenerator.RefreshBlackholeFromGeneratorProgress(level);
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
            player.StateMachine.State = Player.StDummy;
            
            if (first)
            {
                // First time: player walks forward
                yield return player.DummyWalkToExact((int)(player.X + 16.0));
                yield return 0.5f;

                yield return charaAppears();

                yield return level.ZoomTo(
                    (player.Position + new Vector2(0f, -16f)) - level.Camera.Position,
                    2f, 0.5f);

                yield return Textbox.Say("MAGGYHELPER_CH19_LAST_ROOM", new Func<IEnumerator>[]
                {
                    NoOpTrigger,
                    BlindingLightPoursThroughCracks,
                    CameraPansToRevealVoid
                });

                yield return level.ZoomBack(0.5f);
            }
            else
            {
                // Return visit: player is already sitting
                player.DummyAutoAnimate = false;
                player.Sprite.Play("sitDown");
                player.Sprite.SetAnimationFrame(player.Sprite.CurrentAnimationTotalFrames - 1);
                yield return 1.25f;

                yield return charaAppears();
                yield return Textbox.Say("MAGGYHELPER_CH19_LAST_ROOM_ALT");
                yield return charaVanishes();
                yield return Textbox.Say("MAGGYHELPER_CH19_LAST_ROOM_ALT_2");
                EndCutscene(level);
                yield break;
            }
            
            // Remove Chara and end cutscene
            yield return charaVanishes();
            EndCutscene(level);
        }

        /// <summary>
        /// Placeholder callback so dialog trigger indices remain aligned.
        /// </summary>
        private IEnumerator NoOpTrigger()
        {
            yield return null;
        }

        /// <summary>
        /// Trigger 1: a blinding light pours through the cracks.
        /// </summary>
        private IEnumerator BlindingLightPoursThroughCracks()
        {
            Level?.Flash(Color.White, drawPlayerOver: true);
            Audio.Play("event:/new_content/game/10_farewell/lightning_strike", player.Position);
            yield return 1.0f;
        }

        /// <summary>
        /// Trigger 2: the camera pans to reveal the void beyond.
        /// </summary>
        private IEnumerator CameraPansToRevealVoid()
        {
            if (Level == null)
                yield break;

            // Keep blackhole visibility progression in sync for the void reveal moment.
            PowerGenerator.RefreshBlackholeFromGeneratorProgress(Level);

            Vector2 target = (player.Position + new Vector2(120f, -80f)) - Level.Camera.Position;
            yield return Level.ZoomTo(target, 1.5f, 1.5f);
            yield return 1.0f;
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
                player.StateMachine.State = Player.StNormal;
                
                // Play stand animation if player was sitting and cutscene wasn't skipped
                if (!first && !WasSkipped)
                    Audio.Play("event:/char/pusheen/kirby/stand", player.Position);
            }
            
            // Ensure Chara is properly removed if still present
            if (chara != null)
            {
                chara.RemoveSelf();
                chara = null;
            }

            level?.ResetZoom();
        }
    }
}
