// File 2: CS11_CinematicBar.cs
using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Cutscene for Chapter 11 - Cinematic Bar Scene
    /// Maggy explains the situation to Madeline's mother (Mom) and Theo's sister (Alex)
    /// Includes dramatic triggers: power blackout, ground shaking, and Mt. Celeste eruption with Dark Fountain spawn
    /// </summary>
    [CustomEntity("DesoloZantas/CS11_CinematicBar")]
    public class CS11_CinematicBar : CutsceneEntity
    {
        #region Constants
        private const string FLAG_CUTSCENE_COMPLETE = "ch11_cinematic_bar_complete";
        private const string FLAG_POWER_BLACKOUT = "ch11_power_blackout";
        private const string FLAG_EARTHQUAKE_STARTED = "ch11_earthquake_started";
        private const string FLAG_MOUNTAIN_ERUPTED = "ch11_mountain_erupted";
        private const string FLAG_DARK_FOUNTAIN_SPAWNED = "ch11_dark_fountain_spawned";
        
        private const string DIALOG_KEY = "CH11_CINEMATIC_BAR";
        
        // Audio event paths
        private const string SFX_POWER_BLACKOUT = "event:/game_06_boss_badeline_respawn";
        private const string SFX_EARTHQUAKE_RUMBLE = "event:/game_09_rumbling";
        private const string SFX_RISER = "event:/new_content/game/10_farewell/glitch_short";
        private const string SFX_MOUNTAIN_ERUPTION = "event:/new_content/game/10_farewell/bird_crash_whoosh";
        #endregion

        #region Fields
        private Player player;
        private NPC maggy;
        private NPC mom;
        private NPC alex;
        private float screenFade = 0f;
        #endregion

        public CS11_CinematicBar(EntityData data, Vector2 offset)
            : base(true, false)
        {
        }

        public CS11_CinematicBar(Player player)
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
            maggy = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Maggy") || npc.GetType().Name.Contains("Magolor"));
            mom = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Mom") || npc.GetType().Name.Contains("MadelineMother"));
            alex = level.Entities.FindFirst<NPC>(npc => npc.GetType().Name.Contains("Alex") || npc.GetType().Name.Contains("TheoSister"));
            */
        }

        private IEnumerator CutsceneSequence(Level level)
        {
            // Maggy finishes explaining
            yield return Textbox.Say(DIALOG_KEY, MaggyFinishesExplaining);
            
            // Mom reassures
            yield return Textbox.Say(DIALOG_KEY, MomReassures);
            
            // Maggy questions her optimism
            yield return Textbox.Say(DIALOG_KEY, MaggyQuestions);
            
            // Alex expresses concern
            yield return Textbox.Say(DIALOG_KEY, AlexConcerned);
            
            // Maggy explains about Chara
            yield return Textbox.Say(DIALOG_KEY, MaggyExplainsChara);
            
            // TRIGGER 0: Power blackout
            yield return Textbox.Say(DIALOG_KEY, Trigger0PowerBlackout);
            
            yield return 0.5f;
            
            // Maggy reacts to blackout
            yield return Textbox.Say(DIALOG_KEY, MaggyReactsBlackout);
            
            // Alex thinks power went out
            yield return Textbox.Say(DIALOG_KEY, AlexPowerOut);
            
            // TRIGGER 1: Ground starts to shake
            yield return Textbox.Say(DIALOG_KEY, Trigger1GroundShake);
            
            yield return 0.3f;
            
            // Alex panics
            yield return Textbox.Say(DIALOG_KEY, AlexPanics);
            
            // Mom notices mountain shaking
            yield return Textbox.Say(DIALOG_KEY, MomMountainShaking);
            
            // TRIGGER 2: Riser SFX
            yield return Textbox.Say(DIALOG_KEY, Trigger2RiserSfx);
            
            yield return 0.5f;
            
            // Maggy realizes it's bad
            yield return Textbox.Say(DIALOG_KEY, MaggyRealizesItsBad);
            
            // TRIGGER 3: Mt Celeste erupts and Dark Fountain spawns
            yield return Textbox.Say(DIALOG_KEY, Trigger3MountainEruption);

            // Set completion flags
            level.Session.SetFlag(FLAG_CUTSCENE_COMPLETE, true);
            level.Session.SetFlag(FLAG_POWER_BLACKOUT, true);
            level.Session.SetFlag(FLAG_EARTHQUAKE_STARTED, true);
            level.Session.SetFlag(FLAG_MOUNTAIN_ERUPTED, true);
            level.Session.SetFlag(FLAG_DARK_FOUNTAIN_SPAWNED, true);

            EndCutscene(level);
        }

        #region Dialog Handlers
        
        private IEnumerator MaggyFinishesExplaining()
        {
            // [Maggy left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
                maggy.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MomReassures()
        {
            // [MOM left concerned]
            if (mom != null && mom.Sprite != null)
            {
                mom.Sprite.Play("concerned");
                mom.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MaggyQuestions()
        {
            // [Maggy left annoyed]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("annoyed");
            }
            yield break;
        }

        private IEnumerator AlexConcerned()
        {
            // [ALEX left concerned]
            if (alex != null && alex.Sprite != null)
            {
                alex.Sprite.Play("concerned");
                alex.Sprite.Scale.X = -1;
            }
            yield break;
        }

        private IEnumerator MaggyExplainsChara()
        {
            // [Maggy left normal]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("idle");
            }
            yield break;
        }

        private IEnumerator Trigger0PowerBlackout()
        {
            // {trigger 0 power black out}
            Add(new Coroutine(PowerBlackoutSequence()));
            yield break;
        }

        private IEnumerator PowerBlackoutSequence()
        {
            Level level = Scene as Level;
            
            // Play blackout sound
            Audio.Play(SFX_POWER_BLACKOUT);
            
            // Fade to black quickly
            float fadeSpeed = 2f;
            while (screenFade < 1f)
            {
                screenFade += Engine.DeltaTime * fadeSpeed;
                yield return null;
            }
            screenFade = 1f;
            
            // Stay dark for a moment
            yield return 0.5f;
            
            // Fade back in slightly (dim lighting)
            while (screenFade > 0.6f)
            {
                screenFade -= Engine.DeltaTime * fadeSpeed;
                yield return null;
            }
            screenFade = 0.6f;
        }

        private IEnumerator MaggyReactsBlackout()
        {
            // [Maggy left annoyed]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("annoyed");
            }
            yield break;
        }

        private IEnumerator AlexPowerOut()
        {
            // [alex left worried]
            if (alex != null && alex.Sprite != null)
            {
                alex.Sprite.Play("worried");
            }
            yield break;
        }

        private IEnumerator Trigger1GroundShake()
        {
            // {trigger 1 ground start to shake}
            Add(new Coroutine(GroundShakeSequence()));
            yield break;
        }

        private IEnumerator GroundShakeSequence()
        {
            Level level = Scene as Level;
            
            // Play earthquake rumble
            Audio.Play(SFX_EARTHQUAKE_RUMBLE);
            
            // Start continuous shaking
            level.Shake(0.3f);
            yield return 0.5f;
            
            // Increase shake intensity
            level.Shake(0.5f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }

        private IEnumerator AlexPanics()
        {
            // [alex left worried]
            if (alex != null && alex.Sprite != null)
            {
                alex.Sprite.Play("worried");
            }
            yield break;
        }

        private IEnumerator MomMountainShaking()
        {
            // [MOM left concerned]
            if (mom != null && mom.Sprite != null)
            {
                mom.Sprite.Play("concerned");
            }
            yield break;
        }

        private IEnumerator Trigger2RiserSfx()
        {
            // {trigger 2 riser sfx}
            Add(new Coroutine(RiserSequence()));
            yield break;
        }

        private IEnumerator RiserSequence()
        {
            // Play riser sound effect
            Audio.Play(SFX_RISER);
            
            // Increase shake intensity further
            Level level = Scene as Level;
            level.Shake(0.7f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            
            yield return 1f;
        }

        private IEnumerator MaggyRealizesItsBad()
        {
            // [Maggy left annoyed]
            if (maggy != null && maggy.Sprite != null)
            {
                maggy.Sprite.Play("annoyed");
            }
            yield break;
        }

        private IEnumerator Trigger3MountainEruption()
        {
            // {trigger 3 mt celeste erupted and dark fountain spawned in destructively}
            Add(new Coroutine(MountainEruptionSequence()));
            yield break;
        }

        private IEnumerator MountainEruptionSequence()
        {
            Level level = Scene as Level;
            
            // Play eruption sound
            Audio.Play(SFX_MOUNTAIN_ERUPTION);
            
            // Maximum screen shake
            level.Shake(1.5f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            
            // Flash effect
            level.Flash(Color.Red * 0.5f, true);
            
            yield return 0.5f;
            
            // Another flash for dark fountain
            level.Flash(Color.Purple * 0.8f, true);
            
            yield return 1f;
            
            // Gradual fade to show destruction
            while (screenFade < 1f)
            {
                screenFade += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            
            yield return 1.5f;
            
            // Fade back in to show aftermath
            while (screenFade > 0f)
            {
                screenFade -= Engine.DeltaTime * 0.8f;
                yield return null;
            }
            screenFade = 0f;
        }
        
        #endregion

        public override void Render()
        {
            base.Render();
            
            // Render screen fade overlay
            if (screenFade > 0f)
            {
                Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * screenFade);
            }
        }

        public override void OnEnd(Level level)
        {
            // Restore player control
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
                player.StateMachine.Locked = false;
            }
            
            // Clear any ongoing shakes
            level.Shake(0f);
        }
    }
}