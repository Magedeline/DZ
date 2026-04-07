using System;
using MaggyHelper.Cutscenes;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Triggers;

[CustomEntity("MaggyHelper/EventTrigger", "MaggyHelper/CutsceneEventDispatcher")]
[Tracked]
public class IngesteEventTrigger : Trigger
{
    private readonly string eventName;
    private bool hasTriggered;
    private bool pendingCh2CharaIntro;

    public IngesteEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        eventName = data.Attr("event", string.Empty);
        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"Created trigger for event: {eventName}");
    }

    internal static void Load()
    {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), "Hooks registered");
    }

    internal static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), "Hooks unregistered");
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, global::Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
    {
        orig(self, playerIntro, isFromLoader);
        Logger.Log(LogLevel.Debug, nameof(IngesteEventTrigger), "Level loaded, triggers should be active");
    }

    private bool RunOnceAction(Level level, string flag, Func<bool> action)
    {
        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"RunOnceAction called for flag: {flag}");

        if (!string.IsNullOrWhiteSpace(flag) && level.Session.GetFlag(flag))
        {
            Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"Flag {flag} already set, skipping trigger");
            RemoveSelf();
            return true;
        }

        try
        {
            if (action())
            {
                if (!string.IsNullOrWhiteSpace(flag))
                {
                    level.Session.SetFlag(flag);
                }

                RemoveSelf();
                return true;
            }

            Logger.Log(LogLevel.Warn, nameof(IngesteEventTrigger), $"Action for event {eventName} returned false");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, nameof(IngesteEventTrigger), $"Error running event action: {ex}");
        }

        hasTriggered = false;
        return false;
    }

    private bool TriggerOnce(Level level, string flag, Func<CutsceneEntity> cutsceneFactory)
    {
        return RunOnceAction(level, flag, () => {
            CutsceneEntity cutscene = cutsceneFactory();
            if (cutscene == null)
            {
                Logger.Log(LogLevel.Warn, nameof(IngesteEventTrigger), "Cutscene factory returned null");
                return false;
            }

            Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"Adding cutscene {cutscene.GetType().Name} to scene");
            level.Add(cutscene);
            return true;
        });
    }

    public override void OnEnter(global::Celeste.Player player)
    {
        base.OnEnter(player);

        if (hasTriggered)
        {
            Logger.Log(LogLevel.Debug, nameof(IngesteEventTrigger), "Trigger already fired, ignoring");
            return;
        }

        if (string.IsNullOrWhiteSpace(eventName))
        {
            Logger.Log(LogLevel.Warn, nameof(IngesteEventTrigger), "Trigger has no event configured, removing it");
            RemoveSelf();
            return;
        }

        if (Scene is not Level level)
        {
            Logger.Log(LogLevel.Warn, nameof(IngesteEventTrigger), "Scene is not a Level, cannot trigger cutscene");
            return;
        }

        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"Player entered trigger with event: {eventName}");
        hasTriggered = true;
        DispatchEvent(level, player);
    }

    private void DispatchEvent(Level level, global::Celeste.Player player)
    {
        Logger.Log(LogLevel.Info, nameof(IngesteEventTrigger), $"Processing event: {eventName}");
        CutsceneEventDispatcher.TryDispatch(
            level,
            player,
            eventName,
            (flag, factory) => TriggerOnce(level, flag, factory),
            (flag, action) => RunOnceAction(level, flag, action),
            () => {
                pendingCh2CharaIntro = true;
                hasTriggered = false;
            });
    }

    public override void Update()
    {
        base.Update();

        if (!pendingCh2CharaIntro || Scene is not Level level)
        {
            return;
        }

        if (level.Session.GetFlag("evil_chara_intro") || level.Session.GetFlag("ch2_chara_intro_trigger"))
        {
            pendingCh2CharaIntro = false;
            RemoveSelf();
            return;
        }

        CharaChaser charaChaser = Scene.Entities.FindFirst<CharaChaser>();
        if (charaChaser == null)
        {
            return;
        }

        pendingCh2CharaIntro = false;
        TriggerOnce(level, "ch2_chara_intro_trigger", () => new CS02_CharaIntro(charaChaser));
    }
}
