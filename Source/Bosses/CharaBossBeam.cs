namespace Celeste.Entities;

[Pooled]
[Tracked]
[HotReloadable]
public partial class CharaBossBeam : Entity
{
    public static ParticleType PDissipate;
    public const float CHARGE_TIME = 1.4f;
    public const float FOLLOW_TIME = 0.9f;
    public const float ACTIVE_TIME = 0.12f;
    private const float angle_start_offset = 100f;
    private const float rotation_speed = 200f;
    private const float collide_check_sep = 2f;
    private const float beam_length = 2000f;
    private const float beam_start_dist = 12f;
    private const int beams_drawn = 15;
    private const float side_darkness_alpha = 0.35f;
    private CharaBoss charaboss;
    private global::Celeste.Player player;
    private Sprite beamSprite;
    private Sprite beamStartSprite;
    private float chargeTimer;
    private float followTimer;
    private float activeTimer;
    private float angle;
    private float beamAlpha;
    private float sideFadeAlpha;
    private VertexPositionColor[] fade = new VertexPositionColor[24];

    public CharaBossBeam() : base(Vector2.Zero)
    {
        Add(beamSprite = GFX.SpriteBank.Create("chara_beam"));
        beamSprite.OnLastFrame = anim =>
        {
            if (anim != "charaboss_shoot")
                return;
            destroy();
        };
        Add(beamStartSprite = GFX.SpriteBank.Create("chara_beam_start"));
        beamSprite.Visible = false;
        Depth = -1000000;
    }

