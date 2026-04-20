using Celeste.Cutscenes;

namespace Celeste.Triggers
{
    [CustomEntity(ids: "MaggyHelper/Tower3DTrigger")]
    [HotReloadable]
    public class Tower3DTrigger : Trigger
    {
        private Tower3DTrigger tower;
        private Level level;
        private bool cutscenePlayed = false;
        private EntityData triggerData;
        private Vector2 triggerOffset;
        
        public Tower3DTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            triggerData = data;
            triggerOffset = offset;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            level = SceneAs<Level>();
            if (level != null)
            {
                // Check if this is the first time entering (Chapter 12 ending)
                bool fountainExists = level.Session.GetFlag("ch12_fountain_created");
                bool towerExists = level.Session.GetFlag("ch12_tower_activated");
                
                if (!fountainExists && !towerExists && !cutscenePlayed)
                {
                    // Trigger the fountain creation cutscene
                    StartFountainCutscene(player);
                }
                else if (fountainExists && towerExists)
                {
                    // Tower already exists, just activate climbing mode
                    ActivateExistingTower(player);
                }
            }
        }
        
        private void StartFountainCutscene(global::Celeste.Player player)
        {
            cutscenePlayed = true;
            
            // Create and start the fountain cutscene
            var cutscene = new Cs12TowerFountain(triggerData, triggerOffset, player);
            level.Add(cutscene);
            
            // The cutscene will handle creating the tower and fountain
        }
        
        private void ActivateExistingTower(global::Celeste.Player player)
        {
            // Find existing tower or create one if it doesn't exist
            tower = level.Tracker.GetEntity<Tower3DTrigger>();
            
            // Activate climbing mode
            level.CameraLockMode = Level.CameraLockModes.None;
            
            // Set player to climbing state
            player.StateMachine.State = 1; // Climb state
        }
        
        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            // Only clean up if we're leaving the tower area completely
            if (tower != null && Vector2.Distance(player.Position, tower.Position) > 500f)
            {
                // Restore normal camera if far from tower
                if (level != null)
                {
                    level.CameraLockMode = Level.CameraLockModes.FinalBoss;
                }
                
                // Restore normal player state
                if (player.StateMachine.State == 1) // Climb state
                {
                    player.StateMachine.State = 0; // Normal state
                }
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // Check if player is in climbing range of the tower
            if (tower != null && level != null)
            {
                global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    float distanceToTower = Vector2.Distance(player.Position, tower.Position);
                    
                    // If player is close to tower, ensure they can climb
                    if (distanceToTower < 200f && player.StateMachine.State == 0) // Normal state
                    {
                        // Allow climbing when near the tower
                        player.StateMachine.State = 1; // Climb state
                    }
                    else if (distanceToTower > 300f && player.StateMachine.State == 1) // Climb state
                    {
                        // Return to normal state when away from tower
                        player.StateMachine.State = 0; // Normal state
                    }
                }
            }
        }
    }
}



