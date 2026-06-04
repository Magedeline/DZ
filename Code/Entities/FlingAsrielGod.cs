namespace Celeste.Entities;

/// <summary>
/// Asriel God Fling - Similar to FlingBird but themed as Asriel God helping the player
/// and Kirby reach Siamo Zero to deal damage. Uses asrielgodboss sprites.
/// </summary>
[CustomEntity(ids: "MaggyHelper/FlingAsrielGod")]
[Tracked(true)]
[HotReloadable]
public class FlingAsrielGod : Entity
{
    public const float SKIP_DIST = 100f;
    public static readonly Vector2 FLING_SPEED = new(420f, -140f);

    private readonly EntityData entityData;
    private readonly SoundSource moveSfx;
    private readonly Sprite sprite;
    private readonly Vector2 spriteOffset = new(0.0f, 8f);
    private readonly Color trailColor = Calc.HexToColor("ffdd57"); // Golden trail
    private float flingAccel;
    private Vector2 flingSpeed;
    private Vector2 flingTargetSpeed;
    public bool LightningRemoved;
    public List<Vector2[]> NodeSegments;
    private int segmentIndex;
    public List<bool> SegmentsWaiting;
    private States state;
    private readonly TimeRateModifier timeRateModifier;

    public FlingAsrielGod(Vector2[] nodes, bool skippable)
        : base(nodes[0])
    {
        Depth = -1;
        Add(sprite = GFX.SpriteBank.Create("asrielgodboss"));
        sprite.Play("idle");
        sprite.Scale.X = -1f;
        sprite.Position = spriteOffset;
        Collider = new Circle(20f);
        Add(new PlayerCollider(OnPlayer));
        Add(moveSfx = new SoundSource());
        Add(timeRateModifier = new TimeRateModifier(1f, false));
        NodeSegments = new List<Vector2[]>();
        NodeSegments.Add(nodes);
        SegmentsWaiting = new List<bool>();
        SegmentsWaiting.Add(skippable);
        Add(new TransitionListener
        {
            OnOut = t => sprite.Color = Color.White * (1f - Calc.Map(t, 0.0f, 0.4f))
        });
    }

    public FlingAsrielGod(EntityData data, Vector2 levelOffset)
        : this(data.NodesWithPosition(levelOffset), data.Bool("waiting"))
    {
        entityData = data;
    }

    private void OnPlayer(global::Celeste.Player player)
    {
        if (state != States.Wait)
            return;
        flingSpeed = player.Speed * 0.4f;
        flingSpeed.Y = 120f;
        flingTargetSpeed = Vector2.Zero;
        flingAccel = 1000f;
        player.Speed = Vector2.Zero;
        state = States.Fling;
        Add(new Coroutine(DoFlingRoutine(player)));
        Audio.Play("event:/new_content/char/pusheen/asriel/Asriel_Grab", Center);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        var all = Scene.Entities.FindAll<FlingAsrielGod>();
        for (var index = all.Count - 1; index >= 0; --index)
            if (all[index].entityData?.Level?.Name != entityData?.Level?.Name)
                all.RemoveAt(index);
        all.Sort((a, b) => Math.Sign(a.X - b.X));
        if (all[0] == this)
            for (var index = 1; index < all.Count; ++index)
            {
                NodeSegments.Add(all[index].NodeSegments[0]);
                SegmentsWaiting.Add(all[index].SegmentsWaiting[0]);
                all[index].RemoveSelf();
            }

        if (SegmentsWaiting[0])
        {
            sprite.Play("boss");
            sprite.Scale.X = 1f;
        }

        var entity = scene.Tracker.GetEntity<global::Celeste.Player>();
        if (entity == null || entity.X <= (double)X)
            return;
        RemoveSelf();
    }

    private void Skip()
    {
        state = States.Move;
        Add(new Coroutine(MoveRoutine()));
    }

