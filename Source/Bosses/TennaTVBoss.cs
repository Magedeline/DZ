using Celeste.Helpers;

namespace Celeste.Entities
{
    /// <summary>
    /// Tenna TV Boss - Mini boss with screen-based attacks
    /// A sentient TV that broadcasts deadly content
    /// </summary>
    [CustomEntity(ids: "DZ/TennaTVBoss")]
    [Tracked]
    public class TennaTVBoss : BossActor
    {
        // Boss properties
        private int health = 300;
        private bool isDefeated = false;
        
        // Attack patterns
        private enum AttackType
        {
            StaticShock,
            BroadcastBeam,
            SignalJam,
            ScreenFlash,
            ChannelSwap
        }
        
        // Visual components
        private Sprite tvSprite;
        private Sprite screenSprite;
        private VertexLight tvGlow;
        private SoundSource staticSfx;
        private SoundSource channelSfx;
        
        // Attack state
        private AttackType currentAttack;
        private float attackTimer = 0f;
        private float attackCooldown = 2.5f;
        private int currentChannel = 1;
#pragma warning disable CS0414
        private bool isChannelChanging = false;
#pragma warning restore CS0414
        
        // Screen effects
        private Color screenColor = Color.CornflowerBlue;
        private float screenFlicker = 0f;
        private SineWave scanlines;
        
        public TennaTVBoss(EntityData data, Vector2 offset) 
            : base(
                data.Position + offset,
                "tenna_tv_boss",
                new Vector2(1f, 1f),
                maxFall: 160f,
                collidable: true,
                solidCollidable: true,
                gravityMult: 1f,
                collider: new Hitbox(48f, 56f, -24f, -56f)
            )
        {
            setupVisuals();
        }
        
        private void setupVisuals()
        {
            // TV sprite
            Add(tvSprite = new Sprite(GFX.Game, "characters/DZ/tenna/"));
            tvSprite.AddLoop("idle", "tv_idle", 0.1f);
            tvSprite.AddLoop("attack", "tv_attack", 0.08f);
            tvSprite.AddLoop("damaged", "tv_damaged", 0.12f);
            tvSprite.AddLoop("channelDZ_CHange", "tvDZ_CHannelDZ_CHange", 0.15f);
            tvSprite.Play("idle");
            tvSprite.CenterOrigin();
            
            // Screen sprite (separate layer)
            Add(screenSprite = new Sprite(GFX.Game, "characters/DZ/tenna/"));
            screenSprite.AddLoop("static", "screen_static", 0.05f);
            screenSprite.AddLoop("channel_1", "screenDZ_CH1", 0.1f);
            screenSprite.AddLoop("channel_2", "screenDZ_CH2", 0.1f);
            screenSprite.AddLoop("channel_3", "screenDZ_CH3", 0.1f);
            screenSprite.Play("channel_1");
            screenSprite.CenterOrigin();
            screenSprite.Position = new Vector2(0f, -20f);
            
            // TV glow
            Add(tvGlow = new VertexLight(Color.Cyan, 1f, 64, 96));
            tvGlow.Position = new Vector2(0f, -20f);
            
            // Scanlines effect
            Add(scanlines = new SineWave(0.5f, 0f));
            scanlines.Randomize();
            
            // Audio
            Add(staticSfx = new SoundSource());
            Add(channelSfx = new SoundSource());
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Start intro sequence
            Add(new Coroutine(introSequence()));
        }
        
        private IEnumerator introSequence()
        {
            var level = Scene as Level;
            
            // TV turns on
            Audio.Play("event:/tenna_tv_power_on", Position);
            screenSprite.Play("static");
            staticSfx.Play("event:/tenna_tv_static_loop");
            
            yield return 1f;
            
            // Channel changes to first content
            changeChannel(1);
            
            yield return 0.5f;
            
            // Start attacking
            Add(new Coroutine(attackLoop()));
        }
        
        private IEnumerator attackLoop()
        {
            while (!isDefeated && health > 0)
            {
                // Choose random attack
                currentAttack = (AttackType)Calc.Random.Next(0, 5);
                
                yield return executeAttack(currentAttack);
                
                // Cooldown
                yield return attackCooldown;
            }
        }
        
        private IEnumerator executeAttack(AttackType attack)
        {
            tvSprite.Play("attack");
            
            switch (attack)
            {
                case AttackType.StaticShock:
                    yield return staticShockAttack();
                    break;
                case AttackType.BroadcastBeam:
                    yield return broadcastBeamAttack();
                    break;
                case AttackType.SignalJam:
                    yield return signalJamAttack();
                    break;
                case AttackType.ScreenFlash:
                    yield return screenFlashAttack();
                    break;
                case AttackType.ChannelSwap:
                    yield return channelSwapAttack();
                    break;
            }
            
            tvSprite.Play("idle");
        }
        
