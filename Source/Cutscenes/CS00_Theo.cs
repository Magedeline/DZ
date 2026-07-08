using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

public class CS00_Theo : CutsceneEntity
{
    public const string Flag = "theo";

    private NPC00_Theo theo;

    private Player player;

    private Vector2 endPlayerPosition;

    private Coroutine zoomCoroutine;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS00_Theo(NPC00_Theo theo, Player player)
    {
        this.theo = theo;
        this.player = player;
        endPlayerPosition = theo.Position + new Vector2(48f, 0f);
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene()));
    }

    private IEnumerator Cutscene()
    {
        player.StateMachine.State = 11;
        if (Math.Abs(player.X - theo.X) < 20f)
        {
            yield return player.DummyWalkTo(theo.X - 48f);
        }
        player.Facing = Facings.Right;
        yield return 0.5f;
        yield return Textbox.Say("DZ_CH0_THEO_A", Meet, RunAlong, OminousZoom, PanToMaddy);
        yield return Level.ZoomBack(0.5f);
        EndCutscene(Level);
    }

    private IEnumerator Meet()
    {
        yield return 0.25f;
        theo.Sprite.Scale.X = Math.Sign(player.X - theo.X);
        yield return player.DummyWalkTo(theo.X - 20f);
        theo.Sprite.Scale.X = 1f;
        yield return 0.8f;
    }

    private IEnumerator RunAlong()
    {
        yield return player.DummyWalkToExact((int)endPlayerPosition.X);
        yield return 0.8f;
        player.Facing = Facings.Left;
        yield return 0.4f;
        theo.Sprite.Scale.X = 1f;
        yield return Level.ZoomTo(new Vector2(210f, 90f), 2f, 0.5f);
        yield return 0.2f;
    }

    private IEnumerator OminousZoom()
    {
        Vector2 screenSpaceFocusPoint = new Vector2(210f, 100f);
        zoomCoroutine = new Coroutine(Level.ZoomAcross(screenSpaceFocusPoint, 4f, 3f));
        Add(zoomCoroutine);
        theo.Sprite.Play("idle");
        yield return 0.2f;
    }

    private IEnumerator PanToMaddy()
    {
        while (zoomCoroutine != null && zoomCoroutine.Active)
        {
            yield return null;
        }
        yield return 0.2f;
        yield return Level.ZoomAcross(new Vector2(210f, 90f), 2f, 0.5f);
        yield return 0.2f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        theo.Sprite.Scale.X = 1f;
        player.Position.X = endPlayerPosition.X;
        player.Facing = Facings.Left;
        player.StateMachine.State = 0;
        level.Session.SetFlag("theo");
        level.ResetZoom();
    }
}