    public CharaBossBeam Init(CharaBoss charaboss, global::Celeste.Player target)
    {
        if (PDissipate == null)
        {
            PDissipate = new ParticleType
            {
                Color = Color.DarkRed,
                Size = 1f,
                LifeMin = 1f,
                LifeMax = 1f,
            };
        }

        this.charaboss = charaboss; // Assign the boss field here
        chargeTimer = CHARGE_TIME;
        followTimer = FOLLOW_TIME;
        activeTimer = ACTIVE_TIME;
        beamSprite.Play("charaboss_charge");
        sideFadeAlpha = 0.0f;
        beamAlpha = 0.0f;
        int num = target.Y > charaboss.Y + 16.0f ? -1 : 1;
        if (target.X >= charaboss.X)
            num *= -1;
        angle = Calc.Angle(charaboss.BeamOrigin, target.Center);
        Vector2 to = Calc.ClosestPointOnLine(charaboss.BeamOrigin, charaboss.BeamOrigin + Calc.AngleToVector(angle, beam_length), target.Center) + (target.Center - charaboss.BeamOrigin).Perpendicular().SafeNormalize(100f) * num;
        angle = Calc.Angle(charaboss.BeamOrigin, to);
        return this;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!charaboss.Moving)
            return;
        RemoveSelf();
    }

    public override void Update()
    {
        base.Update();
        player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        beamAlpha = Calc.Approach(beamAlpha, 1f, 2f * Engine.DeltaTime);
        if (chargeTimer > 0.0)
        {
            sideFadeAlpha = Calc.Approach(sideFadeAlpha, 1f, Engine.DeltaTime);
            if (player == null || player.Dead)
                return;
            followTimer -= Engine.DeltaTime;
            chargeTimer -= Engine.DeltaTime;
            if (followTimer > 0.0 && player.Center != charaboss.BeamOrigin)
                angle = Calc.Angle(charaboss.BeamOrigin, Calc.Approach(Calc.ClosestPointOnLine(charaboss.BeamOrigin, charaboss.BeamOrigin + Calc.AngleToVector(angle, 2000f), player.Center), player.Center, 200f * Engine.DeltaTime));
            else if (beamSprite.CurrentAnimationID == "charaboss_charge")
                beamSprite.Play("charaboss_lock");
            if (chargeTimer > 0.0)
                return;
            SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.15f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            dissipateParticles();
        }
        else
        {
            if (activeTimer <= 0.0)
                return;
            sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0.0f, Engine.DeltaTime * 8f);
            if (beamSprite.CurrentAnimationID != "charaboss_shoot")
            {
                beamSprite.Play("charaboss_shoot");
                beamStartSprite.Play("charaboss_shoot", true);
            }
            activeTimer -= Engine.DeltaTime;
            if (activeTimer <= 0.0)
                return;
            playerCollideCheck();
        }
    }

    private void dissipateParticles()
    {
        Level level = SceneAs<Level>();
        Vector2 closestTo = level.Camera.Position + new Vector2(160f, 90f);
        Vector2 lineA = charaboss.BeamOrigin + Calc.AngleToVector(angle, 12f);
        Vector2 lineB = charaboss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
        Vector2 vector = (lineB - lineA).Perpendicular().SafeNormalize();
        Vector2 vector21 = (lineB - lineA).SafeNormalize();
        Vector2 min = -vector * 1f;
        Vector2 max = vector * 1f;
        float direction1 = vector.Angle();
        float direction2 = (-vector).Angle();
        float num = Vector2.Distance(closestTo, lineA) - 12f;
        Vector2 vector22 = Calc.ClosestPointOnLine(lineA, lineB, closestTo);
        for (int index1 = 0; index1 < 200; index1 += 12)
        {
            for (int index2 = -1; index2 <= 1; index2 += 2)
            {
                level.ParticlesFG.Emit(PDissipate, vector22 + vector21 * index1 + vector * 2f * index2 + Calc.Random.Range(min, max), direction1);
                level.ParticlesFG.Emit(PDissipate, vector22 + vector21 * index1 - vector * 2f * index2 + Calc.Random.Range(min, max), direction2);
                if (index1 != 0 && index1 < (double) num)
                {
                    level.ParticlesFG.Emit(PDissipate, vector22 - vector21 * index1 + vector * 2f * index2 + Calc.Random.Range(min, max), direction1);
                    level.ParticlesFG.Emit(PDissipate, vector22 - vector21 * index1 - vector * 2f * index2 + Calc.Random.Range(min, max), direction2);
                }
            }
        }
    }

    private void playerCollideCheck()
    {
        Vector2 from = charaboss.BeamOrigin + Calc.AngleToVector(angle, 12f);
        Vector2 to = charaboss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
        Vector2 vector2 = (to - from).Perpendicular().SafeNormalize(2f);
        global::Celeste.Player player = (Scene.CollideFirst<global::Celeste.Player>(from + vector2, to + vector2) ?? Scene.CollideFirst<global::Celeste.Player>(from - vector2, to - vector2)) ?? Scene.CollideFirst<global::Celeste.Player>(from, to);
        player?.Die((player.Center - charaboss.BeamOrigin).SafeNormalize());
    }

    public override void Render()
    {
        Vector2 beamOrigin = charaboss.BeamOrigin;
        Vector2 vector1 = Calc.AngleToVector(angle, beamSprite.Width);
        beamSprite.Rotation = angle;
        beamSprite.Color = Color.White * beamAlpha;
        beamStartSprite.Rotation = angle;
        beamStartSprite.Color = Color.White * beamAlpha;
        if (beamSprite.CurrentAnimationID == "shoot")
            beamOrigin += Calc.AngleToVector(angle, 8f);
        for (int index = 0; index < 15; ++index)
        {
            beamSprite.RenderPosition = beamOrigin;
            beamSprite.Render();
            beamOrigin += vector1;
        }
        if (beamSprite.CurrentAnimationID == "shoot")
        {
            beamStartSprite.RenderPosition = charaboss.BeamOrigin;
            beamStartSprite.Render();
        }
        GameplayRenderer.End();
        Vector2 vector2 = vector1.SafeNormalize();
        Vector2 vector21 = vector2.Perpendicular();
        Color color = Color.Black * sideFadeAlpha * 0.35f;
        Color transparent = Color.Transparent;
        Vector2 vector22 = vector2 * 4000f;
        Vector2 vector23 = vector21 * 120f;
        int v = 0;
        quad(ref v, beamOrigin, -vector22 + vector23 * 2f, vector22 + vector23 * 2f, vector22 + vector23, -vector22 + vector23, color, color);
        quad(ref v, beamOrigin, -vector22 + vector23, vector22 + vector23, vector22, -vector22, color, transparent);
        quad(ref v, beamOrigin, -vector22, vector22, vector22 - vector23, -vector22 - vector23, transparent, color);
        quad(ref v, beamOrigin, -vector22 - vector23, vector22 - vector23, vector22 - vector23 * 2f, -vector22 - vector23 * 2f, color, color);
        GFX.DrawVertices((Scene as Level).Camera.Matrix, fade, fade.Length);
        GameplayRenderer.Begin();
    }

    private void quad(
        ref int v,
        Vector2 offset,
        Vector2 a,
        Vector2 b,
        Vector2 c,
        Vector2 d,
        Color ab,
        Color cd)
    {
        fade[v].Position.X = offset.X + a.X;
        fade[v].Position.Y = offset.Y + a.Y;
        fade[v++].Color = ab;
        fade[v].Position.X = offset.X + b.X;
        fade[v].Position.Y = offset.Y + b.Y;
        fade[v++].Color = ab;
        fade[v].Position.X = offset.X + c.X;
        fade[v].Position.Y = offset.Y + c.Y;
        fade[v++].Color = cd;
        fade[v].Position.X = offset.X + a.X;
        fade[v].Position.Y = offset.Y + a.Y;
        fade[v++].Color = ab;
        fade[v].Position.X = offset.X + c.X;
        fade[v].Position.Y = offset.Y + c.Y;
        fade[v++].Color = cd;
        fade[v].Position.X = offset.X + d.X;
        fade[v].Position.Y = offset.Y + d.Y;
        fade[v++].Color = cd;
    }

    private void destroy() => RemoveSelf();
}




