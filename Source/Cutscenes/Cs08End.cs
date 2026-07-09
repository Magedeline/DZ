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
        private Npc08DZEnding magolor;

        public Cs08End(global::Celeste.Player player, Npc08MadelineEndingBandage madelineBandage, Npc08TheoEnding theo, Npc08DZEnding magolor)
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
            yield return Textbox.Say("DZ_CH8_ENDING", new Func<IEnumerator>[]
            {
                this.TheoNMaddyEnter,
                this.MagolorEnter,
                this.KirbyTurnsRight,
                this.BadelineTurnsRight,
                this.BadelineandCharaTurnsLeft,
                this.WaitAbit,
                this.CharaTurnsRight,
                this.TurnToLeft,
                this.DZStopTired
            });
            Audio.Play("event:/DZ/char/kirby/backpack_drop", this.player.Position);
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("bagdown");
            base.EndCutscene(level, true);
            yield break;
        }

        private IEnumerator TheoNMaddyEnter()
        {
            yield return 0.25f;
            if (this.badeline != null) this.badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
            if (this.madelineBandage != null) this.madelineBandage.Visible = true;
            if (this.theo != null) this.theo.Visible = true;
            if (this.badeline != null) base.Add(new Coroutine(this.badeline.FloatTo(new Vector2(this.badeline.X - 10f, this.badeline.Y), 1, false), true));
            if (this.madelineBandage != null) yield return this.madelineBandage.WalkTo(this.player.Position + new Vector2(40f, 0.0f));
            if (this.theo != null) yield return this.theo.WalkTo(this.player.Position + new Vector2(45f, 0.0f));
        }

        private IEnumerator MagolorEnter()
        {
            this.player.Facing = Facings.Left;
            if (this.badeline != null) this.badeline.Sprite.Scale.X = -1f;
            yield return 0.25f;
            yield return CutsceneEntity.CameraTo(new Vector2(this.Level.Camera.X - 40f, this.Level.Camera.Y), 1f, null, 0f);
            if (this.magolor != null) this.magolor.Visible = true;
            base.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(this.Level.Camera.X + 40f, this.Level.Camera.Y), 2f, null, 1f), true));
            if (this.badeline != null) base.Add(new Coroutine(this.badeline.FloatTo(new Vector2(this.badeline.X + 6f, this.badeline.Y + 4f), -1, false), true));
            if (this.magolor != null) yield return this.magolor.MoveTo(this.player.Position + new Vector2(-32f, 0.0f));
            if (this.magolor != null) this.magolor.Sprite.Play("idle");
        }

        private IEnumerator DZStopTired()
        {
            if (this.magolor != null) this.magolor.Sprite.Play("idle");
            yield return null;
        }

        private IEnumerator KirbyTurnsRight()
        {
            yield return 0.1f;
            this.player.Facing = Facings.Right;
            yield return 0.1f;
            if (this.badeline != null) yield return this.badeline.FloatTo(this.badeline.Position + new Vector2(-2f, 10f), -1, false);
            yield return 0.1f;
        }

        private IEnumerator BadelineTurnsRight()
        {
            yield return 0.1f;
            if (this.badeline != null) this.badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
        }

        private IEnumerator BadelineandCharaTurnsLeft()
        {
            yield return 0.1f;
            if (this.badeline != null) this.badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
            if (this.chara != null) {
                var charaSprite = chara.Sprite;
                if (charaSprite != null) charaSprite.Scale.X = -1f;
            }
            yield return 0.1f;
        }

        private IEnumerator CharaTurnsRight()
        {
            yield return 0.1f;
            if (this.chara != null) {
                var charaSprite = chara.Sprite;
                if (charaSprite != null) charaSprite.Scale.X = 1f;
            }
            yield return 0.1f;
        }

        private IEnumerator WaitAbit()
        {
            yield return 0.4f;
        }

        private IEnumerator TheoRaiseHand()
        {
            if (this.magolor != null) this.magolor.Sprite.Play("yolo");
            yield return 0.8f;
        }

        private IEnumerator TurnToLeft()
        {
            yield return 0.1f;
            this.player.Facing = Facings.Left;
            yield return 0.05f;
            if (this.badeline != null) this.badeline.Sprite.Scale.X = -1f;
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

