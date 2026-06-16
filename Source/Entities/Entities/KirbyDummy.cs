namespace Celeste.Entities;

[CustomEntity(ids: "DZ/KirbyDummy")]
[Tracked]
[HotReloadable]
public sealed class KirbyDummy : PassiveFollowerDummy
{
    private const float DefaultFollowDelay = 0.3f;
    private const float DefaultFollowSpeed = 360f;

    // Dash color constants
    private static readonly Color UsedHairColor = Calc.HexToColor("44B7FF");
    private static readonly Color NormalHairColor = Calc.HexToColor("AC3232");
    private static readonly Color TwoDashesHairColor = Calc.HexToColor("ff6def");
    private static readonly Color TripleDashHairColor = Calc.HexToColor("ffa500");
    private static readonly Color QuadDashHairColor = Calc.HexToColor("00ff88");
    private static readonly Color PentaDashHairColor = Calc.HexToColor("00ccff");
    private static readonly Color HexaDashHairColor = Calc.HexToColor("aa44ff");
    private static readonly Color SeptaDashHairColor = Calc.HexToColor("ff2288");
    private static readonly Color OctaDashHairColor = Calc.HexToColor("ffee00");
    private static readonly Color NonaDashHairColor = Calc.HexToColor("22ffdd");
    private static readonly Color DecaDashHairColor = Calc.HexToColor("ffffff");

    public Image ScarfImage { get; private set; }

    public KirbyDummy(EntityData data, Vector2 offset)
        : base(data, offset, "KirbyDummy", "kirby", DefaultFollowDelay, DefaultFollowSpeed)
    {
        InitializeScarf();
    }

    public KirbyDummy(Vector2 position, string animation = "idle", bool autoFollow = true)
        : base(position, "KirbyDummy", "kirby", animation, 1, 1f, 1f, true, true, autoFollow, DefaultFollowDelay, DefaultFollowSpeed)
    {
        InitializeScarf();
    }

    private void InitializeScarf()
    {
        try
        {
            MTexture scarfTexture = GFX.Game["characters/kirby/scarf00"];
            ScarfImage = new Image(scarfTexture);
            ScarfImage.CenterOrigin();
            ScarfImage.Position = new Vector2(0f, -6f);
            ScarfImage.Color = NormalHairColor;
            Add(ScarfImage);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "KirbyDummy", $"Scarf image initialization failed: {ex.Message}");
            ScarfImage = null;
        }
    }

    private static Color GetDashColor(int dashes)
    {
        return dashes switch
        {
            >= 10 => DecaDashHairColor,
            9 => NonaDashHairColor,
            8 => OctaDashHairColor,
            7 => SeptaDashHairColor,
            6 => HexaDashHairColor,
            5 => PentaDashHairColor,
            4 => QuadDashHairColor,
            3 => TripleDashHairColor,
            2 => TwoDashesHairColor,
            _ => NormalHairColor,
        };
    }

    public override void Update()
    {
        base.Update();

        if (ScarfImage == null) return;

        Player player = Scene?.Tracker?.GetEntity<Player>();
        if (player == null) return;

        Color targetColor;
        if (player.Dashes == 0 && player.Dashes < player.MaxDashes)
            targetColor = UsedHairColor;
        else
            targetColor = GetDashColor(player.Dashes);

        ScarfImage.Color = Color.Lerp(ScarfImage.Color, targetColor, 6f * Engine.DeltaTime);

        // Flip scarf to match sprite facing
        if (Sprite != null)
            ScarfImage.Scale.X = Math.Sign(Sprite.Scale.X);
    }
}