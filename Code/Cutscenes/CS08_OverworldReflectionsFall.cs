using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Custom Overworld Reflections Fall scene that returns to 08truthAlt1.bin room lvl_a-00
    /// Based on vanilla OverworldReflectionsFall but with custom return destination
    /// </summary>
    public class MaggyOverworldReflectionFall : Scene
    {
        private Level returnTo;
        private Action returnCallback;
        private Maddy3D maddy;
        private MountainRenderer mountain;
        private MountainCamera startCamera = new MountainCamera(new Vector3(-8f, 12f, -0.4f), new Vector3(-2f, 9f, -0.5f));
        private MountainCamera fallCamera = new MountainCamera(new Vector3(-10f, 6f, -0.4f), new Vector3(-4.25f, 1.5f, -1.25f));

        public MaggyOverworldReflectionFall(Level returnTo, Action returnCallback)
        {
            this.returnTo = returnTo;
            this.returnCallback = returnCallback;
            Add(mountain = new MountainRenderer());
            mountain.SnapCamera(-1, new MountainCamera(startCamera.Position + (startCamera.Target - startCamera.Position).SafeNormalize() * 2f, startCamera.Target));
            Add(new HiresSnow
            {
                ParticleAlpha = 0f
            });
            Add(new Snow3D(mountain.Model));
            Add(maddy = new Maddy3D(mountain));
            maddy.Falling();
            Add(new Entity
            {
                new Coroutine(Routine())
            });
        }

        private IEnumerator Routine()
        {
            Audio.Play("event:/pusheen/game/08_edge/free_falling", null, 0f);
            mountain.EaseCamera(-1, startCamera, 0.4f, true);
            float duration = 4f;
            maddy.Position = startCamera.Target;
            for (int i = 0; i < 30; i++)
            {
                maddy.Position = startCamera.Target + new Vector3(Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f));
                yield return 0.01f;
            }
            yield return 0.1f;
            maddy.Add(new Coroutine(MaddyFall(duration + 0.1f)));
            yield return 0.1f;
            mountain.EaseCamera(-1, fallCamera, duration, true);
            mountain.ForceNearFog = true;
            yield return duration;
            yield return 0.25f;
            MountainCamera transform = new MountainCamera(fallCamera.Position + mountain.Model.Forward * 3f, fallCamera.Target);
            mountain.EaseCamera(-1, transform, 0.5f, true);
            Return();
        }

        private IEnumerator MaddyFall(float duration)
        {
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                maddy.Position = Vector3.Lerp(startCamera.Target, fallCamera.Target, p);
                yield return null;
            }
        }

        private void Return()
        {
            new FadeWipe(this, wipeIn: false, () =>
            {
                mountain.Dispose();
                if (returnTo != null)
                {
                    Engine.Scene = returnTo;
                }
                returnCallback();
            });
        }

        /// <summary>
        /// Creates a MaggyOverworldReflectionFall scene that transitions to 08truth_A.bin room a-00
        /// </summary>
        public static MaggyOverworldReflectionFall CreateFor08TruthAlt1(Level level)
        {
            // Store session info before creating the scene
            Session session = level.Session;
            
            // Don't pass the level - we'll create a fresh one to avoid mod tracker issues
            return new MaggyOverworldReflectionFall(null, delegate
            {
                Audio.SetAmbience(null, true);
                session.Level = "a-04";
                session.RespawnPoint = null; // Will be set by LoadLevel
                
                // Create a fresh LevelLoader instead of reusing the old level
                // This avoids null tracker crashes that happen when mods try to access
                // Scene.Tracker before it's properly re-initialized
                LevelLoader loader = new LevelLoader(session, session.RespawnPoint);
                
                // Start post-load setup as a managed coroutine
                // The coroutine will wait for the loader to finish and then get
                // the actual Level from Engine.Scene
                CoroutineRunner.StartCoroutine(PostLoadSetup());
                
                Engine.Scene = loader;
            });
        }
        
        /// <summary>
        /// Simple coroutine runner that doesn't require being attached to an entity.
        /// </summary>
        private static class CoroutineRunner
        {
            private static Coroutine activeCoroutine;
            
            public static void StartCoroutine(IEnumerator routine)
            {
                activeCoroutine = new Coroutine(routine);
                On.Monocle.Scene.Update += SceneUpdateHook;
            }
            
            private static void SceneUpdateHook(On.Monocle.Scene.orig_Update orig, Scene scene)
            {
                orig(scene);
                
                if (activeCoroutine != null)
                {
                    activeCoroutine.Update();
                    if (activeCoroutine.Finished)
                    {
                        activeCoroutine = null;
                        On.Monocle.Scene.Update -= SceneUpdateHook;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles post-load setup after level transition.
        /// </summary>
        private static IEnumerator PostLoadSetup()
        {
            // Wait for the level to be fully loaded and active
            while (Engine.Scene is LevelLoader)
            {
                yield return null;
            }
            
            // IMPORTANT: Get the CURRENT level from Engine.Scene, not the captured reference
            // The LevelLoader creates a new Level instance, so the original reference is stale
            Level level = Engine.Scene as Level;
            if (level == null)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper/MaggyOverworldReflectionFall", 
                    "PostLoadSetup: Engine.Scene is not a Level after LevelLoader completed!");
                yield break;
            }
            
            // Now we're in the actual level - wait multiple frames for all systems to initialize
            // This is critical to avoid DustEdges/MaxHelpingHand crashes
            yield return null;
            yield return null;
            yield return null;
            
            // Add the background fade in
            level.Add(new global::Celeste.BackgroundFadeIn(Color.Black, 2f, 30f));
            
            // Ensure entity lists are up to date
            level.Entities.UpdateLists();
            
            // Force instantiate our safe spinners only - skip vanilla spinners entirely
            // to avoid DustEdges crashes with MaxHelpingHand's GradientDustTrigger hook
            SafeCrystalSpinner.ForceInstantiateAll(level);
            // NOTE: Removed TrySafeInstantiateVanillaSpinners call - vanilla spinners will
            // instantiate naturally when they're ready, avoiding the DustEdges.BeforeRender crash
            
            // Set up player with Fall intro type
            Player player = level.Tracker?.GetEntity<Player>();
            if (player != null)
            {
                // Force the fall intro type
                player.IntroType = Player.IntroTypes.Fall;
            }
            
            // Break dash blocks when falling into the level
            player = level.Tracker?.GetEntity<Player>();
            if (player != null)
            {
                foreach (Entity entity in level.Tracker.GetEntities<CelesteDashBlock>())
                {
                    CelesteDashBlock dashBlock = (CelesteDashBlock)entity;
                    dashBlock.Break(player.Center, player.Speed, true, true);
                }
            }
        }
    }
}
