#nullable enable
namespace Celeste.Triggers
{
    [CustomEntity(new string[] { "DZ/DetachPinkPlatberryTrigger" })]
    [Tracked(false)]
    internal class DetachPinkPlatberryTrigger : Trigger
    {
        public Vector2 Target;
        public bool Global;

        public DetachPinkPlatberryTrigger(EntityData data, Vector2 offset)
          : base(data, offset)
        {
            Vector2[] vector2Array = data.NodesOffset(offset);
            if (vector2Array.Length != 0)
                this.Target = vector2Array[0];
            this.Global = data.Bool("global", true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            for (int index = player.Leader.Followers.Count - 1; index >= 0; --index)
            {
                if (player.Leader.Followers[index].Entity is Entities.PinkPlatinumBerry)
                    this.Add((Component)new Coroutine(this.detatchFollower(player.Leader.Followers[index])));
            }
        }

        private IEnumerator detatchFollower(Follower follower)
        {
            Leader leader = follower.Leader;
            Entity entity = follower.Entity;
            float num = (entity.Position - Target).Length();
            float time = num / 200f;
            if (entity is Entities.PinkPlatinumBerry strawberry)
                strawberry.ReturnHomeWhenLost = false;
            leader.LoseFollower(follower);
            entity.Active = false;
            entity.Collidable = false;
            if (Global)
            {
                entity.AddTag((int)Tags.Global);
                follower.OnGainLeader += (Action)(() => entity.RemoveTag((int)Tags.Global));
            }
            else
                entity.AddTag((int)Tags.Persistent);
            Audio.Play("event:/final_content/game/19_TheEnd/DetachPinkPlatBerry", entity.Position);
            Vector2 position = entity.Position;
            SimpleCurve curve = new SimpleCurve(position, Target, position + (Target - position) * 0.5f + new Vector2(0.0f, -64f));
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / time)
            {
                entity.Position = curve.GetPoint(Ease.CubeInOut(p));
                yield return null;
            }
            entity.Active = true;
            entity.Collidable = true;
        }
    }
}




