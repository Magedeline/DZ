using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

[HotReloadable]
public class CS09_GoldenFlower : CutsceneEntity
{
    public const string Flag = "ch9_goldenflower_complete";

    private const string DialogKey = "DZ_CH9_GOLDENFLOWER";
    private const float FloweySpawnOffsetX = 72f;

    private readonly global::Celeste.Player player;
    private FloweyNPC flowey;
    private bool spawnedFlowey;

    public CS09_GoldenFlower(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (player?.StateMachine == null)
        {
            yield break;
        }

        player.StateMachine.State = global::Celeste.Player.StDummy;
        player.StateMachine.Locked = true;
        player.Speed = Vector2.Zero;

        flowey = level.Tracker.GetEntity<FloweyNPC>();
        if (flowey == null)
        {
            flowey = new FloweyNPC(new Vector2(player.X + FloweySpawnOffsetX, player.Y), startHidden: true);
            level.Add(flowey);
            spawnedFlowey = true;
        }

        flowey.FacePlayer();
        yield return flowey.Emerge();
        yield return 0.2f;

        yield return Textbox.Say(DialogKey);

        yield return 0.2f;

        if (spawnedFlowey && flowey?.Scene != null)
        {
            yield return flowey.Retreat();
            flowey.RemoveSelf();
        }

        level.Session.SetFlag(Flag, true);
        level.Add(new CS09_AreaComplete(player, skipCredits: true, skipMessage: true));
        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = global::Celeste.Player.StNormal;
        }

        if (spawnedFlowey && flowey?.Scene != null)
        {
            flowey.RemoveSelf();
        }
    }
}