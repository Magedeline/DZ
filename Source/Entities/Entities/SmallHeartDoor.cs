using System.Collections;
using Celeste.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Trigger entity that gates progress behind small/mini heart gem collection.
    /// Place this in front of a door; it will block the player with a dialog if they
    /// have not collected enough hearts for the configured chapter, and will trigger
    /// the chapter's unlock cutscene once the requirement is met.
    /// </summary>
    [CustomEntity("DZ/SmallHeartDoor")]
    public class SmallHeartDoor : Entity
    {
        private const string COLLECTED_COUNTER_KEY = "ch{0}_mini_hearts_collected";
        private const string UNLOCKED_FLAG_KEY = "small_heart_door_unlocked_ch{0}";

        private readonly int chapterNumber;
        private readonly int requiredHearts;
        private readonly string notEnoughDialogKey;
        private readonly string unlockCutsceneId;
        private bool dialogRunning;
        private bool unlocked;

        public SmallHeartDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            chapterNumber = data.Int("chapter", 10);
            requiredHearts = data.Int("requires", 3);
            notEnoughDialogKey = data.Attr("notEnoughDialog", "");
            unlockCutsceneId = data.Attr("unlockCutscene", "");
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            if (level != null && level.Session.GetFlag(GetUnlockedFlag(chapterNumber)))
            {
                unlocked = true;
                Collidable = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (unlocked || dialogRunning)
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || !CollideCheck(player))
                return;

            Level level = Scene as Level;
            if (level == null)
                return;

            if (GetCollectedCount(level.Session, chapterNumber) >= requiredHearts)
            {
                Unlock(level, player);
            }
            else if (!string.IsNullOrEmpty(notEnoughDialogKey))
            {
                dialogRunning = true;
                Add(new Coroutine(ShowNotEnoughDialog(level)));
            }
        }

        private IEnumerator ShowNotEnoughDialog(Level level)
        {
            yield return Textbox.Say(notEnoughDialogKey);
            dialogRunning = false;
        }

        private void Unlock(Level level, Player player)
        {
            if (unlocked)
                return;

            unlocked = true;
            Collidable = false;
            level.Session.SetFlag(GetUnlockedFlag(chapterNumber), true);

            if (!string.IsNullOrEmpty(unlockCutsceneId) && player != null)
            {
                CutsceneEntity cutscene = CreateCutscene(unlockCutsceneId, player);
                if (cutscene != null)
                {
                    level.Add(cutscene);
                }
            }
        }

        private static CutsceneEntity CreateCutscene(string id, Player player)
        {
            return id switch
            {
                "CS10" => new CS10_HeartDoorUnlock(player),
                "CS11" => new CS11_HeartDoorUnlock(player),
                "CS12" => new CS12_HeartDoorUnlock(player),
                "CS13" => new CS13_HeartDoorUnlock(player),
                "CS14" => new CS14_HeartDoorUnlock(player),
                "CS15" => new CS15_HeartDoorUnlock(player),
                _ => null
            };
        }

        public static string GetCounterKey(int chapter) => string.Format(COLLECTED_COUNTER_KEY, chapter);
        public static string GetUnlockedFlag(int chapter) => string.Format(UNLOCKED_FLAG_KEY, chapter);

        public static int GetCollectedCount(Session session, int chapter)
        {
            return session?.GetCounter(GetCounterKey(chapter)) ?? 0;
        }

        public static void CollectMiniHeart(Session session, int chapter)
        {
            if (session == null)
                return;
            int current = session.GetCounter(GetCounterKey(chapter));
            session.SetCounter(GetCounterKey(chapter), current + 1);
        }
    }
}
