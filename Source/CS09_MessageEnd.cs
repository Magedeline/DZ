namespace MaggyHelper.Cutscenes
{
    /// <summary>
    /// Sans voice message cutscene for Chapter 9 ending.
    /// Sans leaves a message to Madeline warning that something is happening
    /// at Mt. Desolo Zantas and they need backup.
    /// Plays after credits and before area complete.
    /// </summary>
    public class CS09_MessageEnd : CutsceneEntity
    {
        public const string FlagMessageComplete = "ch9_sans_message_complete";
        
        private global::Celeste.Player player;
        private float fadeAlpha;
        private Entity phone;
        private Sprite phoneSprite;
        
        public CS09_MessageEnd(global::Celeste.Player player) : base(true, false)
        {
            base.Depth = -8500;
            this.player = player;
        }
        
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(MessageSequence(level), true));
        }
        
        private IEnumerator MessageSequence(Level level)
        {
            // Set player to dummy state and make invisible
            if (player != null)
            {
                player.StateMachine.State = 11;
                player.DummyGravity = true;
                player.Speed = Vector2.Zero;
                player.Visible = false;
            }
            
            // Fade to black background for phone call atmosphere
            fadeAlpha = 0.9f;
            
            yield return 0.5f;
            
            // Spawn phone entity in center of screen
            Vector2 phonePos = new Vector2(level.Camera.X + 160f, level.Camera.Y + 90f);
            phone = new Entity(phonePos) { Depth = -10000 };
            
            // Try to create phone sprite
            try
            {
                phoneSprite = GFX.SpriteBank.Create("phone");
                phoneSprite.Play("idle");
            }
            catch
            {
                // Fallback if phone sprite doesn't exist
                phoneSprite = new Sprite(GFX.Game, "objects/phone/");
                phoneSprite.AddLoop("idle", "idle", 0.1f);
                phoneSprite.Play("idle");
            }
            
            phone.Add(phoneSprite);
            level.Add(phone);
            
            // Phone ring sound
            Audio.Play("event:/game/02_old_site/theoselfie_phone_ring", phonePos);
            yield return 0.8f;
            
            yield return 0.3f;
            
            // Play the voice message dialog
            yield return Textbox.Say("CH9_SANS_MESSAGE_END", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(PhoneRings),
                new Func<IEnumerator>(SansWarning),
                new Func<IEnumerator>(SansRequestsBackup),
                new Func<IEnumerator>(MessageEnds),
                new Func<IEnumerator>(PapyrusSpeaks),
                new Func<IEnumerator>(UndyneSpeaks),
                new Func<IEnumerator>(AlphysSpeaks),
                new Func<IEnumerator>(AsgoreSpeaks),
                new Func<IEnumerator>(TorielSpeaks)
            });
            
            yield return 0.5f;
            
            // Remove phone
            if (phone != null && phone.Scene != null)
            {
                phone.RemoveSelf();
                phone = null;
            }
            
            // Fade out
            fadeAlpha = 0f;
            
            yield return 0.5f;
            
            // Mark message as complete
            level.Session.SetFlag(FlagMessageComplete, true);
            
            // Teleport to area complete
            level.Add(new CS09_AreaComplete(player, skipCredits: true, skipMessage: true));
            
            EndCutscene(level, false);
        }
        
        private IEnumerator PhoneRings()
        {
            if (phone != null)
            {
                // Phone vibration effect
                for (int i = 0; i < 3; i++)
                {
                    phone.Position += new Vector2(1f, 0f);
                    yield return 0.05f;
                    phone.Position -= new Vector2(2f, 0f);
                    yield return 0.05f;
                    phone.Position += new Vector2(1f, 0f);
                    yield return 0.1f;
                }
            }
            yield return 0.2f;
        }
        
        private IEnumerator SansWarning()
        {
            // Dramatic pause effect
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            yield return 0.3f;
        }
        
        private IEnumerator SansRequestsBackup()
        {
            // Screen slight shake for emphasis
            if (Level != null)
            {
                Level.Shake(0.2f);
            }
            yield return 0.2f;
        }
        
        private IEnumerator MessageEnds()
        {
            // Phone hang up sound
            Audio.Play("event:/game/02_old_site/theoselfie_phone_end", player?.Position ?? Position);
            yield return 0.5f;
        }
        
        private IEnumerator PapyrusSpeaks()
        {
            // Papyrus's enthusiastic energy!
            if (Level != null)
            {
                Level.Shake(0.1f);
            }
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            yield return 0.2f;
        }
        
        private IEnumerator UndyneSpeaks()
        {
            // Undyne's fierce energy - stronger shake
            if (Level != null)
            {
                Level.Shake(0.3f);
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 0.3f;
        }
        
        private IEnumerator AlphysSpeaks()
        {
            // Alphys's nervous energy - subtle effect
            if (phone != null)
            {
                phone.Position += new Vector2(0.5f, 0f);
                yield return 0.03f;
                phone.Position -= new Vector2(0.5f, 0f);
            }
            yield return 0.1f;
        }
        
        private IEnumerator AsgoreSpeaks()
        {
            // Asgore's warm presence - gentle fade effect
            fadeAlpha = 0.05f;
            yield return 0.5f;
            fadeAlpha = 0f;
        }
        
        private IEnumerator TorielSpeaks()
        {
            // Toriel's motherly warmth - soft rumble
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            yield return 0.3f;
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw dark overlay for phone call atmosphere
            if (fadeAlpha > 0f)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fadeAlpha);
            }
        }
        
        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = 0;
                // Keep player invisible - will be restored by area complete
            }
            
            if (phone != null && phone.Scene != null)
            {
                phone.RemoveSelf();
            }
        }
    }
}
