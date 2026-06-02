using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Visual effect entity for the 4th wall barrier breaking sequence.
/// Displays animated barrier cracking, shattering, and crumbling effects
/// synchronized with the break_fourth_wall_part1/2/3 audio events.
/// </summary>
[Tracked]
public class BarrierBreakEffect : Entity
{
    #region Constants
    private const string BARRIER_GRAPHICS_PATH = "cutscenes/barrierbreak/";
    private const float CRACK_DURATION = 0.8f;
    private const float SHATTER_DURATION = 2.5f;
    private const float CRUMBLE_DURATION = 3f;
    #endregion

    #region Fields
    private Level level;
    
    // Barrier textures
    private MTexture barrierTexture;
    private MTexture crackTexture1;
    private MTexture crackTexture2;
    private MTexture crackTexture3;
    private List<MTexture> shatterFrames;
    private List<MTexture> crumbleFrames;
    
    // Animation state
    private BarrierPhase currentPhase = BarrierPhase.Idle;
    private int crackCount = 0;
    private float phaseTimer = 0f;
    private float alpha = 1f;
    private float shakeAmount = 0f;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float frameRate = 0.08f;
    
    // Visual effects
    private List<BarrierShard> shards = new List<BarrierShard>();
    private List<CrackLine> cracks = new List<CrackLine>();
    private Color barrierColor = Color.White;
    private Color glowColor = Color.Gold;
    private float glowIntensity = 0f;
    private Vector2 offset = Vector2.Zero;
    
    // Particles
    private ParticleType shardParticle;
    private ParticleType glowParticle;
    #endregion

    #region Enums
    public enum BarrierPhase
    {
        Idle,
        Cracking,      // Part 1 - Three hits
        Shattering,    // Part 2 - Wall opens and crumbles sideways
        Destroyed      // Part 3 - Complete destruction
    }
    #endregion

    public BarrierBreakEffect(Vector2 position) : base(position)
    {
        base.Depth = -1000001; // Render in front of most things
        base.Tag = Tags.TransitionUpdate | Tags.FrozenUpdate;
        
        InitializeParticles();
    }

    private void InitializeParticles()
    {
        shardParticle = new ParticleType
        {
            Color = Color.White,
            Color2 = Color.LightGray,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 1f,
            LifeMax = 3f,
            Size = 1f,
            SizeRange = 0.5f,
            SpeedMin = 50f,
            SpeedMax = 150f,
            Direction = MathHelper.PiOver2,
            DirectionRange = MathHelper.Pi,
            RotationMode = ParticleType.RotationModes.Random,
            SpinMin = 2f,
            SpinMax = 6f
        };
        
        glowParticle = new ParticleType
        {
            Color = Color.Gold,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Linear,
            LifeMin = 0.5f,
            LifeMax = 1.5f,
            Size = 2f,
            SpeedMin = 20f,
            SpeedMax = 60f,
            Direction = -MathHelper.PiOver2,
            DirectionRange = MathHelper.PiOver4
        };
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
        LoadTextures();
    }

    private void LoadTextures()
    {
        try
        {
            // Load base barrier texture
            if (GFX.Game.Has(BARRIER_GRAPHICS_PATH + "barrier"))
                barrierTexture = GFX.Game[BARRIER_GRAPHICS_PATH + "barrier"];
            
            // Load crack textures for each hit
            if (GFX.Game.Has(BARRIER_GRAPHICS_PATH + "crack1"))
                crackTexture1 = GFX.Game[BARRIER_GRAPHICS_PATH + "crack1"];
            if (GFX.Game.Has(BARRIER_GRAPHICS_PATH + "crack2"))
                crackTexture2 = GFX.Game[BARRIER_GRAPHICS_PATH + "crack2"];
            if (GFX.Game.Has(BARRIER_GRAPHICS_PATH + "crack3"))
                crackTexture3 = GFX.Game[BARRIER_GRAPHICS_PATH + "crack3"];
            
            // Load shatter animation frames
            shatterFrames = GFX.Game.GetAtlasSubtextures(BARRIER_GRAPHICS_PATH + "shatter");
            
            // Load crumble animation frames
            crumbleFrames = GFX.Game.GetAtlasSubtextures(BARRIER_GRAPHICS_PATH + "crumble");
        }
        catch (Exception)
        {
            // Textures not found - will use procedural rendering
        }
    }

