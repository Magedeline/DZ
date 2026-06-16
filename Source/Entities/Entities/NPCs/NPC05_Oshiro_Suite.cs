using System.Runtime.CompilerServices;

namespace Celeste.NPCs;
[CustomEntity(ids: "DZ/NPC05_Oshiro_Suite")]

public class NPC05_Oshiro_Suite : NPC
{
    private const string ConversationCounter = "oshiroSuiteSadConversation";

    private bool finishedTalking;
    protected new VertexLight Light;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC05_Oshiro_Suite(Vector2 position)
        : base(position)
    {
        Add(Sprite = GFX.SpriteBank.Create("oshiro"));
        Sprite.Visible = true;
        Sprite.Play("idle");
        Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
        Add(Talker = new TalkComponent(new Rectangle(-16, -8, 32, 8), new Vector2(0f, -24f), OnTalk));
        Talker.Enabled = false;
        MoveAnim = "move";
        IdleAnim = "idle";
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public NPC05_Oshiro_Suite(EntityData data, Vector2 offset)
        : this(data.Position + offset)
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!base.Session.GetFlag("oshiro_resort_suite"))
        {
            base.Scene.Add(new Cutscenes.CS05_OshiroMasterSuite(this));
            return;
        }
        Sprite.Play("idle_ground");
        Talker.Enabled = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnTalk(global::Celeste.Player player)
    {
        finishedTalking = false;
        Level.StartCutscene(EndTalking);
        Add(new Coroutine(Talk(player)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Talk(global::Celeste.Player player)
    {
        int conversation = Session.GetCounter("oshiroSuiteSadConversation");
        yield return PlayerApproach(player, turnToFace: false, 12f);
        yield return Textbox.Say("DZ_CH5_OSHIRO_SUITE_SAD" + conversation);
        yield return PlayerLeave(player);
        EndTalking(SceneAs<Level>());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EndTalking(Level level)
    {
        global::Celeste.Player player = base.Scene.Entities.FindFirst<global::Celeste.Player>();
        if (player != null)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = Player.StNormal;
        }
        if (!finishedTalking)
        {
            int counter = base.Session.GetCounter("oshiroSuiteSadConversation");
            counter++;
            counter %= 7;
            if (counter == 0)
            {
                counter++;
            }
            base.Session.SetCounter("oshiroSuiteSadConversation", counter);
            finishedTalking = true;
        }
    }
}




