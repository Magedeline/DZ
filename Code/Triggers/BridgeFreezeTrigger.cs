using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Triggers
{
    [CustomEntity(ids: "MaggyHelper/BridgeFreezeTrigger")]
    public class BridgeFreezeTrigger : Trigger
    {
        private readonly float freezeStrength;
        private readonly TimeRateModifier timeRateModifier;
        private bool triggered;

        public BridgeFreezeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            freezeStrength = data.Float("freezeStrength", 0.001f);
            Add(timeRateModifier = new TimeRateModifier(1f, false));
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            timeRateModifier.SetTimeRateMultiplier(freezeStrength);

            if (!triggered)
            {
                triggered = true;
                var bird = Scene.Tracker.GetEntity<global::Celeste.Entities.BirdNPC>();
                if (bird != null)
                {
                    bird.Add(new Coroutine(bird.StartleAndFlyAway()));
                }
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            timeRateModifier.ResetTimeRateMultiplier();
        }

        public override void SceneEnd(Scene scene)
        {
            timeRateModifier.ResetTimeRateMultiplier();
            base.SceneEnd(scene);
        }
    }
}
