using Celeste.Helpers;

namespace Celeste.Entities
{
    /// <summary>
    /// King Titan Boss - Ultimate titan battle
    /// Massive end-game boss encounter
    /// </summary>
    [CustomEntity(ids: "DZ/KingTitanBoss")]
    [Tracked]
    public class KingTitanBoss : BossActor
    {
        private int health = 2000;
        private int maxHealth = 2000;
        private bool isDefeated = false;
        private int currentPhase = 1;

        private Sprite titanSprite;
        private VertexLight titanGlow;
        private List<Sprite> armorPlates = new List<Sprite>();
        private SoundSource titanRoarSfx;

        private enum TitanAttackType
        {
            TitanicSlam,
            EarthquakeWave,
            MeteorRain,
            TitanRoar,
            ColossusStomp,
            RagingTempest,
            CataclysmicPillar
        }

        public KingTitanBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, "king_titan_boss", new Vector2(1.5f, 1.5f), 300f, true, true, 1.2f, new Hitbox(80f, 120f, -40f, -120f))
        {
            maxHealth = data.Int("maxHealth", 2000);
            health = Math.Min(data.Int("health", maxHealth), maxHealth);
            Health = health;
            MaxHealth = maxHealth;
            ConfigureEncounter(autoStart: false, hideUntilStart: true);
            setupTitanVisuals();
        }

        private void setupTitanVisuals()
        {
            Add(titanSprite = new Sprite(GFX.Game, "characters/kingtitan/"));
            titanSprite.AddLoop("idle", "titan_idle", 0.08f);
            titanSprite.AddLoop("attack", "titan_attack", 0.06f);
            titanSprite.AddLoop("phase2", "titan_phase2", 0.07f);
            titanSprite.AddLoop("phase3", "titan_phase3", 0.05f);
            titanSprite.Play("idle");
            titanSprite.CenterOrigin();

            Add(titanGlow = new VertexLight(Color.Orange, 1f, 192, 256));
            titanGlow.Position = new Vector2(0f, -60f);

            Add(titanRoarSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            StartBossRoutine(titanBattleSequence());
        }

        private IEnumerator titanBattleSequence()
        {
            var level = Scene as Level;

            // Epic intro
            Audio.Play("event:/titan_awakening", Position);
            titanRoarSfx.Play("event:/titan_roar_loop");

            level?.Shake(2f);
            level?.Flash(Color.Orange, true);

            yield return 3f;

            // Start combat
            while (!isDefeated && health > 0)
            {
                // Check phase transitions
                if (currentPhase == 1 && health <= maxHealth * 0.66f)
                {
                    yield return enterPhase2();
                }
                else if (currentPhase == 2 && health <= maxHealth * 0.33f)
                {
                    yield return enterPhase3();
                }

                var attack = (TitanAttackType)Calc.Random.Next(0, 7);
                yield return executeTitanAttack(attack);

                float cooldown = currentPhase switch
                {
                    1 => 3f,
                    2 => 2.5f,
                    3 => 2f,
                    _ => 3f
                };

                yield return cooldown;
            }
        }

        private IEnumerator enterPhase2()
        {
            currentPhase = 2;

            Audio.Play("event:/titan_phase2_transform", Position);
            titanSprite.Play("phase2");

            var level = Scene as Level;
            level?.Shake(2.5f);
            level?.Flash(Color.Red, true);

            titanGlow.Color = Color.Red;
            titanGlow.StartRadius = 256f;

            yield return 2f;
        }

        private IEnumerator enterPhase3()
        {
            currentPhase = 3;

            Audio.Play("event:/titan_phase3_transform", Position);
            titanSprite.Play("phase3");

            var level = Scene as Level;
            level?.Shake(3f);
            level?.Flash(Color.Purple, true);

            titanGlow.Color = Color.Purple;
            titanGlow.StartRadius = 320f;

            yield return 2f;
        }

        private IEnumerator executeTitanAttack(TitanAttackType attack)
        {
            yield return TelegraphIntent(attack switch
            {
                TitanAttackType.MeteorRain => BossTelegraphType.PositioningOrange,
                TitanAttackType.RagingTempest => BossTelegraphType.SpecialPurple,
                TitanAttackType.CataclysmicPillar => BossTelegraphType.SpecialPurple,
                _ => BossTelegraphType.DangerRed
            }, attack == TitanAttackType.CataclysmicPillar ? 0.65f : 0.5f);

            titanSprite.Play("attack");

            switch (attack)
            {
                case TitanAttackType.TitanicSlam:
                    yield return titanicSlamAttack();
                    break;
                case TitanAttackType.EarthquakeWave:
                    yield return earthquakeWaveAttack();
                    break;
                case TitanAttackType.MeteorRain:
                    yield return meteorRainAttack();
                    break;
                case TitanAttackType.TitanRoar:
                    yield return titanRoarAttack();
                    break;
                case TitanAttackType.ColossusStomp:
                    yield return colossusStompAttack();
                    break;
                case TitanAttackType.RagingTempest:
                    if (currentPhase >= 2)
                        yield return ragingTempestAttack();
                    break;
                case TitanAttackType.CataclysmicPillar:
                    if (currentPhase >= 3)
                        yield return cataclysmicPillarAttack();
                    break;
            }

            titanSprite.Play("idle");
        }

        private IEnumerator titanicSlamAttack()
        {
            Audio.Play("event:/titan_titanic_slam", Position);

            var level = Scene as Level;
            level?.Shake(3f);

            // Massive ground slam
            level?.Displacement.AddBurst(Position, 2f, 192f, 384f, 1.5f);

            yield return 2f;
        }

        private IEnumerator earthquakeWaveAttack()
        {
            Audio.Play("event:/titan_earthquake", Position);

            var level = Scene as Level;

            // Create earthquake waves spreading outward
            for (int i = 0; i < 5; i++)
            {
                level?.Shake(1.5f);
                yield return 0.3f;
            }
        }

        private IEnumerator meteorRainAttack()
        {
            Audio.Play("event:/titan_meteor_rain", Position);

            var level = Scene as Level;

            // Rain meteors from sky
            int meteorCount = currentPhase * 5;

            for (int i = 0; i < meteorCount; i++)
            {
                Vector2 meteorPos = Position + Calc.Random.Range(Vector2.One * -300f, Vector2.One * 300f);
                meteorPos.Y = level.Bounds.Top - 50f;

                // Create falling meteor

                yield return 0.2f;
            }

            yield return 1f;
        }

        private IEnumerator titanRoarAttack()
        {
            Audio.Play("event:/titan_roar_attack", Position);

            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Orange, false);

            // Shockwave from roar
            level?.Displacement.AddBurst(Position, 1.5f, 256f, 512f, 1f);

            yield return 2f;
        }

