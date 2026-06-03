using Celeste.Entities;
using Celeste.NPCs;
using BadelineDummy = Celeste.Entities.BadelineDummy;

namespace Celeste.Cutscenes
{
    public class Cs08End : CutsceneEntity
    {
        private global::Celeste.Player player;
        private BadelineDummy badeline;
        private CharaDummy chara;
        private Npc08MadelineEndingBandage madelineBandage;
        private Npc08TheoEnding theo;
        private Npc08MaggyEnding magolor;

        public Cs08End(global::Celeste.Player player, Npc08MadelineEndingBandage madelineBandage, Npc08TheoEnding theo, Npc08MaggyEnding magolor)
            : base(false, true)
        {
            this.player = player;
            this.madelineBandage = madelineBandage;
            this.theo = theo;
            this.magolor = magolor;
        }

        public Cs08End(CelestePlayer player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            this.player.StateMachine.State = Player.StDummy;
            this.player.StateMachine.Locked = true;
            yield return 1f;
            this.player.Dashes = 5;
            level.Session.Inventory.Dashes = 5;
            level.Add(this.badeline = new BadelineDummy(this.player.Center));
            this.badeline.Appear(level, true);
            this.badeline.FloatSpeed = 80f;
            this.badeline.Sprite.Scale.X = -1f;
            level.Add(this.chara = new CharaDummy(this.player.Center));
            this.chara.Float = -80f;
            var charaSprite = chara.Sprite;
            if (charaSprite != null) charaSprite.Scale.X = 1f;
            Audio.Play("event:/char/badeline/maddy_split", this.player.Center);
            yield return this.badeline.FloatTo(this.player.Position + new Vector2(24f, -20f), -1, false);
            yield return this.chara.FloatTo(this.player.Position + new Vector2(-24f, -20f), -1, false);
            yield return level.ZoomTo(new Vector2(160f, 120f), 2f, 1f);
            yield return Textbox.Say("ch8_ending", new Func<IEnumerator>[]
            {
                this.TheoNMaddyEnter,
                this.MagolorEnter,
                this.KirbyTurnsRight,
                this.BadelineTurnsRight,
                this.BadelineandCharaTurnsLeft,
                this.WaitAbit,
                this.CharaTurnsRight,
                this.TurnToLeft,
                this.MaggyStopTired
            });
            Audio.Play("event:/char/pusheen/kirby/backpack_drop", this.player.Position);
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("bagdown");
            base.EndCutscene(level, true);
            yield break;
        }

        private IEnumerator TheoNMaddyEnter()
        {
            yield return 0.25f;
            this.badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
            this.madelineBandage.Visible = true;
            this.theo.Visible = true;
            base.Add(new Coroutine(this.badeline.FloatTo(new Vector2(this.badeline.X - 10f, this.badeline.Y), 1, false), true));
            yield return this.madelineBandage.WalkTo(this.player.Position + new Vector2(40f, 0.0f));
            yield return this.theo.WalkTo(this.player.Position + new Vector2(45f, 0.0f));
        }

        private IEnumerator MagolorEnter()
        {
            this.player.Facing = Facings.Left;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 0.25f;
            yield return CutsceneEntity.CameraTo(new Vector2(this.Level.Camera.X - 40f, this.Level.Camera.Y), 1f, null, 0f);
            this.magolor.Visible = true;
            base.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(this.Level.Camera.X + 40f, this.Level.Camera.Y), 2f, null, 1f), true));
            base.Add(new Coroutine(this.badeline.FloatTo(new Vector2(this.badeline.X + 6f, this.badeline.Y + 4f), -1, false), true));
            yield return this.magolor.MoveTo(this.player.Position + new Vector2(-32f, 0.0f));
            this.magolor.Sprite.Play("idle");
        }

        private IEnumerator MaggyStopTired()
        {
            this.magolor.Sprite.Play("idle");
            yield return null;
        }

        private IEnumerator KirbyTurnsRight()
        {
            yield return 0.1f;
            this.player.Facing = Facings.Right;
            yield return 0.1f;
            yield return this.badeline.FloatTo(this.badeline.Position + new Vector2(-2f, 10f), -1, false);
            yield return 0.1f;
        }

        private IEnumerator BadelineTurnsRight()
        {
            yield return 0.1f;
            this.badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
        }

        private IEnumerator BadelineandCharaTurnsLeft()
        {
            yield return 0.1f;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
            var charaSprite = chara.Sprite;
            if (charaSprite != null) charaSprite.Scale.X = -1f;
            yield return 0.1f;
        }

        private IEnumerator CharaTurnsRight()
        {
            yield return 0.1f;
            var charaSprite = chara.Sprite;
            if (charaSprite != null) charaSprite.Scale.X = 1f;
            yield return 0.1f;
        }

        private IEnumerator WaitAbit()
        {
            yield return 0.4f;
        }

        private IEnumerator TheoRaiseHand()
        {
            this.magolor.Sprite.Play("yolo");
            yield return 0.8f;
        }

        private IEnumerator TurnToLeft()
        {
            yield return 0.1f;
            this.player.Facing = Facings.Left;
            yield return 0.05f;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
        }

        public override void OnEnd(Level level)
        {
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = Player.StNormal;
            level.CompleteArea(true, true, true);
        }
    }
}
