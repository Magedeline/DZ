using MaggyHelper.Helpers;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Meta Knight Terminator Boss - Fast sword-based combat
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MetaKnightTerminatorBoss")]
    [Tracked]
    public class MetaKnightTerminatorBoss : BossActor
    {
        private int health = 400;
        private bool isDefeated = false;
        
        private enum AttackType
        {
            RapidSlash,
            TornadoSlash,
            DimensionalCape,
            SwordBeam,
            MetaQuick
        }
        
        private Sprite knightSprite;
        private VertexLight maskGlow;
        private bool isInvisible = false;
        private float dashSpeed = 400f;
        
        public MetaKnightTerminatorBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "meta_knight_boss", Vector2.One, 180f, true, true, 0.8f, new Hitbox(24f, 32f, -12f, -32f))
        {
            setupVisuals();
        }
        
        private void setupVisuals()
        {
            Add(knightSprite = new Sprite(GFX.Game, "characters/metaknight/"));
            knightSprite.AddLoop("idle", "mk_idle", 0.1f);
            knightSprite.AddLoop("slash", "mk_slash", 0.05f);
            knightSprite.AddLoop("dash", "mk_dash", 0.06f);
            knightSprite.Play("idle");
            knightSprite.CenterOrigin();
            
            Add(maskGlow = new VertexLight(Color.Yellow, 1f, 32, 48));
            maskGlow.Position = new Vector2(0f, -20f);
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(combatLoop()));
        }
        
        private IEnumerator combatLoop()
        {
            while (!isDefeated && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 5);
                yield return executeAttack(attack);
                yield return 1.5f;
            }
        }
        
        private IEnumerator executeAttack(AttackType attack)
        {
            knightSprite.Play("slash");
            
            switch (attack)
            {
                case AttackType.RapidSlash:
                    yield return rapidSlashAttack();
                    break;
                case AttackType.TornadoSlash:
                    yield return tornadoSlashAttack();
                    break;
                case AttackType.DimensionalCape:
                    yield return dimensionalCapeAttack();
                    break;
                case AttackType.SwordBeam:
                    yield return swordBeamAttack();
                    break;
                case AttackType.MetaQuick:
                    yield return metaQuickAttack();
                    break;
            }
            
            knightSprite.Play("idle");
        }
        
        private IEnumerator rapidSlashAttack()
        {
            Audio.Play("event:/metaknight_rapid_slash", Position);
            
            for (int i = 0; i < 5; i++)
            {
                // Create slash hitbox
                yield return 0.15f;
            }
        }
        
        private IEnumerator tornadoSlashAttack()
        {
            Audio.Play("event:/metaknight_tornado", Position);
            
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.6f, 48f, 96f, 0.4f);
            
            // Spin and create tornado
            for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
            {
                knightSprite.Rotation += Engine.DeltaTime * 10f;
                yield return null;
            }
            
            knightSprite.Rotation = 0f;
        }
        
        private IEnumerator dimensionalCapeAttack()
        {
            Audio.Play("event:/metaknight_cape_vanish", Position);
            
            isInvisible = true;
            Collidable = false;
            
            yield return 1f;
            
            // Teleport behind player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Position = player.Position + new Vector2(player.Facing == Facings.Right ? -48f : 48f, 0f);
            }
            
            Audio.Play("event:/metaknight_cape_appear", Position);
            isInvisible = false;
            Collidable = true;
        }
        
        private IEnumerator swordBeamAttack()
        {
            Audio.Play("event:/metaknight_sword_beam", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                // Create sword beam projectile
            }
            
            yield return 0.8f;
        }
        
        private IEnumerator metaQuickAttack()
        {
            knightSprite.Play("dash");
            Audio.Play("event:/metaknight_meta_quick", Position);
            
            // Ultra-fast dash attacks
            for (int i = 0; i < 3; i++)
            {
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Speed = direction * dashSpeed;
                }
                
                yield return 0.3f;
                Speed = Vector2.Zero;
                yield return 0.2f;
            }
        }
        
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/metaknight_damage", Position);
            
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
            Audio.Play("event:/metaknight_defeat", Position);
            
            yield return 1f;
            
            var level = Scene as Level;
            level?.Session.SetFlag("metaknight_terminator_boss_defeated");
            
            RemoveSelf();
        }
        
        public override void Render()
        {
            if (!isInvisible)
            {
                base.Render();
            }
        }
    }
    
    /// <summary>
    /// Digital King DDD Boss - Cyber-corrupted version
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DigitalKingDDDBoss")]
    [Tracked]
    public class DigitalKingDDDBoss : BossActor
    {
        private int health = 600;
        private bool isDefeated = false;
        
        private Sprite dddSprite;
        
        public DigitalKingDDDBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "digital_ddd_boss", Vector2.One, 200f, true, true, 1f, new Hitbox(40f, 56f, -20f, -56f))
        {
            Add(dddSprite = new Sprite(GFX.Game, "characters/digitalddd/"));
            dddSprite.AddLoop("idle", "ddd_idle", 0.1f);
            dddSprite.CenterOrigin();
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(combatLoop()));
        }
        
        private IEnumerator combatLoop()
        {
            while (!isDefeated && health > 0)
            {
                yield return hammerSlamAttack();
                yield return 2f;
            }
        }
        
        private IEnumerator hammerSlamAttack()
        {
            Audio.Play("event:/ddd_hammer_slam", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            yield return 1f;
        }
        
        public override void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) { isDefeated = true; RemoveSelf(); }
        }
    }
    
    /// <summary>
    /// Martlet Bird Possess Boss - Corrupted bird boss
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MartletBirdPossessBoss")]
    [Tracked]
    public class MartletBirdPossessBoss : BossActor
    {
        private int health = 350;
        private bool isDefeated = false;
        
        private Sprite birdSprite;
        
        public MartletBirdPossessBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "martlet_boss", Vector2.One, 160f, true, false, 0.5f, new Hitbox(20f, 24f, -10f, -24f))
        {
            Add(birdSprite = new Sprite(GFX.Game, "characters/martlet/"));
            birdSprite.AddLoop("fly", "martlet_fly", 0.1f);
            birdSprite.Play("fly");
            birdSprite.CenterOrigin();
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(flyingCombat()));
        }
        
        private IEnumerator flyingCombat()
        {
            while (!isDefeated && health > 0)
            {
                // Aerial attacks
                yield return featherBarrageAttack();
                yield return 2f;
            }
        }
        
        private IEnumerator featherBarrageAttack()
        {
            Audio.Play("event:/martlet_feather_barrage", Position);
            
            for (int i = 0; i < 10; i++)
            {
                // Create feather projectiles
                yield return 0.1f;
            }
        }
        
        public override void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) { isDefeated = true; RemoveSelf(); }
        }
    }
    
    /// <summary>
    /// Black/Dark Matter Boss
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/BlackDarkMatterBoss")]
    [Tracked]
    public class BlackDarkMatterBoss : BossActor
    {
        private int health = 450;
        private bool isDefeated = false;
        
        private Sprite darkMatterSprite;
        private VertexLight darkGlow;
        
        public BlackDarkMatterBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "dark_matter_boss", Vector2.One, 0f, true, false, 0f, new Hitbox(32f, 32f, -16f, -16f))
        {
            Add(darkMatterSprite = new Sprite(GFX.Game, "characters/darkmatter/"));
            darkMatterSprite.AddLoop("idle", "dm_idle", 0.1f);
            darkMatterSprite.Play("idle");
            darkMatterSprite.CenterOrigin();
            
            Add(darkGlow = new VertexLight(Color.Purple, 1f, 64, 96));
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(darkCombat()));
        }
        
        private IEnumerator darkCombat()
        {
            while (!isDefeated && health > 0)
            {
                yield return voidBeamAttack();
                yield return 2.5f;
            }
        }
        
        private IEnumerator voidBeamAttack()
        {
            Audio.Play("event:/darkmatter_void_beam", Position);
            
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.8f, 64f, 128f, 0.6f);
            
            yield return 1.5f;
        }
        
        public override void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) { isDefeated = true; RemoveSelf(); }
        }
    }
    
    /// <summary>
    /// Dark Matter with Black Knife Boss - Enhanced version
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DarkMatterKnifeBoss")]
    [Tracked]
    public class DarkMatterKnifeBoss : BossActor
    {
        private int health = 550;
        private bool isDefeated = false;
        
        private Sprite darkMatterSprite;
        private Sprite knifeSprite;
        
        public DarkMatterKnifeBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "dark_matter_knife_boss", Vector2.One, 0f, true, false, 0f, new Hitbox(32f, 32f, -16f, -16f))
        {
            Add(darkMatterSprite = new Sprite(GFX.Game, "characters/darkmatter/"));
            darkMatterSprite.AddLoop("idle", "dmk_idle", 0.1f);
            darkMatterSprite.Play("idle");
            darkMatterSprite.CenterOrigin();
            
            Add(knifeSprite = new Sprite(GFX.Game, "characters/darkmatter/"));
            knifeSprite.AddLoop("spin", "knife_spin", 0.08f);
            knifeSprite.Play("spin");
            knifeSprite.Position = new Vector2(24f, 0f);
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(knifeCombat()));
        }
        
        private IEnumerator knifeCombat()
        {
            while (!isDefeated && health > 0)
            {
                yield return knifeThrowAttack();
                yield return 2f;
            }
        }
        
        private IEnumerator knifeThrowAttack()
        {
            Audio.Play("event:/darkmatter_knife_throw", Position);
            
            // Throw spinning knives
            for (int i = 0; i < 5; i++)
            {
                yield return 0.3f;
            }
        }
        
        public override void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) { isDefeated = true; RemoveSelf(); }
        }
    }
}




