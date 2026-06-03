using Celeste.Entities;

namespace Celeste.Cutscenes
{
    // Note: Removed [CustomEntity] - cutscene entities are created programmatically, not placed in maps
    public class Cs12TowerFountain : CutsceneEntity
    {
        #region Constants
        private const float fountain_rise_duration = 3.0f;
        private const float tower_growth_duration = 4.0f;
        private const float camera_zoom_duration = 2.0f;
        private const float player_approach_speed = 120f;
        private const float fountain_height = 200f;
        private const float tower_max_height = 1000f;
        private const string fountain_flag = "ch12_fountain_created";
        private const string tower_flag = "ch12_tower_activated";
        #endregion

        #region Fields
        private readonly global::Celeste.Player player;
        private Vector2 fountainPosition;
        private Vector2 playerStartPosition;
        private Vector2 cameraStartPosition;
        private TitanTower3D tower;
        private MagicFountain fountain;
        private Entities.TowerBackgroundStyleground towerBackground;
        public float FountainHeight = 0f;
        private bool fountainCreated = false;
        private bool towerActivated = false;
        private FMOD.Studio.EventInstance fountainSfx;
        private FMOD.Studio.EventInstance towerSfx;
        private EntityData cutsceneData;
        #endregion

        public Cs12TowerFountain(EntityData entityData, Vector2 offset, global::Celeste.Player player1) : base() {
            player = player1;
            cutsceneData = entityData;
            fountainPosition = entityData.Position + offset;

            // Initialize flags and positions
            fountainCreated = false;
            towerActivated = false;
            playerStartPosition = player1.Position;
            cameraStartPosition = Vector2.Zero; // Will be set during cutscene initialization

            // Set tag for transition updates
            Tag = Tags.TransitionUpdate;
        }

        public override void OnBegin(Level level)
        {
            // Store initial positions
            playerStartPosition = player.Position;
            cameraStartPosition = level.Camera.Position;

            // Start the cutscene
            Add(new Coroutine(cutscene(level)));
        }

        private IEnumerator cutscene(Level level)
        {
            // Lock player controls
            player.StateMachine.State = global::Celeste.Player.StDummy;
            player.StateMachine.Locked = true;
            player.DummyAutoAnimate = false;

            yield return 0.5f;

            // Phase 1: Player approaches fountain location
            yield return playerApproachFountain();

            // Phase 2: Fountain creation sequence
            yield return createFountain(level);

            // Phase 3: Tower emergence with background
            yield return createTower(level);

            // Phase 4: Camera work and player preparation
            yield return prepareTowerClimbing(level);

            // Phase 5: Begin climbing
            yield return beginClimbing(level);

            // End cutscene
            endCutscene(level);
        }

        private IEnumerator playerApproachFountain()
        {
            // Player walks to fountain position
            player.Facing = fountainPosition.X > player.X ? Facings.Right : Facings.Left;
            player.Sprite.Play("walk");

            var targetPosition = fountainPosition + new Vector2(-32f, 0f);

            while (Vector2.Distance(player.Position, targetPosition) > 4f)
            {
                Vector2 direction = (targetPosition - player.Position).SafeNormalize();
                player.Position += direction * player_approach_speed * Engine.DeltaTime;
                yield return null;
            }

            player.Position = targetPosition;
            player.Sprite.Play("idle");
            yield return 0.3f;
        }

        private IEnumerator createFountain(Level level)
        {
            // Create fountain entity
            fountain = new MagicFountain(fountainPosition);
            level.Add(fountain);

            // Ground rumble
            level.Shake(0.3f);
            Audio.Play("event:/game/general/crystalheart_pulse", fountainPosition);

            yield return 0.5f;

            // Fountain starts to emerge from the ground
            fountainSfx = Audio.Play("event:/game/general/crystalheart_blue_get", fountainPosition);

            var timer = 0f;
            var startPos = fountainPosition;
            var endPos = fountainPosition - new Vector2(0f, fountain_height);

            while (timer < fountain_rise_duration)
            {
                timer += Engine.DeltaTime;
                var progress = Ease.CubeOut(timer / fountain_rise_duration);
                fountain.Position = Vector2.Lerp(startPos, endPos, progress);

                // Particle effects during emergence
                if (Scene.OnInterval(0.1f))
                {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, 1, fountain.Position, Vector2.One * 8f);
                }

                yield return null;
            }

            fountain.Position = endPos;
            fountain.Activate();
            fountainCreated = true;
            level.Session.SetFlag(fountain_flag);

            yield return 1.0f;
        }

