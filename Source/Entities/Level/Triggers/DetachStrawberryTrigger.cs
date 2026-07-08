#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/DetachStrawberryTrigger")]
public class DetachStrawberryTrigger : Trigger {
    private Vector2 target;
    private bool global;

    public DetachStrawberryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        target = data.NodesOffset(offset).Length > 0 ? data.NodesOffset(offset)[0] : data.Position + offset;
        global = data.Bool("global", true);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        for (int i = player.Leader.Followers.Count - 1; i >= 0; i--) {
            if (player.Leader.Followers[i].Entity is Strawberry strawberry)
                Add(new Coroutine(DetachFollower(player.Leader, player.Leader.Followers[i])));
        }
    }

    private IEnumerator DetachFollower(Leader leader, Follower follower) {
        Entity entity = follower.Entity;
        if (entity is Strawberry strawberry)
            strawberry.ReturnHomeWhenLost = false;
        leader.LoseFollower(follower);
        entity.Active = false;
        entity.Collidable = false;
        if (global)
            entity.Tag = Tags.Global;
        else
            entity.Tag = Tags.Persistent;
        Audio.Play("event:/new_content/game/10_farewell/strawberry_gold_detach");
        Vector2 start = entity.Position;
        Vector2 control = start + (target - start) * 0.5f + new Vector2(0f, -64f);
        float duration = (start - target).Length() / 200f;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration) {
            float eased = p < 0.5f ? 4f * p * p * p : 1f - MathF.Pow(-2f * p + 2f, 3f) / 2f;
            entity.Position = new Vector2(
                MathHelper.Lerp(MathHelper.Lerp(start.X, control.X, eased), MathHelper.Lerp(control.X, target.X, eased), eased),
                MathHelper.Lerp(MathHelper.Lerp(start.Y, control.Y, eased), MathHelper.Lerp(control.Y, target.Y, eased), eased)
            );
            yield return null;
        }
        entity.Active = true;
        entity.Collidable = true;
    }
}
