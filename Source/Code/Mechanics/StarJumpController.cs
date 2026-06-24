// Decompiled with JetBrains decompiler
// Type: Celeste.StarJumpController
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
namespace Celeste.Entities;

[CustomEntity(ids: "MaggyHelper/StarClimbControl")]
[Tracked(true)]

public class StarClimbGraphicsController : Entity
{
  private const int ray_count = 100;
  public static VirtualRenderTarget BlockFill;
  private static StarClimbGraphicsController.Ray[] rays = new StarClimbGraphicsController.Ray[100];
  private VertexPositionColor[] vertices = new VertexPositionColor[600];
  private int vertexCount;
  private Color rayColor;
  private Color wipeColor;
  private Level level;

  public StarClimbGraphicsController(EntityData data, Vector2 offset)
  {
    this.Tag = (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
    this.rayColor = Calc.HexToColor(data.Attr("fgColor", "a3ffff")) * 0.25f;
    this.wipeColor = Calc.HexToColor(data.Attr("bgColor", "293E4B"));
  }

  public override void Added(Scene scene)
  {
    base.Added(scene);
    this.level = scene as Level;
    if (!this.detectOtherController())
      this.initBlockFill();
    this.Add((Component)new BeforeRenderHook(new System.Action(this.beforeRender)));
  }

  public override void Update()
  {
    base.Update();
    this.updateBlockFill();
  }

  private bool detectOtherController()
  {
    foreach (Entity entity in this.level.Tracker.GetEntities<StarClimbGraphicsController>())
    {
      if (entity is StarClimbGraphicsController graphicsController && graphicsController != this)
        return true;
    }
    return false;
  }

  private void initBlockFill()
  {
    for (int index = 0; index < StarClimbGraphicsController.rays.Length; ++index)
    {
      StarClimbGraphicsController.rays[index].Reset();
      StarClimbGraphicsController.rays[index].Percent = Calc.Random.NextFloat();
    }
  }

  private void updateBlockFill()
  {
    Vector2 vector = Calc.AngleToVector(-1.670796f, 1f);
    Vector2 vector21 = new Vector2(-vector.Y, vector.X);
    // Each ray produces 2 triangles = 6 vertices in the strip.
    int vertexIndex = 0;
    for (int index1 = 0; index1 < StarClimbGraphicsController.rays.Length; ++index1)
    {
      if ((double)StarClimbGraphicsController.rays[index1].Percent >= 1.0)
        StarClimbGraphicsController.rays[index1].Reset();
      StarClimbGraphicsController.rays[index1].Percent += Engine.DeltaTime / StarClimbGraphicsController.rays[index1].Duration;
      StarClimbGraphicsController.rays[index1].Y += 8f * Engine.DeltaTime;
      Vector2 rayCenter = new Vector2(StarClimbGraphicsController.mod(StarClimbGraphicsController.rays[index1].X - this.level.Camera.X * 0.9f, 480f) - 80f, StarClimbGraphicsController.mod(StarClimbGraphicsController.rays[index1].Y - this.level.Camera.Y * 0.7f, 580f) - 200f);
      float width = StarClimbGraphicsController.rays[index1].Width;
      float length = StarClimbGraphicsController.rays[index1].Length;
      Color color = this.rayColor * Ease.CubeInOut(Calc.YoYo(StarClimbGraphicsController.rays[index1].Percent));
      VertexPositionColor topRight = new VertexPositionColor(new Vector3(rayCenter + vector21 * width + vector * length, 0.0f), color);
      VertexPositionColor topLeft = new VertexPositionColor(new Vector3(rayCenter - vector21 * width, 0.0f), color);
      VertexPositionColor topRightInner = new VertexPositionColor(new Vector3(rayCenter + vector21 * width, 0.0f), color);
      VertexPositionColor bottomLeft = new VertexPositionColor(new Vector3(rayCenter - vector21 * width - vector * length, 0.0f), color);

      // Triangle 1
      this.vertices[vertexIndex++] = topRight;
      this.vertices[vertexIndex++] = topLeft;
      this.vertices[vertexIndex++] = topRightInner;
      // Triangle 2
      this.vertices[vertexIndex++] = topLeft;
      this.vertices[vertexIndex++] = topRightInner;
      this.vertices[vertexIndex++] = bottomLeft;
    }
    this.vertexCount = vertexIndex;
  }

  private void beforeRender()
  {
    if (StarClimbGraphicsController.BlockFill == null)
      StarClimbGraphicsController.BlockFill = VirtualContent.CreateRenderTarget("block-fill", 320, 180);
    if (this.vertexCount <= 0)
      return;
    Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)StarClimbGraphicsController.BlockFill);
    Engine.Graphics.GraphicsDevice.Clear(this.wipeColor);
    GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, this.vertices, this.vertexCount);
  }

  public override void Removed(Scene scene)
  {
    this.dispose();
    base.Removed(scene);
  }

  public override void SceneEnd(Scene scene)
  {
    this.dispose();
    base.SceneEnd(scene);
  }

  private void dispose()
  {
    if (this.detectOtherController())
      return;
    if (StarClimbGraphicsController.BlockFill != null)
      StarClimbGraphicsController.BlockFill.Dispose();
    StarClimbGraphicsController.BlockFill = (VirtualRenderTarget)null;
  }

  private static float mod(float x, float m) => (x % m + m) % m;

  private struct Ray
  {
    public float X;
    public float Y;
    public float Percent;
    public float Duration;
    public float Width;
    public float Length;

    public void Reset()
    {
      this.Percent = 0.0f;
      this.X = Calc.Random.NextFloat(480f);
      this.Y = Calc.Random.NextFloat(580f);
      this.Duration = (float)(4.0 + (double)Calc.Random.NextFloat() * 8.0);
      this.Width = (float)Calc.Random.Next(8, 80);
      this.Length = (float)Calc.Random.Next(20, 200);
    }
  }
}




