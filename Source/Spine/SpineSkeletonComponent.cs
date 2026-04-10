using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Spine;

namespace MaggyHelper.Entities;

/// <summary>
/// Monocle Component that loads, updates, and renders a Spine skeleton.
/// Attach to any Entity to give it a Spine animation.
///
/// Usage:
///   var spine = new SpineSkeletonComponent("path/to/file.atlas", "path/to/file.json");
///   entity.Add(spine);
///   spine.PlayAnimation("walk", loop: true);
/// </summary>
public class SpineSkeletonComponent : Component
{
    // ──────────────────── Public properties ─────────────────────────────────
    /// <summary>The loaded Spine skeleton. Null until the component is added to an entity.</summary>
    public Skeleton Skeleton { get; private set; }

    /// <summary>Controls which animations are playing and how they blend.</summary>
    public AnimationState AnimationState { get; private set; }

    /// <summary>Animation playback speed multiplier. Default 1.</summary>
    public float TimeScale { get; set; } = 1f;

    /// <summary>Horizontal scale of the skeleton. Use -1 to flip horizontally.</summary>
    public float ScaleX { get; set; } = 1f;

    /// <summary>Vertical scale of the skeleton.</summary>
    public float ScaleY { get; set; } = 1f;

    /// <summary>Tint color applied to the entire skeleton.</summary>
    public Color Tint { get; set; } = Color.White;

    // ──────────────────── Private fields ────────────────────────────────────
    private readonly string atlasPath;
    private readonly string skeletonPath;
    private readonly bool isBinary;
    private SkeletonRenderer renderer;

    // ──────────────────── Constructor ───────────────────────────────────────
    /// <param name="atlasPath">Absolute path to the .atlas file.</param>
    /// <param name="skeletonPath">Absolute path to the .json or .skel file.</param>
    /// <param name="isBinary">True if using binary .skel format; false for JSON.</param>
    public SpineSkeletonComponent(string atlasPath, string skeletonPath, bool isBinary = false)
        : base(active: true, visible: true)
    {
        this.atlasPath = atlasPath;
        this.skeletonPath = skeletonPath;
        this.isBinary = isBinary;
    }

    // ──────────────────── Lifecycle ─────────────────────────────────────────
    public override void Added(Entity entity)
    {
        base.Added(entity);
        Load();
    }

    private void Load()
    {
        var gd = Engine.Graphics.GraphicsDevice;

        // Load atlas + skeleton data
        var atlas = new Spine.Atlas(atlasPath, new XnaTextureLoader(gd));
        SkeletonData data = isBinary
            ? new SkeletonBinary(atlas).ReadSkeletonData(skeletonPath)
            : new SkeletonJson(atlas).ReadSkeletonData(skeletonPath);

        // Build skeleton and animation state
        Skeleton = new Skeleton(data);
        AnimationState = new AnimationState(new AnimationStateData(data));

        // Build renderer and configure its effect so world coords match Celeste's camera
        renderer = new SkeletonRenderer(gd);
        UpdateCameraProjection();
    }

    // ──────────────────── Update ────────────────────────────────────────────
    public override void Update()
    {
        if (Skeleton == null) return;

        // Advance time
        float dt = Engine.DeltaTime * TimeScale;
        AnimationState.Update(dt);
        AnimationState.Apply(Skeleton);

        // Sync skeleton position/scale with the entity
        Skeleton.X = Entity.X;
        Skeleton.Y = Entity.Y;
        Skeleton.ScaleX = ScaleX;
        Skeleton.ScaleY = ScaleY;
        Skeleton.R = Tint.R / 255f;
        Skeleton.G = Tint.G / 255f;
        Skeleton.B = Tint.B / 255f;
        Skeleton.A = Tint.A / 255f;
        Skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
    }

    // ──────────────────── Render ────────────────────────────────────────────
    public override void Render()
    {
        if (Skeleton == null || renderer == null) return;

        // Update the BasicEffect projection in case the camera moved this frame
        UpdateCameraProjection();

        // SkeletonRenderer uses raw GPU calls (MeshBatcher), not SpriteBatch.
        // Celeste's SpriteBatch must be flushed first, then restarted afterwards.
        var sb = Draw.SpriteBatch;
        sb.End();

        renderer.Begin();
        renderer.Draw(Skeleton);
        renderer.End();

        // Restart SpriteBatch to match what Celeste expects for subsequent draw calls
        var camera = (Entity?.Scene as Level)?.Camera;
        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            camera?.Matrix ?? Matrix.Identity
        );
    }

    // ──────────────────── Animation helpers ─────────────────────────────────
    /// <summary>Starts playing an animation immediately, replacing the current one.</summary>
    public TrackEntry PlayAnimation(string animationName, bool loop = true, int trackIndex = 0)
    {
        return AnimationState.SetAnimation(trackIndex, animationName, loop);
    }

    /// <summary>Queues an animation to play after the current one finishes.</summary>
    public TrackEntry QueueAnimation(string animationName, bool loop = false, float delay = 0f, int trackIndex = 0)
    {
        return AnimationState.AddAnimation(trackIndex, animationName, loop, delay);
    }

    /// <summary>Immediately sets the skeleton to its setup pose, clearing all animations.</summary>
    public void SetToSetupPose()
    {
        Skeleton?.SetToSetupPose();
        AnimationState?.ClearTracks();
    }

    // ──────────────────── Internal helpers ──────────────────────────────────
    private void UpdateCameraProjection()
    {
        if (renderer?.Effect is not BasicEffect fx) return;

        var camera = (Entity?.Scene as Level)?.Camera;
        if (camera != null)
        {
            // Map world coordinates to the camera's visible rectangle
            fx.Projection = Matrix.CreateOrthographicOffCenter(
                camera.Left, camera.Right, camera.Bottom, camera.Top, 0f, 1f);
        }
        else
        {
            // Fallback: map to the raw viewport
            var vp = Engine.Graphics.GraphicsDevice.Viewport;
            fx.Projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0f, 1f);
        }

        fx.View = Matrix.Identity;
    }
}
