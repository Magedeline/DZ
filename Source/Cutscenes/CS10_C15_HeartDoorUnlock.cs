using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Shared base for the chapter 10-15 small heart door unlock cutscenes.
    /// Each concrete subclass only provides the chapter-specific dialog, flags, and chapter number.
    /// </summary>
    public abstract class HeartDoorUnlockBase : CutsceneEntity
    {
        private readonly string dialogKey;
        private readonly string completionFlag;
        private readonly string doorUnlockedFlag;
        private Player player;

        protected HeartDoorUnlockBase(Player player, string dialogKey, string completionFlag, string doorUnlockedFlag)
            : base(true, false)
        {
            this.player = player;
            this.dialogKey = dialogKey ?? throw new ArgumentNullException(nameof(dialogKey));
            this.completionFlag = completionFlag ?? throw new ArgumentNullException(nameof(completionFlag));
            this.doorUnlockedFlag = doorUnlockedFlag ?? throw new ArgumentNullException(nameof(doorUnlockedFlag));
        }

        public override void OnBegin(Level level)
        {
            if (player == null)
                player = level.Tracker.GetEntity<Player>();

            if (level.Session.GetFlag(completionFlag))
            {
                WasSkipped = true;
                EndCutscene(level);
                return;
            }

            Add(new Coroutine(CutsceneSequence(level)));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                player.StateMachine.Locked = true;
            }

            yield return Textbox.Say(dialogKey);

            level.Session.SetFlag(completionFlag, true);
            level.Session.SetFlag(doorUnlockedFlag, true);

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.StateMachine.Locked = false;
            }
        }
    }

    /// <summary>
    /// Chapter 10 - small heart door unlock after collecting enough mini hearts in the ruins.
    /// </summary>
    [CustomEntity("DesoloZantas/CS10_HeartDoorUnlock")]
    public class CS10_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH10_DOOR_UNLOCKED_AFTER_ENOUGH_MINIHEARTS";
        private const string FLAG_COMPLETE = "ch10_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH10";

        public CS10_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS10_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }

    /// <summary>
    /// Chapter 11 - small heart door unlock after collecting enough mini hearts in Snowdin.
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_HeartDoorUnlock")]
    public class CS11_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH11_COLLECTING_MINIHEART_ENOUGH";
        private const string FLAG_COMPLETE = "ch11_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH11";

        public CS11_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS11_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }

    /// <summary>
    /// Chapter 12 - small heart door unlock after collecting enough mini hearts in Wateredge.
    /// </summary>
    [CustomEntity("DesoloZantas/CS12_HeartDoorUnlock")]
    public class CS12_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH12_COLLECTING_MINIHEART_ENOUGH";
        private const string FLAG_COMPLETE = "ch12_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH12";

        public CS12_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS12_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }

    /// <summary>
    /// Chapter 13 - small heart door unlock after collecting the heart gem in Hotland.
    /// </summary>
    [CustomEntity("DesoloZantas/CS13_HeartDoorUnlock")]
    public class CS13_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH13_HEARTGEM_POEM";
        private const string FLAG_COMPLETE = "ch13_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH13";

        public CS13_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS13_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }

    /// <summary>
    /// Chapter 14 - small heart door unlock after collecting the heart gem in the Digital Dimension.
    /// </summary>
    [CustomEntity("DesoloZantas/CS14_HeartDoorUnlock")]
    public class CS14_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH14_HEARTGEM_POEM";
        private const string FLAG_COMPLETE = "ch14_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH14";

        public CS14_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS14_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }

    /// <summary>
    /// Chapter 15 - small heart door unlock after collecting the heart gem at the Titan summit.
    /// </summary>
    [CustomEntity("DesoloZantas/CS15_HeartDoorUnlock")]
    public class CS15_HeartDoorUnlock : HeartDoorUnlockBase
    {
        private const string DIALOG_KEY = "DZ_CH15_HEARTGEM_POEM";
        private const string FLAG_COMPLETE = "ch15_heartdoor_unlock_complete";
        private const string FLAG_DOOR_UNLOCKED = "small_heart_door_unlockedDZ_CH15";

        public CS15_HeartDoorUnlock(Player player) : base(player, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
        public CS15_HeartDoorUnlock(EntityData data, Vector2 offset) : base(null, DIALOG_KEY, FLAG_COMPLETE, FLAG_DOOR_UNLOCKED) { }
    }
}
