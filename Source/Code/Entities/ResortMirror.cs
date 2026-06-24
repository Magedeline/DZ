using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Entities;
[CustomEntity(ids: "MaggyHelper/ResortMirror")]
[Tracked]
public class ResortMirror : Entity
{
    private bool smashed;
    private Image bg;
    private Image frame;
    private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
    private Sprite breakingGlass;
    private VirtualRenderTarget mirror;
    private float shineAlpha = 0.7f;
    private float mirrorAlpha = 0.7f;
    private CharaDummy evil;
    private bool shardReflection;

    public ResortMirror(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Add(new BeforeRenderHook(beforeRender));
        Depth = 9500;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        smashed = SceneAs<Level>().Session.GetFlag("oshiro_resort_suite");
        Entity entity = new Entity(Position)
        {
            Depth = 9000
        };
        entity.Add(frame = new Image(GFX.Game["objects/mirror/resortframe"]));
        frame.JustifyOrigin(0.5f, 1f);
        Scene.Add(entity);
        MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
        int w = (int)frame.Width - 2;
        int h = (int)frame.Height - 12;
        if (!smashed)
            mirror = VirtualContent.CreateRenderTarget("resort-mirror", w, h);
        else
            glassbg = GFX.Game["objects/mirror/glassbreak09"];
        Add(bg = new Image(glassbg.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h)));
        bg.JustifyOrigin(0.5f, 1f);
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask");
        MTexture temp = new MTexture();
        foreach (MTexture mtexture in atlasSubtextures)
        {
            MTexture shard = mtexture;
            MirrorSurface surface = new MirrorSurface();
            surface.OnRender = () => shard.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h, temp).DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (shardReflection ? 1f : 0.0f));
            surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
            Add(surface);
        }
    }

    public void EvilAppear()
    {
        Add(new Coroutine(evilAppearRoutine()));
        Add(new Coroutine(fadeLights()));
    }

    private IEnumerator evilAppearRoutine()
    {
        evil = new CharaDummy(new Vector2(mirror.Width + 8, mirror.Height));
        yield return evil.WalkTo(mirror.Width / 2);
    }

    private IEnumerator fadeLights()
    {
        Level level = SceneAs<Level>();
        while (level.Lighting.Alpha != 0.34999999403953552)
        {
            level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, 0.35f, Engine.DeltaTime * 0.1f);
            yield return null;
        }
    }

    public IEnumerator SmashRoutine()
    {
        ResortMirror resortMirror = this;
        yield return resortMirror.evil.FloatTo(new Vector2(resortMirror.mirror.Width / 2, resortMirror.mirror.Height - 8), null, false, false);
        resortMirror.breakingGlass = GFX.SpriteBank.Create("glass");
        resortMirror.breakingGlass.Position = new Vector2(resortMirror.mirror.Width / 2, resortMirror.mirror.Height);
        resortMirror.breakingGlass.Play("break");
        resortMirror.breakingGlass.Color = Color.White * resortMirror.shineAlpha;
        Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
        while (resortMirror.breakingGlass.CurrentAnimationID == "break")
        {
            if (resortMirror.breakingGlass.CurrentAnimationFrame == 7)
                resortMirror.SceneAs<Level>().Shake();
            resortMirror.shineAlpha = Calc.Approach(resortMirror.shineAlpha, 1f, Engine.DeltaTime * 2f);
            resortMirror.mirrorAlpha = Calc.Approach(resortMirror.mirrorAlpha, 1f, Engine.DeltaTime * 2f);
            yield return null;
        }
        resortMirror.SceneAs<Level>().Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        for (float x = (float)(-(double)resortMirror.breakingGlass.Width / 2.0); x < resortMirror.breakingGlass.Width / 2.0; x += 8f)
        {
            for (float y = -resortMirror.breakingGlass.Height; y < 0.0; y += 8f)
            {
                if (Calc.Random.Chance(0.5f))
                    (resortMirror.Scene as Level).Particles.Emit(DreamMirror.P_Shatter, 2, resortMirror.Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
            }
        }
        resortMirror.shardReflection = true;
        resortMirror.evil = null;
    }

    public void Broken()
    {
        evil = null;
        smashed = true;
        shardReflection = true;
        MTexture mtexture = GFX.Game["objects/mirror/glassbreak09"];
        bg.Texture = mtexture.GetSubtexture((int)(mtexture.Width - (double)bg.Width) / 2, mtexture.Height - (int)bg.Height, (int)bg.Width, (int)bg.Height);
    }

    private void beforeRender()
    {
        if (smashed || mirror == null)
            return;
        Level level = SceneAs<Level>();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(mirror);
        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        NPCs.NPC05_Oshiro_Suite oshiroSuite = Scene.Entities.FindFirst<NPCs.NPC05_Oshiro_Suite>();
        if (oshiroSuite != null)
        {
            Vector2 renderPosition = oshiroSuite.Sprite.RenderPosition;
            oshiroSuite.Sprite.RenderPosition = renderPosition - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, -4f);
            oshiroSuite.Sprite.Render();
            oshiroSuite.Sprite.RenderPosition = renderPosition;
        }
        global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
        if (entity != null)
        {
            Vector2 position = entity.Position;
            entity.Position = position - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, 0.0f);
            Vector2 vector2 = entity.Position - position;
            for (int index = 0; index < entity.Hair.Nodes.Count; ++index)
                entity.Hair.Nodes[index] += vector2;
            entity.Render();
            for (int index = 0; index < entity.Hair.Nodes.Count; ++index)
                entity.Hair.Nodes[index] -= vector2;
            entity.Position = position;
        }
        if (evil != null)
        {
            evil.Update();
            evil.Render();
        }
        if (breakingGlass != null)
        {
            breakingGlass.Color = Color.White * shineAlpha;
            breakingGlass.Update();
            breakingGlass.Render();
        }
        else
        {
            int y = -(int)(level.Camera.Y * 0.800000011920929 % glassfg.Height);
            glassfg.DrawJustified(new Vector2(mirror.Width / 2, y), new Vector2(0.5f, 1f), Color.White * shineAlpha);
            glassfg.DrawJustified(new Vector2(mirror.Height / 2, y - glassfg.Height), new Vector2(0.5f, 1f), Color.White * shineAlpha);
        }
        Draw.SpriteBatch.End();
    }

    public override void Render()
    {
        bg.Render();
        if (!smashed)
            Draw.SpriteBatch.Draw((RenderTarget2D)mirror, Position + new Vector2(-mirror.Width / 2, -mirror.Height), Color.White * mirrorAlpha);
        frame.Render();
    }

    public override void SceneEnd(Scene scene)
    {
        dispose();
        base.SceneEnd(scene);
    }

    public override void Removed(Scene scene)
    {
        dispose();
        base.Removed(scene);
    }

    private void dispose()
    {
        if (mirror != null)
            mirror.Dispose();
        mirror = null;
    }
}




