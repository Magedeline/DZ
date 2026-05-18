// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.VanishingWall
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/VanishingWall"})]
[Tracked(false)]
internal class VanishingWall : Solid
{
  private char fillTile;
  private TileGrid tiles;
  private bool fade;
  private EffectCutout cutout;
  private float transitionStartAlpha;
  private bool transitionFade;
  private EntityID eid;

  public VanishingWall(EntityID eid, Vector2 position, char tile, float width, float height)
    : base(position, width, height, true)
  {
    this.eid = eid;
    this.fillTile = tile;
    ((Entity) this).Collider = (Collider) new Hitbox(width, height, 0.0f, 0.0f);
    ((Entity) this).Depth = -13000;
    ((Entity) this).Add((Component) (this.cutout = new EffectCutout()));
  }

  public VanishingWall(EntityData data, Vector2 offset, EntityID eid)
    : this(eid, ((data.Position) + (offset)), data.Char("tiletype", '3'), (float) data.Width, (float) data.Height)
  {
  }

  public override void Added(Scene scene)
  {
    ((Entity) this).Added(scene);
    int num1 = (int) ((Entity) this).Width / 8;
    int num2 = (int) ((Entity) this).Height / 8;
    Level level = ((Entity) this).SceneAs<Level>();
    Rectangle tileBounds = level.Session.MapData.TileBounds;
    VirtualMap<char> solidsData = level.SolidsData;
    int num3 = (int) ((Entity) this).X / 8 - tileBounds.Left;
    int num4 = (int) ((Entity) this).Y / 8 - tileBounds.Top;
    this.tiles = GFX.FGAutotiler.GenerateOverlay(this.fillTile, num3, num4, num1, num2, solidsData).TileGrid;
    ((Entity) this).Add((Component) this.tiles);
    ((Entity) this).Add((Component) new TileInterceptor(this.tiles, false));
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    if (((Entity) this).CollideCheck<Player>())
    {
      this.tiles.Alpha = 0.0f;
      this.fade = true;
      ((Component) this.cutout).Visible = false;
      Audio.Play("event:/game/general/secret_revealed", ((Entity) this).Center);
      ((Entity) this).SceneAs<Level>().Session.DoNotLoad.Add(this.eid);
    }
    else
      ((Entity) this).Add((Component) new TransitionListener()
      {
        OnOut = new Action<float>(this.OnTransitionOut),
        OnOutBegin = new Action(this.OnTransitionOutBegin),
        OnIn = new Action<float>(this.OnTransitionIn),
        OnInBegin = new Action(this.OnTransitionInBegin)
      });
  }

  private void OnTransitionOutBegin()
  {
    if (Collide.CheckRect((Entity) this, ((Entity) this).SceneAs<Level>().Bounds))
    {
      this.transitionFade = true;
      this.transitionStartAlpha = this.tiles.Alpha;
    }
    else
      this.transitionFade = false;
  }

  private void OnTransitionOut(float percent)
  {
    if (!this.transitionFade)
      return;
    this.tiles.Alpha = this.transitionStartAlpha * (1f - percent);
  }

  private void OnTransitionInBegin()
  {
    Level level = ((Entity) this).SceneAs<Level>();
    if (level.PreviousBounds.HasValue && Collide.CheckRect((Entity) this, level.PreviousBounds.Value))
    {
      this.transitionFade = true;
      this.tiles.Alpha = 0.0f;
    }
    else
      this.transitionFade = false;
  }

  private void OnTransitionIn(float percent)
  {
    if (!this.transitionFade)
      return;
    this.tiles.Alpha = percent;
  }

  public override void Update()
  {
    base.Update();
    if (this.fade)
    {
      this.tiles.Alpha = Calc.Approach(this.tiles.Alpha, 0.0f, 2f * Engine.DeltaTime);
      this.cutout.Alpha = this.tiles.Alpha;
      if ((double) this.tiles.Alpha > 0.0)
        return;
      ((Entity) this).RemoveSelf();
    }
    else
    {
      ((Entity) this).Collidable = true;
      if (((Entity) this).CollideCheck<Player>())
        ((Entity) this).Collidable = false;
      if (((Entity) this).Collidable)
        return;
      ((Entity) this).Active = false;
    }
  }

  public void RemoveBlock()
  {
    Player entity = ((Entity) this).Scene.Tracker.GetEntity<Player>();
    if (entity == null || entity.StateMachine.State == 9)
      return;
    ((Entity) this).SceneAs<Level>().Session.DoNotLoad.Add(this.eid);
    this.fade = true;
    Audio.Play("event:/game/general/secret_revealed", ((Entity) this).Center);
  }

  public override void Render()
  {
    Level scene = ((Entity) this).Scene as Level;
    int num1;
    if ((double) scene.ShakeVector.X < 0.0)
    {
      double x1 = (double) scene.Camera.X;
      Rectangle bounds1 = scene.Bounds;
      double left1 = (double) bounds1.Left;
      if (x1 <= left1)
      {
        double x2 = (double) ((Entity) this).X;
        Rectangle bounds2 = scene.Bounds;
        double left2 = (double) bounds2.Left;
        num1 = x2 <= left2 ? 1 : 0;
        goto label_4;
      }
    }
    num1 = 0;
label_4:
    if (num1 != 0)
      this.tiles.RenderAt(((((Entity) this).Position) + (new Vector2(-3f, 0.0f))));
    int num2;
    if ((double) scene.ShakeVector.X > 0.0)
    {
      double num3 = (double) scene.Camera.X + 320.0;
      Rectangle bounds3 = scene.Bounds;
      double right1 = (double) bounds3.Right;
      if (num3 >= right1)
      {
        double num4 = (double) ((Entity) this).X + (double) ((Entity) this).Width;
        Rectangle bounds4 = scene.Bounds;
        double right2 = (double) bounds4.Right;
        num2 = num4 >= right2 ? 1 : 0;
        goto label_10;
      }
    }
    num2 = 0;
label_10:
    if (num2 != 0)
      this.tiles.RenderAt(((((Entity) this).Position) + (new Vector2(3f, 0.0f))));
    int num5;
    if ((double) scene.ShakeVector.Y < 0.0)
    {
      double y1 = (double) scene.Camera.Y;
      Rectangle bounds5 = scene.Bounds;
      double top1 = (double) bounds5.Top;
      if (y1 <= top1)
      {
        double y2 = (double) ((Entity) this).Y;
        Rectangle bounds6 = scene.Bounds;
        double top2 = (double) bounds6.Top;
        num5 = y2 <= top2 ? 1 : 0;
        goto label_16;
      }
    }
    num5 = 0;
label_16:
    if (num5 != 0)
      this.tiles.RenderAt(((((Entity) this).Position) + (new Vector2(0.0f, -3f))));
    int num6;
    if ((double) scene.ShakeVector.Y > 0.0)
    {
      double num7 = (double) scene.Camera.Y + 180.0;
      Rectangle bounds7 = scene.Bounds;
      double bottom1 = (double) bounds7.Bottom;
      if (num7 >= bottom1)
      {
        double num8 = (double) ((Entity) this).Y + (double) ((Entity) this).Height;
        Rectangle bounds8 = scene.Bounds;
        double bottom2 = (double) bounds8.Bottom;
        num6 = num8 >= bottom2 ? 1 : 0;
        goto label_22;
      }
    }
    num6 = 0;
label_22:
    if (num6 != 0)
      this.tiles.RenderAt(((((Entity) this).Position) + (new Vector2(0.0f, 3f))));
    ((Entity) this).Render();
  }
}
