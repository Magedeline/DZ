using MaggyHelper.Triggers;

namespace MaggyHelper.Cutscenes {
    [HotReloadable]
    public class Cs05MaddyPhone : CutsceneEntity 
    {
        private global::Celeste.Player player;
        private float targetX;

        public Cs05MaddyPhone(global::Celeste.Player player, float targetX) : this(false, false) {
            this.player = player;
            this.targetX = targetX;
        }

        public Cs05MaddyPhone(bool v, bool v1) : base(v, v1) {
        }

        public override void OnBegin(Level level) {
            this.Add(new Coroutine(this.routine()));
        }

        private IEnumerator routine() {
            Cs05MaddyPhone cs05TheoPhone = this;
            cs05TheoPhone.player.StateMachine.State = 11;
            if ((double)cs05TheoPhone.player.X != (double)cs05TheoPhone.targetX)
                cs05TheoPhone.player.Facing = (Facings)Math.Sign(cs05TheoPhone.targetX - cs05TheoPhone.player.X);
            yield return 0.5f;
            yield return cs05TheoPhone.Level.ZoomTo(new Vector2(80f, 60f), 2f, 0.5f);
            yield return global::Celeste.Textbox.Say("CH7_PHONE", cs05TheoPhone.walkToPhone, cs05TheoPhone.standBackUp);
            yield return cs05TheoPhone.Level.ZoomBack(0.5f);
            cs05TheoPhone.EndCutscene(cs05TheoPhone.Level, false);
        }

        private IEnumerator walkToPhone() {
            yield return 0.25f;
            yield return this.player.DummyWalkToExact((int)this.targetX);
            this.player.Facing = Facings.Right; // Fixed: use proper Facings enum instead of StateMachine
            yield return 0.5f;
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("duck");
            yield return 0.5f;
        }

        private IEnumerator standBackUp() {
            this.removePhone();
            yield return 0.6f;
            this.player.Sprite.Play("idle");
            yield return 0.2f;
        }

        public override void OnEnd(Level level) {
            this.removePhone();
            this.player.StateMachine.State = 0;
        }

        private void removePhone() => this.Scene.Entities.FindFirst<MaddyPhone>()?.RemoveSelf();
    }
}



