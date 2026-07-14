using Celeste.Cutscenes;
using Celeste.Entities;
using DZ;
using DZBirdNPC = Celeste.Entities.DZBirdNPC;
using DZBridge = Celeste.Entities.Bridge;

namespace Celeste.Triggers;

[CustomEntity("DZ/BirdNPCDashingTutorialTrigger")]
[HotReloadable]
public class BirdNPCDashingTutorialTrigger : Trigger
{
    private bool triggered;

    public BirdNPCDashingTutorialTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = scene as Level;
        if (level != null && level.Session.GetFlag("birdfirstdash"))
        {
            RemoveSelf();
        }
    }

    public override void Update()
    {
        base.Update();
        if (triggered)
            return;

        K_Player kPlayer = Scene?.Tracker.GetEntity<K_Player>();
        if (kPlayer != null && CollideCheck(kPlayer))
        {
            TryFireTutorial(kPlayer: kPlayer, vanillaPlayer: null);
            return;
        }

        Player vanillaPlayer = Scene?.Tracker.GetEntity<Player>();
        if (vanillaPlayer != null
            && !K_PlayerHooks.ShadowPlayers.Contains(vanillaPlayer)
            && CollideCheck(vanillaPlayer))
        {
            TryFireTutorial(kPlayer: null, vanillaPlayer: vanillaPlayer);
        }
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (!K_PlayerHooks.ShadowPlayers.Contains(player))
            TryFireTutorial(kPlayer: null, vanillaPlayer: player);
    }

    private void TryFireTutorial(K_Player kPlayer, Player vanillaPlayer)
    {
        if (triggered)
            return;

        Level level = SceneAs<Level>();
        if (level == null || level.Session.GetFlag("birdfirstdash"))
            return;

        triggered = true;
        level.Session.SetFlag("birdfirstdash");

        DZBirdNPC bird = level.Entities.FindFirst<DZBirdNPC>();
        DZBridge bridge = level.Tracker.GetEntity<DZBridge>();

        if (kPlayer != null)
        {
            kPlayer.StateMachine.State = K_Player.StBirdDashTutorial;
            kPlayer.Dashes = 0;
            level.Session.Inventory.Dashes = 1;
            if (bird != null)
                bird.Add(new Coroutine(bird.StartleAndFlyAway()));
        }
        else if (vanillaPlayer != null)
        {
            level.Add(new CS00_EndingDZ(vanillaPlayer, bird, bridge));
        }
    }
}
