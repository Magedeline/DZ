using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities;

/// <summary>
/// A ZipMover-style platform that zooms to a target position when the player stands on it.
/// Supports multiple visual themes: default, moon, foundlevels, and finallevels.
/// </summary>
[CustomEntity(ids: "DZ/ZoomMover")]
[Tracked]
public class ZoomMover : Solid
{
    public enum Themes
    {
        Normal,
        Moon,
        FoundLevels,
        FinalLevels
    }

    // Constants
    private const float ReturnTime = 0.8f;
    
    // Movement properties
    private readonly Themes theme;
    private readonly Vector2 start;
    private readonly Vector2 target;
    private readonly float moveSpeed;
    private readonly bool permanent;
    private readonly bool waits;
    private readonly bool timed;
    
    // State
    private bool activated;
    private float percent;
    private bool returning;
    
    // Visual components
    private MTexture[,] edges = new MTexture[3, 3];
    private Sprite streetlight;
    private BloomPoint bloom;
    private ZoomMoverPathRenderer pathRenderer;
    private SoundSource sfx;
    
    // Inner cog animation
    private List<MTexture> innerCogs;
    private float cogRotation;

    public ZoomMover(EntityData data, Vector2 offset) 
        : this(data.Position + offset, data.Width, data.Height, 
               data.Nodes.Length > 0 ? data.Nodes[0] + offset : data.Position + offset + new Vector2(0, -100),
               Enum.TryParse(data.Attr("theme", "Normal"), true, out Themes t) ? t : Themes.Normal,
               data.Float("moveSpeed", 300f),
               data.Bool("permanent", false),
               data.Bool("waits", false),
               data.Bool("timed", false))
    {
    }

    public ZoomMover(Vector2 position, int width, int height, Vector2 target, Themes theme, 
                     float moveSpeed, bool permanent, bool waits, bool timed)
        : base(position, width, height, safe: false)
    {
        Depth = -9999;
        
        this.start = position;
        this.target = target;
        this.theme = theme;
        this.moveSpeed = moveSpeed;
        this.permanent = permanent;
        this.waits = waits;
        this.timed = timed;
        
        activated = false;
        percent = 0f;
        returning = false;

        Add(new Coroutine(Sequence()));
        Add(new LightOcclude());
        
        // Setup textures based on theme
        string themePath = GetThemePath();
        
        // Load block edge textures
        MTexture blockTexture = AtlasPathHelper.TryGetTexture($"objects/DZ/DZ/zoommover/{themePath}block");
        if (blockTexture == null)
        {
            blockTexture = GFX.Game["objects/DZ/DZ/zipmover/block"];
        }
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                edges[i, j] = blockTexture.GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }
        
        // Load inner cog animation
        innerCogs = AtlasPathHelper.GetAtlasSubtextures($"objects/DZ/DZ/zoommover/{themePath}innercog");
        if (innerCogs == null || innerCogs.Count == 0)
        {
            innerCogs = GFX.Game.GetAtlasSubtextures("objects/DZ/DZ/zipmover/innercog");
        }
        
        // Setup streetlight indicator
        string lightPath = AtlasPathHelper.ResolveAtlasPath($"objects/DZ/DZ/zoommover/{themePath}light");
        if (GFX.Game.Has(lightPath + "00"))
        {
            streetlight = new Sprite(GFX.Game, lightPath);
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(0);
            streetlight.Position = new Vector2(Width / 2f - streetlight.Width / 2f, 0f);
            Add(streetlight);
        }
        
        // Bloom for light
        Add(bloom = new BloomPoint(1f, 6f));
        bloom.Visible = false;
        
