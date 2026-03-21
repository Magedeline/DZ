using MaggyHelper.Cutscenes;
using MaggyHelper.Entities;

namespace MaggyHelper.Triggers
{
    [CustomEntity(ids: "MaggyHelper/TowerFountainTrigger")]
    [HotReloadable]
    public class TowerFountainTrigger : Trigger
    {
        #region Fields
        private bool activated = false;
        private bool cutsceneTriggered = false;
        private Vector2 fountainSpawnPosition;
        private Vector2 towerSpawnPosition;
        private string activationFlag;
        private bool onlyOnce;
        private bool requiresChapter12Completion;
        private EntityData triggerData;
        private Vector2 triggerOffset;
        #endregion

        public TowerFountainTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerData = data;
            triggerOffset = offset;
            
            fountainSpawnPosition = data.NodesOffset(offset)[0]; // First node is fountain position
            towerSpawnPosition = data.NodesOffset(offset).Length > 1 ?
                                data.NodesOffset(offset)[1] : // Second node is tower position
                                fountainSpawnPosition + new Vector2(0f, -100f); // Default offset

            activationFlag = data.Attr(nameof(activationFlag), "ch12_completion");
            onlyOnce = data.Bool(nameof(onlyOnce), true);
            requiresChapter12Completion = data.Bool("requiresChapter12", true);
            
            // Set up the trigger area
            Tag = Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            
            Level level = scene as Level;
            if (level != null)
            {
                // Check if cutscene should be automatically triggered based on flags
                if (requiresChapter12Completion && !level.Session.GetFlag(activationFlag))
                {
                    // Don't activate until required conditions are met
                    return;
                }
                
                // Check if fountain and tower already exist
                bool fountainExists = level.Session.GetFlag("ch12_fountain_created");
                bool towerExists = level.Session.GetFlag("ch12_tower_activated");
                
                if (fountainExists && towerExists)
                {
                    // Spawn existing fountain and tower
                    spawnExistingStructures(level);
                }
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            Level level = SceneAs<Level>();
            if (level == null || cutsceneTriggered) return;
            
            // Check activation conditions
            if (requiresChapter12Completion && !level.Session.GetFlag(activationFlag))
            {
                return; // Don't trigger yet
            }
            
            bool fountainExists = level.Session.GetFlag("ch12_fountain_created");
            bool towerExists = level.Session.GetFlag("ch12_tower_activated");
            
            if (!fountainExists && !towerExists)
            {
                // First time - trigger the cutscene
                triggerFountainCutscene(player, level);
            }
            else if (fountainExists && towerExists)
            {
                // Structures exist, enable climbing
                enableTowerClimbing(player, level);
            }
        }

        private void triggerFountainCutscene(global::Celeste.Player player, Level level)
        {
            if (cutsceneTriggered && onlyOnce) return;
            
            cutsceneTriggered = true;
            activated = true;
            
            // Create and start the fountain creation cutscene
            var cutsceneData = new EntityData
            {
                Position = fountainSpawnPosition,
                Values = new()
                {
                    ["towerPosition"] = towerSpawnPosition,
                    [nameof(activationFlag)] = activationFlag
                }
            };
            
            var cutscene = new Cs12TowerFountain(cutsceneData, Vector2.Zero, player);
            level.Add(cutscene);
            
            // Play dramatic music
            level.Session.Audio.Music.Event = "event:/game/general/crystalheart_blue_get";
            level.Session.Audio.Apply();
        }

        private void enableTowerClimbing(global::Celeste.Player player, Level level)
        {
            // Find existing tower
            TitanTower3D tower = level.Tracker.GetEntity<TitanTower3D>();
            
            if (tower == null)
            {
                // Create tower if it doesn't exist (shouldn't happen)
                tower = new TitanTower3D(towerSpawnPosition);
                tower.SetGrowthProgress(1f);
                tower.EnableClimbing(true);
                level.Add(tower);
            }
            else
            {
                // Enable climbing on existing tower
                tower.EnableClimbing(true);
            }
            
            // Modify camera to accommodate tower climbing
            level.CameraLockMode = Level.CameraLockModes.None;
        }

        private void spawnExistingStructures(Level level)
        {
            // Spawn fountain if it doesn't exist
            if (level.Tracker.GetEntity<MagicFountain>() == null)
            {
                var fountain = new MagicFountain(fountainSpawnPosition);
                fountain.Activate(); // Already active
                level.Add(fountain);
            }
            
            // Spawn tower if it doesn't exist
            if (level.Tracker.GetEntity<TitanTower3D>() == null)
            {
                var tower = new TitanTower3D(towerSpawnPosition);
                tower.SetGrowthProgress(1f); // Fully grown
                tower.EnableClimbing(true);
                level.Add(tower);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            // Optional: Handle leaving the trigger area
            Level level = SceneAs<Level>();
            if (level != null)
            {
                TitanTower3D tower = level.Tracker.GetEntity<TitanTower3D>();
                if (tower != null)
                {
                    float distanceToTower = Vector2.Distance(player.Position, tower.Position);
                    
                    // If player is far from tower, disable special climbing mechanics
                    if (distanceToTower > 400f)
                    {
                        tower.EnableClimbing(false);
                        
                        // Restore normal camera mode
                        level.CameraLockMode = Level.CameraLockModes.FinalBoss;
                        
                        // Return player to normal state if they were climbing
                        if (player.StateMachine.State == global::Celeste.Player.StClimb)
                        {
                            player.StateMachine.State = global::Celeste.Player.StNormal;
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Continuously check for tower climbing opportunities
            Level level = SceneAs<Level>();
            if (level != null && activated)
            {
                global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
                TitanTower3D tower = level.Tracker.GetEntity<TitanTower3D>();
                
                if (player != null && tower != null && tower.IsClimbingEnabled)
                {
                    float distanceToTower = Vector2.Distance(player.Position, tower.Position);
                    
                    // Auto-enable climbing when near tower
                    if (distanceToTower < 150f && player.StateMachine.State == global::Celeste.Player.StNormal)
                    {
                        // Check if player is touching the tower
                        if (distanceToTower < 80f)
                        {
                            player.StateMachine.State = global::Celeste.Player.StClimb;
                        }
                    }
                    else if (distanceToTower > 200f && player.StateMachine.State == global::Celeste.Player.StClimb)
                    {
                        // Return to normal when away from tower
                        player.StateMachine.State = global::Celeste.Player.StNormal;
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            
            // Debug rendering (only in debug mode)
            if (Engine.Commands.Open)
            {
                // Draw trigger area
                Draw.HollowRect(Collider, Color.Yellow);
                
                // Draw fountain and tower spawn positions
                Draw.Point(fountainSpawnPosition, Color.Blue);
                Draw.Point(towerSpawnPosition, Color.Red);
                
                // Draw connection line
                Draw.Line(fountainSpawnPosition, towerSpawnPosition, Color.Green);
            }
        }
    }
}



