using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS07_SeeMaddy : CutsceneEntity
    {
        private const float NewDarknessAlpha = 0.3f;
        public const string Flag = "seeMaddyInCrystal";
        private int index;
        private Player player;
        private TheoCrystal theo;

        public CS07_SeeMaddy(Player player, int index)
        {
            this.player = player;
            this.index = index;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS07_SeeMaddy cs07SeeMaddy = this;
            while (cs07SeeMaddy.player.Scene == null || !cs07SeeMaddy.player.OnGround())
                yield return null;
            cs07SeeMaddy.player.StateMachine.State = 11;
            cs07SeeMaddy.player.StateMachine.Locked = true;
            yield return 0.25f;
            cs07SeeMaddy.theo = cs07SeeMaddy.Scene.Tracker.GetEntity<TheoCrystal>();
            if (cs07SeeMaddy.theo != null && Math.Sign(cs07SeeMaddy.player.X - cs07SeeMaddy.theo.X) != 0)
                cs07SeeMaddy.player.Facing = (Facings)Math.Sign(cs07SeeMaddy.theo.X - cs07SeeMaddy.player.X);
            yield return 0.25f;
            if (cs07SeeMaddy.index == 0)
                yield return Textbox.Say("ch5_see_maddy", cs07SeeMaddy.ZoomIn, cs07SeeMaddy.MadelineTurnsAround, cs07SeeMaddy.WaitABit, cs07SeeMaddy.KirbyTurnsBackAndBrighten);
            else if (cs07SeeMaddy.index == 1)
                yield return Textbox.Say("ch5_see_maddy_b");
            yield return cs07SeeMaddy.Level.ZoomBack(0.5f);
            cs07SeeMaddy.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator ZoomIn()
        {
            yield return Level.ZoomTo(Vector2.Lerp(player.Position, theo.Position, 0.5f) - Level.Camera.Position + new Vector2(0f, -20f), 2f, 0.5f);
        }

        private IEnumerator MadelineTurnsAround()
        {
            yield return 0.3f;
            player.Facing = Facings.Left;
            yield return 0.1f;
        }

        private IEnumerator WaitABit()
        {
            yield return 1f;
        }

        private IEnumerator KirbyTurnsBackAndBrighten()
        {
            CS07_SeeMaddy cs07SeeMaddy = this;
            yield return 0.1f;
            Coroutine coroutine = new Coroutine(cs07SeeMaddy.Brighten());
            cs07SeeMaddy.Add(coroutine);
            yield return 0.2f;
            cs07SeeMaddy.player.Facing = Facings.Right;
            yield return 0.1f;
            while (coroutine.Active)
                yield return null;
        }

        private IEnumerator Brighten()
        {
            CS07_SeeMaddy cs07SeeMaddy = this;
            yield return cs07SeeMaddy.Level.ZoomBack(0.5f);
            yield return 0.3f;
            cs07SeeMaddy.Level.Session.DarkRoomAlpha = 0.3f;
            float darkness = cs07SeeMaddy.Level.Session.DarkRoomAlpha;
            while (cs07SeeMaddy.Level.Lighting.Alpha != (double)darkness)
            {
                cs07SeeMaddy.Level.Lighting.Alpha = Calc.Approach(cs07SeeMaddy.Level.Lighting.Alpha, darkness, Engine.DeltaTime * 0.5f);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            player.DummyAutoAnimate = true;
            level.Session.DarkRoomAlpha = 0.3f;
            level.Lighting.Alpha = level.Session.DarkRoomAlpha;
            level.Session.SetFlag("seeMaddyInCrystal");
        }
    }
}