    public override void Update()
    {
        base.Update();
        
        phaseTimer += Engine.DeltaTime;
        
        // Update shake offset
        if (shakeAmount > 0f)
        {
            offset = new Vector2(
                Calc.Random.Range(-shakeAmount, shakeAmount),
                Calc.Random.Range(-shakeAmount, shakeAmount)
            );
            shakeAmount = Calc.Approach(shakeAmount, 0f, Engine.DeltaTime * 10f);
        }
        else
        {
            offset = Vector2.Zero;
        }
        
        // Update glow
        if (glowIntensity > 0f)
        {
            glowIntensity = Calc.Approach(glowIntensity, 0f, Engine.DeltaTime);
        }
        
        // Update animation frames
        if (currentPhase == BarrierPhase.Shattering && shatterFrames != null && shatterFrames.Count > 0)
        {
            frameTimer += Engine.DeltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer = 0f;
                currentFrame++;
                if (currentFrame >= shatterFrames.Count)
                {
                    currentFrame = shatterFrames.Count - 1;
                }
            }
        }
        
        // Update shards
        for (int i = shards.Count - 1; i >= 0; i--)
        {
            shards[i].Update();
            if (shards[i].IsDead)
            {
                shards.RemoveAt(i);
            }
        }
        
        // Update cracks
        foreach (var crack in cracks)
        {
            crack.Update();
        }
        
