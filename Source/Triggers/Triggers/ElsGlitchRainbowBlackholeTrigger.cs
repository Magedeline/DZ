using Monocle;
using Celeste.Entities;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger that combines ELS glitch effects with Rainbow Blackhole strength control.
    /// When the player enters, it triggers a glitch effect and simultaneously changes
    /// the Rainbow Blackhole background strength.
    /// </summary>
    [CustomEntity(ids: "DZ/ElsGlitchRainbowBlackholeTrigger")]
    public class ElsGlitchRainbowBlackholeTrigger : Trigger
    {
        public enum GlitchDuration
        {
            Short,
            Medium,
            Long
        }

        public enum BlackholeStrength
        {
            Mild,
            Medium,
            High,
            Wild,
            Insane,
            RainbowChaos,
            Cosmic
        }

        private GlitchDuration glitchDuration;
        private BlackholeStrength blackholeStrength;
        private bool triggered;
        private bool stayOn;
        private bool running;
        private bool doGlitch;
        private bool changeBlackholeStrength;
        private bool triggerOnce;
        private string sessionFlag;
        private Level level;

        public ElsGlitchRainbowBlackholeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            // Parse glitch duration
            string durationName = data.Attr("glitchDuration", "Medium");
            if (!Enum.TryParse(durationName, true, out glitchDuration))
            {
                glitchDuration = GlitchDuration.Medium;
            }

            // Parse blackhole strength
            string strengthName = data.Attr("blackholeStrength", "Medium");
            if (!Enum.TryParse(strengthName, true, out blackholeStrength))
            {
                blackholeStrength = BlackholeStrength.Medium;
            }

            stayOn = data.Bool("stayOn", false);
            doGlitch = data.Bool("doGlitch", true);
            changeBlackholeStrength = data.Bool("changeBlackholeStrength", true);
            triggerOnce = data.Bool("triggerOnce", true);
            sessionFlag = data.Attr("sessionFlag", "");

            triggered = false;
            running = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            // Check if we should trigger based on session flag
            if (!string.IsNullOrEmpty(sessionFlag) && level != null)
            {
                if (level.Session.GetFlag(sessionFlag))
                {
                    return; // Flag already set, don't trigger
                }
            }

            Invoke();
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            if (!stayOn)
            {
                Audio.SetMusicParam("glitch", 0f);
            }
        }

        public void Invoke()
        {
            if (triggerOnce && triggered)
                return;

            triggered = true;

            // Set session flag if specified
            if (!string.IsNullOrEmpty(sessionFlag) && level != null)
            {
                level.Session.SetFlag(sessionFlag, true);
            }

            // Change Rainbow Blackhole strength first
            if (changeBlackholeStrength)
            {
                ChangeRainbowBlackholeStrength();
            }

            // Then trigger glitch effect
            if (doGlitch)
            {
                Add(new Coroutine(InternalGlitchRoutine()));
            }
            else
            {
                if (!stayOn)
                {
                    ToggleRainbowBlackhole(false);
                }
            }
        }

        private void ChangeRainbowBlackholeStrength()
        {
            if (level == null) return;

            RainbowBlackholeBg.Strengths backdropStrength = blackholeStrength switch
            {
                BlackholeStrength.Mild => RainbowBlackholeBg.Strengths.Mild,
                BlackholeStrength.Medium => RainbowBlackholeBg.Strengths.Medium,
                BlackholeStrength.High => RainbowBlackholeBg.Strengths.High,
                BlackholeStrength.Wild => RainbowBlackholeBg.Strengths.Wild,
                BlackholeStrength.RainbowChaos => RainbowBlackholeBg.Strengths.RainbowChaos,
                BlackholeStrength.Cosmic => RainbowBlackholeBg.Strengths.Cosmic,
                _ => RainbowBlackholeBg.Strengths.Insane,
            };
            RainbowBlackholeBg.Strengths rainbowMode = (blackholeStrength == BlackholeStrength.RainbowChaos || blackholeStrength == BlackholeStrength.Cosmic) 
                ? RainbowBlackholeBg.Strengths.RainbowChaos 
                : backdropStrength;

            // Find and update all RainbowBlackholeBg instances
            foreach (var backdrop in level.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.RainbowMode = rainbowMode;
                    rainbowBg.NextStrength(level, backdropStrength);
                    rainbowBg.Visible = true;
                }
            }

            foreach (var backdrop in level.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.RainbowMode = rainbowMode;
                    rainbowBg.NextStrength(level, backdropStrength);
                    rainbowBg.Visible = true;
                }
            }

            // Set session flag for strength
            level.Session.SetFlag($"rainbow_blackhole_strength_{blackholeStrength.ToString().ToLower()}", true);
        }

        private IEnumerator InternalGlitchRoutine()
        {
            running = true;
            Tag = (int)Tags.Persistent;

            float duration = glitchDuration switch
            {
                GlitchDuration.Short => 0.2f,
                GlitchDuration.Medium => 0.5f,
                GlitchDuration.Long => 1.25f,
                _ => 0.5f
            };

            // Play appropriate glitch sound and rumble
            if (glitchDuration == GlitchDuration.Short)
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("event:/final_content/game/19_TheEnd/giygas_glitch_short");
            }
            else if (glitchDuration == GlitchDuration.Medium)
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("event:/final_content/game/19_TheEnd/giygas_glitch_medium");
            }
            else
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                Audio.Play("event:/final_content/game/19_TheEnd/giygas_glitch_long");
            }

            // Set music parameter for glitch
            Audio.SetMusicParam("giygas_glitch", 1f);

            // Create glitch tween effect
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
            tween.OnUpdate = (t) => Glitch.Value = 0.6f * t.Eased;
            Add(tween);

            // Run the glitch routine
            yield return GlitchRoutine(duration, stayOn);

            Tag = 0;
            running = false;
        }

        private static void ToggleRainbowBlackhole(bool on)
        {
            Level scene = Engine.Scene as Level;
            if (scene == null) return;

            foreach (Backdrop backdrop in scene.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = on;
                }
            }

            foreach (Backdrop backdrop in scene.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Visible = on;
                }
            }
        }

        private static void FadeRainbowBlackhole(float alpha, bool max = false)
        {
            Level scene = Engine.Scene as Level;
            if (scene == null) return;

            foreach (Backdrop backdrop in scene.Background.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Alpha = max ? Math.Max(rainbowBg.Alpha, alpha) : alpha;
                }
            }

            foreach (Backdrop backdrop in scene.Foreground.Backdrops)
            {
                if (backdrop is RainbowBlackholeBg rainbowBg)
                {
                    rainbowBg.Alpha = max ? Math.Max(rainbowBg.Alpha, alpha) : alpha;
                }
            }
        }

        private static IEnumerator GlitchRoutine(float duration, bool stayOn)
        {
            ToggleRainbowBlackhole(true);

            if (duration > 0.4f)
            {
                Glitch.Value = 0.3f;
                yield return 0.2f;
                Glitch.Value = 0.0f;
                yield return duration - 0.4f;
                
                if (!stayOn)
                {
                    Glitch.Value = 0.3f;
                    yield return 0.2f;
                    Glitch.Value = 0.0f;
                }
            }
            else
            {
                Glitch.Value = 0.3f;
                yield return duration;
                Glitch.Value = 0.0f;
            }

            if (!stayOn)
            {
                float a;
                for (a = 0.0f; a < 1.0; a += Engine.DeltaTime / 0.1f)
                {
                    FadeRainbowBlackhole(a, true);
                    yield return null;
                }
                FadeRainbowBlackhole(1f);
                yield return duration;

                for (a = 0.0f; a < 1.0; a += Engine.DeltaTime / 0.1f)
                {
                    FadeRainbowBlackhole(1f - a);
                    yield return null;
                }
                FadeRainbowBlackhole(1f);
            }

            if (!stayOn)
            {
                ToggleRainbowBlackhole(false);
            }
        }

        public override void Removed(Scene scene)
        {
            if (running)
            {
                Glitch.Value = 0.0f;
                FadeRainbowBlackhole(1f);
                
                if (!stayOn)
                {
                    ToggleRainbowBlackhole(false);
                }
            }

            base.Removed(scene);
        }
    }
}




