namespace Celeste.Entities;

[Pooled]
[Tracked]
[HotReloadable]
public partial class CharaBossBiggerBeam : Entity
{
    public static ParticleType PDissipate;
    public const float CHARGE_TIME = 2.0f; // Longer charge time for bigger beam
    public const float FOLLOW_TIME = 0.5f; // Shorter follow time since it's horizontal
    public const float ACTIVE_TIME = 5.0f; // 5-second active time for the final beam
    private const float collideDZ_CHeck_sep = 4f; // Bigger collision separation
    private const float beam_length = 8000f;
    private const float beam_start_dist = 12f;
    // beams_drawn is computed at render time from beam_length / sprite width
    private const float side_darkness_alpha = 0.5f; // Darker alpha for bigger beam
    
    private CharaBoss charaboss;
    private Sprite beamSprite;
    private Sprite beamStartSprite;
    private float chargeTimer;
    private float followTimer;
    private float activeTimer;
    private float angle;
    private float beamAlpha;
    private float sideFadeAlpha;
    private bool isLocked;
    private BeamKillZone killZone;
    private VertexPositionColor[] fade = new VertexPositionColor[24];

    // ------------------------------------------------------------------
    // Kill zone: a persistent hazard entity that tracks the beam's angle
    // and kills the player on contact every frame they're vulnerable.
    // Spawned when the beam fires; removed when the beam ends.
    // ------------------------------------------------------------------
    private class BeamKillZone : Entity
    {
        // How many circles to chain along the beam, and their spacing
        // 250 circles × 32px = 8000px — matches beam_length exactly
        private const int CircleCount = 250;
        private const float CircleSpacing = 32f; // pixels between circle centres
        private const float CircleRadius = 40f;  // matches the scaled visual beam half-height

        private CharaBossBiggerBeam beam;
        private Circle[] circles;

        public BeamKillZone(CharaBossBiggerBeam beam) : base(Vector2.Zero)
        {
            this.beam = beam;
            Depth = beam.Depth; // same depth so it's always active

            // Build a chain of circles — we'll reposition them every Update
            circles = new Circle[CircleCount];
            var colliders = new Collider[CircleCount];
            for (int i = 0; i < CircleCount; i++)
            {
                circles[i] = new Circle(CircleRadius, i * CircleSpacing, 0f);
                colliders[i] = circles[i];
            }
            Collider = new ColliderList(colliders);
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(global::Celeste.Player p)
        {
            p.Die((p.Center - beam.charaboss.BeamOrigin).SafeNormalize());
        }

        public override void Update()
        {
            base.Update();
            // Anchor our origin to the beam start, then rotate all circles along the beam angle
            Vector2 origin = beam.charaboss.BeamOrigin + Calc.AngleToVector(beam.angle, beam_start_dist);
            Position = origin;

            Vector2 dir = Calc.AngleToVector(beam.angle, 1f);

            for (int i = 0; i < CircleCount; i++)
            {
                // Offset each circle along the beam direction
                Vector2 worldPos = origin + dir * (i * CircleSpacing);
                // Store as local offset (ColliderList circles are relative to entity Position)
                circles[i].Position = worldPos - Position;
            }
        }
    }

    public CharaBossBiggerBeam() : base(Vector2.Zero)
    {
        Add(beamSprite = GFX.SpriteBank.Create("chara_final_beam"));
        beamSprite.OnLastFrame = anim =>
        {
            if (anim != "charaboss_end")
                return;
            destroy();
        };
        Add(beamStartSprite = GFX.SpriteBank.Create("chara_beam_start"));
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
        beamSprite.Play("charaboss_charge");
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
        var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
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
            else if (beamSprite.CurrentAnimationID == "charaboss_charge" && followTimer <= 0.0)
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
            if (activeTimer < 0.0)
                return;
                
            sideFadeAlpha = Calc.Approach(sideFadeAlpha, 0.0f, Engine.DeltaTime * 6f);
            if (beamSprite.CurrentAnimationID == "charaboss_lock" || beamSprite.CurrentAnimationID == "charaboss_charge")
            {
                beamSprite.Play("charaboss_shoot");
                beamStartSprite.Play("charaboss_shoot", true);
                // Spawn the persistent kill zone when the beam first fires
                killZone = new BeamKillZone(this);
                Scene.Add(killZone);
            }
            
            activeTimer -= Engine.DeltaTime;
            if (activeTimer <= 0.0)
            {
                // Beam duration over — remove kill zone, play end/dissipate animation
                killZone?.RemoveSelf();
                killZone = null;
                beamSprite.Play("charaboss_end");
                beamStartSprite.Stop();
                return;
            }
        }
    }

    private void dissipateParticles()
    {
        Level level = SceneAs<Level>();
        Vector2 closestTo = level.Camera.Position + new Vector2(160f, 90f);
        Vector2 lineA = charaboss.BeamOrigin + Calc.AngleToVector(angle, 12f);
        Vector2 lineB = charaboss.BeamOrigin + Calc.AngleToVector(angle, beam_length);
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

    // Scale factor applied to the beam sprite height to make it visually wide
    private const float beam_scale = 3.5f;

    public override void Render()
    {
        Vector2 beamOrigin = charaboss.BeamOrigin;
        Vector2 vector1 = Calc.AngleToVector(angle, beamSprite.Width);
        beamSprite.Rotation = angle;
        beamSprite.Scale = new Vector2(1f, beam_scale); // widen perpendicular to firing direction
        beamSprite.Color = Color.White * beamAlpha;
        beamStartSprite.Rotation = angle;
        beamStartSprite.Scale = new Vector2(beam_scale, beam_scale); // scale start cap uniformly
        beamStartSprite.Color = Color.White * beamAlpha;
        
        bool isFiring = beamSprite.CurrentAnimationID == "charaboss_shoot" || beamSprite.CurrentAnimationID == "charaboss_end";
        if (isFiring)
            beamOrigin += Calc.AngleToVector(angle, 8f);
            
        // Render enough tiles to cover the full beam length
        int tilesNeeded = beamSprite.Width > 0 ? (int)Math.Ceiling(beam_length / beamSprite.Width) + 1 : 1;
        for (int index = 0; index < tilesNeeded; ++index)
        {
            beamSprite.RenderPosition = beamOrigin;
            beamSprite.Render();
            beamOrigin += vector1;
        }
        
        if (isFiring)
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
        // Side fade width scaled to match the visual beam thickness
        Vector2 vector23 = vector21 * (180f * beam_scale);
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

    private void destroy()
    {
        killZone?.RemoveSelf();
        killZone = null;
        RemoveSelf();
    }

    public override void Removed(Scene scene)
    {
        killZone?.RemoveSelf();
        killZone = null;
        base.Removed(scene);
    }
}




