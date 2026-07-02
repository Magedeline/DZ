using System.Runtime.CompilerServices;
using Celeste.Entities;
using NPC = Celeste.NPCs.NPC;

namespace DZ;

[CustomEntity(ids: "DZ/NPC15X_Madeline_and_Badeline_Ending")]
[Tracked(true)]
public class NPC15X_Madeline_and_Badeline_Ending : NPC
{
    private CelestePlayer player;

    private MadelineNPCDummy dummy;

    private TalkComponent talker;

    private Coroutine talkRoutine;

    private int conversation;

    private bool ch15EasterEgg;

    private Sprite badelineSprite;

    private Vector2 badelineOffset = new Vector2(24f, -20f); // Float to the right and above Madeline

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC15X_Madeline_and_Badeline_Ending(EntityData data, Vector2 offset)
        : this(data, offset, false)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC15X_Madeline_and_Badeline_Ending(EntityData data, Vector2 offset, bool ch15EasterEgg)
        : base(data.Position + offset)
    {
        // Madeline dummy
        dummy = new MadelineNPCDummy(Vector2.Zero);
        Add(Sprite = dummy.Sprite);
        Sprite.Play("idle");
        Sprite.Scale.X = -1f;
        MoveAnim = "walk";
        Maxspeed = 64f;
        
        // Badeline sprite - separate from Madeline
        badelineSprite = GFX.SpriteBank.Create("DZ_badeline");
        badelineSprite.Play("fall");
        badelineSprite.Scale.X = -1f;
        Add(badelineSprite);
        
        Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0f, -24f), OnTalk));
        this.ch15EasterEgg = ch15EasterEgg;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        
        // Position Badeline floating next to Madeline with a subtle bobbing animation
        if (badelineSprite != null)
        {
            float bobOffset = (float)Math.Sin(base.Scene.TimeActive * 2f) * 2f;
            badelineSprite.Position = badelineOffset + new Vector2(0f, bobOffset);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnTalk(CelestePlayer player)
    {
        this.player = player;
        (base.Scene as Level).StartCutscene(EndTalking);
        Add(talkRoutine = new Coroutine(TalkRoutine(player)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator TalkRoutine(CelestePlayer player)
    {
        player.StateMachine.State = Player.StDummy;
        player.ForceCameraUpdate = true;
        while (!player.OnGround())
        {
            yield return null;
        }
        yield return player.DummyWalkToExact((int)X - 16);
        player.Facing = Facings.Right;
        if (ch15EasterEgg)
        {
            yield return 0.5f;
            yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
            Dialog.Language.Dialog["DZ_CH15_MADELINE_AND_BADELINE_ENDING_EASTEREGG"] = _GetDebugModeDialog("{portrait madeline normal} You've unlocked Debug Mode!");
            yield return Textbox.Say("DZ_CH15_MADELINE_AND_BADELINE_ENDING_EASTEREGG");
            talker.Enabled = false;
        }
        else if (conversation == 0)
        {
            yield return 0.5f;
            yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
            yield return Textbox.Say("DZ_CH09_MADELINE_AND_BADELINE_ENDING_A");
        }
        else if (conversation == 1)
        {
            yield return 0.5f;
            yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
            yield return Textbox.Say("DZ_CH15_MADELINE_AND_BADELINE_ENDING_B");
            talker.Enabled = false;
        }
        yield return Level.ZoomBack(0.5f);
        Level.EndCutscene();
        EndTalking(Level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EndTalking(Level level)
    {
        if (player != null)
        {
            player.StateMachine.State = Player.StNormal;
            player.ForceCameraUpdate = false;
        }
        conversation++;
        if (talkRoutine != null)
        {
            talkRoutine.RemoveSelf();
            talkRoutine = null;
        }
        Sprite.Play("idle");
    }

    private static string _GetDebugModeDialog(string vanillaDialog)
    {
        if (Dialog.Has("DZ_CH15_MADELINE_AND_BADELINE_ENDING_EASTEREGG"))
        {
            return Dialog.Get("DZ_CH15_MADELINE_AND_BADELINE_ENDING_EASTEREGG");
        }
        return vanillaDialog;
    }
}