    public override void Update()
    {
        base.Update();
        if (state != States.Wait)
            sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
        switch (state)
        {
            case States.Wait:
                var entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (entity != null && entity.X - (double)X >= SKIP_DIST)
                {
                    Skip();
                    break;
                }

                if (SegmentsWaiting[segmentIndex] && LightningRemoved)
                {
                    Skip();
                    break;
                }

                if (entity == null)
                    break;
                var num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0.0f);
                sprite.Position = Calc.Approach(sprite.Position,
                    spriteOffset + (entity.Center - Position).SafeNormalize() * num, 32f * Engine.DeltaTime);
                break;
            case States.Fling:
                if (flingAccel > 0.0)
                    flingSpeed = Calc.Approach(flingSpeed, flingTargetSpeed, flingAccel * Engine.DeltaTime);
                Position += flingSpeed * Engine.DeltaTime;
                break;
            case States.WaitForLightningClear:
                if (Scene.Entities.FindFirst<Lightning>() != null && X <= (double)(Scene as Level).Bounds.Right)
                    break;
                sprite.Scale.X = 1f;
                state = States.Leaving;
                Add(new Coroutine(LeaveRoutine()));
                break;
        }
    }

    private IEnumerator DoFlingRoutine(global::Celeste.Player player)
    {
        var level = Scene as Level;
        var screenSpaceFocusPoint = player.Position - level.Camera.Position;
        screenSpaceFocusPoint.X = Calc.Clamp(screenSpaceFocusPoint.X, 145f, 215f);
        screenSpaceFocusPoint.Y = Calc.Clamp(screenSpaceFocusPoint.Y, 85f, 95f);
        Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.1f, 0.2f)));
        timeRateModifier.SetTimeRateMultiplier(0.8f);
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        while (flingSpeed != Vector2.Zero)
            yield return null;
        // Asriel grabs and throws
        sprite.Play("castsp");
        sprite.Scale.X = 1f;
        flingSpeed = new Vector2(-140f, 140f);
        flingTargetSpeed = Vector2.Zero;
        flingAccel = 1400f;
        yield return 0.1f;
        Engine.FreezeTimer = 0.05f;
        flingTargetSpeed = FLING_SPEED;
        flingAccel = 6000f;
        yield return 0.1f;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        timeRateModifier.ResetTimeRateMultiplier();
        level.Shake();
        Add(new Coroutine(level.ZoomBack(0.1f)));
        player.FinishFlingBird();
        flingTargetSpeed = Vector2.Zero;
        flingAccel = 4000f;
        yield return 0.3f;
        Add(new Coroutine(MoveRoutine()));
    }

    private IEnumerator MoveRoutine()
    {
        state = States.Move;
        sprite.Play("star");
        sprite.Scale.X = 1f;
        moveSfx.Play("guid://{9ba95702-66ae-441f-944e-c123175195ff}");
        for (var nodeIndex = 1; nodeIndex < NodeSegments[segmentIndex].Length - 1; nodeIndex += 2)
        {
            var position = Position;
            var anchor = NodeSegments[segmentIndex][nodeIndex];
            var to = NodeSegments[segmentIndex][nodeIndex + 1];
            yield return MoveOnCurve(position, anchor, to);
        }

        ++segmentIndex;
        var atEnding = segmentIndex >= NodeSegments.Count;
        if (!atEnding)
        {
            var position = Position;
            var anchor = NodeSegments[segmentIndex - 1][NodeSegments[segmentIndex - 1].Length - 1];
            var to = NodeSegments[segmentIndex][0];
            yield return MoveOnCurve(position, anchor, to);
        }

        sprite.Rotation = 0.0f;
        sprite.Scale = Vector2.One;
        if (atEnding)
        {
            sprite.Play("boss");
            sprite.Scale.X = 1f;
            state = States.WaitForLightningClear;
        }
        else
        {
            if (SegmentsWaiting[segmentIndex])
                sprite.Play("boss");
            else
                sprite.Play("idle");
            sprite.Scale.X = -1f;
            state = States.Wait;
        }
    }

    private IEnumerator LeaveRoutine()
    {
        sprite.Scale.X = 1f;
        sprite.Play("star");
        var vector = new Vector2((Scene as Level).Bounds.Right + 32, Y);
        yield return MoveOnCurve(Position, (Position + vector) * 0.5f - Vector2.UnitY * 12f, vector);
        RemoveSelf();
    }

    private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
    {
        var curve = new SimpleCurve(from, to, anchor);
        var duration = curve.GetLengthParametric(32) / 500f;
        var was = from;
        for (var t = 0.016f; t <= 1.0f; t += Engine.DeltaTime / duration)
        {
            Position = curve.GetPoint(t).Floor();
            sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0.0f, t - 0.05f)),
                curve.GetPoint(Math.Min(1f, t + 0.05f)));
            sprite.Scale.X = 1.25f;
            sprite.Scale.Y = 0.7f;
            if ((was - Position).Length() > 32.0f)
            {
                TrailManager.Add(this, trailColor, 1f);
                was = Position;
            }

            yield return null;
        }

        Position = to;
    }

    public override void Render()
    {
        base.Render();
    }

    private enum States
    {
        Wait,
        Fling,
        Move,
        WaitForLightningClear,
        Leaving
    }
}

