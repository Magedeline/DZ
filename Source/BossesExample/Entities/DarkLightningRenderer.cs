// Decompiled with JetBrains decompiler
// Type: global::Celeste.Mod.ricky06ModPack.Entities.DarkLightningRenderer
// Assembly: ricky06ModPack, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A006BC09-9B58-4275-A339-ACDC10C611D0
// Assembly location: C:\Users\warsh\Downloads\conquerorpeak119\Code\ricky06ModPack.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using On.Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Celeste.Mod.DZ.BossesExample.Entities;

[Tracked(false)]
public class DarkLightningRenderer : Entity
{
  public List<DarkLightning> list = new List<DarkLightning>();
  private List<DarkLightningRenderer.Edge> edges = new List<DarkLightningRenderer.Edge>();
  private List<DarkLightningRenderer.Bolt> bolts = new List<DarkLightningRenderer.Bolt>();
  private VertexPositionColor[] edgeVerts;
  private VirtualMap<bool> tiles;
  private Rectangle levelTileBounds;
  private uint edgeSeed;
  private uint leapSeed;
  private bool dirty;
  private Color[] electricityColors = new Color[2]
  {
    Calc.HexToColor("353b6e"),
    Calc.HexToColor("382c4d")
  };
  private Color[] electricityColorsLerped;
  public float Fade;
  public bool UpdateSeeds = true;
  public const int BoltBufferSize = 160 /*0xA0*/;
  public bool DrawEdges = true;
  public SoundSource AmbientSfx;

  public DarkLightningRenderer()
  {
    this.Tag = ((int) (Tags.Global)) | ((int) (Tags.TransitionUpdate));
    this.Depth = -1000100;
    this.electricityColorsLerped = new Color[this.electricityColors.Length];
    this.Add((Component) new CustomBloom(new Action(this.OnRenderBloom)));
    this.Add((Component) new BeforeRenderHook(new Action(this.OnBeforeRender)));
    this.Add((Component) (this.AmbientSfx = new SoundSource()));
    this.AmbientSfx.DisposeOnTransition = false;
  }

  public static void Load()
  {
    // ISSUE: method pointer
    On.Celeste.LevelLoader.LoadingThread += onLevelLoadingThread;
  }

  private static void onLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
  {
    orig(self);
    MapData mapData = self.Level.Session.MapData;
    bool? nullable;
    if (mapData == null)
    {
      nullable = new bool?();
    }
    else
    {
      List<LevelData> levels = mapData.Levels;
      nullable = levels != null ? new bool?(levels.Any<LevelData>((Func<LevelData, bool>) (level =>
      {
        List<EntityData> entities = level.Entities;
        return entities != null && entities.Any<EntityData>((Func<EntityData, bool>) (entity => entity.Name == "ricky06/DarkLightning"));
      }))) : new bool?();
    }
    if (!nullable.GetValueOrDefault())
      return;
    ((Scene) self.Level).Add((Entity) new DarkLightningRenderer());
  }

  public static void Unload()
  {
    // ISSUE: method pointer
    On.Celeste.LevelLoader.LoadingThread -= onLevelLoadingThread;
  }

  public override void Awake(Scene scene)
  {
    base.Awake(scene);
    for (int index = 0; index < 4; ++index)
    {
      this.bolts.Add(new DarkLightningRenderer.Bolt(this.electricityColors[0], 1f, 160 /*0xA0*/, 160 /*0xA0*/));
      this.bolts.Add(new DarkLightningRenderer.Bolt(this.electricityColors[1], 0.35f, 160 /*0xA0*/, 160 /*0xA0*/));
    }
  }

  public void StartAmbience()
  {
    if (this.AmbientSfx.Playing)
      return;
    this.AmbientSfx.Play("event:/new_content/env/10_electricity", (string) null, 0.0f);
  }

  public void StopAmbience() => this.AmbientSfx.Stop(true);

  public void Reset()
  {
    this.UpdateSeeds = true;
    this.Fade = 0.0f;
  }

