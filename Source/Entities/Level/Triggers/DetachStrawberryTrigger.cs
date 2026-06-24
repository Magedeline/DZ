using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that detaches strawberries (collectibles) from the player and moves them to a target.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class DetachStrawberryTrigger : CelesteTrigger
{
    public Vector2 Target;
    public bool Global;

    public DetachStrawberryTrigger(Vector2 position, int width, int height, Vector2 target, bool global = true) : base(position, width, height)
    {
        Target = target;
        Global = global;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Detach followers (strawberries)
        // for (int i = player.Leader.Followers.Count - 1; i >= 0; i--)
        // {
        //     if (player.Leader.Followers[i].Entity is Strawberry)
        //     {
        //         AddComponent(new Coroutine(DetachFollower(player.Leader.Followers[i])));
        //     }
        // }
    }

    private IEnumerator DetachFollower(Entity follower)
    {
        // float time = (follower.Position - Target).Length() / 200f;
        
        // TODO: Configure strawberry
        // if (follower is Strawberry strawberry)
        //     strawberry.ReturnHomeWhenLost = false;
        
        // TODO: Remove from leader
        // leader.LoseFollower(follower);
        
        // follower.Active = false; // TODO: Active not available in Nez.Entity
        // follower.Collider?.SetEnabled(false); // TODO: Access collider via GetComponent<Collider>()
        
        // TODO: Set tags
        // if (Global)
        // {
        //     follower.AddTag(Tags.Global);
        //     // follower.GetComponent<Follower>().OnGainLeader += () => follower.RemoveTag(Tags.Global);
        // }
        // else
        //     follower.AddTag(Tags.Persistent);
        
        // TODO: play sound: event:/new_content/game/10_farewell/strawberry_gold_detach
        
        Vector2 start = follower.Position;
        Vector2 control = start + (Target - start) * 0.5f + new Vector2(0f, -64f);
        
        for (float p = 0f; p < 1f; p += Time.DeltaTime / ((start - Target).Length() / 200f))
        {
            float eased = p < 0.5f ? 4f * p * p * p : 1f - MathF.Pow(-2f * p + 2f, 3f) / 2f; // CubeInOut easing
            follower.Position = new Vector2(
                MathHelper.Lerp(MathHelper.Lerp(start.X, control.X, eased), MathHelper.Lerp(control.X, Target.X, eased), eased),
                MathHelper.Lerp(MathHelper.Lerp(start.Y, control.Y, eased), MathHelper.Lerp(control.Y, Target.Y, eased), eased)
            );
            yield return null;
        }

        // follower.Active = true; // TODO: Active not available in Nez.Entity
        // follower.Collider?.SetEnabled(true); // TODO: Access collider via GetComponent<Collider>()
    }
}
