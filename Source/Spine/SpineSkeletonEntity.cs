using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

/// <summary>
/// A standalone entity that renders a Spine skeleton at a world position.
///
/// Usage — place in a scene:
///   var actor = new SpineSkeletonEntity(
///       position:      new Vector2(160, 90),
///       atlasPath:     "/path/to/skeleton.atlas",
///       skeletonPath:  "/path/to/skeleton.json",
///       firstAnim:     "idle",
///       loop:          true);
///   Scene.Add(actor);
///
/// Or access the component directly for fine-grained control:
///   actor.Spine.PlayAnimation("walk", loop: true);
///   actor.Spine.TimeScale = 2f;
///   actor.Spine.ScaleX = -1f; // flip horizontally
/// </summary>
public class SpineSkeletonEntity : Entity
{
    /// <summary>Direct access to the underlying component.</summary>
    public SpineSkeletonComponent Spine { get; }

    /// <param name="position">World position for the entity.</param>
    /// <param name="atlasPath">Absolute path to the .atlas file.</param>
    /// <param name="skeletonPath">Absolute path to the .json or .skel file.</param>
    /// <param name="firstAnim">If non-null, this animation plays immediately on load.</param>
    /// <param name="loop">Whether <paramref name="firstAnim"/> loops.</param>
    /// <param name="depth">Render depth. Lower values draw on top. Default: 0.</param>
    /// <param name="isBinary">True if the skeleton file is binary (.skel); false for JSON.</param>
    public SpineSkeletonEntity(
        Vector2 position,
        string atlasPath,
        string skeletonPath,
        string firstAnim = null,
        bool loop = true,
        int depth = 0,
        bool isBinary = false)
        : base(position)
    {
        Depth = depth;
        Spine = new SpineSkeletonComponent(atlasPath, skeletonPath, isBinary);
        Add(Spine);

        if (firstAnim != null)
        {
            // Play after Load() runs in Added(), so queue via a post-add callback
            _firstAnim = firstAnim;
            _firstAnimLoop = loop;
        }
    }

    private readonly string _firstAnim;
    private readonly bool _firstAnimLoop;

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (_firstAnim != null)
            Spine.PlayAnimation(_firstAnim, _firstAnimLoop);
    }
}
