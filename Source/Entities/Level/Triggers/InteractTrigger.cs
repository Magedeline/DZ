using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that allows player interaction (talk) to trigger events.
/// Note: This is not a trigger, it extends Entity directly.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class InteractTrigger : Entity
{
    public const string FlagPrefix = "it_";
    
    public List<string> Events;
    private int eventIndex;
    private float timeout;
    private bool used;
    private Vector2 talkerDrawAt;

    private float _width;
    private float _height;

    public InteractTrigger(Vector2 position, float width, float height, string event1, Vector2? talkerOffset = null, params string[] additionalEvents) : base("InteractTrigger")
    {
        Position = position;
        _width = width;
        _height = height;
        
        Events = new List<string> { event1 };
        Events.AddRange(additionalEvents.Where(e => !string.IsNullOrEmpty(e)));
        
        talkerDrawAt = talkerOffset ?? new Vector2(_width / 2f, 0f);
        
        // TODO: Add TalkComponent
        // Talker = new TalkComponent(new Rectangle(0, 0, (int)width, (int)height), talkerDrawAt, OnTalk);
        // Talker.PlayerMustBeFacing = false;
        // AddComponent(Talker);
    }

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        
        // TODO: Check flags and advance event index
        // for (int i = 0; i < Events.Count; i++)
        // {
        //     if (Session.GetFlag(FlagPrefix + Events[i]))
        //         eventIndex++;
        // }
        
        if (eventIndex >= Events.Count)
        {
            Destroy();
        }
        else
        {
            // Special handling for Theo phone
            // if (Events[eventIndex] == "ch5_theo_phone")
            //     Scene.Add(new TheoPhone(Position + new Vector2(_width / 2 - 8, _height - 1)));
        }
    }

    public void OnTalk(PlayerController player)
    {
        if (used)
            return;
        
        bool flag = true;
        
        // TODO: Handle specific events
        switch (Events[eventIndex])
        {
            case "ch2_poem":
                // Scene.Add(new CS02_Journal(player));
                flag = false;
                break;
            case "ch3_diary":
                // Scene.Add(new CS03_Diary(player));
                flag = false;
                break;
            case "ch3_guestbook":
                // Scene.Add(new CS03_Guestbook(player));
                flag = false;
                break;
            case "ch3_memo":
                // Scene.Add(new CS03_Memo(player));
                flag = false;
                break;
            case "ch5_mirror_reflection":
                // Scene.Add(new CS05_Reflection1(player));
                break;
            case "ch5_see_theo":
                // Scene.Add(new CS05_SeeTheo(player, 0));
                break;
            case "ch5_see_theo_b":
                // Scene.Add(new CS05_SeeTheo(player, 1));
                break;
            case "ch5_theo_phone":
                // Scene.Add(new CS05_TheoPhone(player, Position.X + _width / 2));
                break;
        }
        
        if (!flag)
            return;
        
        // TODO: Set flag
        // Session.SetFlag(FlagPrefix + Events[eventIndex]);
        eventIndex++;
        
        if (eventIndex >= Events.Count)
        {
            used = true;
            timeout = 0.25f;
        }
    }

    public override void Update()
    {
        if (used)
        {
            timeout -= Time.DeltaTime;
            if (timeout <= 0f)
                Destroy();
        }
        else
        {
            // Advance past already-triggered events
            // while (eventIndex < Events.Count && Session.GetFlag(FlagPrefix + Events[eventIndex]))
            //     eventIndex++;
            
            if (eventIndex >= Events.Count)
                Destroy();
        }
        
        base.Update();
    }
}