        // Emit particles during active phases
        if (currentPhase == BarrierPhase.Shattering && level != null)
        {
            if (Calc.Random.Chance(0.3f))
            {
                level.ParticlesFG.Emit(shardParticle, Position + offset + new Vector2(Calc.Random.Range(-100f, 100f), Calc.Random.Range(-50f, 50f)));
            }
        }
    }

    #region Public Methods for Triggering Effects

    /// <summary>
    /// Play the crack effect (Part 1) - call this 3 times
    /// </summary>
    public IEnumerator PlayCrackEffect()
    {
        crackCount++;
        currentPhase = BarrierPhase.Cracking;
        phaseTimer = 0f;
        
        // Screen effects
        shakeAmount = 5f + crackCount * 3f;
        glowIntensity = 0.5f + crackCount * 0.15f;
        
        // Add procedural crack
        AddCrack(crackCount);
        
        // Flash effect
        if (level != null)
        {
            level.Flash(Color.White * (0.3f + crackCount * 0.1f), false);
            level.Displacement.AddBurst(Position, 0.5f, 0f, 100f + crackCount * 50f, 0.5f);
        }
        
        // Emit burst of particles
        for (int i = 0; i < 10 + crackCount * 5; i++)
        {
            if (level != null)
            {
                level.ParticlesFG.Emit(glowParticle, Position + new Vector2(Calc.Random.Range(-30f, 30f), Calc.Random.Range(-20f, 20f)));
            }
        }
        
        yield return CRACK_DURATION;
    }

    /// <summary>
    /// Play the shatter effect (Part 2) - wall opens and crumbles sideways
    /// </summary>
    public IEnumerator PlayShatterEffect()
    {
        currentPhase = BarrierPhase.Shattering;
        phaseTimer = 0f;
        currentFrame = 0;
        
        // Big screen effects
        shakeAmount = 15f;
        glowIntensity = 1f;
        
        if (level != null)
        {
            level.Flash(Color.White, true);
            level.Displacement.AddBurst(Position, 2f, 0f, 400f, 2f);
        }
        
        // Create shards flying sideways
        CreateShards(50);
        
        // Animate crumbling
        for (float t = 0f; t < SHATTER_DURATION; t += Engine.DeltaTime)
        {
            // Continuous particle emission
            if (level != null && Calc.Random.Chance(0.5f))
            {
                level.ParticlesFG.Emit(shardParticle, Position + new Vector2(Calc.Random.Range(-150f, 150f), Calc.Random.Range(-80f, 80f)));
            }
            
            // Fade the barrier
            alpha = 1f - Ease.CubeIn(t / SHATTER_DURATION);
            
            yield return null;
        }
        
        alpha = 0f;
        yield return 0.5f;
    }

    /// <summary>
    /// Play the final destruction effect (Part 3)
    /// </summary>
    public IEnumerator PlayDestroyedEffect()
    {
        currentPhase = BarrierPhase.Destroyed;
        phaseTimer = 0f;
        
        // Final flash
        if (level != null)
        {
            level.Flash(Color.White * 0.5f, false);
        }
        
        glowIntensity = 0.8f;
        shakeAmount = 5f;
        
        // Final burst of particles rising upward (souls released)
        for (int i = 0; i < 30; i++)
        {
            if (level != null)
            {
                level.ParticlesFG.Emit(glowParticle, Position + new Vector2(Calc.Random.Range(-100f, 100f), Calc.Random.Range(-50f, 50f)), Color.Lerp(Color.Gold, Color.White, Calc.Random.NextFloat()));
            }
        }
        
        yield return CRUMBLE_DURATION;
    }

    /// <summary>
    /// Complete sequence that plays all three parts
    /// </summary>
    public IEnumerator PlayFullSequence()
    {
        // Part 1: Three cracks
        for (int i = 0; i < 3; i++)
        {
            Audio.Play("event:/pusheen/extra_content/music/lvl20/cinematic/break_fourth_wall_part1");
            yield return PlayCrackEffect();
        }
        
        yield return 0.5f;
        
        // Part 2: Shatter
        Audio.Play("event:/pusheen/extra_content/music/lvl20/cinematic/break_fourth_wall_part2");
        yield return PlayShatterEffect();
        
        // Part 3: Destroyed
        Audio.Play("event:/pusheen/extra_content/music/lvl20/cinematic/break_fourth_wall_part3");
        yield return PlayDestroyedEffect();
    }

    #endregion

    #region Helper Methods

    private void AddCrack(int crackNumber)
    {
        // Create procedural crack lines
        Vector2 startPoint = Position + new Vector2(Calc.Random.Range(-50f, 50f), Calc.Random.Range(-30f, 30f));
        
        for (int i = 0; i < 3 + crackNumber * 2; i++)
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            float length = Calc.Random.Range(30f, 80f + crackNumber * 20f);
            
            cracks.Add(new CrackLine
            {
                Start = startPoint,
                End = startPoint + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length,
                Alpha = 1f,
                Width = Calc.Random.Range(1f, 3f)
            });
        }
    }

    private void CreateShards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Calc.Random.Range(-MathHelper.PiOver4, MathHelper.PiOver4);
            // Shards fly left or right (sideways crumble)
            if (Calc.Random.Chance(0.5f))
            {
                angle += MathHelper.Pi;
            }
            
            shards.Add(new BarrierShard
            {
                Position = Position + new Vector2(Calc.Random.Range(-80f, 80f), Calc.Random.Range(-60f, 60f)),
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Calc.Random.Range(100f, 300f),
                Rotation = Calc.Random.NextFloat() * MathHelper.TwoPi,
                RotationSpeed = Calc.Random.Range(-10f, 10f),
                Size = Calc.Random.Range(4f, 16f),
                Alpha = 1f,
                Color = Color.Lerp(Color.White, Color.LightGray, Calc.Random.NextFloat())
            });
        }
    }

    #endregion

    #region Rendering

    public override void Render()
    {
        base.Render();
        
        Vector2 renderPos = Position + offset;
        
        // Draw glow effect
        if (glowIntensity > 0f)
        {
            Draw.Circle(renderPos, 100f + glowIntensity * 50f, glowColor * glowIntensity * 0.3f, 64);
            Draw.Circle(renderPos, 60f + glowIntensity * 30f, glowColor * glowIntensity * 0.5f, 32);
        }
        
        // Draw barrier (if texture exists and not fully destroyed)
        if (alpha > 0f)
        {
            if (barrierTexture != null)
            {
                barrierTexture.DrawCentered(renderPos, barrierColor * alpha);
            }
            else
            {
                // Procedural barrier rendering
                DrawProceduralBarrier(renderPos);
            }
            
            // Draw crack textures based on crack count
            if (crackCount >= 1 && crackTexture1 != null)
                crackTexture1.DrawCentered(renderPos, Color.White * alpha);
            if (crackCount >= 2 && crackTexture2 != null)
                crackTexture2.DrawCentered(renderPos, Color.White * alpha);
            if (crackCount >= 3 && crackTexture3 != null)
                crackTexture3.DrawCentered(renderPos, Color.White * alpha);
        }
        
        // Draw shatter animation
        if (currentPhase == BarrierPhase.Shattering && shatterFrames != null && shatterFrames.Count > 0 && currentFrame < shatterFrames.Count)
        {
            shatterFrames[currentFrame].DrawCentered(renderPos, Color.White * alpha);
        }
        
        // Draw procedural cracks
        foreach (var crack in cracks)
        {
            if (crack.Alpha > 0f)
            {
                Draw.Line(crack.Start + offset, crack.End + offset, Color.Black * crack.Alpha * alpha, crack.Width);
                Draw.Line(crack.Start + offset + Vector2.One, crack.End + offset + Vector2.One, Color.DarkGray * crack.Alpha * 0.5f * alpha, crack.Width * 0.5f);
            }
        }
        
        // Draw shards
        foreach (var shard in shards)
        {
            shard.Render();
        }
    }

    private void DrawProceduralBarrier(Vector2 pos)
    {
        // Draw a glowing barrier rectangle
        float width = 200f;
        float height = 120f;
        
        // Outer glow
        Draw.Rect(pos.X - width / 2 - 4f, pos.Y - height / 2 - 4f, width + 8f, height + 8f, Color.Purple * 0.3f * alpha);
        
        // Main barrier
        Draw.Rect(pos.X - width / 2, pos.Y - height / 2, width, height, Color.Lerp(Color.DarkViolet, Color.Black, 0.5f) * alpha);
        
        // Inner glow
        Draw.HollowRect(pos.X - width / 2 + 2f, pos.Y - height / 2 + 2f, width - 4f, height - 4f, Color.MediumPurple * alpha);
        
        // Energy lines
        for (int i = 0; i < 5; i++)
        {
            float y = pos.Y - height / 2 + (height / 5f) * i + phaseTimer * 20f % (height / 5f);
            Draw.Line(pos.X - width / 2, y, pos.X + width / 2, y, Color.Purple * 0.5f * alpha);
        }
    }

    #endregion

    #region Inner Classes

    private class BarrierShard
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float RotationSpeed;
        public float Size;
        public float Alpha;
        public Color Color;
        public float Gravity = 200f;
        public float LifeTime = 0f;
        public float MaxLife = 3f;
        
        public bool IsDead => LifeTime >= MaxLife || Alpha <= 0f;

        public void Update()
        {
            LifeTime += Engine.DeltaTime;
            
            // Apply gravity
            Velocity.Y += Gravity * Engine.DeltaTime;
            
            // Move
            Position += Velocity * Engine.DeltaTime;
            Rotation += RotationSpeed * Engine.DeltaTime;
            
            // Fade out
            if (LifeTime > MaxLife * 0.7f)
            {
                Alpha = Calc.Approach(Alpha, 0f, Engine.DeltaTime * 2f);
            }
        }

        public void Render()
        {
            if (Alpha <= 0f) return;
            
            // Draw rotated rectangle shard
            Vector2[] points = new Vector2[4];
            float halfSize = Size / 2f;
            
            for (int i = 0; i < 4; i++)
            {
                float angle = Rotation + MathHelper.PiOver4 + MathHelper.PiOver2 * i;
                points[i] = Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * halfSize;
            }
            
            // Simple quad rendering
            Draw.Line(points[0], points[1], Color * Alpha, 2f);
            Draw.Line(points[1], points[2], Color * Alpha, 2f);
            Draw.Line(points[2], points[3], Color * Alpha, 2f);
            Draw.Line(points[3], points[0], Color * Alpha, 2f);
        }
    }

    private class CrackLine
    {
        public Vector2 Start;
        public Vector2 End;
        public float Alpha;
        public float Width;

        public void Update()
        {
            // Cracks persist until barrier is destroyed
        }
    }

    #endregion
}