  public void Track(DarkLightning block)
  {
    this.list.Add(block);
    if (this.tiles == null)
    {
      this.levelTileBounds = (this.Scene as Level).TileBounds;
      this.tiles = new VirtualMap<bool>(this.levelTileBounds.Width, this.levelTileBounds.Height, false);
    }
    for (int index1 = (int) block.X / 8; index1 < ((int) block.X + block.VisualWidth) / 8; ++index1)
    {
      for (int index2 = (int) block.Y / 8; index2 < ((int) block.Y + block.VisualHeight) / 8; ++index2)
        this.tiles[index1 - this.levelTileBounds.X, index2 - this.levelTileBounds.Y] = true;
    }
    this.dirty = true;
  }

  public void Untrack(DarkLightning block)
  {
    this.list.Remove(block);
    if (this.list.Count <= 0)
    {
      this.tiles = (VirtualMap<bool>) null;
    }
    else
    {
      for (int index1 = (int) block.X / 8; (double) index1 < (double) block.Right / 8.0; ++index1)
      {
        for (int index2 = (int) block.Y / 8; (double) index2 < (double) block.Bottom / 8.0; ++index2)
          this.tiles[index1 - this.levelTileBounds.X, index2 - this.levelTileBounds.Y] = false;
      }
    }
    this.dirty = true;
  }

  public override void Update()
  {
    if (this.dirty)
      this.RebuildEdges();
    this.ToggleEdges();
    if (this.list.Count <= 0)
    {
      this.StopAmbience();
      return;
    }
    this.StartAmbience();
    foreach (DarkLightningRenderer.Bolt bolt in this.bolts)
      bolt.Update(this.Scene);
    if (!this.UpdateSeeds)
      return;
    if (this.Scene.OnInterval(0.1f))
      this.edgeSeed = (uint) Calc.Random.Next();
    if (this.Scene.OnInterval(0.7f))
      this.leapSeed = (uint) Calc.Random.Next();
  }

