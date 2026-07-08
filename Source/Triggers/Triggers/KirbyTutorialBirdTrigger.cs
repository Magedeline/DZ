namespace Celeste.Triggers;

[CustomEntity("DZ/KirbyTutorialBirdTrigger")]
[Tracked]
[HotReloadable]
public class KirbyTutorialBirdTrigger : Trigger
{
    public string BirdID { get; }
    public int TutorialIndex { get; }
    public string ConditionFunction { get; }

    private readonly MethodInfo conditionMethod;
    private bool triggered;

    public KirbyTutorialBirdTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        BirdID = data.Attr("birdId");
        TutorialIndex = data.Int("tutorialIndex", 0);
        ConditionFunction = data.Attr("conditionFunction");
        conditionMethod = ResolveConditionMethod(ConditionFunction);

        AddTag(Tags.FrozenUpdate);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        TryTrigger();
    }

    public override void OnStay(Player player)
    {
        base.OnStay(player);

        if (!triggered)
        {
            TryTrigger();
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        triggered = false;
    }

    private void TryTrigger()
    {
        Entity bird = Scene?.Entities
            .OfType<Entity>()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.GetType().Name, "KirbyTutorialBird", StringComparison.Ordinal)
                && string.Equals(candidate.GetType().GetProperty("BirdID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(candidate) as string, BirdID, StringComparison.Ordinal));
        if (bird == null)
        {
            return;
        }

        bool conditionSatisfied = conditionMethod == null || InvokeCondition(conditionMethod, SceneAs<Level>());
        if (!conditionSatisfied)
        {
            return;
        }

        if (TutorialIndex >= 0)
        {
            bird.GetType().GetMethod("TriggerShowTutorial", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(bird, new object[] { TutorialIndex });
        }
        else
        {
            bird.GetType().GetMethod("TriggerCloseTutorial", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(bird, Array.Empty<object>());
        }

        triggered = true;
    }

    private static bool InvokeCondition(MethodInfo method, Level level)
    {
        if (method == null || level == null)
        {
            return false;
        }

        try
        {
            return method.Invoke(null, new object[] { level }) is true;
        }
        catch
        {
            return false;
        }
    }

    private static MethodInfo ResolveConditionMethod(string conditionFunction)
    {
        if (string.IsNullOrWhiteSpace(conditionFunction) || !conditionFunction.StartsWith("mod:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string identifier = conditionFunction[4..].Trim();
        if (!TryParseIdentifier(identifier, out string modName, out string typeName, out string methodName))
        {
            return null;
        }

        EverestModule module = Everest.Modules.FirstOrDefault(mod =>
            string.Equals(mod.Metadata?.Name, modName, StringComparison.OrdinalIgnoreCase));
        if (module == null)
        {
            return null;
        }

        Assembly assembly = module.GetType().Assembly;
        Type type = assembly.GetType(typeName)
            ?? assembly.GetTypes().FirstOrDefault(candidate =>
                string.Equals(candidate.FullName, typeName, StringComparison.Ordinal)
                || string.Equals(candidate.Name, typeName, StringComparison.Ordinal)
                || candidate.FullName?.EndsWith('.' + typeName, StringComparison.Ordinal) == true);

        MethodInfo method = type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Level) }, null);
        return method?.ReturnType == typeof(bool) ? method : null;
    }

    private static bool TryParseIdentifier(string identifier, out string modName, out string typeName, out string methodName)
    {
        modName = null;
        typeName = null;
        methodName = null;

        if (identifier.Contains('/'))
        {
            string[] path = identifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length < 3)
            {
                return false;
            }

            modName = path[0];
            methodName = path[^1];
            typeName = string.Join('.', path.Skip(1).Take(path.Length - 2));
            return true;
        }

        string[] tokens = identifier.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 3)
        {
            return false;
        }

        modName = tokens[0];
        methodName = tokens[^1];
        typeName = string.Join('.', tokens.Skip(1).Take(tokens.Length - 2));
        return true;
    }
}