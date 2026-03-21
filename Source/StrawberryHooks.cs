using System.Reflection;
using MaggyHelper.Entities;

namespace MaggyHelper
{
    /// <summary>
    /// This class sets up some hooks that will be useful for rainbow berries.
    /// They mod the following things:
    /// - strawberry sprite for rainbows
    /// - death sounds for rainbows
    /// - collect sounds for rainbows
    /// </summary>
    static class StrawberryHooks
    {
        internal static void Load()
        {
            On.Celeste.Strawberry.Added += modStrawberrySprite;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Level.End += onLevelEnd;
            Everest.Events.Level.OnCreatePauseMenuButtons += onCreatePauseMenuButtons;
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, global::Celeste.Player self, Scene scene)
        {
            orig(self, scene);
            var deltaBerry = scene.Tracker.GetEntity<PopstarBerry>();
            var follower = deltaBerry?.Get<Follower>();
            if (deltaBerry != null && follower != null)
                follower.Leader = self.Leader;
        }

        internal static void Unload()
        {
            On.Celeste.Strawberry.Added -= modStrawberrySprite;
            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollectRoutine;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Level.End -= onLevelEnd;
            Everest.Events.Level.OnCreatePauseMenuButtons -= onCreatePauseMenuButtons;
        }

        private static void onCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
        {
            PopstarBerry berry = level.Tracker.GetEntity<PopstarBerry>();
            var follower = berry?.Get<Follower>();
            if (berry != null && follower != null && follower.HasLeader && !minimal)
            {
                TextMenu.Button item = new TextMenu.Button(Dialog.Clean("restartpopstarberry"))
                {
                    OnPressed = () =>
                    {
                        level.Paused = false;
                        level.PauseMainMenuOpen = false;
                        menu.RemoveSelf();
                        berry.RemoveSelf();
                    }
                };
                menu.Add(item);
            }
        }

        private static void modStrawberrySprite(On.Celeste.Strawberry.orig_Added orig, CelesteStrawberry self, Scene scene)
        {
            orig(self, scene);
            PopstarBerry deltaBerry = scene.Tracker.GetEntity<PopstarBerry>();
            if (deltaBerry != null)
            {
                var spriteField = typeof(CelesteStrawberry).GetField("Sprite", BindingFlags.NonPublic | BindingFlags.Instance);
                if (spriteField != null)
                {
                    if (SaveDataExtensions.IsDeltaBerryCollected(deltaBerry.ToString()))
                        spriteField.SetValue(self, GFX.SpriteBank.Create("popstarberry"));
                    else
                        spriteField.SetValue(self, GFX.SpriteBank.Create("popstarberry"));
                }
            }
        }

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, CelesteStrawberry self, int collectIndex)
        {
            Scene scene = self.Scene;
            IEnumerator origEnum = orig(self, collectIndex);
            while (origEnum.MoveNext())
            {
                yield return origEnum.Current;
            }
            PopstarBerry deltaBerry = null;
            if (deltaBerry != null)
            {
                SaveDataExtensions.MarkDeltaBerryAsCollected(deltaBerry.ToString());
                StrawberryPoints points = scene.Entities.ToAdd.OfType<StrawberryPoints>().FirstOrDefault();
                if (points != null) scene.Entities.ToAdd.Remove(points);
                scene.Add(new CSGEN_StrawberrySeeds(self));
            }
        }

        private static void onLevelEnd(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
        }
    }

// Stub for SaveDataExtensions
    public static class SaveDataExtensions
    {
        public static bool IsDeltaBerryCollected(string id)
        {
            // TODO: Implement actual logic
            return false;
        }
        public static void MarkDeltaBerryAsCollected(string id)
        {
            // TODO: Implement actual logic
        }
    }
}



