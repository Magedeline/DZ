namespace Celeste.Triggers {
    [CustomEntity(new string[] { "MaggyHelper/PinkPlatBerryCollectTrigger" })]
    [Monocle.Tracked]
    [HotReloadable]
    public class PinkPlatBerryCollectTrigger : Trigger {
        
        public PinkPlatBerryCollectTrigger(EntityData e, Vector2 offset) : base(e, offset) {
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            // Find and collect all Pink Platinum Berries being carried
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Entities.PinkPlatinumBerry berry)
                {
                    berry.OnCollect();
                }
            }
        }
    }
}



