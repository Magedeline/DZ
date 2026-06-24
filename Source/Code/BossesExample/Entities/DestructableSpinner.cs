// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.DestructableSpinner
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

#nullable disable
namespace Celeste.Mod.MaggyHelper.BossesExample.Entities;

[CustomEntity(new string[] {"ricky06/DestructableSpinner"})]
[Tracked(false)]
public class DestructableSpinner : Entity
{
  public static ParticleType P_Move;
  public const float ParticleInterval = 0.02f;
  public string bgDirectory = "danger/crystal/bg";
  public string fgDirectory = "danger/crystal/fg";
  private Color tint;
  public bool AttachToSolid;
  private Entity filler;
  private DestructableSpinner.Border border;
  private float offset;
  private bool expanded;
  private int randomSeed;
  private int ID;
  private bool persistent;

  public DestructableSpinner(EntityData data, Vector2 offset)
    : base(((data.Position) + (offset)))
  {
    string str = data.Attr(nameof (tint), "");
    if (this.persistent = data.Bool("Persistent", false))
      this.AddTag(((int) (Tags.Global)));
    this.ID = data.ID;
    if (str.Length == 0)
      str = "FFFFFF";
    this.tint = Calc.HexToColor(str);
    this.offset = Calc.NextFloat(Calc.Random);
    this.Tag = ((int) (Tags.TransitionUpdate));
    this.Collider = (Collider) new ColliderList(new Collider[2]
    {
      (Collider) new Circle(6f, 0.0f, 0.0f),
      (Collider) new Hitbox(16f, 4f, -8f, -3f)
    });
    this.Visible = false;
    this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer), (Collider) null, (Collider) null));
    this.Add((Component) new HoldableCollider(new Action<Holdable>(this.OnHoldable), (Collider) null));
    this.Add((Component) new LedgeBlocker((Func<Player, bool>) null));
    this.Depth = -8500;
    this.AttachToSolid = data.Bool("attachToSolid", false);
    if (this.AttachToSolid)
      this.Add((Component) new StaticMover()
      {
        OnShake = new Action<Vector2>(this.OnShake),
        SolidChecker = new Func<Solid, bool>(this.IsRiding),
        OnDestroy = new Action(((Entity) this).RemoveSelf)
      });
    this.randomSeed = Calc.Random.Next();
    if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.noResetDict.ContainsKey((Entity) this))
      return;
    global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.noResetDict.Add((Entity) this, new EntityID(data.Level.Name, data.ID));
  }

  public override void Awake(Scene scene) => this.orig_Awake(scene);

  public void ForceInstantiate()
  {
    this.CreateSprites();
    this.Visible = true;
  }

  public override void Update()
  {
    if (!this.Visible)
    {
      this.Collidable = false;
      if (this.InView())
      {
        this.Visible = true;
        if (!this.expanded)
          this.CreateSprites();
      }
    }
    else
    {
      base.Update();
      if (this.Scene.OnInterval(0.25f, this.offset) && !this.InView())
        this.Visible = false;
      if (this.Scene.OnInterval(0.05f, this.offset))
      {
        Player entity = this.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
          this.Collidable = (double) Math.Abs(((Entity) entity).X - this.X) < 128.0 && (double) Math.Abs(((Entity) entity).Y - this.Y) < 128.0;
      }
    }
    if (this.filler == null)
      return;
    this.filler.Position = this.Position;
  }

  private bool InView()
  {
    Camera camera = (this.Scene as Level).Camera;
    return (double) this.X > (double) camera.X - 16.0 && (double) this.Y > (double) camera.Y - 16.0 && (double) this.X < (double) camera.X + 320.0 + 16.0 && (double) this.Y < (double) camera.Y + 180.0 + 16.0;
  }

  private void CreateSprites()
  {
    if (this.expanded)
      return;
    Calc.PushRandom(this.randomSeed);
    List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(this.fgDirectory);
    MTexture mtexture = Calc.Choose<MTexture>(Calc.Random, atlasSubtextures);
    Color tint = this.tint;
    if (!this.SolidCheck(new Vector2(this.X - 4f, this.Y - 4f)))
      this.Add((Component) new Image(mtexture.GetSubtexture(0, 0, 14, 14, (MTexture) null)).SetOrigin(12f, 12f).SetColor(tint));
    if (!this.SolidCheck(new Vector2(this.X + 4f, this.Y - 4f)))
      this.Add((Component) new Image(mtexture.GetSubtexture(10, 0, 14, 14, (MTexture) null)).SetOrigin(2f, 12f).SetColor(tint));
    if (!this.SolidCheck(new Vector2(this.X + 4f, this.Y + 4f)))
      this.Add((Component) new Image(mtexture.GetSubtexture(10, 10, 14, 14, (MTexture) null)).SetOrigin(2f, 2f).SetColor(tint));
    if (!this.SolidCheck(new Vector2(this.X - 4f, this.Y + 4f)))
      this.Add((Component) new Image(mtexture.GetSubtexture(0, 10, 14, 14, (MTexture) null)).SetOrigin(12f, 2f).SetColor(tint));
    foreach (DestructableSpinner entity in this.Scene.Tracker.GetEntities<DestructableSpinner>())
    {
      int num;
      if (entity.ID > this.ID && entity.AttachToSolid == this.AttachToSolid)
      {
        Vector2 vector2 = ((entity.Position) - (this.Position));
        num = (double) vector2.LengthSquared() < 576.0 ? 1 : 0;
      }
      else
        num = 0;
      if (num != 0)
        this.AddSprite(((((((this.Position) + (entity.Position))) / (2f))) - (this.Position)));
    }
    this.Scene.Add((Entity) (this.border = new DestructableSpinner.Border((Entity) this, this.filler)));
    this.expanded = true;
    Calc.PopRandom();
  }

  private void AddSprite(Vector2 offset)
  {
    if (this.filler == null)
    {
      this.Scene.Add(this.filler = new Entity(this.Position));
      this.filler.Depth = this.Depth + 1;
    }
    List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(this.bgDirectory);
    Image image = new Image(Calc.Choose<MTexture>(Calc.Random, atlasSubtextures));
    ((GraphicsComponent) image).Position = offset;
    ((GraphicsComponent) image).Rotation = (float) Calc.Choose<int>(Calc.Random, 0, 1, 2, 3) * 1.57079637f;
    ((GraphicsComponent) image).Color = this.tint;
    image.CenterOrigin();
    this.filler.Add((Component) image);
  }

  private bool SolidCheck(Vector2 position)
  {
    if (this.AttachToSolid)
      return false;
    foreach (Solid solid in this.Scene.CollideAll<Solid>(position))
    {
      if (solid is SolidTiles)
        return true;
    }
    return false;
  }

  private void OnShake(Vector2 pos)
  {
    foreach (Component component in this.Components)
    {
      if (component is Image)
        ((GraphicsComponent) (component as Image)).Position = pos;
    }
  }

  private bool IsRiding(Solid solid) => this.CollideCheck((Entity) solid);

  private void OnPlayer(Player player)
  {
    if (global::Celeste.Mod.MaggyHelper.BossesExample.BossesExampleModule.destroyDashActive)
      this.Destroy();
    else
      player.Die(Calc.SafeNormalize(((((Entity) player).Position) - (this.Position))), false, true);
  }

  private void OnHoldable(Holdable h) => h.HitSpinner((Entity) this);

  public override void Removed(Scene scene)
  {
    if (this.filler != null && this.filler.Scene == scene)
      this.filler.RemoveSelf();
    if (this.border != null && this.border.Scene == scene)
      this.border.RemoveSelf();
    base.Removed(scene);
  }

  public void Destroy(bool boss = false)
  {
    if (this.InView())
    {
      for (int index = 0; index < 5; ++index)
        global::Celeste.Audio.Play("event:/game/06_reflection/fall_spike_smash", this.Position);
      CrystalDebris.Burst(this.Position, this.tint, boss, 8);
      if (this.persistent)
      {
        Level level = this.SceneAs<Level>();
        level.Session.SetFlag($"destructablespinner_{this.ID.ToString()}{level.Session.Level}", true);
      }
    }
    this.RemoveSelf();
  }

  public void orig_Awake(Scene scene)
  {
    base.Awake(scene);
    Level level = this.SceneAs<Level>();
    if (level.Session.GetFlag($"destructablespinner_{this.ID.ToString()}{level.Session.Level}"))
    {
      this.RemoveSelf();
    }
    else
    {
      if (!this.InView())
        return;
      this.CreateSprites();
    }
  }

  private class Border : Entity
  {
    private Entity[] drawing = new Entity[2];

    public Border(Entity parent, Entity filler)
    {
      this.drawing[0] = parent;
      this.drawing[1] = filler;
      this.Depth = parent.Depth + 2;
    }

    public override void Render()
    {
      if (!this.drawing[0].Visible)
        return;
      this.DrawBorder(this.drawing[0]);
      this.DrawBorder(this.drawing[1]);
    }

    private void DrawBorder(Entity entity)
    {
      if (entity == null)
        return;
      foreach (Component component in entity.Components)
      {
        if (component is Image image)
        {
          Color color = ((GraphicsComponent) image).Color;
          Vector2 position = ((GraphicsComponent) image).Position;
          ((GraphicsComponent) image).Color = Color.Black;
          ((GraphicsComponent) image).Position = ((position) + (new Vector2(0.0f, -1f)));
          ((Component) image).Render();
          ((GraphicsComponent) image).Position = ((position) + (new Vector2(0.0f, 1f)));
          ((Component) image).Render();
          ((GraphicsComponent) image).Position = ((position) + (new Vector2(-1f, 0.0f)));
          ((Component) image).Render();
          ((GraphicsComponent) image).Position = ((position) + (new Vector2(1f, 0.0f)));
          ((Component) image).Render();
          ((GraphicsComponent) image).Color = color;
          ((GraphicsComponent) image).Position = position;
        }
      }
    }
  }
}

