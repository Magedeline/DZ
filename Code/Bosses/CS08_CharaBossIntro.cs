#nullable disable
using Celeste.Entities;

namespace Celeste.Cutscenes {
    public class Cs08CharaBossIntro : CutsceneEntity {
        public const string FLAG = "chara_boss_intro";
        public global::Celeste.Player Player;
        private Vector2 charabossEndPosition;
        private CharaAutoAnimator animator;
        public CharaBoss CharaBoss;
        private readonly float playerTargetX;

        public Cs08CharaBossIntro(float playerTargetX, global::Celeste.Player player, CharaBoss boss) {
            this.playerTargetX = playerTargetX;
            this.Player = player;
            CharaBoss = boss;
            charabossEndPosition = boss?.Position + new Vector2(0.0f, -16f) ?? Vector2.Zero;
            animator = new CharaAutoAnimator();
            Add(animator);
        }

        public Cs08CharaBossIntro(global::Celeste.Player player) : this(player.X, player, null) {
        }

        public override void OnBegin(Level level) {
            if (CharaBoss == null)
                CharaBoss = level.Tracker.GetEntity<CharaBoss>();

            if (CharaBoss != null)
                charabossEndPosition = CharaBoss.Position + new Vector2(0.0f, -16f);

            this.Add((Component)new Coroutine(this.Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            Cs08CharaBossIntro cs08CharaBossIntro = this;
            cs08CharaBossIntro.Player.StateMachine.State = Player.StDummy;
            cs08CharaBossIntro.Player.StateMachine.Locked = true;
            while (!cs08CharaBossIntro.Player.Dead && !cs08CharaBossIntro.Player.OnGround())
                yield return null;
            while (cs08CharaBossIntro.Player.Dead)
                yield return null;

            if (cs08CharaBossIntro.CharaBoss == null)
            {
                cs08CharaBossIntro.EndCutscene(level);
                yield break;
            }

            cs08CharaBossIntro.Player.Facing = Facings.Right;
            cs08CharaBossIntro.Add((Component)new Coroutine(CutsceneEntity.CameraTo(new Vector2((float)(((double)cs08CharaBossIntro.Player.X + (double)cs08CharaBossIntro.CharaBoss.X) / 2.0 - 160.0), (float)(level.Bounds.Bottom - 180)), 1f)));
            yield return 0.5f;
            if (!cs08CharaBossIntro.Player.Dead)
                yield return cs08CharaBossIntro.Player.DummyWalkToExact((int)((double)cs08CharaBossIntro.playerTargetX - 8.0));
            cs08CharaBossIntro.Player.Facing = Facings.Right;
            yield return Textbox.Say("MAGGYHELPER_CH8_CHARA_BOSS_START", new Func<IEnumerator>(cs08CharaBossIntro.CharaFloat), new Func<IEnumerator>(cs08CharaBossIntro.PlayerStepForward));
            yield return level.ZoomBack(0.5f);
            cs08CharaBossIntro.EndCutscene(level);
        }

        private IEnumerator CharaFloat() {
            Cs08CharaBossIntro cs08CharaBossIntro = this;
            cs08CharaBossIntro.Add((Component)new Coroutine(cs08CharaBossIntro.Level.ZoomTo(new Vector2(170f, 110f), 2f, 1f)));
            Audio.Play("guid://{646b0a01-2101-424f-804b-18842f72a62d}", cs08CharaBossIntro.CharaBoss.Position);
            if (cs08CharaBossIntro.CharaBoss.NormalSprite is global::Celeste.PlayerSprite normalSprite && normalSprite.Has("pretendDead")) {
                normalSprite.Play("pretendDead");
            }
            if (cs08CharaBossIntro.CharaBoss.NormalSprite is global::Celeste.PlayerSprite normalSprite2) {
                normalSprite2.Play("fallSlow");
                normalSprite2.Scale.X = -1f;
            }
            cs08CharaBossIntro.CharaBoss.Add((Component)(cs08CharaBossIntro.animator = new CharaAutoAnimator()));
            float fromY = cs08CharaBossIntro.CharaBoss.Y;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 4f) {
                cs08CharaBossIntro.CharaBoss.Position.Y = MathHelper.Lerp(fromY, cs08CharaBossIntro.charabossEndPosition.Y, Ease.CubeInOut(p));
                yield return null;
            }
        }

        private IEnumerator PlayerStepForward() {
            yield return this.Player.DummyWalkToExact((int)this.Player.X + 8);
            this.Player.StateMachine.State = Player.StIntroWalk;
        }

        public override void OnEnd(Level level) {
            if (this.WasSkipped && this.Player != null) {
                this.Player.X = this.playerTargetX;
                // Fix the infinite loop by properly incrementing Y
                while (!this.Player.OnGround() && (double)this.Player.Y < (double)level.Bounds.Bottom) {
                    this.Player.Y += 1f;
                }
            }

            global::Celeste.Player player = this.Player;
            if (player != null) {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StDummy;
            }

            if (this.CharaBoss != null) {
                this.CharaBoss.Position = this.charabossEndPosition;
                if (this.CharaBoss.NormalSprite is global::Celeste.PlayerSprite normalSprite) {
                    normalSprite.Scale.X = -1f;
                    if (normalSprite.Has("angry"))
                        normalSprite.Play("angry");
                    else if (normalSprite.Has("idle"))
                        normalSprite.Play("idle");
                }
                this.CharaBoss.Sitting = true;
                if (this.CharaBoss.NormalSprite != null && this.animator != null)
                    this.CharaBoss.Add(this.animator);
            }
            level.Session.SetFlag("chara_boss_intro");
        }
    }
}




