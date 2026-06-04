// File: CS11_BossOutro.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Boss Outro
    /// Marlet is freed from Dark Matter's possession
    /// Dark Matter retreats with a threat to return
    /// Starlo gives directions to Water Edge Falls and the group discusses their cowboy/cowgirl experiences
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_BossOutro")]
    public class CS11_BossOutro : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_boss_outro_complete";
        private const string FLAG_MARLET_FREED = "ch11_marlet_freed";
        private const string FLAG_DARK_MATTER_RETREATED = "ch11_dark_matter_retreated";
        private const string FLAG_WATERFALL_UNLOCKED = "ch11_waterfall_unlocked";
        
        private const string DIALOG_KEY = "MAGGYHELPER_CH11_BOSS_OUTRO";
        private const string MUSIC_VICTORY = "";
        
        // Audio events
        private const string SFX_MARLET_FREED = "event:/new_content/char/badeline/vanish";
        private const string SFX_DARK_MATTER_DEFEAT = "event:/game_06_boss_badeline_vanish";
        private const string SFX_DARK_MATTER_THREAT = "guid://{42d600c5-32c3-4433-8a1e-be394b30bcde}";
        #endregion

        #region Fields
        private Player player;
        private NPC marlet;
        private NPC starlo;
        private NPC badeline;
        private NPC chara;
        private NPC madeline;
        private NPC kirby;
        #endregion

        public CS11_BossOutro(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_BossOutro(Player player)
            : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void OnBegin(Level level)
        {
            if (player == null)
                player = level.Tracker.GetEntity<Player>();

            if (ShouldSkipCutscene(level))
            {
                WasSkipped = true;
                EndCutscene(level);
                return;
            }

            FindOrSpawnNPCs(level);
            
            // Lock player movement
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;

            Add(new Coroutine(CutsceneSequence(level)));
        }

        private bool ShouldSkipCutscene(Level level)
        {
            return level.Session.GetFlag(FLAG_CUTSCENE_COMPLETE);
        }

        private void FindOrSpawnNPCs(Level level)
        {
            // FindFirst API not available - NPCs would need different lookup
            /*
            marlet = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Marlet"));
            starlo = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Starlo"));
            badeline = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Badeline"));
            chara = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Chara"));
            madeline = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Madeline"));
            kirby = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Kirby"));
            */
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Change to victory music
            Audio.SetMusic(MUSIC_VICTORY);
            
            // Marlet is freed
            Add(new Coroutine(MarletFreedEffect(level)));
            
            yield return Textbox.Say(DIALOG_KEY, MarletFreed);
            
            yield return 0.5f;
            
            // Starlo relieved
            yield return Textbox.Say(DIALOG_KEY, StarloRelieved);
            
            yield return 0.3f;
            
            // Dark Matter appears and threatens
            Add(new Coroutine(DarkMatterAppearEffect(level)));
            
            yield return Textbox.Say(DIALOG_KEY, DarkMatterThreat);
            
            // Dark Matter disappears
            Add(new Coroutine(DarkMatterDisappearEffect(level)));
            
            yield return 0.8f;
            
            // Starlo acknowledges threat and gives directions
            yield return Textbox.Say(DIALOG_KEY, StarloDirections);
            
            yield return 0.3f;
            
            // Badeline's sarcastic response
            yield return Textbox.Say(DIALOG_KEY, BadelineSarcastic);
            
            // Badeline changes mind
            yield return Textbox.Say(DIALOG_KEY, BadelineChangedMind);
            
            // Chara joins in
            yield return Textbox.Say(DIALOG_KEY, CharaJoinsIn);
            
            // Madeline's response
            yield return Textbox.Say(DIALOG_KEY, MadelineResponse);
            
            // Kirby's response
            yield return Textbox.Say(DIALOG_KEY, KirbyResponse);
            
            yield return 0.3f;
            
            // Starlo's farewell
            yield return Textbox.Say(DIALOG_KEY, StarloFarewell);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MARLET_FREED, true);
            level.Session.SetFlag(FLAG_DARK_MATTER_RETREATED, true);
            level.Session.SetFlag(FLAG_WATERFALL_UNLOCKED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator MarletFreed()
        {
            // [MARLET left normal]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("relieved");
                marlet.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator StarloRelieved()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("happy");
                starlo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator DarkMatterThreat()
        {
            // [DarkMatter left normal]
            Audio.Play(SFX_DARK_MATTER_THREAT);
            yield break;
        }

        private IEnumerator StarloDirections()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("idle");
            }
            yield break;
        }

        private IEnumerator BadelineSarcastic()
        {
            // [BADELINE right angry]
            if (badeline != null && badeline.Sprite != null)
            {
                badeline.Sprite.Play("angry");
                badeline.Sprite.Scale.X = 1;
            }
            yield break;
        }

        private IEnumerator BadelineChangedMind()
        {
            // [BADELINE right SCOFF]
            if (badeline != null && badeline.Sprite != null)
            {
                badeline.Sprite.Play("scoff");
            }
            yield break;
        }

        private IEnumerator CharaJoinsIn()
        {
            // [CHARA left normal]
            if (chara != null && chara.Sprite != null)
            {
                chara.Sprite.Play("idle");
                chara.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MadelineResponse()
        {
            // [MADELINE right upset] then [MADELINE right distracted]
            if (madeline != null && madeline.Sprite != null)
            {
                madeline.Sprite.Play("upset");
                madeline.Sprite.Scale.X = 1;
            }
            yield break;
        }

        private IEnumerator KirbyResponse()
        {
            // [KIRBY right normal]
            if (kirby != null && kirby.Sprite != null)
            {
                kirby.Sprite.Play("idle");
                kirby.Sprite.Scale.X = 1;
            }
            yield break;
        }

        private IEnumerator StarloFarewell()
        {
            // [STARLO left normal]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("proud");
            }
            yield break;
        }
        
        #endregion

        #region Visual Effects
        
        private IEnumerator MarletFreedEffect(Level level)
        {
            if (marlet == null) yield break;
            
            // Play freed sound
            Audio.Play(SFX_MARLET_FREED, marlet.Position);
            
            // Purple particles dissipating
            for (int i = 0; i < 30; i++)
            {
                Vector2 offset = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(16f, 48f));
                level.ParticlesFG.Emit(ParticleTypes.Dust, marlet.Position + offset, Color.Purple);
                
                if (i % 5 == 0)
                {
                    yield return 0.05f;
                }
            }
            
            // Flash of light
            level.Flash(Color.White * 0.5f, true);
            
            // Golden particles (freedom)
            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(8f, 24f));
                level.ParticlesFG.Emit(ParticleTypes.Dust, marlet.Position + offset, Color.Gold);
            }
        }

        private IEnumerator DarkMatterAppearEffect(Level level)
        {
            // Spawn Dark Matter entity
            Vector2 spawnPos = level.Camera.Position + new Vector2(160f, 90f);
            
            // Dark portal effect
            level.Flash(Color.Black * 0.6f, true);
            Audio.Play(SFX_DARK_MATTER_DEFEAT);
            
            // Dark particles swirling
            for (int i = 0; i < 15; i++)
            {
                float angle = (i / 15f) * MathHelper.TwoPi;
                Vector2 offset = Calc.AngleToVector(angle, 32f);
                level.ParticlesFG.Emit(ParticleTypes.Dust, spawnPos + offset, Color.DarkViolet);
                yield return 0.05f;
            }
        }

        private IEnumerator DarkMatterDisappearEffect(Level level)
        {
            Vector2 pos = level.Camera.Position + new Vector2(160f, 90f);
            
            // Spiral disappear effect
            for (int i = 0; i < 20; i++)
            {
                float angle = (i / 20f) * MathHelper.TwoPi * 3f;
                float radius = 32f - (i / 20f) * 32f;
                Vector2 offset = Calc.AngleToVector(angle, radius);
                level.ParticlesFG.Emit(ParticleTypes.Dust, pos + offset, Color.Purple);
                yield return 0.03f;
            }
            
            // Final dark flash
            level.Flash(Color.Black * 0.8f, true);
            Audio.Play(SFX_DARK_MATTER_THREAT);
            level.Shake(0.4f);
            
            yield return 0.5f;
        }
        
        #endregion

        public override void OnEnd(Level level)
        {
            // Restore player control
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.StateMachine.Locked = false;
            }
        }
    }
}

