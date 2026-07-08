// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.VanishingWall
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\AppData\Local\Temp\Mihavec\86958f101c\Code\ricky06ModPack.dll

#nullable disable
namespace Celeste.Entities
{
  [CustomEntity(new string[] { "DZ/VanishingWall" })]
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
      fillTile = tile;
      Collider = new Hitbox(width, height);
      Depth = -13000;
      Add(cutout = new EffectCutout());
    }

    public VanishingWall(EntityData data, Vector2 offset, EntityID eid)
      : this(eid, data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height)
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      int tilesX = (int)Width / 8;
      int tilesY = (int)Height / 8;
      Level level = SceneAs<Level>();
      Rectangle tileBounds = level.Session.MapData.TileBounds;
      VirtualMap<char> solidsData = level.SolidsData;
      int x = (int)X / 8 - tileBounds.Left;
      int y = (int)Y / 8 - tileBounds.Top;
      tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
      Add(tiles);
      Add(new TileInterceptor(tiles, false));
    }

    public void Awake(Scene scene, object title)
    {
      base.Awake(scene);
      if (CollideCheck<global::Celeste.Player>())
      {
        tiles.Alpha = 0.0f;
        fade = true;
        cutout.Visible = false;
        Audio.Play("event:/game/general/secret_revealed", Center);
        SceneAs<Level>().Session.DoNotLoad.Add(eid);
      }
      else
        Add(new TransitionListener()
        {
          OnOut = new System.Action<float>(OnTransitionOut),
          OnOutBegin = new System.Action(OnTransitionOutBegin),
          OnIn = new System.Action<float>(OnTransitionIn),
          OnInBegin = new System.Action(OnTransitionInBegin)
        });
    }

    private void OnTransitionOutBegin()
    {
      if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
      {
        transitionFade = true;
        transitionStartAlpha = tiles.Alpha;
      }
      else
        transitionFade = false;
    }

    private void OnTransitionOut(float percent)
    {
      if (!transitionFade)
        return;
      tiles.Alpha = transitionStartAlpha * (1f - percent);
    }

    private void OnTransitionInBegin()
    {
      Level level = SceneAs<Level>();
      if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
      {
        transitionFade = true;
        tiles.Alpha = 0.0f;
      }
      else
        transitionFade = false;
    }

    private void OnTransitionIn(float percent)
    {
      if (!transitionFade)
        return;
      tiles.Alpha = percent;
    }

    public override void Update()
    {
      base.Update();
      if (fade)
      {
        tiles.Alpha = Calc.Approach(tiles.Alpha, 0.0f, 2f * Engine.DeltaTime);
        cutout.Alpha = tiles.Alpha;
        if (tiles.Alpha > 0.0)
          return;
        RemoveSelf();
      }
      else
      {
        Collidable = true;
        if (CollideCheck<global::Celeste.Player>())
          Collidable = false;
        if (Collidable)
          return;
        Active = false;
      }
    }

    public void RemoveBlock()
    {
      global::Celeste.Player entity = Scene.Tracker.GetEntity<global::Celeste.Player>();
      if (entity == null || entity.StateMachine.State == Player.StDreamDash)
        return;
      SceneAs<Level>().Session.DoNotLoad.Add(eid);
      fade = true;
      Audio.Play("event:/game/general/secret_revealed", Center);
    }

    public override void Render()
    {
      Level scene = Scene as Level;
      if (scene.ShakeVector.X < 0.0 && (double)scene.Camera.X <= scene.Bounds.Left && (double)X <= scene.Bounds.Left)
        tiles.RenderAt(Position + new Vector2(-3f, 0.0f));
      if (scene.ShakeVector.X > 0.0 && (double)scene.Camera.X + 320.0 >= scene.Bounds.Right && (double)X + (double)Width >= scene.Bounds.Right)
        tiles.RenderAt(Position + new Vector2(3f, 0.0f));
      if (scene.ShakeVector.Y < 0.0 && (double)scene.Camera.Y <= scene.Bounds.Top && (double)Y <= scene.Bounds.Top)
        tiles.RenderAt(Position + new Vector2(0.0f, -3f));
      if (scene.ShakeVector.Y > 0.0 && (double)scene.Camera.Y + 180.0 >= scene.Bounds.Bottom && (double)Y + (double)Height >= scene.Bounds.Bottom)
        tiles.RenderAt(Position + new Vector2(0.0f, 3f));
      base.Render();
    }
  }
}

