using System.Runtime.CompilerServices;
using MaggyHelper.Cutscenes;

namespace MaggyHelper.Entities
{
    [CustomEntity(new string[] { "MaggyHelper/FlingBirdIntroMod" })]
    [Tracked(true)]
    [HotReloadable]
    public class FlingBirdIntro : Entity
    {
        public Vector2 BirdEndPosition;

        public Sprite Sprite;

        public SoundEmitter CrashSfxEmitter;

        private Vector2[] nodes;

        private bool startedRoutine;

        private Vector2 start;

        private InvisibleBarrier fakeRightWall;

        private bool crashes;

        private Coroutine flyToRoutine;

        private bool emitParticles;

        private bool inCutscene;

        // added cached fields for performance
        private Level levelCache;
        private int nodesCount;
        private ParticleType featherType;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public FlingBirdIntro(Vector2 position, Vector2[] nodes, bool crashes)
            : base(position)
        {
            this.crashes = crashes;
            
            // Ensure sprite creation is safe
            try
            {
                Add(Sprite = GFX.SpriteBank.Create("bird"));
                if (Sprite != null)
                {
                    Sprite.Play(crashes ? "hoverStressed" : "hover");
                    // fixed immediate scale assignment
                    Sprite.Scale.X = crashes ? 1f : -1f;
                    Sprite.OnFrameChange = (string anim) =>
                    {
                        if (!inCutscene && Sprite != null)
                        {
                            BirdNPC.FlapSfxCheck(Sprite);
                        }
                    };
                }
            }
            catch (Exception)
            {
                // Fallback if sprite creation fails - just create a basic sprite
                Sprite = new Sprite(GFX.Game, "");
            }
            
            // Ensure collider is always set
            base.Collider = new Circle(16f, 0f, -8f);
            Add(new PlayerCollider(OnPlayer));
            
            // avoid per-instance allocation when null
            this.nodes = nodes ?? new Vector2[0];
            // cache node count
            nodesCount = this.nodes.Length;
            start = position;
            if (nodesCount > 0)
                BirdEndPosition = this.nodes[nodesCount - 1];
            else
                BirdEndPosition = position;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public FlingBirdIntro(EntityData data, Vector2 levelOffset)
            : this(data.Position + levelOffset, data.NodesOffset(levelOffset), data.Bool("crashes"))
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            levelCache = scene as Level;
            if (levelCache == null)
            {
                RemoveSelf();
                return;
            }
            
            // Ensure collider is properly set before any collision checks
            if (base.Collider == null)
            {
                base.Collider = new Circle(16f, 0f, -8f);
            }
            
            // cache feather particle type once we have a level reference
            featherType = BirdNPC.P_Feather;

            // quick-exit if non-crashing bird already missed
            if (!crashes && levelCache.Session != null && levelCache.Session.GetFlag("MissTheBird"))
            {
                RemoveSelf();
                return;
            }
            
            // Add null check for Scene and Tracker before accessing
            if (base.Scene?.Tracker == null)
            {
                RemoveSelf();
                return;
            }
            
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null && entity.X > base.X)
            {
                if (crashes)
                {
                    levelCache.Session?.SetFlag("MissTheBird", true);
                    levelCache.Session?.SetFlag("KillTheBird", true);
                }
                TapeBlockManager entity2 = base.Scene.Tracker.GetEntity<TapeBlockManager>();
                if (entity2 != null)
                {
                    entity2.StopBlocks();
                    entity2.Finish();
                }
                RemoveSelf();
            }
            else
            {
                // Add null check before adding entities to scene
                if (scene != null)
                {
                    scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
                }
            }
            if (!crashes)
            {
                Vector2 position = Position;
                Position = new Vector2(base.X - 150f, levelCache.Bounds.Top - 8);
                Add(flyToRoutine = new Coroutine(FlyTo(position)));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator FlyTo(Vector2 to)
        {
            Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_flappyscene_entry"));
            Sprite.Play("fly");
            Vector2 from = Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.3f)
            {
                Position = from + (to - from) * Ease.SineOut(p);
                yield return null;
            }
            Sprite.Play("hover");
            float sine = 0f;
            while (true)
            {
                Position = to + Vector2.UnitY * (float)Math.Sin(sine) * 8f;
                sine += Engine.DeltaTime * 2f;
                yield return null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Removed(Scene scene)
        {
            if (fakeRightWall != null)
            {
                fakeRightWall.RemoveSelf();
                fakeRightWall = null;
            }
            
            // Clean up coroutine if it exists
            if (flyToRoutine != null)
            {
                flyToRoutine.RemoveSelf();
                flyToRoutine = null;
            }
            
            // Clear cached references to prevent potential memory leaks
            levelCache = null;
            
            base.Removed(scene);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnPlayer(global::Celeste.Player player)
        {
            // Add null checks for player and scene
            if (player == null || player.Dead || startedRoutine || base.Scene == null)
            {
                return;
            }
            if (flyToRoutine != null)
            {
                flyToRoutine.RemoveSelf();
            }
            startedRoutine = true;
            // Avoid hard locks here; let cutscenes manage minimal state safely
            base.Depth = player.Depth - 5;
            
            // Add null check for Sprite
            if (Sprite != null)
            {
                Sprite.Play("hoverStressed");
                Sprite.Scale.X = 1f;
            }
            
            if (fakeRightWall != null)
            {
                fakeRightWall.RemoveSelf();
                fakeRightWall = null;
            }
            if (!crashes)
            {
                base.Scene.Add(new CS19_MissTheBird(player, this));
                return;
            }
            
            // Add null check for Scene.Tracker before accessing
            if (base.Scene.Tracker != null)
            {
                TapeBlockManager entity = base.Scene.Tracker.GetEntity<TapeBlockManager>();
                if (entity != null)
                {
                    entity.StopBlocks();
                    entity.Finish();
                }
            }
            base.Scene.Add(new CS19_KillTheBird(player, this));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            // Add null checks for Scene before any operations
            if (base.Scene == null)
            {
                return;
            }
            
            // use cached level reference
            if (!startedRoutine && fakeRightWall != null && levelCache != null && levelCache.Camera != null)
            {
                if (levelCache.Camera.X > fakeRightWall.X - 320f - 16f)
                {
                    levelCache.Camera.X = fakeRightWall.X - 320f - 16f;
                }
            }
            if (emitParticles && levelCache != null && levelCache.ParticlesBG != null && base.Scene.OnInterval(0.1f))
            {
                // use cached particle type
                levelCache.ParticlesBG.Emit(featherType, 1, Position + new Vector2(0f, -8f), new Vector2(6f, 4f));
            }
            base.Update();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerator DoGrabbingRoutine(global::Celeste.Player player)
        {
            // Add null checks for critical objects
            if (player == null || Scene == null)
            {
                yield break;
            }
            
            Level level = levelCache ?? Scene as Level;
            if (level == null)
            {
                yield break;
            }
            
            inCutscene = true;
            CrashSfxEmitter = SoundEmitter.Play(crashes ? "event:/new_content/game/10_farewell/bird_crashscene_start" : "event:/new_content/game/10_farewell/bird_flappyscene", this);
            
            // Lock player movement during bird grab so they don't fall
            player.StateMachine.State = 11; // Dummy state
            player.DummyGravity = false;
            player.DummyAutoAnimate = false;
            player.Speed = Vector2.Zero;
            player.ForceCameraUpdate = true;
            
            // Add null checks for player sprite
            if (player.Sprite != null)
            {
                player.Sprite.Play("jumpSlow_carry");
            }
            player.Facing = Facings.Right;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            emitParticles = true;
            Add(new Coroutine(level.ZoomTo(new Vector2(140f, 120f), 1.5f, 4f)));
            float sin = 0f;
            // use cached nodesCount for loop bounds and ensure nodes array is valid
            if (nodes != null && nodesCount > 1)
            {
                for (int index = 0; index < Math.Max(0, nodesCount - 1); index++)
                {
                    Vector2 position = Position;
                    Vector2 vector = nodes[index];
                    // local reused references reduce field access
                    SimpleCurve curve = new SimpleCurve(position, vector, position + (vector - position) * 0.5f + new Vector2(0f, -24f));
                    float lengthParametric = curve.GetLengthParametric(32);
                    float duration = Math.Max(0.001f, lengthParametric / 100f);
                    if (vector.Y < position.Y)
                    {
                        duration *= 1.1f; 
                        if (Sprite != null) Sprite.Rate = 2f;
                    }
                    else
                    {
                        duration *= 0.8f; 
                        if (Sprite != null) Sprite.Rate = 1f;
                    }
                    for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
                    {
                        sin += Engine.DeltaTime * 10f;
                        Position = (curve.GetPoint(p) + Vector2.UnitY * (float)Math.Sin(sin) * 8f).Floor();
                        player.Position = Position + new Vector2(2f, 10f);
                        yield return null;
                    }
                    level.Shake();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                }
            }
            if (Sprite != null) Sprite.Rate = 1f;
            yield return null;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White);
            emitParticles = false;
            inCutscene = false;
        }
    }
}




