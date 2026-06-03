using System.Collections;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;
using Celeste.NPCs;

namespace Celeste;

public class CS19_Gravestone : CutsceneEntity
{
    private Player player;

    private NPC19_Gravestone gravestone;

    private CharaDummy chara;

    private BirdNPC bird;

    private Vector2 boostTarget;

    private bool addedBooster;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS19_Gravestone(Player player, NPC19_Gravestone gravestone, Vector2 boostTarget)
    {
        this.player = player;
        this.gravestone = gravestone;
        this.boostTarget = boostTarget;
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene()));
    }

    private IEnumerator Cutscene()
    {
        player.StateMachine.State = 11;
        player.ForceCameraUpdate = true;
        player.DummyGravity = false;
        player.Speed.Y = 0f;
        yield return 0.1f;
        yield return player.DummyWalkToExact((int)gravestone.X - 30);
        yield return 0.1f;
        player.Facing = Facings.Right;
        yield return 0.2f;
        yield return Level.ZoomTo(new Vector2(160f, 90f), 2f, 3f);
        player.ForceCameraUpdate = false;
        yield return 0.5f;
        yield return Textbox.Say("MAGGYHELPER_CH19_GRAVESTONE", StepForward, BadelineAppears, SitDown);
        yield return 1f;
        yield return BirdStuff();
        yield return BadelineRejoin();
        yield return 0.1f;
        yield return Level.ZoomBack(0.5f);
        yield return 0.3f;
        addedBooster = true;
        Level.Displacement.AddBurst(boostTarget, 0.5f, 8f, 32f, 0.5f);
        Audio.Play("event:/new_content/char/badeline/booster_first_appear", boostTarget);
        Level.Add(new BadelineBoost(new Vector2[1] { boostTarget }, lockCamera: false));
        yield return 0.2f;
        EndCutscene(Level);
    }

    private IEnumerator StepForward()
    {
        yield return player.DummyWalkTo(player.X + 8f);
    }

    private IEnumerator BadelineAppears()
    {
        Level.Session.Inventory.Dashes = 1;
        player.Dashes = 1;
        Vector2 vector = player.Position + new Vector2(-12f, -10f);
        Level.Displacement.AddBurst(vector, 0.5f, 8f, 32f, 0.5f);
        Level.Add(chara = new CharaDummy(vector));
        Audio.Play("event:/char/badeline/maddy_split", vector);
        chara.Sprite.Scale.X = 1f;
        yield return chara.FloatTo(vector + new Vector2(0f, -6f), 1, faceDirection: false);
    }

    private IEnumerator SitDown()
    {
        yield return 0.2f;
        player.DummyAutoAnimate = false;
        player.Sprite.Play("sitDown");
        yield return 0.3f;
    }

    private IEnumerator BirdStuff()
    {
        bird = new BirdNPC(player.Position + new Vector2(88f, -200f), BirdNPC.Modes.None);
        bird.DisableFlapSfx = true;
        Scene.Add(bird);
        EventInstance instance = Audio.Play("event:/game/general/bird_in", bird.Position);
        bird.Facing = Facings.Left;
        bird.Sprite.Play("fall");
        Vector2 from = bird.Position;
        Vector2 to = gravestone.Position + new Vector2(1f, -16f);
        float percent = 0f;
        while (percent < 1f)
        {
            bird.Position = from + (to - from) * Ease.QuadOut(percent);
            Audio.Position(instance, bird.Position);
            if (percent > 0.5f)
            {
                bird.Sprite.Play("fly");
            }
            percent += Engine.DeltaTime * 0.5f;
            yield return null;
        }
        bird.Position = to;
        bird.Sprite.Play("idle");
        yield return 0.5f;
        bird.Sprite.Play("croak");
        yield return 0.6f;
        Audio.Play("event:/game/general/bird_squawk", bird.Position);
        yield return 0.9f;
        Audio.Play("event:/char/madeline/stand", player.Position);
        player.Sprite.Play("idle");
        yield return 1f;
        yield return bird.StartleAndFlyAway();
    }

    private IEnumerator BadelineRejoin()
    {
        Audio.Play("event:/new_content/char/badeline/maddy_join_quick", chara.Position);
        Vector2 from = chara.Position;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.25f)
        {
            chara.Position = Vector2.Lerp(from, player.Position, Ease.CubeIn(p));
            yield return null;
        }
        Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
        Level.Session.Inventory.Dashes = 5;
        player.Dashes = 5;
        chara.RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        player.Facing = Facings.Right;
        player.DummyAutoAnimate = true;
        player.DummyGravity = true;
        player.StateMachine.State = 0;
        Level.Session.Inventory.Dashes = 2;
        player.Dashes = 2;
        if (chara != null)
        {
            chara.RemoveSelf();
        }
        if (bird != null)
        {
            bird.RemoveSelf();
        }
        if (!addedBooster)
        {
            level.Add(new CharaBoost(new Vector2[1] { boostTarget }, lockCamera: false));
        }
        level.ResetZoom();
    }
}