  public void ToggleEdges(bool immediate = false)
  {
    Camera camera = (this.Scene as Level).Camera;
    Rectangle view = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4, (int) ((double) camera.Right - (double) camera.Left) + 8, (int) ((double) camera.Bottom - (double) camera.Top) + 8);
    for (int index = 0; index < this.edges.Count; ++index)
    {
      if (immediate)
        this.edges[index].Visible = this.edges[index].InView(ref view);
      else if (!this.edges[index].Visible && this.Scene.OnInterval(0.05f, (float) index * 0.01f) && this.edges[index].InView(ref view))
        this.edges[index].Visible = true;
      else if (this.edges[index].Visible && this.Scene.OnInterval(0.25f, (float) index * 0.01f) && !this.edges[index].InView(ref view))
        this.edges[index].Visible = false;
    }
  }

  private void RebuildEdges()
  {
    this.dirty = false;
    this.edges.Clear();
    if (this.list.Count <= 0)
      return;
    Level scene = this.Scene as Level;
    Rectangle tileBounds = scene.TileBounds;
    int left = tileBounds.Left;
    tileBounds = scene.TileBounds;
    int top = tileBounds.Top;
    tileBounds = scene.TileBounds;
    int right = tileBounds.Right;
    tileBounds = scene.TileBounds;
    int bottom = tileBounds.Bottom;
    Point[] pointArray = new Point[4]
    {
      new Point(0, -1),
      new Point(0, 1),
      new Point(-1, 0),
      new Point(1, 0)
    };
    foreach (DarkLightning parent in this.list)
    {
      for (int index1 = (int) parent.X / 8; (double) index1 < (double) parent.Right / 8.0; ++index1)
      {
        for (int index2 = (int) parent.Y / 8; (double) index2 < (double) parent.Bottom / 8.0; ++index2)
        {
          foreach (Point point1 in pointArray)
          {
            Point point2 = new Point(-point1.Y, point1.X);
            if (!this.Inside(index1 + point1.X, index2 + point1.Y) && (!this.Inside(index1 - point2.X, index2 - point2.Y) || this.Inside(index1 + point1.X - point2.X, index2 + point1.Y - point2.Y)))
            {
              Point point3 = new Point(index1, index2);
              Point point4 = new Point(index1 + point2.X, index2 + point2.Y);
              Vector2 vector2 = ((new Vector2(4f)) + (((new Vector2((float) (point1.X - point2.X), (float) (point1.Y - point2.Y))) * (4f))));
              int num = 1;
              while (this.Inside(point4.X, point4.Y) && !this.Inside(point4.X + point1.X, point4.Y + point1.Y))
              {
                point4.X += point2.X;
                point4.Y += point2.Y;
                ++num;
                if (num > 8)
                {
                  Vector2 a = ((((((new Vector2((float) point3.X, (float) point3.Y)) * (8f))) + (vector2))) - (parent.Position));
                  Vector2 b = ((((((new Vector2((float) point4.X, (float) point4.Y)) * (8f))) + (vector2))) - (parent.Position));
                  this.edges.Add(new DarkLightningRenderer.Edge(parent, a, b));
                  num = 0;
                  point3 = point4;
                }
              }
              if (num > 0)
              {
                Vector2 a = ((((((new Vector2((float) point3.X, (float) point3.Y)) * (8f))) + (vector2))) - (parent.Position));
                Vector2 b = ((((((new Vector2((float) point4.X, (float) point4.Y)) * (8f))) + (vector2))) - (parent.Position));
                this.edges.Add(new DarkLightningRenderer.Edge(parent, a, b));
              }
            }
          }
        }
      }
    }
    if (this.edgeVerts != null)
      return;
    this.edgeVerts = new VertexPositionColor[1024 /*0x0400*/];
  }

  private bool Inside(int tx, int ty)
  {
    return this.tiles[tx - this.levelTileBounds.X, ty - this.levelTileBounds.Y];
  }

  private void OnRenderBloom()
  {
    Camera camera = (this.Scene as Level).Camera;
    Rectangle rectangle = new Rectangle((int) camera.Left, (int) camera.Top, (int) ((double) camera.Right - (double) camera.Left), (int) ((double) camera.Bottom - (double) camera.Top));
    Color color = ((Color.White) * ((float) (0.25 + (double) this.Fade * 0.75)));
    foreach (DarkLightning darkLightning in this.list)
    {
      if (darkLightning.Visible)
        Draw.Rect(darkLightning.X, darkLightning.Y, (float) darkLightning.VisualWidth, (float) darkLightning.VisualHeight, color);
    }
    if ((double) this.Fade <= 0.0)
      return;
    Level scene = this.Scene as Level;
    Draw.Rect(scene.Camera.X, scene.Camera.Y, 320f, 180f, ((Color.White) * (this.Fade)));
  }

  private void OnBeforeRender()
  {
    if (this.list.Count <= 0)
      return;
    Engine.Graphics.GraphicsDevice.SetRenderTarget(((RenderTarget2D) (GameplayBuffers.Lightning)));
    Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(((Calc.HexToColor("f7b262")) * (0.1f)), Color.White, this.Fade));
    Draw.SpriteBatch.Begin();
    foreach (DarkLightningRenderer.Bolt bolt in this.bolts)
      bolt.Render();
    Draw.SpriteBatch.End();
    Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
  }

  public override void Render()
  {
    if (this.list.Count <= 0)
      return;
    Camera camera = (this.Scene as Level).Camera;
    Rectangle rectangle = new Rectangle((int) camera.Left, (int) camera.Top, (int) ((double) camera.Right - (double) camera.Left), (int) ((double) camera.Bottom - (double) camera.Top));
    foreach (DarkLightning darkLightning in this.list)
    {
      if (darkLightning.Visible)
        Draw.SpriteBatch.Draw((Texture2D) ((RenderTarget2D) (GameplayBuffers.Lightning)), darkLightning.Position, new Rectangle?(new Rectangle((int) darkLightning.X, (int) darkLightning.Y, darkLightning.VisualWidth, darkLightning.VisualHeight)), Color.Purple);
    }
    if (this.edges.Count <= 0 || !this.DrawEdges)
      return;
    for (int index = 0; index < this.electricityColorsLerped.Length; ++index)
      this.electricityColorsLerped[index] = Color.Lerp(this.electricityColors[index], Color.White, this.Fade);
    int index1 = 0;
    uint leapSeed = this.leapSeed;
    foreach (DarkLightningRenderer.Edge edge in this.edges)
    {
      if (edge.Visible)
      {
        DarkLightningRenderer.DrawSimpleLightning(ref index1, ref this.edgeVerts, this.edgeSeed, edge.Parent.Position, edge.A, edge.B, this.electricityColorsLerped[0], (float) (1.0 + (double) this.Fade * 3.0));
        DarkLightningRenderer.DrawSimpleLightning(ref index1, ref this.edgeVerts, this.edgeSeed + 1U, edge.Parent.Position, edge.A, edge.B, this.electricityColorsLerped[1], (float) (1.0 + (double) this.Fade * 3.0));
        if (DarkLightningRenderer.PseudoRand(ref leapSeed) % 30U == 0U)
          DarkLightningRenderer.DrawBezierLightning(ref index1, ref this.edgeVerts, this.edgeSeed, edge.Parent.Position, edge.A, edge.B, 24f, 10, this.electricityColorsLerped[1]);
      }
    }
    if (index1 <= 0)
      return;
    GameplayRenderer.End();
    GFX.DrawVertices<VertexPositionColor>(camera.Matrix, this.edgeVerts, index1, (Effect) null, (BlendState) null);
    GameplayRenderer.Begin();
  }

  private static void DrawSimpleLightning(
    ref int index,
    ref VertexPositionColor[] verts,
    uint seed,
    Vector2 pos,
    Vector2 a,
    Vector2 b,
    Color color,
    float thickness = 1f)
  {
    seed += (uint) (a.GetHashCode() + b.GetHashCode());
    a = ((a) + (pos));
    b = ((b) + (pos));
    Vector2 vector2_1 = ((b) - (a));
    float num1 = vector2_1.Length();
    Vector2 vector2_2 = ((((b) - (a))) / (num1));
    Vector2 vector2_3 = Calc.TurnRight(vector2_2);
    a = ((a) + (vector2_3));
    b = ((b) + (vector2_3));
    Vector2 vector2_4 = a;
    int num2 = DarkLightningRenderer.PseudoRand(ref seed) % 2U != 0U ? 1 : -1;
    float num3 = DarkLightningRenderer.PseudoRandRange(ref seed, 0.0f, 6.28318548f);
    float num4 = 0.0f;
    double num5 = (double) index;
    Vector2 vector2_5 = ((b) - (a));
    double num6 = ((double) vector2_5.Length() / 4.0 + 1.0) * 6.0;
    float num7 = (float) (num5 + num6);
    while ((double) num7 >= (double) verts.Length)
      Array.Resize<VertexPositionColor>(ref verts, verts.Length * 2);
    for (int index1 = index; (double) index1 < (double) num7; ++index1)
      verts[index1].Color = color;
    do
    {
      float num8 = DarkLightningRenderer.PseudoRandRange(ref seed, 0.0f, 4f);
      num3 += 0.1f;
      num4 += 4f + num8;
      Vector2 vector2_6 = ((a) + (((vector2_2) * (num4))));
      Vector2 vector2_7 = (double) num4 >= (double) num1 ? b : ((vector2_6) + ((((((((float) num2) * (vector2_3))) * (num8))) - (vector2_3))));
      verts[index++].Position = new Vector3(((vector2_4) - (((vector2_3) * (thickness)))), 0.0f);
      verts[index++].Position = new Vector3(((vector2_7) - (((vector2_3) * (thickness)))), 0.0f);
      verts[index++].Position = new Vector3(((vector2_7) + (((vector2_3) * (thickness)))), 0.0f);
      verts[index++].Position = new Vector3(((vector2_4) - (((vector2_3) * (thickness)))), 0.0f);
      verts[index++].Position = new Vector3(((vector2_7) + (((vector2_3) * (thickness)))), 0.0f);
      verts[index++].Position = new Vector3(vector2_4, 0.0f);
      vector2_4 = vector2_7;
      num2 = -num2;
    }
    while ((double) num4 < (double) num1);
  }

  private static void DrawBezierLightning(
    ref int index,
    ref VertexPositionColor[] verts,
    uint seed,
    Vector2 pos,
    Vector2 a,
    Vector2 b,
    float anchor,
    int steps,
    Color color)
  {
    seed += (uint) (a.GetHashCode() + b.GetHashCode());
    a = ((a) + (pos));
    b = ((b) + (pos));
    Vector2 vector2_1 = Calc.TurnRight(Calc.SafeNormalize(((b) - (a))));
    SimpleCurve simpleCurve = new SimpleCurve(a, b, ((((((b) + (a))) / (2f))) + (((vector2_1) * (anchor)))));
    int num = index + (steps + 2) * 6;
    while (num >= verts.Length)
      Array.Resize<VertexPositionColor>(ref verts, verts.Length * 2);
    Vector2 vector2_2 = simpleCurve.GetPoint(0.0f);
    for (int index1 = 0; index1 <= steps; ++index1)
    {
      Vector2 vector2_3 = simpleCurve.GetPoint((float) index1 / (float) steps);
      if (index1 != steps)
        vector2_3 = ((vector2_3) + (new Vector2(DarkLightningRenderer.PseudoRandRange(ref seed, -2f, 2f), DarkLightningRenderer.PseudoRandRange(ref seed, -2f, 2f))));
      verts[index].Position = new Vector3(((vector2_2) - (vector2_1)), 0.0f);
      verts[index++].Color = color;
      verts[index].Position = new Vector3(((vector2_3) - (vector2_1)), 0.0f);
      verts[index++].Color = color;
      verts[index].Position = new Vector3(vector2_3, 0.0f);
      verts[index++].Color = color;
      verts[index].Position = new Vector3(((vector2_2) - (vector2_1)), 0.0f);
      verts[index++].Color = color;
      verts[index].Position = new Vector3(vector2_3, 0.0f);
      verts[index++].Color = color;
      verts[index].Position = new Vector3(vector2_2, 0.0f);
      verts[index++].Color = color;
      vector2_2 = vector2_3;
    }
  }

  private static void DrawFatLightning(
    uint seed,
    Vector2 a,
    Vector2 b,
    float size,
    float gap,
    Color color)
  {
    seed += (uint) (a.GetHashCode() + b.GetHashCode());
    Vector2 vector2_1 = ((b) - (a));
    float num1 = vector2_1.Length();
    Vector2 vector2_2 = ((((b) - (a))) / (num1));
    Vector2 vector2_3 = Calc.TurnRight(vector2_2);
    Vector2 vector2_4 = a;
    int num2 = 1;
    double num3 = (double) DarkLightningRenderer.PseudoRandRange(ref seed, 0.0f, 6.28318548f);
    float num4 = 0.0f;
    do
    {
      num4 += DarkLightningRenderer.PseudoRandRange(ref seed, 10f, 14f);
      Vector2 vector2_5 = ((a) + (((vector2_2) * (num4))));
      Vector2 vector2_6 = (double) num4 >= (double) num1 ? b : ((vector2_5) + ((((((float) num2) * (vector2_3))) * (DarkLightningRenderer.PseudoRandRange(ref seed, 0.0f, 6f)))));
      Vector2 vector2_7 = vector2_6;
      if ((double) gap > 0.0)
      {
        vector2_7 = ((vector2_4) + (((((vector2_6) - (vector2_4))) * (1f - gap))));
        Draw.Line(vector2_4, ((vector2_6) + (vector2_2)), color, size * 0.5f);
      }
      Draw.Line(vector2_4, ((vector2_7) + (vector2_2)), color, size);
      vector2_4 = vector2_6;
      num2 = -num2;
    }
    while ((double) num4 < (double) num1);
  }

  private static uint PseudoRand(ref uint seed)
  {
    seed ^= seed << 13;
    seed ^= seed >> 17;
    return seed;
  }

  public static float PseudoRandRange(ref uint seed, float min, float max)
  {
    return min + (float) ((double) (DarkLightningRenderer.PseudoRand(ref seed) & 1023U /*0x03FF*/) / 1024.0 * ((double) max - (double) min));
  }

  private class Bolt
  {
    private List<Vector2> nodes = new List<Vector2>();
    private Coroutine routine;
    private bool visible;
    private float size;
    private float gap;
    private float alpha;
    private uint seed;
    private float flash;
    private readonly Color color;
    private readonly float scale;
    private readonly int width;
    private readonly int height;

    public Bolt(Color color, float scale, int width, int height)
    {
      this.color = color;
      this.width = width;
      this.height = height;
      this.scale = scale;
      this.routine = new Coroutine(this.Run(), true);
    }

    public void Update(Scene scene)
    {
      ((Component) this.routine).Update();
      this.flash = Calc.Approach(this.flash, 0.0f, Engine.DeltaTime * 2f);
    }

    private IEnumerator Run()
    {
      yield return (object) Calc.Range(Calc.Random, 0.0f, 4f);
      while (true)
      {
        List<Vector2> list = new List<Vector2>();
        for (int k = 0; k < 3; ++k)
        {
          Vector2 item = Calc.Choose<Vector2>(Calc.Random, new Vector2(0.0f, (float) Calc.Range(Calc.Random, 8, this.height - 16 /*0x10*/)), new Vector2((float) Calc.Range(Calc.Random, 8, this.width - 16 /*0x10*/), 0.0f), new Vector2((float) this.width, (float) Calc.Range(Calc.Random, 8, this.height - 16 /*0x10*/)), new Vector2((float) Calc.Range(Calc.Random, 8, this.width - 16 /*0x10*/), (float) this.height));
          Vector2 item2 = (double) item.X <= 0.0 || (double) item.X >= (double) this.width ? new Vector2((float) this.width - item.X, item.Y) : new Vector2(item.X, (float) this.height - item.Y);
          list.Add(item);
          list.Add(item2);
          item = new Vector2();
          item2 = new Vector2();
        }
        List<Vector2> list2 = new List<Vector2>();
        for (int l = 0; l < 3; ++l)
          list2.Add(new Vector2(Calc.Range(Calc.Random, 0.25f, 0.75f) * (float) this.width, Calc.Range(Calc.Random, 0.25f, 0.75f) * (float) this.height));
        this.nodes.Clear();
        foreach (Vector2 vector2 in list)
        {
          Vector2 item4 = vector2;
          this.nodes.Add(item4);
          this.nodes.Add(Calc.ClosestTo(list2, item4));
          item4 = new Vector2();
        }
        Vector2 item3 = list2[list2.Count - 1];
        foreach (Vector2 vector2 in list2)
        {
          Vector2 item5 = vector2;
          this.nodes.Add(item3);
          this.nodes.Add(item5);
          item3 = item5;
          item5 = new Vector2();
        }
        this.flash = 1f;
        this.visible = true;
        this.size = 5f;
        this.gap = 0.0f;
        this.alpha = 1f;
        for (int j = 0; j < 4; ++j)
        {
          this.seed = (uint) Calc.Random.Next();
          yield return (object) 0.1f;
        }
        for (int j = 0; j < 5; ++j)
        {
          if (!Settings.Instance.DisableFlashes)
            this.visible = false;
          yield return (object) (float) (0.05000000074505806 + (double) j * 0.019999999552965164);
          float num = (float) j / 5f;
          this.visible = true;
          this.size = (float) ((1.0 - (double) num) * 5.0);
          this.gap = num;
          this.alpha = 1f - num;
          this.visible = true;
          this.seed = (uint) Calc.Random.Next();
          yield return (object) 0.025f;
        }
        this.visible = false;
        yield return (object) Calc.Range(Calc.Random, 4f, 8f);
        list = (List<Vector2>) null;
        list2 = (List<Vector2>) null;
        item3 = new Vector2();
      }
    }

    public void Render()
    {
      Draw.Rect(0.0f, 0.0f, (float) this.width, (float) this.height, ((Calc.HexToColor("0b001f")) * (0.1f)));
      if ((double) this.flash > 0.0 && !Settings.Instance.DisableFlashes)
        Draw.Rect(0.0f, 0.0f, (float) this.width, (float) this.height, ((((((Color.White) * (this.flash))) * (0.15f))) * (this.scale)));
      if (!this.visible)
        return;
      for (int index = 0; index < this.nodes.Count; index += 2)
        DarkLightningRenderer.DrawFatLightning(this.seed, this.nodes[index], this.nodes[index + 1], this.size * this.scale, this.gap, ((this.color) * (this.alpha)));
    }
  }

  private class Edge
  {
    public DarkLightning Parent;
    public bool Visible;
    public Vector2 A;
    public Vector2 B;
    public Vector2 Min;
    public Vector2 Max;

    public Edge(DarkLightning parent, Vector2 a, Vector2 b)
    {
      this.Parent = parent;
      this.Visible = true;
      this.A = a;
      this.B = b;
      this.Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
      this.Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    public bool InView(ref Rectangle view)
    {
      return (double) view.Left < (double) this.Parent.X + (double) this.Max.X && (double) view.Right > (double) this.Parent.X + (double) this.Min.X && (double) view.Top < (double) this.Parent.Y + (double) this.Max.Y && (double) view.Bottom > (double) this.Parent.Y + (double) this.Min.Y;
    }
  }
}

