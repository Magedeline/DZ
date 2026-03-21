    namespace MaggyHelper.Entities;

[HotReloadable]
public class CharaBossStarfield : Backdrop
{
    public float Alpha = 1f;
    private const int particle_count = 200;
    private Particle[] charaparticles = new Particle[200];
    private VertexPositionColor[] verts = new VertexPositionColor[1206];
    private static readonly Color[] colors = new Color[4]
    {
        Calc.HexToColor("030c1b"),
        Calc.HexToColor("0b031b"),
        Calc.HexToColor("1b0319"),
        Calc.HexToColor("0f0301")
    };

    public CharaBossStarfield()
    {
        UseSpritebatch = false;
        for (int index = 0; index < 200; ++index)
        {
            charaparticles[index].Speed = Calc.Random.Range(500f, 1200f);
            charaparticles[index].Direction = new Vector2(-1f, 0.0f);
            charaparticles[index].DirectionApproach = Calc.Random.Range(0.25f, 4f);
            charaparticles[index].Position.X = Calc.Random.Range(0, 384);
            charaparticles[index].Position.Y = Calc.Random.Range(0, 244);
            charaparticles[index].Color = Calc.Random.Choose(CharaBossStarfield.colors);
        }
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (!Visible || Alpha <= 0.0)
            return;
        Vector2 vector = new Vector2(-1f, 0.0f);
        Level level = scene as Level;
        if (level.Bounds.Height > level.Bounds.Width)
            vector = new Vector2(0.0f, -1f);
        float target = vector.Angle();
        for (int index = 0; index < 200; ++index)
        {
            charaparticles[index].Position += charaparticles[index].Direction * charaparticles[index].Speed * Engine.DeltaTime;
            float angleRadians = Calc.AngleApproach(charaparticles[index].Direction.Angle(), target, charaparticles[index].DirectionApproach * Engine.DeltaTime);
            charaparticles[index].Direction = Calc.AngleToVector(angleRadians, 1f);
        }
    }

    public override void Render(Scene scene)
    {
        Vector2 position1 = (scene as Level).Camera.Position;
        Color color1 = Color.Black * Alpha;
        verts[0].Color = color1;
        verts[0].Position = new Vector3(-10f, -10f, 0.0f);
        verts[1].Color = color1;
        verts[1].Position = new Vector3(330f, -10f, 0.0f);
        verts[2].Color = color1;
        verts[2].Position = new Vector3(330f, 190f, 0.0f);
        verts[3].Color = color1;
        verts[3].Position = new Vector3(-10f, -10f, 0.0f);
        verts[4].Color = color1;
        verts[4].Position = new Vector3(330f, 190f, 0.0f);
        verts[5].Color = color1;
        verts[5].Position = new Vector3(-10f, 190f, 0.0f);
        for (int index1 = 0; index1 < 200; ++index1)
        {
            int index2 = (index1 + 1) * 6;
            float num1 = Calc.ClampedMap(charaparticles[index1].Speed, 0.0f, 1200f, 1f, 64f);
            float num2 = Calc.ClampedMap(charaparticles[index1].Speed, 0.0f, 1200f, 3f, 0.6f);
            Vector2 direction = charaparticles[index1].Direction;
            Vector2 vector21 = direction.Perpendicular();
            Vector2 position2 = charaparticles[index1].Position;
            position2.X = mod(position2.X - position1.X * 0.9f, 384f) - 32f;
            position2.Y = mod(position2.Y - position1.Y * 0.9f, 244f) - 32f;
            Vector2 vector22 = position2 - direction * num1 * 0.5f - vector21 * num2;
            Vector2 vector23 = position2 + direction * num1 * 1f - vector21 * num2;
            Vector2 vector24 = position2 + direction * num1 * 0.5f + vector21 * num2;
            Vector2 vector25 = position2 - direction * num1 * 1f + vector21 * num2;
            Color color2 = charaparticles[index1].Color * Alpha;
            verts[index2].Color = color2;
            verts[index2].Position = new Vector3(vector22, 0.0f);
            verts[index2 + 1].Color = color2;
            verts[index2 + 1].Position = new Vector3(vector23, 0.0f);
            verts[index2 + 2].Color = color2;
            verts[index2 + 2].Position = new Vector3(vector24, 0.0f);
            verts[index2 + 3].Color = color2;
            verts[index2 + 3].Position = new Vector3(vector22, 0.0f);
            verts[index2 + 4].Color = color2;
            verts[index2 + 4].Position = new Vector3(vector24, 0.0f);
            verts[index2 + 5].Color = color2;
            verts[index2 + 5].Position = new Vector3(vector25, 0.0f);
        }
        GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
    }

    private float mod(float x, float m) => (x % m + m) % m;

    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public Color Color;
        public float DirectionApproach;
    }
}