        private IEnumerator createTower(Level level)
        {
            // Camera zooms out to show tower emergence
            var zoomTarget = fountainPosition + new Vector2(0f, -300f);
            yield return level.ZoomTo(zoomTarget - level.Camera.Position, 1.5f, camera_zoom_duration);

            towerBackground = new Entities.TowerBackgroundStyleground(fountainPosition + new Vector2(0f, -100f));
            level.Add((IEnumerable<Entity>)towerBackground);

            yield return 0.5f;

            // Fade in background
            towerBackground.SetActive(true);
            var bgFadeTimer = 0f;
            const float bg_fade_duration = 1.5f;

            while (bgFadeTimer < bg_fade_duration)
            {
                bgFadeTimer += Engine.DeltaTime;
                var bgProgress = Ease.SineInOut(bgFadeTimer / bg_fade_duration);
                towerBackground.SetAlpha(bgProgress * 0.6f);
                yield return null;
            }

            yield return 0.5f;

            // Create the 3D tower
            tower = new TitanTower3D(fountainPosition + new Vector2(0f, -100f));
            tower.SetGrowthProgress(0f); // Start with no growth
            tower.SetBackground(towerBackground); // Link background to tower
            level.Add(tower);

            // Tower growth sound
            towerSfx = Audio.Play("event:/game/general/crystalheart_blue_get", tower.Position);

            // Tower grows upward dramatically
            var timer = 0f;
            while (timer < tower_growth_duration)
            {
                timer += Engine.DeltaTime;
                var progress = Ease.CubeInOut(timer / tower_growth_duration);

                // Update tower growth progress (this will also update background)
                tower.SetGrowthProgress(progress);

                // Camera shaking during growth
                if (progress > 0.3f)
                {
                    level.Shake(0.2f);
                }

                // Enhanced particle effects during growth
                if (Scene.OnInterval(0.05f))
                {
                    Vector2 particlePos = tower.Position + new Vector2(
                        Calc.Random.Range(-50f, 50f),
                        -progress * tower_max_height * 0.5f
                    );
                    level.ParticlesBG.Emit(ParticleTypes.Dust, 2, particlePos, Vector2.One * 16f);

                    // Add magical sparkles using DustStaticSpinner instead
                    level.ParticlesBG.Emit(ParticleTypes.SparkyDust, 1, particlePos, Vector2.One * 8f, Color.Gold);
                }

                yield return null;
            }

            // Ensure tower is fully grown
            tower.SetGrowthProgress(1f);
            towerActivated = true;
            level.Session.SetFlag(tower_flag);

            // Final tower completion effect with background flash
            level.Flash(Color.White);
            towerBackground.SetTintColor(Color.Gold);
            Audio.Play("event:/game/general/crystalheart_pulse", tower.Position);

            // Restore background color after flash
            yield return 0.5f;
            towerBackground.SetTintColor(Color.MediumPurple);

            yield return 1.0f;
        }

        private IEnumerator prepareTowerClimbing(Level level)
        {
            // Camera moves to focus on tower base and player
            var cameraTarget = new Vector2(
                (player.X + tower.X) * 0.5f - 160f,
                tower.Y - 90f
            );

            yield return level.ZoomTo(cameraTarget - level.Camera.Position, 2.0f, 1.5f);

            // Player looks up at the tower in amazement
            player.Facing = tower.X > player.X ? Facings.Right : Facings.Left;
            player.Sprite.Play("lookUp");

            yield return 1.0f;

            // Optional dialogue
            yield return Textbox.Say("MAGGYHELPER_CH12_TOWER_FOUNTAIN_CREATED");

            yield return 0.5f;
        }

        private IEnumerator beginClimbing(Level level)
        {
            // Player approaches tower base
            Vector2 towerBase = tower.Position + new Vector2(0f, 50f);
            player.Sprite.Play("walk");

            while (Vector2.Distance(player.Position, towerBase) > 8f)
            {
                Vector2 direction = (towerBase - player.Position).SafeNormalize();
                player.Position += direction * player_approach_speed * Engine.DeltaTime;
                yield return null;
            }

            player.Position = towerBase;
            player.Sprite.Play("idle");

            yield return 0.3f;

            // Player prepares to climb
            player.Sprite.Play("jumpSlow");
            yield return 0.2f;

            // Enable climbing mechanics on the tower
            tower.EnableClimbing(true);

            // Transition to climbing state
            player.StateMachine.State = global::Celeste.Player.StClimb;
            player.Sprite.Play("wallslide");
            yield return 0.3f;
        }

        private void endCutscene(Level level)
        {
            // Restore player control but keep them in climbing mode
            player.StateMachine.Locked = false;
            player.DummyAutoAnimate = true;

            // Set camera to follow player normally
            level.CameraLockMode = Level.CameraLockModes.None;

            level.EndCutscene();
        }

        public override void OnEnd(Level level)
        {
            // Clean up audio
            Audio.Stop(fountainSfx);
            Audio.Stop(towerSfx);

            // Ensure flags are set
            if (fountainCreated)
                level.Session.SetFlag(fountain_flag);
            if (towerActivated)
                level.Session.SetFlag(tower_flag);

            // Restore player state if cutscene was interrupted
            if (player.StateMachine.Locked)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = global::Celeste.Player.StNormal;
                player.DummyAutoAnimate = true;
            }
        }
    }
}
