// Decompiled with JetBrains decompiler
// Type: Celeste.TemplePortalTorch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Users\User\OneDrive\Desktop\Celeste!\Celeste\Celeste.exe

#nullable disable
namespace Celeste.Entities
{
  public class TesseractPortalTorch : Entity
  {
    private Sprite sprite;
    private VertexLight light;
    private BloomPoint bloom;
    private SoundSource loopSfx;
    private bool lit;

    public TesseractPortalTorch(Vector2 pos)
      : base(pos)
    {
      this.Add((Component) (this.sprite = new Sprite(GFX.Game, "objects/temple/portal/portaltorch")));
      this.sprite.AddLoop("idle", "", 0.0f, new int[1]);
      this.sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
      this.sprite.Play("idle");
      this.sprite.Origin = new Vector2(32f, 64f);
      this.Depth = 8999;
    }

    public void Light(int count = 0, bool silent = false)
    {
      if (this.lit)
        return;
      this.lit = true;
      this.sprite.Play("lit");
      this.Add((Component) (this.bloom = new BloomPoint(1f, 16f)));
      this.Add((Component) (this.light = new VertexLight(Color.LightSeaGreen, 0.0f, 32, 128)));
      if (!silent)
        Audio.Play(count == 0 ? "guid://{b857bbcf-540e-46fd-af37-5ea6d12e28f2}" : "guid://{35abfea4-7b3b-489d-bcd8-431b93b87fa4}", this.Position);
      this.Add((Component) (this.loopSfx = new SoundSource()));
      this.loopSfx.Play("guid://{36604f79-14aa-4a46-a7b3-42a15fc7fbc1}");
    }

    public override void Update()
    {
      base.Update();
      if (this.bloom != null && (double) this.bloom.Alpha > 0.5)
        this.bloom.Alpha -= Engine.DeltaTime;
      if (this.light == null || (double) this.light.Alpha >= 1.0)
        return;
      this.light.Alpha = Calc.Approach(this.light.Alpha, 1f, Engine.DeltaTime);
    }
  }
}

