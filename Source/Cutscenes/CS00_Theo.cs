using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS00_Theo : CutsceneEntity
    {
        public const string Flag = "theo";
        private NPC00_Theo theo;
        private Player player;
        private Vector2 endPlayerPosition;
        private Coroutine zoomCoroutine;

        public CS00_Theo(NPC00_Theo theo, Player player)
        {
            this.theo = theo;
            this.player = player;
            endPlayerPosition = theo.Position + new Vector2(48f, 0.0f);
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene()));

        private IEnumerator Cutscene()
        {
            CS00_Theo cs00Theo = this;
            cs00Theo.player.StateMachine.State = 11;
            if (Math.Abs(cs00Theo.player.X - cs00Theo.theo.X) < 20.0)
                yield return cs00Theo.player.DummyWalkTo(cs00Theo.theo.X - 48f);
            cs00Theo.player.Facing = Facings.Right;
            yield return 0.5f;
            yield return Textbox.Say("DZ_CH0_THEO", cs00Theo.Meet, cs00Theo.RunAlong, cs00Theo.LaughAndAirQuotes, cs00Theo.Laugh, cs00Theo.StopLaughing, cs00Theo.OminousZoom, cs00Theo.PanToMaddy);
            yield return cs00Theo.Level.ZoomBack(0.5f);
            cs00Theo.EndCutscene(cs00Theo.Level);
        }

        private IEnumerator Meet()
        {
            yield return 0.25f;
            theo.Sprite.Scale.X = Math.Sign(player.X - theo.X);
            yield return player.DummyWalkTo(theo.X - 20f);
            player.Facing = Facings.Right;
            yield return 0.8f;
        }

        private IEnumerator RunAlong()
        {
            CS00_Theo cs00Theo = this;
            yield return cs00Theo.player.DummyWalkToExact((int)cs00Theo.endPlayerPosition.X);
            yield return 0.8f;
            cs00Theo.player.Facing = Facings.Left;
            yield return 0.4f;
            cs00Theo.theo.Sprite.Scale.X = 1f;
            yield return cs00Theo.Level.ZoomTo(new Vector2(210f, 90f), 2f, 0.5f);
            yield return 0.2f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator OminousZoom()
        {
            Vector2 screenSpaceFocusPoint = new Vector2(210f, 100f);
            zoomCoroutine = new Coroutine(Level.ZoomAcross(screenSpaceFocusPoint, 4f, 3f));
            Add(zoomCoroutine);
            theo.Sprite.Play("idle");
            yield return 0.2f;
        }

        private IEnumerator LaughAndAirQuotes()
        {
            theo.Sprite.Play("laugh");
            yield return 0.4f;
        }

        private IEnumerator Laugh()
        {
            theo.Hahaha.Play("event:/char/theo/laugh");
            yield return 0.6f;
        }

        private IEnumerator StopLaughing()
        {
            theo.Hahaha.Stop();
            theo.Sprite.Play("idle");
            yield return 0.2f;
        }

        private IEnumerator PanToMaddy()
        {
            CS00_Theo cs00Theo = this;
            while (cs00Theo.zoomCoroutine != null && cs00Theo.zoomCoroutine.Active)
                yield return null;
            yield return 0.2f;
            yield return cs00Theo.Level.ZoomAcross(new Vector2(210f, 90f), 2f, 0.5f);
            yield return 0.2f;
        }

        public override void OnEnd(Level level)
        {
            theo.Sprite.Play("laugh");
            theo.Sprite.Scale.X = 1f;
            player.Position.X = endPlayerPosition.X;
            player.Facing = Facings.Left;
            player.StateMachine.State = 0;
            level.Session.SetFlag("theo");
            level.ResetZoom();
        }
    }
}