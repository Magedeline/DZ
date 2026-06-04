// File: CS11_BossIntro.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Boss Introduction
    /// Starlo realizes Marlet isn't the thief - she's been possessed by Dark Matter
    /// The possessed Marlet threatens to eliminate everyone
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_BossIntro")]
    public class CS11_BossIntro : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_boss_intro_complete";
        private const string FLAG_BOSS_BATTLE_STARTED = "ch11_boss_battle_started";
        private const string FLAG_MARLET_POSSESSED = "ch11_marlet_possessed";
        
        private const string DIALOG_KEY_INTRO = "MAGGYHELPER_CH11_BOSS_INTRO";
        private const string DIALOG_KEY_WARNING = "MAGGYHELPER_CH11_BOSS_INTRO_DO_NOT_HURT_HER";
        
        // Audio events
        private const string SFX_POSSESSION_REVEAL = "guid://{42d600c5-32c3-4433-8a1e-be394b30bcde}";
        private const string SFX_DARK_MATTER_LAUGH = "event:/game_06_boss_badeline_laugh";
        private const string MUSIC_BOSS_INTRO = "event:/music/pusheen/lvl11/pmarlet_fight";
        #endregion

        #region Fields
        private Player player;
        private NPC starlo;
        private NPC marlet;
        private NPC theo;
        private NPC chara;
        #endregion

        public CS11_BossIntro(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_BossIntro(Player player)
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
            starlo = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Starlo"));
            marlet = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Marlet"));
            theo = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Theo"));
            chara = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Chara"));
            */
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Start ominous music
            Audio.SetMusic(MUSIC_BOSS_INTRO);
            
            // Starlo senses something wrong
            yield return Textbox.Say(DIALOG_KEY_INTRO, StarloUnsettled);
            
            yield return 0.5f;
            
            // Mysterious voice reveals itself
            yield return Textbox.Say(DIALOG_KEY_INTRO, MysteriousVoice);
            
            // Dark possession effect
            Add(new Coroutine(PossessionRevealEffect(level)));
            
            yield return 0.8f;
            
            // Marlet possessed apologizes
            yield return Textbox.Say(DIALOG_KEY_INTRO, MarletPossessedApologizes);
            
            yield return 0.3f;
            
            // Starlo reacts in shock
            yield return Textbox.Say(DIALOG_KEY_INTRO, StarloShocked);
            
            yield return 0.2f;
            
            // Possessed Marlet reveals full control
            yield return Textbox.Say(DIALOG_KEY_INTRO, MarletFullyPossessed);
            
            yield return 0.5f;
            
            // Theo warns not to hurt her
            yield return Textbox.Say(DIALOG_KEY_WARNING, TheoWarnsCareful);
            
            // Chara agrees and prepares to help
            yield return Textbox.Say(DIALOG_KEY_WARNING, CharaAgrees);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_BOSS_BATTLE_STARTED, true);
            level.Session.SetFlag(FLAG_MARLET_POSSESSED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator StarloUnsettled()
        {
            // [STARLO left unsettle]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("unsettle");
                starlo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MysteriousVoice()
        {
            // [??? left normal] - mysterious voice before possession reveal
            Audio.Play(SFX_POSSESSION_REVEAL);
            yield break;
        }

        private IEnumerator MarletPossessedApologizes()
        {
            // [marletpossesed right normal]
            if (marlet != null && marlet.Sprite != null)
            {
                // Change Marlet's appearance to show possession
                marlet.Sprite.Play("possessed");
                marlet.Sprite.Scale.X = 1; // Face right
                
                // Add dark aura effect (using valid component constructor)
                // marlet.Add(new Component(true, false));
            }
            yield break;
        }

        private IEnumerator StarloShocked()
        {
            // [STARLO left angry]
            if (starlo != null && starlo.Sprite != null)
            {
                starlo.Sprite.Play("angry");
            }
            yield break;
        }

        private IEnumerator MarletFullyPossessed()
        {
            // [marletpossesed right normal] - reveals full control
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("possessed_evil");
            }
            
            Audio.Play(SFX_DARK_MATTER_LAUGH);
            yield break;
        }

        private IEnumerator TheoWarnsCareful()
        {
            // [THEO left wtf]
            if (theo != null && theo.Sprite != null)
            {
                theo.Sprite.Play("wtf");
                theo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator CharaAgrees()
        {
            // [CHARA left worried] then [CHARA left normal]
            if (chara != null && chara.Sprite != null)
            {
                chara.Sprite.Play("worried");
                chara.Sprite.Scale.X = -1;
            }
            yield break;
        }
        
        #endregion

        #region Visual Effects
        
        private IEnumerator PossessionRevealEffect(Level level)
        {
            if (marlet == null) yield break;
            
            // Screen flash
            level.Flash(Color.Purple * 0.6f, true);
            
            // Screen shake
            level.Shake(0.5f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            
            // Dark particles around Marlet
            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, Calc.Random.Range(8f, 24f));
                level.ParticlesFG.Emit(ParticleTypes.Dust, marlet.Position + offset, Color.Purple);
            }
            
            yield return 0.3f;
            
            // Second flash
            level.Flash(Color.Black * 0.8f, true);
        }
        
        #endregion

        public override void OnEnd(Level level)
        {
            // Keep player locked for boss battle
            if (player != null)
            {
                player.StateMachine.State = Player.StDummy;
                player.StateMachine.Locked = true;
            }
        }
    }
}

