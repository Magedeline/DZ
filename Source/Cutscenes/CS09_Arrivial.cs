using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

public class CS09_Arrivial : CutsceneEntity
{
    private Player player;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS09_Arrivial(Player player)
        : base(fadeInOnSkip: true, endingChapterAfter: false)
    {
        this.player = player;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        player.StateMachine.State = Player.StDummy;
        player.Speed = Vector2.Zero;

        yield return 0.35f;

        yield return Textbox.Say("DZ_CH9_ARRIVIAL",
            Trigger0MadelineTurnLeft,
            Trigger1GigaSfxAndFlash,
            Trigger2BlackKatanaStrike,
            Trigger3TeleportToCreditRoom);

        yield return 0.2f;

        level.Session.SetFlag("ch9_arrivial_complete", true);
        EndCutscene(level);
    }

    private IEnumerator Trigger0MadelineTurnLeft()
    {
        player.Facing = Facings.Left;
        yield return 0.15f;
    }

    private IEnumerator Trigger1GigaSfxAndFlash()
    {
        Level level = Scene as Level;

        Audio.Play("event:/new_content/game/10_farewell/bird_crash_whoosh", player.Position);
        level?.Flash(Color.White, false);
        yield return 0.1f;
    }

    private IEnumerator Trigger2BlackKatanaStrike()
    {
        Level level = Scene as Level;

        level?.Shake(0.45f);
        level?.Displacement.AddBurst(player.Center, 0.6f, 12f, 64f, 0.7f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

        Vector2 from = player.Position;
        Vector2 to = from + new Vector2(0f, -28f);

        for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime / 0.45f)
        {
            player.Position = Vector2.Lerp(from, to, Ease.CubeOut(progress));
            yield return null;
        }

        player.Position = to;
        player.Speed = new Vector2(0f, -140f);
        yield return 0.2f;
    }

    private IEnumerator Trigger3TeleportToCreditRoom()
    {
        Level level = Scene as Level;
        level?.TeleportTo(player, "lvl_credit", Player.IntroTypes.Respawn, Vector2.Zero);
        yield return 0.5f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        // Reset any velocity left over from the katana strike so a skip doesn't
        // launch the player upward, and ensure the arrival actually completes:
        // if the cutscene was skipped before Trigger3 ran, finish the teleport
        // and set the completion flag so the player isn't left stranded.
        player.Speed = Vector2.Zero;
        if (!level.Session.GetFlag("ch9_arrivial_complete"))
        {
            level.Session.SetFlag("ch9_arrivial_complete", true);
            level.TeleportTo(player, "lvl_credit", Player.IntroTypes.Respawn, Vector2.Zero);
        }
        player.StateMachine.State = Player.StNormal;
    }
}