namespace MaggyHelper.Triggers
{
  public class MaddyPhone : Entity
  {
    private readonly VertexLight light;

    public MaddyPhone(Vector2 position)
      : base(position)
    {
      this.Add((Component) (this.light = new VertexLight(Color.LightSkyBlue, 1f, 8, 16)));
      this.Add((Component) new Monocle.Image(GFX.Game["characters/madeline/phone"]).JustifyOrigin(0.5f, 1f));
    }

    public override void Update()
    {
      if (this.Scene.OnInterval(0.5f))
        this.light.Visible = !this.light.Visible;
      base.Update();
    }
  }
}




