using System;
using MaggyHelper.Cutscenes;
using MaggyHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities;

[Tracked]
[CustomEntity(ids: "MaggyHelper/CutsceneEventEntity,DesoloZantas/CutsceneEventEntity")]
public class CutsceneEventEntity : Entity
{
    private enum ActivationMode
    {
        Interact,
        Touch,
        RoomEnter
    }

    private const string DefaultTexturePath = "objects/Ingeste/sampleEntity/idle00";

    private readonly string eventId;
    private readonly string requireFlag;
    private readonly string completionFlag;
    private readonly bool removeAfterTrigger;
    private readonly bool showSprite;
    private readonly string texturePath;
    private readonly ActivationMode activationMode;

    private Level level;
    private TalkComponent talker;
    private PlayerCollider playerCollider;
    private bool pendingRoomEnter;
    private bool completed;
    private float cooldownTimer;

    public CutsceneEventEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        eventId = data.Attr("eventId", string.Empty);
        requireFlag = data.Attr("requireFlag", string.Empty);
        completionFlag = data.Attr("completionFlag", string.Empty);
        removeAfterTrigger = data.Bool("removeAfterTrigger", true);
        showSprite = data.Bool("showSprite", false);
        texturePath = data.Attr("texturePath", DefaultTexturePath);

        if (!Enum.TryParse(data.Attr("activationMode", "interact"), true, out activationMode))
        {
            activationMode = ActivationMode.Interact;
        }

        Collider = new Hitbox(16f, 16f, -8f, -16f);
        Depth = 0;

        if (showSprite)
        {
            AddMarkerSprite();
        }

        if (activationMode == ActivationMode.Interact)
        {
            Add(talker = new TalkComponent(new Rectangle(-12, -24, 24, 32), new Vector2(0f, -24f), OnTalk));
            talker.PlayerMustBeFacing = false;
        }
        else if (activationMode == ActivationMode.Touch)
        {
            Add(playerCollider = new PlayerCollider(OnPlayerTouch));
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;

        if (string.IsNullOrWhiteSpace(eventId))
        {
            Logger.Log(LogLevel.Warn, nameof(CutsceneEventEntity), $"{GetType().Name} at {Position} has no eventId configured");
            SetInactive();
            return;
        }

        if (HasCompletionFlag())
        {
            if (removeAfterTrigger)
            {
                RemoveSelf();
            }
            else
            {
                SetInactive();
            }
        }
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (activationMode == ActivationMode.RoomEnter && !completed)
        {
            pendingRoomEnter = true;
        }
    }

    public override void Update()
    {
        base.Update();

        if (cooldownTimer > 0f)
        {
            cooldownTimer = Calc.Approach(cooldownTimer, 0f, Engine.DeltaTime);
        }

        if (!pendingRoomEnter || completed || level == null)
        {
            return;
        }

        global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
        if (player == null || !RequirementsMet())
        {
            return;
        }

        pendingRoomEnter = false;
        TryDispatch(player, allowRoomEnterRetry: true);
    }

    private void AddMarkerSprite()
    {
        string path = GFX.Game.Has(texturePath) ? texturePath : DefaultTexturePath;
        MTexture markerTexture = GFX.Game[path];
        Image marker = new Image(markerTexture);
        marker.Origin = new Vector2(markerTexture.Width * 0.5f, markerTexture.Height);
        Add(marker);
    }

    private void OnTalk(global::Celeste.Player player)
    {
        TryDispatch(player, allowRoomEnterRetry: false);
    }

    private void OnPlayerTouch(global::Celeste.Player player)
    {
        TryDispatch(player, allowRoomEnterRetry: false);
    }

    private bool TryDispatch(global::Celeste.Player player, bool allowRoomEnterRetry)
    {
        if (completed || level == null || cooldownTimer > 0f || player == null)
        {
            return false;
        }

        if (!RequirementsMet())
        {
            if (allowRoomEnterRetry)
            {
                pendingRoomEnter = true;
            }

            return false;
        }

        bool dispatched = CutsceneEventDispatcher.TryDispatch(
            level,
            player,
            eventId,
            (flag, factory) => TriggerCutscene(flag, factory),
            (flag, action) => RunAction(flag, action),
            allowRoomEnterRetry ? () => pendingRoomEnter = true : null);

        if (!dispatched)
        {
            return false;
        }

        cooldownTimer = 0.25f;

        if (!string.IsNullOrWhiteSpace(completionFlag))
        {
            level.Session.SetFlag(completionFlag, true);
        }

        if (removeAfterTrigger)
        {
            completed = true;
            RemoveSelf();
        }
        else if (!string.IsNullOrWhiteSpace(completionFlag))
        {
            SetInactive();
        }

        return true;
    }

    private bool TriggerCutscene(string flag, Func<CutsceneEntity> cutsceneFactory)
    {
        return RunAction(flag, () => {
            CutsceneEntity cutscene = cutsceneFactory();
            if (cutscene == null)
            {
                return false;
            }

            level.Add(cutscene);
            return true;
        });
    }

    private bool RunAction(string flag, Func<bool> action)
    {
        if (!string.IsNullOrWhiteSpace(flag) && level.Session.GetFlag(flag))
        {
            return true;
        }

        try
        {
            if (!action())
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(flag))
            {
                level.Session.SetFlag(flag, true);
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, nameof(CutsceneEventEntity), $"Failed to dispatch event '{eventId}': {ex}");
            return false;
        }
    }

    private bool RequirementsMet()
    {
        return string.IsNullOrWhiteSpace(requireFlag) || level.Session.GetFlag(requireFlag);
    }

    private bool HasCompletionFlag()
    {
        return !string.IsNullOrWhiteSpace(completionFlag) && level != null && level.Session.GetFlag(completionFlag);
    }

    private void SetInactive()
    {
        completed = true;
        pendingRoomEnter = false;
        Collidable = false;

        if (talker != null)
        {
            talker.Enabled = false;
        }

        if (playerCollider != null)
        {
            playerCollider.Active = false;
        }

        Active = false;
    }
}