        private IEnumerator colossusStompAttack()
        {
            Audio.Play("event:/titan_colossus_stomp", Position);

            var level = Scene as Level;

            // Powerful stomp
            for (int i = 0; i < 3; i++)
            {
                level?.Shake(2.5f);
                level?.Displacement.AddBurst(Position, 1.5f, 128f, 256f, 0.8f);

                yield return 0.5f;
            }
        }

        private IEnumerator ragingTempestAttack()
        {
            Audio.Play("event:/titan_raging_tempest", Position);

            var level = Scene as Level;

            // Create swirling tempest
            for (float t = 0f; t < 3f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 96f, 192f, 0.3f);

                yield return null;
            }
        }

        private IEnumerator cataclysmicPillarAttack()
        {
            Audio.Play("event:/titan_cataclysmic_pillar", Position);

            var level = Scene as Level;

            // Summon pillars of destruction
            for (int i = 0; i < 4; i++)
            {
                float angle = (i / 4f) * MathHelper.TwoPi;
                Vector2 pillarPos = Position + new Vector2(
                    (float)System.Math.Cos(angle) * 150f,
                    (float)System.Math.Sin(angle) * 150f
                );

                // Create pillar
                level?.Shake(1f);
                level?.Displacement.AddBurst(pillarPos, 1.2f, 96f, 192f, 0.8f);

                yield return 0.5f;
            }
        }

        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;

            health = Math.Max(0, health - damage);
            Health = health;
            Audio.Play("event:/titan_damage", Position);

            var level = Scene as Level;
            level?.Shake(0.5f);

            if (health <= 0)
            {
                IsDefeated = true;
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
            Audio.Play("event:/titan_final_roar", Position);
            titanRoarSfx.Stop();

            var level = Scene as Level;

            // Epic death sequence
            level?.Shake(4f);
            level?.Flash(Color.White, true);

            for (int i = 0; i < 10; i++)
            {
                Vector2 explosionPos = Position + Calc.Random.Range(Vector2.One * -60f, Vector2.One * 60f);
                Audio.Play("event:/titan_explosion", explosionPos);
                level?.Displacement.AddBurst(explosionPos, 1.5f, 96f, 192f, 0.6f);

                yield return 0.3f;
            }

            // Titan falls
            Audio.Play("event:/titan_fall", Position);
            level?.Shake(5f);

            yield return 2f;

            level?.Session.SetFlag("king_titan_boss_defeated");

            // Trigger CS10_TitanBossBattle completion
            level?.Session.SetFlag("ch12_titan_boss_complete");

            RemoveSelf();
        }
    }
}
