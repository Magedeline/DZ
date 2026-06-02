using System.Collections;
using Celeste.Entities;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Marlet checks her list and flies away
    /// </summary>
    public class CS11_Intro_Marlet : CutsceneEntity
    {
        public const string Flag = "cs11_intro_marlet_complete";
        
        private readonly Player player;

        public CS11_Intro_Marlet(Player player) : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            // Set player to dummy state
            player.StateMachine.State = Player.StDummy;
            
            // Play the dialog
            yield return Textbox.Say("CH11_INTRO_MARLET");
            
            // Optional: Add any additional cutscene logic here
            // For example, if you want Marlet to actually fly away visually
            // yield return MarletFlyAway();
            
            yield return 0.5f;
            
            // End the cutscene
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            // Set the completion flag
            level.Session.SetFlag(Flag);
            
            // Return player to normal state
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
        }
    }
}
