using System.Runtime.CompilerServices;

namespace MaggyHelper.Cutscenes
{
    public class CS19_FreeBird : CutsceneEntity
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator Cutscene(Level level)
        {
            yield return Textbox.Say("CH19_FREE_BIRD");
            FadeWipe fadeWipe = new FadeWipe(level, wipeIn: false);
            fadeWipe.Duration = 3f;
            yield return fadeWipe.Duration;
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: false);
        }
    }
}



