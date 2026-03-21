namespace MaggyHelper.Entities;

[CustomEntity("MaggyHelper/KirbyTutorialBird")]
[Tracked]
[HotReloadable]
public class KirbyTutorialBird : BirdNPC
{
    private static readonly Dictionary<string, Vector2> Directions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Left"] = new Vector2(-1f, 0f),
        ["Right"] = new Vector2(1f, 0f),
        ["Up"] = new Vector2(0f, -1f),
        ["Down"] = new Vector2(0f, 1f),
        ["UpLeft"] = new Vector2(-1f, -1f),
        ["UpRight"] = new Vector2(1f, -1f),
        ["DownLeft"] = new Vector2(-1f, 1f),
        ["DownRight"] = new Vector2(1f, 1f),
    };

    private static readonly Dictionary<string, BirdTutorialGui.ButtonPrompt> ButtonAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dash"] = BirdTutorialGui.ButtonPrompt.Dash,
        ["DashButton"] = BirdTutorialGui.ButtonPrompt.Dash,
        ["Dodge"] = BirdTutorialGui.ButtonPrompt.Dash,
        ["Jump"] = BirdTutorialGui.ButtonPrompt.Jump,
        ["Hop"] = BirdTutorialGui.ButtonPrompt.Jump,
        ["Grab"] = BirdTutorialGui.ButtonPrompt.Grab,
        ["Hold"] = BirdTutorialGui.ButtonPrompt.Grab,
        ["Talk"] = BirdTutorialGui.ButtonPrompt.Talk,
        ["Interact"] = BirdTutorialGui.ButtonPrompt.Talk,
        ["Confirm"] = BirdTutorialGui.ButtonPrompt.Talk,
        ["Climb"] = BirdTutorialGui.ButtonPrompt.Grab,
    };

    private static readonly string[] CompoundControlKeywords =
    {
        "DownRight", "DownLeft", "UpRight", "UpLeft",
        "TinyArrow", "Confirm", "Interact", "Climb",
        "Right", "Left", "Down", "Up",
        "Press", "Hold", "Then", "Plus",
        "Dash", "Jump", "Grab", "Talk",
    };

    public string BirdID { get; }
    public int StartupIndex { get; }
    public bool TriggerOnce { get; }

    private readonly bool cawOnTutorial;
    private readonly bool onlyOnce;
    private readonly List<BirdTutorialGui> guis = new();
    private readonly List<bool> triggered = new();

    private int currentActiveIndex = -1;
    private bool flewAway;

    public KirbyTutorialBird(EntityData data, Vector2 offset)
        : base(data.Position + offset, Modes.None)
    {
        BirdID = data.Attr("birdId");
        StartupIndex = data.Int("startupIndex", 0);
        TriggerOnce = data.Bool("triggerOnce", true);
        cawOnTutorial = data.Bool("caw", true);
        onlyOnce = data.Bool("onlyOnce", false);
        EntityID = new EntityID(data.Level.Name, data.ID);

        Facing = data.Bool("faceLeft", true) ? Facings.Left : Facings.Right;
        Sprite.Scale.X = (float) Facing;

        BuildTutorials(data.Attr("dialogs"), data.Attr("controls"));
    }

    public bool IsTutorialTriggered(int index)
    {
        return index >= 0 && index < triggered.Count && triggered[index];
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (scene is not Level lvl || !lvl.Session.Area.SID.Equals(AreaModeExtender.BuildASideSID("20_TheEnd"), StringComparison.OrdinalIgnoreCase))
        {
            RemoveSelf();
            return;
        }

        bool hasMatchingTrigger = scene.Entities.Any(entity =>
        {
            if (!string.Equals(entity.GetType().Name, "KirbyTutorialBirdTrigger", StringComparison.Ordinal))
            {
                return false;
            }

            PropertyInfo birdIdProperty = entity.GetType().GetProperty("BirdID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return string.Equals(birdIdProperty?.GetValue(entity) as string, BirdID, StringComparison.Ordinal);
        });

        if (!hasMatchingTrigger && StartupIndex >= 0)
        {
            TriggerShowTutorial(StartupIndex);
        }
    }

    public void TriggerShowTutorial(int index)
    {
        if (index == currentActiveIndex)
        {
            return;
        }

        if (index >= 0 && index < guis.Count && (!TriggerOnce || !triggered[index]))
        {
            triggered[index] = true;
            currentActiveIndex = index;
            Add(new Coroutine(ChangeTutorial(guis[index], cawOnTutorial), true));
            return;
        }

        currentActiveIndex = -1;
        Add(new Coroutine(HideTutorial(), true));
    }

    public void TriggerCloseTutorial()
    {
        if (flewAway)
        {
            return;
        }

        flewAway = true;
        Add(new Coroutine(CloseRoutine(), true));
    }

    private IEnumerator ChangeTutorial(BirdTutorialGui gui, bool caw)
    {
        yield return HideTutorial();
        yield return ShowTutorial(gui, caw);
    }

    private IEnumerator CloseRoutine()
    {
        currentActiveIndex = -1;
        yield return HideTutorial();

        if (onlyOnce && Scene is Level level)
        {
            level.Session.DoNotLoad.Add(EntityID);
        }

        yield return Startle("event:/game/general/bird_startle");
        yield return FlyAway();
    }

    private void BuildTutorials(string dialogsText, string controlsText)
    {
        if (string.IsNullOrWhiteSpace(dialogsText) || string.IsNullOrWhiteSpace(controlsText))
        {
            return;
        }

        string[] dialogIds = dialogsText.Split(';');
        string[] controls = controlsText.Split(';');
        int count = Math.Min(dialogIds.Length, controls.Length);

        for (int i = 0; i < count; i++)
        {
            string dialogId = dialogIds[i].Trim();
            string controlLine = controls[i].Trim();

            if (string.IsNullOrWhiteSpace(dialogId) || string.IsNullOrWhiteSpace(controlLine))
            {
                continue;
            }

            guis.Add(new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean(dialogId), ParseControls(controlLine)));
            triggered.Add(false);
        }
    }

    private object[] ParseControls(string controlLine)
    {
        string[] tokens = controlLine.Split(',');
        List<object> parsed = new(tokens.Length);

        foreach (string rawToken in tokens)
        {
            string token = rawToken.Trim();
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            if (TrySplitCompoundToken(token, out List<string> expanded))
            {
                foreach (string subToken in expanded)
                {
                    AddParsedToken(parsed, subToken);
                }

                continue;
            }

            AddParsedToken(parsed, token);
        }

        return parsed.ToArray();
    }

    private static void AddParsedToken(List<object> parsed, string token)
    {
        if (TryParseLiteralToken(token, out object literal))
        {
            parsed.Add(literal);
            return;
        }

        if (TryParseButtonPromptMethod(token, out BirdTutorialGui.ButtonPrompt methodPrompt))
        {
            parsed.Add(methodPrompt);
            return;
        }

        if (TryParseButtonPrompt(token, out BirdTutorialGui.ButtonPrompt prompt))
        {
            parsed.Add(prompt);
            return;
        }

        if (TryParseDirectionToken(token, out Vector2 direction))
        {
            parsed.Add(direction);
            return;
        }

        if (string.Equals(token, "tinyarrow", StringComparison.OrdinalIgnoreCase))
        {
            parsed.Add(GFX.Gui["tinyarrow"]);
            return;
        }

        if (token.StartsWith("mod:", StringComparison.OrdinalIgnoreCase) && TryResolveModButtonTexture(token[4..], out MTexture texture))
        {
            parsed.Add(texture);
            return;
        }

        parsed.Add(token);
    }

    private static bool TryParseButtonPrompt(string token, out BirdTutorialGui.ButtonPrompt prompt)
    {
        if (Enum.TryParse(token, true, out prompt))
        {
            return true;
        }

        return ButtonAliases.TryGetValue(token, out prompt);
    }

    private static bool TryParseButtonPromptMethod(string token, out BirdTutorialGui.ButtonPrompt prompt)
    {
        prompt = default;
        int separatorIndex = token.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex >= token.Length - 1)
        {
            return false;
        }

        string method = token[..separatorIndex].Trim();
        string argument = token[(separatorIndex + 1)..].Trim();

        if (!(string.Equals(method, "btn", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "button", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "prompt", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "method", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "vb", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return TryParseButtonPrompt(argument, out prompt);
    }

    private static bool TryParseDirectionToken(string token, out Vector2 direction)
    {
        if (Directions.TryGetValue(token, out direction))
        {
            return true;
        }

        string normalized = token
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return Directions.TryGetValue(normalized, out direction);
    }

    private static bool TryParseLiteralToken(string token, out object literal)
    {
        switch (token.ToUpperInvariant())
        {
            case "PLUS":
            case "+":
                literal = "+";
                return true;
            case "ARROW":
            case "TINYARROW":
                literal = GFX.Gui["tinyarrow"];
                return true;
            case "HOLD":
            case "PRESS":
            case "THEN":
                literal = token.ToUpperInvariant();
                return true;
            default:
                literal = null;
                return false;
        }
    }

    private static bool TrySplitCompoundToken(string token, out List<string> split)
    {
        split = null;

        if (string.IsNullOrWhiteSpace(token) || token.Contains(':') || token.Contains("/"))
        {
            return false;
        }

        string remaining = token.Replace(" ", string.Empty, StringComparison.Ordinal);
        List<string> result = new();

        while (remaining.Length > 0)
        {
            string matched = CompoundControlKeywords
                .OrderByDescending(k => k.Length)
                .FirstOrDefault(keyword => remaining.StartsWith(keyword, StringComparison.OrdinalIgnoreCase));

            if (matched == null)
            {
                if (remaining.StartsWith("+", StringComparison.Ordinal))
                {
                    result.Add("PLUS");
                    remaining = remaining[1..];
                    continue;
                }

                return false;
            }

            result.Add(matched);
            remaining = remaining[matched.Length..];
        }

        if (result.Count <= 1)
        {
            return false;
        }

        split = result;
        return true;
    }

    private static bool TryResolveModButtonTexture(string identifier, out MTexture texture)
    {
        texture = null;

        string[] path = identifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (path.Length < 2)
        {
            return false;
        }

        string modName = path[0];
        string propertyName = path[1];

        EverestModule module = Everest.Modules.FirstOrDefault(mod =>
            string.Equals(mod.Metadata?.Name, modName, StringComparison.OrdinalIgnoreCase));
        if (module?.SettingsType == null)
        {
            return false;
        }

        object settingsInstance = typeof(EverestModule)
            .GetField("_Settings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.GetValue(module);
        if (settingsInstance == null)
        {
            return false;
        }

        PropertyInfo property = module.SettingsType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object binding = property?.GetValue(settingsInstance);
        if (binding == null)
        {
            return false;
        }

        object button = binding.GetType().GetProperty("Button", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(binding);
        if (button is not VirtualButton virtualButton)
        {
            return false;
        }

        texture = Input.GuiButton(virtualButton, Input.PrefixMode.Latest, "controls/keyboard/oemquestion");
        return texture != null;
    }
}