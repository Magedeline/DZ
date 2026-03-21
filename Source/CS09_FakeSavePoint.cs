using MaggyHelper.Entities;
using MaggyHelper.NPCs;
using FMOD.Studio;
using BadelineDummy = MaggyHelper.Entities.BadelineDummy;

namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Fake save point cutscene sequence (A through E) that leads to a trap.
    /// Uses moon landing mechanic for gentle gravity landing.
    /// Connects to NPC09_SavePoint on the final stage (E).
    /// </summary>
    public class CS09_FakeSavePoint : CutsceneEntity
    {
        // Flags for tracking progression through the cutscene stages
        public const string FlagStageA = "ch9_fakesave_stage_a";
        public const string FlagStageB = "ch9_fakesave_stage_b";
        public const string FlagStageC = "ch9_fakesave_stage_c";
        public const string FlagStageD = "ch9_fakesave_stage_d";
        public const string FlagStageE = "ch9_fakesave_stage_e";
        public const string FlagTrapTriggered = "ch9_fakesave_trap";

        // Entity references
        private global::Celeste.Player player;
        private CharaDummy chara;
        private BadelineDummy badeline;
        private RalseiDummy ralsei;
        private Entity theo;
        private Entity madeline;
        
        // NPC reference for stage E
        private NPC09_SavePoint savePointNPC;
        
        // Audio
        private EventInstance entrySfx;
        
        // Stage tracking
        private readonly SavePointStage stage;

        public enum SavePointStage
        {
            StageA,
            StageB,
            StageC,
            StageD,
            StageE,
            PreTrap,
            Trap,
            MadelineFreakout
        }

        public CS09_FakeSavePoint(global::Celeste.Player player, SavePointStage stage, NPC09_SavePoint npc = null) 
            : base(true, false)
        {
            base.Depth = -8500;
            this.player = player;
            this.stage = stage;
            this.savePointNPC = npc;
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            Vector2 spawn = level.GetSpawnPoint(player.Position);
            
            // Set up player state for cutscene
            player.StateMachine.State = 11; // Dummy state
            player.DummyGravity = false;
            
            // Play entry sound and perform moon landing (from CS19_CharaHelps)
            entrySfx = Audio.Play("event:/new_content/char/madeline/screenentry_stubborn", player.Position);
            yield return player.MoonLanding(spawn);
            
            // Zoom to focus on the scene
            yield return level.ZoomTo(new Vector2(spawn.X - level.Camera.X, 134f), 2f, 0.5f);
            yield return 0.5f;

            // Run the appropriate stage
            switch (stage)
            {
                case SavePointStage.StageA:
                    yield return RunStageA(level);
                    break;
                case SavePointStage.StageB:
                    yield return RunStageB(level);
                    break;
                case SavePointStage.StageC:
                    yield return RunStageC(level);
                    break;
                case SavePointStage.StageD:
                    yield return RunStageD(level);
                    break;
                case SavePointStage.StageE:
                    yield return RunStageE(level);
                    break;
                case SavePointStage.PreTrap:
                    yield return RunPreTrap(level);
                    break;
                case SavePointStage.Trap:
                    yield return RunTrap(level);
                    break;
                case SavePointStage.MadelineFreakout:
                    yield return RunMadelineFreakout(level);
                    break;
            }

            yield return level.ZoomBack(0.5f);
            base.EndCutscene(level, true);
        }

        #region Stage A - Madeline discovers the save point with Chara warning
        
        private IEnumerator RunStageA(Level level)
        {
            // Spawn Madeline dummy and Chara
            yield return SpawnMadeline();
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILEA", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(MadelineLooksDistracted),
                new Func<IEnumerator>(CharaWarns)
            });

            yield return CleanupEntities();
            level.Session.SetFlag(FlagStageA, true);
        }

        private IEnumerator MadelineLooksDistracted()
        {
            if (madeline != null)
            {
                var sprite = madeline.Get<Sprite>();
                if (sprite != null)
                {
                    sprite.Play("idle");
                }
            }
            yield return 0.3f;
        }

        private IEnumerator CharaWarns()
        {
            if (chara != null)
            {
                chara.Sprite.Scale.X = -1f; // Face the save point
            }
            yield return 0.2f;
        }

        #endregion

        #region Stage B - Theo questions Chara about the location
        
        private IEnumerator RunStageB(Level level)
        {
            yield return SpawnTheo();
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILEB", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(TheoThinks),
                new Func<IEnumerator>(CharaConcerned)
            });

            yield return CleanupEntities();
            level.Session.SetFlag(FlagStageB, true);
        }

        private IEnumerator TheoThinks()
        {
            if (theo != null)
            {
                var sprite = theo.Get<Sprite>();
                if (sprite != null)
                {
                    sprite.Play("idle");
                }
            }
            yield return 0.3f;
        }

        private IEnumerator CharaConcerned()
        {
            if (chara != null)
            {
                chara.Sprite.Scale.X = 1f;
            }
            yield return 0.2f;
        }

        #endregion

        #region Stage C - Badeline questions what's going on, Chara feels something
        
        private IEnumerator RunStageC(Level level)
        {
            yield return SpawnBadeline();
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILEC", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(BadelineUpset),
                new Func<IEnumerator>(CharaFeelsSomething),
                new Func<IEnumerator>(CharaAngry)
            });

            yield return CleanupEntities();
            level.Session.SetFlag(FlagStageC, true);
        }

        private IEnumerator BadelineUpset()
        {
            if (badeline != null)
            {
                badeline.Sprite.Scale.X = -1f;
            }
            yield return 0.3f;
        }

        private IEnumerator CharaFeelsSomething()
        {
            if (chara != null)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }
            yield return 0.5f;
        }

        private IEnumerator CharaAngry()
        {
            if (chara != null)
            {
                Level.Displacement.AddBurst(chara.Center, 0.3f, 8f, 24f, 0.3f);
            }
            yield return 0.2f;
        }

        #endregion

        #region Stage D - Ralsei asks if Chara is okay, Kirby decides to act
        
        private IEnumerator RunStageD(Level level)
        {
            yield return SpawnRalsei();
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILED", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(RalseiConcerned),
                new Func<IEnumerator>(CharaNotOkay)
            });

            yield return CleanupEntities();
            level.Session.SetFlag(FlagStageD, true);
        }

        private IEnumerator RalseiConcerned()
        {
            if (ralsei != null)
            {
                ralsei.Sprite.Scale.X = 1f;
            }
            yield return 0.3f;
        }

        private IEnumerator CharaNotOkay()
        {
            if (chara != null)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Level.Displacement.AddBurst(chara.Center, 0.4f, 8f, 32f, 0.4f);
            }
            yield return 0.4f;
        }

        #endregion

        #region Stage E - Final stage connected to NPC09_SavePoint interaction
        
        private IEnumerator RunStageE(Level level)
        {
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILEPRETRAP", Array.Empty<Func<IEnumerator>>());

            // Interact with the save point
            if (savePointNPC != null)
            {
                yield return 0.5f;
                
                // Trigger the save point animation
                if (savePointNPC.Sprite != null)
                {
                    savePointNPC.Sprite.Play("activate");
                }
            }
            
            yield return 0.3f;
            
            // This triggers the trap sequence
            yield return CleanupEntities();
            level.Session.SetFlag(FlagStageE, true);
            
            // Automatically transition to trap sequence
            yield return RunTrap(level);
        }

        #endregion

        #region PreTrap and Trap Sequences
        
        private IEnumerator RunPreTrap(Level level)
        {
            yield return SpawnChara();
            yield return 0.3f;

            yield return Textbox.Say("CH9_SPACE_SAVE_FILEPRETRAP", Array.Empty<Func<IEnumerator>>());

            yield return CleanupEntities();
        }

        private IEnumerator RunTrap(Level level)
        {
            // Dramatic screen effects
            level.Flash(Color.Red * 0.5f, false);
            Audio.Play("event:/desolozantas/final_content/sfx/trap_activate", player.Position);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            
            yield return 0.5f;
            
            // Spawn Chara for reaction
            yield return SpawnChara();
            chara.Sprite.Scale.X = 1f;
            
            yield return Textbox.Say("CH9_SPACE_TRAP_SAVE_FILE", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(OmegaZeroAppears),
                new Func<IEnumerator>(CharaPanics)
            });

            // Big shake and flash
            level.Shake(0.5f);
            level.Flash(Color.White, false);
            
            yield return 0.3f;
            yield return CleanupEntities();
            level.Session.SetFlag(FlagTrapTriggered, true);
        }

        private IEnumerator OmegaZeroAppears()
        {
            // Screen darkens
            Level.NextColorGrade("ominous", 0.3f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            yield return 0.5f;
        }

        private IEnumerator CharaPanics()
        {
            if (chara != null)
            {
                Level.Displacement.AddBurst(chara.Center, 0.5f, 16f, 48f, 0.5f);
                Audio.Play("event:/char/badeline/disappear", chara.Position);
            }
            yield return 0.3f;
        }

        #endregion

        #region Madeline Freakout (aftermath)
        
        private IEnumerator RunMadelineFreakout(Level level)
        {
            yield return SpawnMadeline();
            
            // Madeline is alone and panicking
            if (madeline != null)
            {
                var sprite = madeline.Get<Sprite>();
                if (sprite != null)
                {
                    sprite.Play("runFast");
                }
            }
            
            yield return 0.3f;

            yield return Textbox.Say("CH9_MADELINE_FREAKOUT", Array.Empty<Func<IEnumerator>>());

            yield return CleanupEntities();
            
            // Teleport to credits sequence
            level.Add(new CS09_Credits(player));
        }

        #endregion

        #region Entity Spawning Helpers
        
        private IEnumerator SpawnChara()
        {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(chara = new CharaDummy(player.Center + new Vector2(-24f, 0f)));
            Level.Displacement.AddBurst(chara.Center, 0.5f, 8f, 32f, 0.5f);
            chara.Sprite.Scale.X = 1f;
            yield return chara.FloatTo(player.Center + new Vector2(-32f, -8f), -1, false, false, false);
            yield return 0.2f;
        }

        private IEnumerator SpawnBadeline()
        {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(badeline = new BadelineDummy(player.Center + new Vector2(24f, 0f)));
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            badeline.Sprite.Scale.X = -1f;
            yield return badeline.FloatTo(player.Center + new Vector2(32f, -8f), 1, false, false, false);
            yield return 0.2f;
        }

        private IEnumerator SpawnRalsei()
        {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(ralsei = new RalseiDummy(player.Position + new Vector2(-48f, 0f)));
            Level.Displacement.AddBurst(ralsei.Center, 0.5f, 8f, 32f, 0.5f);
            ralsei.Sprite.Scale.X = 1f;
            yield return ralsei.FloatTo(player.Position + new Vector2(-40f, -8f), 1, false, false, 120f);
            yield return 0.2f;
        }

        private IEnumerator SpawnMadeline()
        {
            madeline = new Entity(player.Position + new Vector2(-24f, 0f)) { Depth = player.Depth + 1 };
            var madelineSprite = GFX.SpriteBank.Create("Madeline");
            madelineSprite.Play("idle");
            madelineSprite.Scale.X = 1f;
            madeline.Add(madelineSprite);
            Level.Add(madeline);
            yield return 0.2f;
        }

        private IEnumerator SpawnTheo()
        {
            theo = new Entity(player.Position + new Vector2(-32f, 0f)) { Depth = player.Depth + 1 };
            var theoSprite = GFX.SpriteBank.Create("Theo");
            theoSprite.Play("idle");
            theoSprite.Scale.X = 1f;
            theo.Add(theoSprite);
            Level.Add(theo);
            yield return 0.2f;
        }

        #endregion

        #region Utility Methods
        
        private IEnumerator WalkEntityTo(Entity entity, Vector2 target, float speed = 64f)
        {
            while ((entity.Position - target).LengthSquared() > 0.25f)
            {
                var dir = target - entity.Position;
                var step = Math.Min(speed * Engine.DeltaTime, dir.Length());
                if (step <= 0f) break;
                entity.Position += dir.SafeNormalize() * step;
                yield return null;
            }
            entity.Position = target;
        }

        private IEnumerator CleanupEntities()
        {
            yield return 0.2f;

            if (chara != null && chara.Scene != null)
            {
                chara.Vanish();
                chara = null;
            }

            if (badeline != null && badeline.Scene != null)
            {
                Audio.Play("event:/char/badeline/disappear", badeline.Position);
                Level.Displacement.AddBurst(badeline.Position, 0.5f, 24f, 96f, 0.4f);
                badeline.RemoveSelf();
                badeline = null;
            }

            if (ralsei != null && ralsei.Scene != null)
            {
                ralsei.Vanish();
                ralsei = null;
            }

            if (madeline != null && madeline.Scene != null)
            {
                madeline.RemoveSelf();
                madeline = null;
            }

            if (theo != null && theo.Scene != null)
            {
                theo.RemoveSelf();
                theo = null;
            }

            yield return 0.2f;
        }

        #endregion

        public override void OnEnd(Level level)
        {
            player.Depth = 0;
            player.Speed = Vector2.Zero;
            player.Position = level.GetSpawnPoint(player.Position);
            player.Active = true;
            player.Visible = true;
            player.StateMachine.State = 0;
            player.DummyGravity = true;

            // Clean up any remaining entities
            if (chara != null)
            {
                chara.RemoveSelf();
                chara = null;
            }
            if (badeline != null)
            {
                badeline.RemoveSelf();
                badeline = null;
            }
            if (ralsei != null)
            {
                ralsei.RemoveSelf();
                ralsei = null;
            }
            if (madeline != null)
            {
                madeline.RemoveSelf();
                madeline = null;
            }
            if (theo != null)
            {
                theo.RemoveSelf();
                theo = null;
            }

            level.ResetZoom();

            if (WasSkipped)
            {
                Audio.Stop(entrySfx, true);
            }
        }

        /// <summary>
        /// Static helper to determine which stage to run based on session flags
        /// </summary>
        public static SavePointStage GetCurrentStage(Level level)
        {
            if (level.Session.GetFlag(FlagTrapTriggered))
                return SavePointStage.MadelineFreakout;
            if (level.Session.GetFlag(FlagStageE))
                return SavePointStage.Trap;
            if (level.Session.GetFlag(FlagStageD))
                return SavePointStage.StageE;
            if (level.Session.GetFlag(FlagStageC))
                return SavePointStage.StageD;
            if (level.Session.GetFlag(FlagStageB))
                return SavePointStage.StageC;
            if (level.Session.GetFlag(FlagStageA))
                return SavePointStage.StageB;
            
            return SavePointStage.StageA;
        }
    }
}
