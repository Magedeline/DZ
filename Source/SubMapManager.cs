// Add this namespace import

// Add this namespace import

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Manages submap transitions and progression for chapters 10-14
    /// </summary>
    [Tracked(false)]
    public class SubMapManager : Entity
    {
        public static SubMapManager Instance { get; private set; }
        
        private readonly Dictionary<string, SubMapData> submapRegistry;
        private readonly Dictionary<int, List<int>> chapterSubmaps;
        
        public SubMapManager() : base(Vector2.Zero)
        {
            Instance = this;
            submapRegistry = new Dictionary<string, SubMapData>();
            chapterSubmaps = new Dictionary<int, List<int>>();
            
            Tag = Tags.Global | Tags.Persistent;
            Depth = -1000000;
            
            initializeSubmaps();
        }

        private void initializeSubmaps()
        {
            // Register submaps for chapters 10-14
            for (int chapter = 10; chapter <= 14; chapter++)
            {
                chapterSubmaps[chapter] = new List<int>();
                
                for (int submap = 1; submap <= 6; submap++)
                {
                    chapterSubmaps[chapter].Add(submap);
                    
                    string submapId = getSubmapId(chapter, submap);
                    submapRegistry[submapId] = new SubMapData
                    {
                        ChapterNumber = chapter,
                        SubmapNumber = submap,
                        MapPath = $"Maps/Maggy/Submaps/Ch{chapter}_Submap{submap}",
                        RequiredHeartGems = getRequiredHeartGems(chapter, submap),
                        UnlockFlag = $"submap_unlock_ch{chapter}_{submap}",
                        CompletionFlag = $"submap_complete_ch{chapter}_{submap}",
                        IsUnlocked = false
                    };
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Update unlock status based on current save data
            Level level = SceneAs<Level>();
            if (level != null)
            {
                updateSubmapUnlockStatus(level);
            }
        }

        private void updateSubmapUnlockStatus(Level level)
        {
            foreach (var kvp in submapRegistry)
            {
                var submap = kvp.Value;
                
                // Check if submap should be unlocked
                bool shouldUnlock = checkSubmapUnlockConditions(level, submap);
                
                if (shouldUnlock && !submap.IsUnlocked)
                {
                    unlockSubmap(level, submap);
                }
                
                submap.IsUnlocked = level.Session.GetFlag(submap.UnlockFlag);
                submap.IsCompleted = level.Session.GetFlag(submap.CompletionFlag);
            }
        }

        private bool checkSubmapUnlockConditions(Level level, SubMapData submap)
        {
            // First submap (1) of each chapter unlocks automatically when entering lobby
            if (submap.SubmapNumber == 1)
            {
                string chapterCompleteFlag = $"ch{submap.ChapterNumber}_completed";
                return level.Session.GetFlag(chapterCompleteFlag);
            }
            
            // Subsequent submaps unlock when previous submap is completed
            string previousSubmapFlag = $"submap_complete_ch{submap.ChapterNumber}_{submap.SubmapNumber - 1}";
            return level.Session.GetFlag(previousSubmapFlag);
        }

        private void unlockSubmap(Level level, SubMapData submap)
        {
            level.Session.SetFlag(submap.UnlockFlag, true);
            submap.IsUnlocked = true;
            
            // Play unlock sound and show notification
            Audio.Play("event:/game/general/crystalheart_blue_get");
            level.Add(new MiniTextbox($"SUBMAP_UNLOCKED_CH{submap.ChapterNumber}_{submap.SubmapNumber}"));
        }

        public bool CanEnterSubmap(int chapter, int submap)
        {
            string submapId = getSubmapId(chapter, submap);
            if (submapRegistry.TryGetValue(submapId, out SubMapData data))
            {
                return data.IsUnlocked;
            }
            return false;
        }

        public void EnterSubmap(Level level, global::Celeste.Player player, int chapter, int submap)
        {
            string submapId = getSubmapId(chapter, submap);
            if (!submapRegistry.TryGetValue(submapId, out SubMapData data))
            {
                Engine.Commands.Log($"Submap {submapId} not found!");
                return;
            }

            if (!data.IsUnlocked)
            {
                Engine.Commands.Log($"Submap {submapId} is locked!");
                return;
            }

            // Store return location
            level.Session.SetFlag("submap_return_chapter", true);

            // Store the chapter number in a counter instead
            level.Session.SetCounter("submap_return_chapter", chapter);
            level.Session.SetCounter("submap_return_x", (int)player.X); // Fixed: Changed SetFlag to SetCounter
            level.Session.SetCounter("submap_return_y", (int)player.Y); // Fixed: Changed SetFlag to SetCounter

            // Transition to submap
            level.OnEndOfFrame += () =>
            {
                Engine.Scene = new LevelLoader(new Session(level.Session.Area)
                {
                    Level = $"submap_ch{chapter}_{submap}"
                });
            };
        }

        public void ReturnToLobby(Level level, global::Celeste.Player player)
        {
            // Get return location
            int returnChapter = level.Session.GetFlag("submap_return_chapter") ?
                level.Session.GetCounter("submap_return_chapter") : 10;
            int returnX = level.Session.GetCounter("submap_return_x");
            int returnY = level.Session.GetCounter("submap_return_y");

            // Clear return flags
            level.Session.SetFlag("submap_return_chapter", false);
            level.Session.SetFlag("submap_return_x", false); // Fixed: Changed SetFlag to SetCounter
            level.Session.SetFlag("submap_return_y", false); // Fixed: Changed SetFlag to SetCounter

            // Return to lobby
            level.OnEndOfFrame += () =>
            {
                var session = new Session(level.Session.Area)
                {
                    Level = $"lobby_ch{returnChapter}",
                    RespawnPoint = new Vector2(returnX, returnY)
                };
                Engine.Scene = new LevelLoader(session);
            };
        }

        public List<SubMapData> GetChapterSubmaps(int chapter)
        {
            var result = new List<SubMapData>();
            
            if (chapterSubmaps.TryGetValue(chapter, out List<int> submaps))
            {
                foreach (int submap in submaps)
                {
                    string submapId = getSubmapId(chapter, submap);
                    if (submapRegistry.TryGetValue(submapId, out SubMapData data))
                    {
                        result.Add(data);
                    }
                }
            }
            
            return result;
        }

        public int GetCollectedHeartGems(Level level, int chapter, int submap)
        {
            int count = 0;
            string prefix = SmallHeartGem.COLLECTED_FLAG_PREFIX + $"ch{chapter}_submap{submap}";
            
            // Count collected gems (this is a simplified version)
            for (int i = 1; i <= 10; i++) // Max 10 gems per submap
            {
                if (level.Session.GetFlag(prefix + $"_gem{i}"))
                    count++;
            }
            
            return count;
        }

        private string getSubmapId(int chapter, int submap)
        {
            return $"ch{chapter}_submap{submap}";
        }

        private int getRequiredHeartGems(int chapter, int submap)
        {
            // Define required heart gems for progression
            string key = $"{chapter}_{submap}";
            switch (key)
            {
                case "10_3": return 0;  // First submap requires no gems
                case "10_4": return 3;  // Need all gems from submap 3
                case "10_5": return 7;  // Need gems from submaps 3+4
                case "10_6": return 12; // Need gems from submaps 3+4+5
                case "11_3": return 0;
                case "11_4": return 3;
                case "11_5": return 7;
                case "11_6": return 12;
                case "12_3": return 0;
                case "12_4": return 4;
                case "12_5": return 9;
                case "12_6": return 15;
                case "13_3": return 0;
                case "13_4": return 5;
                case "13_5": return 11;
                case "13_6": return 18;
                case "14_3": return 0;
                case "14_4": return 6;
                case "14_5": return 13;
                case "14_6": return 21;
                default: return 0;
            }
        }
    }

    public class SubMapData
    {
        public int ChapterNumber { get; set; }
        public int SubmapNumber { get; set; }
        public string MapPath { get; set; }
        public int RequiredHeartGems { get; set; }
        public string UnlockFlag { get; set; }
        public string CompletionFlag { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
    }
}