        // Sound effects
        sfx = new SoundSource();
        sfx.Position = new Vector2(Width, Height) / 2f;
        Add(sfx);
    }

    private string GetThemePath()
    {
        return theme switch
        {
            Themes.Moon => "moon/",
            Themes.FoundLevels => "foundlevels/",
            Themes.FinalLevels => "finallevels/",
            _ => ""
        };
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(pathRenderer = new ZoomMoverPathRenderer(this));
    }

    public override void Removed(Scene scene)
    {
        scene.Remove(pathRenderer);
        pathRenderer = null;
        base.Removed(scene);
    }

    public override void Update()
    {
        base.Update();
        
        // Animate bloom position
        bloom.Position = streetlight != null 
            ? streetlight.Position + new Vector2(streetlight.Width / 2f, streetlight.Height / 2f)
            : new Vector2(Width / 2f, 4f);
        
        // Rotate cogs based on movement
        if (activated && !returning)
        {
            cogRotation += Engine.DeltaTime * 10f;
        }
        else if (returning)
        {
            cogRotation -= Engine.DeltaTime * 6f;
        }
    }

    public override void Render()
    {
        Vector2 position = Position;
        Position += Shake;
        
        // Draw block faces
        DrawBlock(position);
        
        base.Render();
        Position = position;
    }

    private void DrawBlock(Vector2 offset)
    {
        int tilesX = (int)(Width / 8f);
        int tilesY = (int)(Height / 8f);
        
        // Draw inner cogs
        if (innerCogs != null && innerCogs.Count > 0)
        {
            int cogFrame = (int)(cogRotation % innerCogs.Count);
            if (cogFrame < 0) cogFrame += innerCogs.Count;
            
            MTexture cog = innerCogs[cogFrame];
            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    Vector2 drawPos = Position + new Vector2(x * 8 + 4, y * 8 + 4);
                    cog.DrawCentered(drawPos, Color.White, 1f, cogRotation * 0.1f);
                }
            }
        }
        
        // Draw 9-slice block edges
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                int edgeX = (x == 0) ? 0 : (x == tilesX - 1) ? 2 : 1;
                int edgeY = (y == 0) ? 0 : (y == tilesY - 1) ? 2 : 1;
                
                edges[edgeX, edgeY].Draw(Position + new Vector2(x * 8, y * 8));
            }
        }
    }

    private void FinishPlayerMovement()
    {
        CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();
        if (player != null)
        {
            if (player.StateMachine.State == CelestePlayer.StDummy)
            {
                player.StateMachine.State = CelestePlayer.StNormal;
            }
        }
    }
    
    private void ScrapeParticlesCheck(Vector2 to)
    {
        if (!Scene.OnInterval(0.03f))
            return;
            
        bool movedV = to.Y != ExactPosition.Y;
        bool movedH = to.X != ExactPosition.X;
        
        if (movedV)
        {
            int direction = Math.Sign(to.Y - ExactPosition.Y);
            Vector2 particlePos = (direction != 1) ? TopLeft : BottomLeft;
            
            int particleCount = (int)(Width / 8f);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 at = particlePos + new Vector2(i * 8 + 4, 0f);
                if (Scene.CollideCheck<Solid>(at))
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, at);
                }
            }
        }
        
        if (movedH)
        {
            int direction = Math.Sign(to.X - ExactPosition.X);
            Vector2 particlePos = (direction != 1) ? TopLeft : TopRight;
            
            int particleCount = (int)(Height / 8f);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 at = particlePos + new Vector2(0f, i * 8 + 4);
                if (Scene.CollideCheck<Solid>(at))
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, at);
                }
            }
        }
    }

    private IEnumerator Sequence()
    {
        // Initial waiting state
        while (!activated)
        {
            // Wait for player to stand on the platform
            if (HasPlayerOnTop())
            {
                // Blink the streetlight
                sfx.Play("event:/game/01_forsaken_city/zip_mover");
                
                if (streetlight != null)
                {
                    streetlight.SetAnimationFrame(3);
                }
                bloom.Visible = true;
                
                // Small activation delay
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                yield return 0.1f;
                
                activated = true;
            }
            yield return null;
        }
        
        // Shaking before moving
        StartShaking(0.2f);
        yield return 0.2f;
        FinishPlayerMovement();
        
        // Move to target
        float at = 0f;
        while (at < 1f)
        {
            yield return null;
            at = Calc.Approach(at, 1f, moveSpeed / Vector2.Distance(start, target) * Engine.DeltaTime);
            percent = Ease.SineIn(at);
            
            Vector2 to = Vector2.Lerp(start, target, percent);
            ScrapeParticlesCheck(to);
            
            if (Scene.OnInterval(0.1f))
            {
                pathRenderer.CreateSparks();
            }
            
            MoveTo(to);
        }
        
        // Arrived at target
        StartShaking(0.2f);
        FinishPlayerMovement();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        SceneAs<Level>().Shake();
        
        if (streetlight != null)
        {
            streetlight.SetAnimationFrame(2);
        }
        
        yield return 0.5f;
        FinishPlayerMovement();
        
        if (!permanent)
        {
            // Wait logic
            if (waits)
            {
                // Wait until player leaves
                while (HasPlayerOnTop())
                {
                    yield return null;
                }
            }
            
            if (timed)
            {
                yield return 0.5f;
            }
            
            // Return to start
            returning = true;
            
            if (streetlight != null)
            {
                streetlight.SetAnimationFrame(1);
            }
            
            sfx.Play("event:/game/01_forsaken_city/zip_mover");
            
            at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 1f / ReturnTime * Engine.DeltaTime);
                percent = 1f - Ease.SineIn(at);
                
                Vector2 to = Vector2.Lerp(target, start, Ease.SineIn(at));
                MoveTo(to);
            }
            
            returning = false;
            
            // Back at start
            StartShaking(0.2f);
            
            if (streetlight != null)
            {
                streetlight.SetAnimationFrame(0);
            }
            bloom.Visible = false;
            
            activated = false;
            
            // Reset for next activation
            yield return 0.5f;
            
            // Recursive call to allow reactivation
            Add(new Coroutine(Sequence()));
        }
    }

    /// <summary>
    /// Renders the path/rope between start and end positions
    /// </summary>
    private class ZoomMoverPathRenderer : Entity
    {
        private readonly ZoomMover mover;
        private MTexture cog;
        
        public ZoomMoverPathRenderer(ZoomMover mover)
        {
            Depth = 5000;
            this.mover = mover;
            
            string themePath = mover.GetThemePath();
            cog = AtlasPathHelper.TryGetTexture($"objects/DZ/DZ/zoommover/{themePath}cog");
            if (cog == null)
            {
                cog = GFX.Game["objects/DZ/DZ/zipmover/cog"];
            }
        }

        public void CreateSparks()
        {
            // Create spark particles along the path
            Vector2 from = mover.start + new Vector2(mover.Width / 2f, mover.Height / 2f);
            Vector2 to = mover.target + new Vector2(mover.Width / 2f, mover.Height / 2f);
            
            for (int i = 0; i < 2; i++)
            {
                float lerp = Calc.Random.NextFloat();
                Vector2 pos = Vector2.Lerp(from, to, lerp);
                SceneAs<Level>()?.ParticlesBG.Emit(ZipMover.P_Sparks, pos);
            }
        }

        public override void Render()
        {
            DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
        }

        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            Vector2 from = mover.start + new Vector2(mover.Width / 2f, mover.Height / 2f);
            Vector2 to = mover.target + new Vector2(mover.Width / 2f, mover.Height / 2f);
            Vector2 blockCenter = mover.Position + new Vector2(mover.Width / 2f, mover.Height / 2f);
            
            Color ropeColor = colorOverride ?? Calc.HexToColor("663931");
            Color cogColor = colorOverride ?? Color.White;
            
            // Draw rope/path line
            float dist = Vector2.Distance(from, to);
            Vector2 dir = (to - from).SafeNormalize();
            Vector2 perpendicular = dir.Perpendicular();
            
            // Draw the connecting line (rope)
            for (float d = 4f; d < dist - 4f; d += 4f)
            {
                Vector2 pos = from + dir * d + offset;
                Draw.Line(pos - perpendicular, pos + perpendicular, ropeColor);
            }
            
            // Draw cogs at both ends
            float rotation = mover.cogRotation * 2f;
            cog.DrawCentered(from + offset, cogColor, 1f, rotation);
            cog.DrawCentered(to + offset, cogColor, 1f, -rotation);
            
            // Draw current position cog
            cog.DrawCentered(blockCenter + offset, cogColor, 1f, rotation);
        }
    }
}

