using Celeste.Entities;
using Microsoft.Xna.Framework;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS07_GenocideWakeup : CutsceneEntity
    {
        private readonly global::Celeste.Player player;
        private BadelineDummy badeline;

        public CS07_GenocideWakeup(global::Celeste.Player player)
            : base(true, false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (level.Session.GetFlag(CH7GenocideMirrorState.WakeupPlayedFlag))
            {
                RemoveSelf();
                return;
            }

            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.StateMachine.Locked = true;

            badeline = level.Entities.FindFirst<BadelineDummy>();
            if (badeline == null)
            {
                badeline = new BadelineDummy(player.Position + new Vector2(24f, -6f));
                level.Add(badeline);
                badeline.Appear(level, true);
                badeline.Sprite.Scale.X = -1f;
            }

            yield return 0.25f;
            yield return Textbox.Say("MAGGYHELPER_CH7_GENO_WAKEUP");

            level.Session.SetFlag(CH7GenocideMirrorState.WakeupPlayedFlag);
            level.Session.SetFlag(CH7GenocideMirrorState.EnabledFlag, false);
            level.Session.SetFlag(CH7GenocideMirrorState.StartedFlag, false);
            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = global::Celeste.Player.StNormal;
        }
    }
}
