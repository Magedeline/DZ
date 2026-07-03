namespace Celeste.Entities;

[Pooled]
[Tracked]
[HotReloadable]
public partial class CharaBossBiggerBeam : Entity
{
    public static ParticleType PDissipate;
    public const float CHARGE_TIME = 2.0f; // Longer charge time for bigger beam
    public const float FOLLOW_TIME = 0.5f; // Shorter follow time since it's horizontal
    public const float ACTIVE_TIME = 0.3f; // Longer active time for bigger beam
    private const float collideDZ_CHeck_sep = 4f; // Bigger collision separation
    private const float beam_length = 2000f;
    private const float beam_start_dist = 12f;
    private const int beams_drawn = 25; // More beams drawn for bigger beam
    private const float side_darkness_alpha = 0.5f; // Darker alpha for bigger beam
    
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
    private bool isLocked;
    private VertexPositionColor[] fade = new VertexPositionColor[24];

    public CharaBossBiggerBeam() : base(Vector2.Zero)
    {
        Add(beamSprite = DZModule.SpriteBank.Create("chara_beam"));
        beamSprite.OnLastFrame = anim =>
        {
            if (anim != "charaboss_shoot")
                return;
            destroy();
        };
        Add(beamStartSprite = DZModule.SpriteBank.Create("chara_beam_start"));
        beamSprite.Visible = false;
        Depth = -1000000;
    }

    public CharaBossBiggerBeam Init(CharaBoss charaboss, global::Celeste.Player target)
    {
        if (PDissipate == null)
        {
            PDissipate = new ParticleType
            {
                Color = Color.DarkRed,
                Size = 2f, // Bigger particles
                LifeMin = 1.5f, // Longer life
                LifeMax = 2f,
            };
        }

        this.charaboss = charaboss;
        chargeTimer = CHARGE_TIME;
        followTimer = FOLLOW_TIME;
        activeTimer = ACTIVE_TIME;
        beamSprite.Play("charabossDZ_CHarge");
        sideFadeAlpha = 0.0f;
        beamAlpha = 0.0f;
        isLocked = false;
        
        // Set horizontal angle based on player position relative to boss
        if (target.X >= charaboss.X)
            angle = 0f; // Fire right
        else
            angle = MathHelper.Pi; // Fire left
            
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
            
            // During follow time, adjust angle slightly towards player but keep it mostly horizontal
            if (followTimer > 0.0 && !isLocked && player.Center != charaboss.BeamOrigin)
            {
                float targetAngle = Calc.Angle(charaboss.BeamOrigin, player.Center);
                // Clamp the angle to be mostly horizontal (within 30 degrees)
                float maxAngleOffset = MathHelper.ToRadians(30f);
                
                if (player.X >= charaboss.X)
                {
                    // Player is to the right, clamp between -30 and +30 degrees
                    targetAngle = MathHelper.Clamp(targetAngle, -maxAngleOffset, maxAngleOffset);
                }
                else
                {
                    // Player is to the left, clamp between 150 and 210 degrees
                    float leftBase = MathHelper.Pi;
                    targetAngle = MathHelper.Clamp(targetAngle, leftBase - maxAngleOffset, leftBase + maxAngleOffset);
                }
                
                angle = Calc.Approach(angle, targetAngle, 2f * Engine.DeltaTime);
            }
            else if (beamSprite.CurrentAnimationID == "charabossDZ_CHarge" && followTimer <= 0.0)
            {
                beamSprite.Play("charaboss_lock");
                isLocked = true;
            }
            
            if (chargeTimer <= 0.0)
            {
                SceneAs<Level>().DirectionalShake(Calc.AngleToVector(angle, 1f), 0.25f); // Stronger shake
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long); // Stronger rumble
                dissipateParticles();
            }
        }
        else
        {
            if (activeTimer <= 0.0)
                return;
                
            sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0.0f, Engine.DeltaTime * 6f);
            if (beamSprite.CurrentAnimationID != "charaboss_shoot")
            {
                beamSprite.Play("charaboss_shoot");
                beamStartSprite.Play("charaboss_shoot", true);
            }
            
            activeTimer -= Engine.DeltaTime;
            if (activeTimer > 0.0)
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
        Vector2 min = -vector * 2f; // Bigger particle spread
        Vector2 max = vector * 2f;
        float direction1 = vector.Angle();
        float direction2 = (-vector).Angle();
        float num = Vector2.Distance(closestTo, lineA) - 12f;
        Vector2 vector22 = Calc.ClosestPointOnLine(lineA, lineB, closestTo);
        
        // More particles for bigger beam
        for (int index1 = 0; index1 < 300; index1 += 10)
        {
            for (int index2 = -2; index2 <= 2; index2 += 1)
            {
                level.ParticlesFG.Emit(PDissipate, vector22 + vector21 * index1 + vector * 4f * index2 + Calc.Random.Range(min, max), direction1);
                level.ParticlesFG.Emit(PDissipate, vector22 + vector21 * index1 - vector * 4f * index2 + Calc.Random.Range(min, max), direction2);
                if (index1 != 0 && index1 < (double) num)
                {
                    level.ParticlesFG.Emit(PDissipate, vector22 - vector21 * index1 + vector * 4f * index2 + Calc.Random.Range(min, max), direction1);
                    level.ParticlesFG.Emit(PDissipate, vector22 - vector21 * index1 - vector * 4f * index2 + Calc.Random.Range(min, max), direction2);
                }
            }
        }
    }

    private void playerCollideCheck()
    {
        Vector2 from = charaboss.BeamOrigin + Calc.AngleToVector(angle, 12f);
        Vector2 to = charaboss.BeamOrigin + Calc.AngleToVector(angle, 2000f);
        Vector2 vector2 = (to - from).Perpendicular().SafeNormalize(4f); // Bigger collision area
        
        // Check multiple collision lines for bigger beam
        global::Celeste.Player player = Scene.CollideFirst<global::Celeste.Player>(from + vector2 * 2f, to + vector2 * 2f) 
            ?? Scene.CollideFirst<global::Celeste.Player>(from + vector2, to + vector2)
            ?? Scene.CollideFirst<global::Celeste.Player>(from, to)
            ?? Scene.CollideFirst<global::Celeste.Player>(from - vector2, to - vector2)
            ?? Scene.CollideFirst<global::Celeste.Player>(from - vector2 * 2f, to - vector2 * 2f);
            
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
            
        // Render more beam sprites for bigger beam
        for (int index = 0; index < beams_drawn; ++index)
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
        Color color = Color.Black * sideFadeAlpha * side_darkness_alpha;
        Color transparent = Color.Transparent;
        Vector2 vector22 = vector2 * 4000f;
        Vector2 vector23 = vector21 * 180f; // Bigger side fade
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