        private IEnumerator staticShockAttack()
        {
            Audio.Play("event:/tenna_static_shock", Position);
            screenSprite.Play("static");
            
            var level = Scene as Level;
            
            // Create static projectiles in a circle
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 direction = new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                );
                
                // Create static projectile
                // level?.Add(new StaticProjectile(Position, direction * 100f));
            }
            
            yield return 0.8f;
        }
        
        private IEnumerator broadcastBeamAttack()
        {
            Audio.Play("event:/tenna_broadcast_beam", Position);
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                // Create beam towards player
                // level?.Add(new BroadcastBeam(Position, direction));
            }
            
            yield return 1.5f;
        }
        
        private IEnumerator signalJamAttack()
        {
            Audio.Play("event:/tenna_signal_jam", Position);
            screenSprite.Play("static");
            staticSfx.Play("event:/tenna_tv_static_loop");
            
            var level = Scene as Level;
            
            // Jam player controls for 2 seconds
            // level?.Session.SetFlag("tenna_signal_jam_active");
            
            yield return 2f;
            
            // level?.Session.SetFlag("tenna_signal_jam_active", false);
            staticSfx.Stop();
        }
        
        private IEnumerator screenFlashAttack()
        {
            Audio.Play("event:/tenna_screen_flash", Position);
            
            var level = Scene as Level;
            
            // Flash screen multiple times
            for (int i = 0; i < 3; i++)
            {
                screenColor = Color.White;
                level?.Flash(screenColor, false);
                
                yield return 0.2f;
                
                screenColor = Color.Black;
                
                yield return 0.2f;
            }
            
            screenColor = Color.CornflowerBlue;
        }
        
        private IEnumerator channelSwapAttack()
        {
            isChannelChanging = true;
            tvSprite.Play("channelDZ_CHange");
            channelSfx.Play("event:/tennaDZ_CHannelDZ_CHange");
            
            // Cycle through channels
            for (int i = 0; i < 3; i++)
            {
                currentChannel = (currentChannel % 3) + 1;
                changeChannel(currentChannel);
                
                // Each channel has different effects
                yield return channelEffect(currentChannel);
                
                yield return 0.5f;
            }
            
            isChannelChanging = false;
        }
        
        private void changeChannel(int channel)
        {
            currentChannel = channel;
            screenSprite.Play($"channel_{channel}");
            
            // Change screen color based on channel
            switch (channel)
            {
                case 1:
                    screenColor = Color.Cyan;
                    tvGlow.Color = Color.Cyan;
                    break;
                case 2:
                    screenColor = Color.Magenta;
                    tvGlow.Color = Color.Magenta;
                    break;
                case 3:
                    screenColor = Color.Yellow;
                    tvGlow.Color = Color.Yellow;
                    break;
            }
        }
        
        private IEnumerator channelEffect(int channel)
        {
            var level = Scene as Level;
            
            switch (channel)
            {
                case 1: // News channel - spawns text projectiles
                    Audio.Play("event:/tenna_news_broadcast", Position);
                    for (int i = 0; i < 5; i++)
                    {
                        // Spawn news ticker projectiles
                    }
                    break;
                case 2: // Action channel - spawns explosions
                    Audio.Play("event:/tenna_actionDZ_CHannel", Position);
                    level?.Shake(0.5f);
                    break;
                case 3: // Horror channel - screen distortion
                    Audio.Play("event:/tenna_horrorDZ_CHannel", Position);
                    level?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.5f);
                    break;
            }
            
            yield return 1f;
        }
        
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            tvSprite.Play("damaged");
            
            Audio.Play("event:/tenna_take_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.3f);
            
            if (health <= 0)
            {
                defeat();
            }
        }
        
        private void defeat()
        {
            isDefeated = true;
            
            Add(new Coroutine(defeatSequence()));
        }
        
        private IEnumerator defeatSequence()
        {
            Audio.Play("event:/tenna_defeat", Position);
            
            screenSprite.Play("static");
            staticSfx.Play("event:/tenna_tv_static_loop");
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            // Flicker and die
            for (int i = 0; i < 5; i++)
            {
                tvGlow.Alpha = 0f;
                yield return 0.1f;
                tvGlow.Alpha = 1f;
                yield return 0.1f;
            }
            
            // TV turns off
            tvGlow.Alpha = 0f;
            staticSfx.Stop();
            Audio.Play("event:/tenna_tv_power_off", Position);
            
            yield return 1f;
            
            // Set defeat flag
            level?.Session.SetFlag("tenna_tv_boss_defeated");
            
            RemoveSelf();
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update screen flicker
            screenFlicker = scanlines.Value * 0.1f;
            
            // Update timers
            if (attackTimer > 0f)
                attackTimer -= Engine.DeltaTime;
        }
        
        public override void Render()
        {
            // Render with screen effects
            if (screenFlicker > 0f)
            {
                screenSprite.Color = Color.Lerp(Color.White, screenColor, 1f - screenFlicker);
            }
            
            base.Render();
        }
    }
}
