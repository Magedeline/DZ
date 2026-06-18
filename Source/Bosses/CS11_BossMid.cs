// File: CS11_BossMid.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Boss Mid-Fight
    /// Plays during the boss battle when Marlet starts to break free from possession
    /// Features dramatic internal struggle between Marlet and Dark Matter
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_BossMid")]
    public class CS11_BossMid : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_boss_mid_complete";
        private const string FLAG_MARLET_RESISTING = "ch11_marlet_resisting";
        
        private const string DIALOG_KEY = "DZ_CH11_BOSS_MID";
        
        // Audio events
        private const string SFX_RESISTANCE = "event:/new_content/char/badeline/disappear";
        private const string SFX_STRUGGLE = "event:/game_06_boss_badeline_attack";
        private const string MUSIC_BOSS_MID = "event:/Mods/pusheen/music/lvl11/pmarlet_glitched";
        #endregion

        #region Fields
        private Player player;
        private NPC theo;
        private NPC marlet;
        private float flashIntensity = 0f;
        #endregion

        public CS11_BossMid(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_BossMid(Player player)
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
            theo = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Theo"));
            marlet = level.Entities.OfType<NPC>().FirstOrDefault(npc => npc.GetType().Name.Contains("Marlet"));
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Theo notices Marlet is fighting back
            yield return Textbox.Say(DIALOG_KEY, TheoNotices);
            
            yield return 0.3f;
            
            // Marlet starts to remember
            yield return Textbox.Say(DIALOG_KEY, MarletRemembering);
            
            // Visual struggle effect
            Add(new Coroutine(InternalStruggleEffect(level)));
            
            yield return 0.5f;
            
            // Dark Matter tries to maintain control
            yield return Textbox.Say(DIALOG_KEY, DarkMatterResists);
            
            yield return 0.3f;
            
            // Marlet's powerful resistance
            yield return Textbox.Say(DIALOG_KEY, MarletResistsHard);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_MARLET_RESISTING, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator TheoNotices()
        {
            // [THEO left worried]
            if (theo != null && theo.Sprite != null)
            {
                theo.Sprite.Play("worried");
                theo.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MarletRemembering()
        {
            // [MARLET right normal]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("confused");
                marlet.Sprite.Scale.X = 1;
            }
            
            Audio.Play(SFX_RESISTANCE);
            yield break;
        }

        private IEnumerator DarkMatterResists()
        {
            // [marletpossesed right normal]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("possessed_angry");
            }
            
            Audio.Play(SFX_STRUGGLE);
            yield break;
        }

        private IEnumerator MarletResistsHard()
        {
            // [MARLET right upset]
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Play("angry_fighting");
            }
            yield break;
        }
        
        #endregion

        #region Visual Effects
        
        private IEnumerator InternalStruggleEffect(Level level)
        {
            if (marlet == null) yield break;
            
            float duration = 3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                
                // Alternating colors to show internal struggle
                Color flashColor = (elapsed % 0.4f < 0.2f) ? Color.Purple : Color.White;
                flashIntensity = (float)Math.Sin(elapsed * 10f) * 0.3f;
                
                // Shake Marlet sprite
                if (marlet.Sprite != null)
                {
                    marlet.Sprite.Position = new Vector2(
                        Calc.Random.Range(-2f, 2f),
                        Calc.Random.Range(-2f, 2f)
                    );
                }
                
                // Screen shake
                if (elapsed % 0.5f < Engine.DeltaTime)
                {
                    level.Shake(0.3f);
                }
                
                // Particles
                if (elapsed % 0.1f < Engine.DeltaTime)
                {
                    level.ParticlesFG.Emit(
                        ParticleTypes.Dust,
                        marlet.Position + new Vector2(Calc.Random.Range(-16f, 16f), Calc.Random.Range(-16f, 16f)),
                        flashColor
                    );
                }
                
                yield return null;
            }
            
            // Final powerful flash
            level.Flash(Color.White, true);
            level.Shake(0.8f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            
            // Reset sprite position
            if (marlet != null && marlet.Sprite != null)
            {
                marlet.Sprite.Position = Vector2.Zero;
            }
        }
        
        #endregion

        public override void Render()
        {
            base.Render();
            
            // Render struggle overlay
            if (flashIntensity > 0f && marlet != null)
            {
                Draw.Rect(
                    marlet.Position.X - 32f,
                    marlet.Position.Y - 32f,
                    64f,
                    64f,
                    Color.Purple * flashIntensity
                );
            }
        }

        public override void OnEnd(Level level)
        {
            // Restore player control for continued battle
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.StateMachine.Locked = false;
            }
        }
    }
}
