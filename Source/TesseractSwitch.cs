namespace MaggyHelper.Entities 
{
    [CustomEntity(ids: "MaggyHelper/TesseractSwitch")]
    public class TesseractSwitch : Entity
    {
        // Entity properties
        private Sprite sprite;
        private bool activated = false;
        private string targetFlag;
        
        // Constructor for Ahorn/L�nn
        public TesseractSwitch(EntityData data, Vector2 offset) 
            : base(data.Position + offset) {
            InitializeFromEntityData(data, offset);
        }
        
        public void InitializeFromEntityData(EntityData data, Vector2 offset) {
            targetFlag = data.Attr(nameof(targetFlag), "tesseract_activated");
            // Initialize other properties from data
            
            // Set up the sprite
            sprite = new Sprite(GFX.Game, "objects/IngesteHelper/tesseractswitch/");
            sprite.AddLoop("idle", "", 0.1f, 0, 1, 2, 3);
            sprite.AddLoop(nameof(activated), "", 0.1f, 4, 5, 6, 7);
            sprite.Play(activated ? nameof(activated) : "idle");
            sprite.CenterOrigin();
            
            // Add components
            Add(sprite);
            
            // Set collision properties
            Collider = new Hitbox(16, 16, -8, -8);
            Depth = -1000; // Adjust as needed

            activated = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Check if the switch should start activated based on session flag
            if (SceneAs<Level>().Session.GetFlag(targetFlag))
            {
                activated = true;
                sprite.Play(nameof(activated));
            }
        }

        public override void Update() {
            base.Update();
            // Custom update logic
        }

        public void Toggle()
        {
            activated = !activated;
            sprite.Play(activated ? nameof(activated) : "idle");

            // Play sound effect
            // activationSound.Play(activated ? "event:/game/general/crystalheart_pulse" : "event:/game/general/crystalheart_fadeout");

            // Set session flag if persistent
            if (!string.IsNullOrEmpty(targetFlag))
            {
                SceneAs<Level>().Session.SetFlag(targetFlag, activated);
            }

            // Create visual effect
            SceneAs<Level>().Particles.Emit(ParticleTypes.SparkyDust, 12, Position, Vector2.One * 4f);

            // Optional: Add a screen shake
            SceneAs<Level>().Shake(0.3f);

            // Trigger any connected entities
            triggerConnectedEntities();
        }

        private void triggerConnectedEntities()
        {
            // Example: Look for entities that might be affected by this switch
            foreach (var entity in Scene.Tracker.GetEntities<TesseractMirror>())
            {
                // You could implement custom behavior based on proximity or other rules
                if (Vector2.Distance(Position, entity.Position) < 200f)
                {
                    // Affect the entity in some way
                    // This is a placeholder - you would need to implement actual behavior
                }
            }
        }

        // Static methods for hooking into game events
        public static void Load()
        {
            // Hook into any necessary game events
            On.Celeste.Level.LoadLevel += OnLoadLevel;
        }

        public static void Unload()
        {
            // Unhook from game events
            On.Celeste.Level.LoadLevel -= OnLoadLevel;
        }

        private static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, global::Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);

            // Additional logic when level loads
            // For example, you could initialize or reset switch states based on level conditions
            Logger.Log(nameof(TesseractSwitch), $"Level {self.Session.Level} loaded");
        }
    }
}




