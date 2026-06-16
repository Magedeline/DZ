namespace Celeste.Entities;
[CustomEntity(ids: "DZ/FlingSans")]
[HotReloadable]
public class FlingSans : Entity
{
    public const float SKIP_DIST = 100f;
    public static ParticleType PFeather;
    public static readonly Vector2 FLING_SPEED = new(380f, -100f);
    private readonly EntityData entityData;
    private readonly SoundSource moveSfx;
    private readonly Sprite sprite;
    private readonly Vector2 spriteOffset = new(0.0f, 8f);
    private readonly Color trailColor = Calc.HexToColor("639bff");
    private float flingAccel;
    private Vector2 flingSpeed;
    private Vector2 flingTargetSpeed;
    public bool LightningRemoved;
    public List<Vector2[]> NodeSegments;
    private int segmentIndex;
    public List<bool> SegmentsWaiting;
    private States state;
    private readonly TimeRateModifier timeRateModifier;

    public static void Load()
    {
        // Fix recursive call
        // FlingSans.Load();
    }

    public FlingSans(Vector2[] nodes, bool skippable)
        : base(nodes[0])
    {
        Depth = -1;
        Add(sprite = GFX.SpriteBank.Create("bird"));
        sprite.Play("hover");
        sprite.Scale.X = -1f;
        sprite.Position = spriteOffset;
        sprite.OnFrameChange = spr => BirdNPC.FlapSfxCheck(sprite);
        Collider = new Circle(16f);
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

    private void OnPlayer(global::Celeste.Player player)
    {
        flingSpeed = player.Speed * 0.4f;
        flingSpeed.Y = 120f;
        flingTargetSpeed = Vector2.Zero;
        flingAccel = 1000f;
        player.Speed = Vector2.Zero;
        state = States.Fling;
        Add(new Coroutine(doFlingRoutine(player)));
        Audio.Play("event:/final_content/game/19_TheEnd/sans_throw", Center);
    }

    public FlingSans(EntityData data, Vector2 levelOffset)
        : this(data.NodesWithPosition(levelOffset), data.Bool("waiting"))
    {
        entityData = data;
    }

    public void Awake(Scene scene, object title)
    {
        base.Awake(scene);
        var all = Scene.Tracker.GetEntities<FlingSans>();
        for (var index = all.Count - 1; index >= 0; --index)
            if (((FlingSans)all[index]).entityData.Level.Name != entityData.Level.Name)
                all.RemoveAt(index);
        all.Sort((a, b) => Math.Sign(a.X - b.X));
        if (all[0] == this)
            for (var index = 1; index < all.Count; ++index)
            {
                var fling = (FlingSans)all[index];
                NodeSegments.Add(fling.NodeSegments[0]);
                SegmentsWaiting.Add(fling.SegmentsWaiting[0]);
                fling.RemoveSelf();
            }

        if (SegmentsWaiting[0])
        {
            sprite.Play("hoverStressed");
            sprite.Scale.X = 1f;
        }

        var entity = scene.Tracker.GetEntity<global::Celeste.Player>();
        if (entity == null || entity.X <= (double)X)
            return;
        RemoveSelf();
    }

    private void skip()
    {
        state = States.Move;
        Add(new Coroutine(moveRoutine()));
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
                if (entity != null && entity.X - (double)X >= 100.0)
                {
                    skip();
                    break;
                }

                if (SegmentsWaiting[segmentIndex] && LightningRemoved)
                {
                    skip();
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
                Add(new Coroutine(leaveRoutine()));
                break;
        }
    }

    private IEnumerator doFlingRoutine(global::Celeste.Player player)
    {
        var flingBird = this;
        var level = flingBird.Scene as Level;
        var screenSpaceFocusPoint = player.Position - level.Camera.Position;
        screenSpaceFocusPoint.X = Calc.Clamp(screenSpaceFocusPoint.X, 145f, 215f);
        screenSpaceFocusPoint.Y = Calc.Clamp(screenSpaceFocusPoint.Y, 85f, 95f);
        flingBird.Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.1f, 0.2f)));
        timeRateModifier.SetTimeRateMultiplier(0.8f);
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
        while (flingBird.flingSpeed != Vector2.Zero)
            yield return null;
        flingBird.sprite.Play("throw");
        flingBird.sprite.Scale.X = 1f;
        flingBird.flingSpeed = new Vector2(-140f, 140f);
        flingBird.flingTargetSpeed = Vector2.Zero;
        flingBird.flingAccel = 1400f;
        yield return 0.1f;
        // Inside the DoFlingRoutine method
        Engine.FreezeTimer = 0.05f;
        flingBird.flingTargetSpeed = FLING_SPEED;
        flingBird.flingAccel = 6000f;
        yield return 0.1f;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        timeRateModifier.ResetTimeRateMultiplier();
        level.Shake();
        flingBird.Add(new Coroutine(level.ZoomBack(0.1f)));
        player.FinishFlingBird();
        flingBird.flingTargetSpeed = Vector2.Zero;
        flingBird.flingAccel = 4000f;
        yield return 0.3f;
        flingBird.Add(new Coroutine(flingBird.moveRoutine()));
    }

    private IEnumerator moveRoutine()
    {
        var flingBird = this;
        flingBird.state = States.Move;
        flingBird.sprite.Play("fly");
        flingBird.sprite.Scale.X = 1f;
        flingBird.moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate");
        for (var nodeIndex = 1; nodeIndex < flingBird.NodeSegments[flingBird.segmentIndex].Length - 1; nodeIndex += 2)
        {
            var position = flingBird.Position;
            var anchor = flingBird.NodeSegments[flingBird.segmentIndex][nodeIndex];
            var to = flingBird.NodeSegments[flingBird.segmentIndex][nodeIndex + 1];
            yield return flingBird.MoveOnCurve(position, anchor, to);
        }

        ++flingBird.segmentIndex;
        var atEnding = flingBird.segmentIndex >= flingBird.NodeSegments.Count;
        if (!atEnding)
        {
            var position = flingBird.Position;
            var anchor =
                flingBird.NodeSegments[flingBird.segmentIndex - 1][
                    flingBird.NodeSegments[flingBird.segmentIndex - 1].Length - 1];
            var to = flingBird.NodeSegments[flingBird.segmentIndex][0];
            yield return flingBird.MoveOnCurve(position, anchor, to);
        }

        flingBird.sprite.Rotation = 0.0f;
        flingBird.sprite.Scale = Vector2.One;
        if (atEnding)
        {
            flingBird.sprite.Play("hoverStressed");
            flingBird.sprite.Scale.X = 1f;
            flingBird.state = States.WaitForLightningClear;
        }
        else
        {
            if (flingBird.SegmentsWaiting[flingBird.segmentIndex])
                flingBird.sprite.Play("hoverStressed");
            else
                flingBird.sprite.Play("hover");
            flingBird.sprite.Scale.X = -1f;
            flingBird.state = States.Wait;
        }
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator leaveRoutine()
    {
        sprite.Scale.X = 1f;
        sprite.Play("fly");
        var vector = new Vector2((Scene as Level).Bounds.Right + 32, Y);
        yield return MoveOnCurve(Position, (Position + vector) * 0.5f - Vector2.UnitY * 12f, vector);
        RemoveSelf();
    }

    private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
    {
        var flingBird = this;
        var curve = new SimpleCurve(from, to, anchor);
        var duration = curve.GetLengthParametric(32) / 500f;
        var was = from;
        for (var t = 0.016f; t <= 1.0f; t += Engine.DeltaTime / duration)
        {
            flingBird.Position = curve.GetPoint(t).Floor();
            flingBird.sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0.0f, t - 0.05f)),
                curve.GetPoint(Math.Min(1f, t + 0.05f)));
            flingBird.sprite.Scale.X = 1.25f;
            flingBird.sprite.Scale.Y = 0.7f;
            if ((was - flingBird.Position).Length() > 32.0f)
            {
                TrailManager.Add(flingBird, flingBird.trailColor, 1f);
                was = flingBird.Position;
            }

            yield return null;
        }

        flingBird.Position = to;
    }

    public override void Render()
    {
        base.Render();
    }

    private void drawLine(Vector2 a, Vector2 anchor, Vector2 b)
    {
        new SimpleCurve(a, b, anchor).Render(Color.Red, 32);
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




