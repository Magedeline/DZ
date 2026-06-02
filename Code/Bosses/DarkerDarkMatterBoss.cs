using Celeste.Helpers;

namespace Celeste.Entities.Bosses
{
    /// <summary>
    /// Darker Dark Matter Boss - The true form of Dark Matter
    /// An enhanced version of Dark Matter with stronger void powers
    /// Multi-phase boss with reality-warping abilities
    /// Sprite path: characters/darkerdarkmatter/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DarkerDarkMatterBoss")]
    [Tracked]
    public class DarkerDarkMatterBoss : BossActor
    {
        #region Enums and Constants
        public enum BossPhase
        {
            Awakening,
            Phase1_Eye,
            Phase2_Swordsman,
            Phase3_TrueForm,
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks - Eye Form
            DarkBeam,
            VoidOrbs,
            DimensionalTear,
            GravityPull,
            
            // Phase 2 Attacks - Swordsman
            RainbowSword,
            DarkSlash,
            DimensionalCut,
            BladeBarrage,
            
            // Phase 3 Attacks - True Form
            AbsoluteVoid,
            RealityShatter,
            DarkMatterCore,
            FinalOblivion
        }
        #endregion

        #region Properties
        private int health = 1600;
        private int maxHealth = 1600;
        private bool isDefeated = false;
        private BossPhase currentPhase = BossPhase.Awakening;
        
        private Sprite coreSprite;
        private Sprite eyeSprite;
        private Sprite swordSprite;
        private Sprite auraSprite;
        private Sprite eyeFormSprite;
        private Sprite swordsmanFormSprite;
        private VertexLight coreGlow;
        private VertexLight eyeGlow;
        private SoundSource voidLoop;

        private string eyeFormSpriteRoot;
        private string eyeDormantAnimationPath;
        private string eyeIdleAnimationPath;
        private string eyeAttackAnimationPath;
        private string eyeTransformAnimationPath;
        private string eyeEnragedAnimationPath;
        private string eyeDefeatAnimationPath;

        private string swordsmanFormSpriteRoot;
        private string swordsmanIdleAnimationPath;
        private string swordsmanReadyAnimationPath;
        private string swordsmanSlashAnimationPath;
        private string swordsmanRainbowAnimationPath;
        private string swordsmanDefeatAnimationPath;
        
        private Color darkColor = new Color(30, 0, 50);
        private Color eyeColor = Color.Red;
#pragma warning disable CS0414
        private bool isSwordsmanForm = false;
#pragma warning restore CS0414
        private float rotationAngle = 0f;
        private List<Vector2> voidParticles = new List<Vector2>();
        #endregion

        #region Constructors
        public DarkerDarkMatterBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, "darker_dark_matter_boss", Vector2.One, 0f, true, false, 0f, 
                   new Hitbox(48f, 48f, -24f, -24f))
        {
            health = data.Int("health", 1600);
            maxHealth = data.Int("maxHealth", 1600);
            InitializeVisualConfig(data);
            SetupVisuals();
        }

        public DarkerDarkMatterBoss(Vector2 position) 
            : base(position, "darker_dark_matter_boss", Vector2.One, 0f, true, false, 0f, 
                   new Hitbox(48f, 48f, -24f, -24f))
        {
            InitializeVisualConfig(null);
            SetupVisuals();
        }
        #endregion

        #region Setup
        private void InitializeVisualConfig(EntityData data)
        {
            eyeFormSpriteRoot = NormalizeSpriteRoot(data?.Attr("eyeFormSpriteRoot", "characters/darkmatter_boss_runtime") ?? "characters/darkmatter_boss_runtime");
            eyeDormantAnimationPath = data?.Attr("eyeDormantAnimationPath", "dormant") ?? "dormant";
            eyeIdleAnimationPath = data?.Attr("eyeIdleAnimationPath", "idle") ?? "idle";
            eyeAttackAnimationPath = data?.Attr("eyeAttackAnimationPath", "attack") ?? "attack";
            eyeTransformAnimationPath = data?.Attr("eyeTransformAnimationPath", "transform") ?? "transform";
            eyeEnragedAnimationPath = data?.Attr("eyeEnragedAnimationPath", "enraged") ?? "enraged";
            eyeDefeatAnimationPath = data?.Attr("eyeDefeatAnimationPath", "defeat") ?? "defeat";

            swordsmanFormSpriteRoot = NormalizeSpriteRoot(data?.Attr("swordsmanFormSpriteRoot", "characters/darkerdark_swordsman_runtime") ?? "characters/darkerdark_swordsman_runtime");
            swordsmanIdleAnimationPath = data?.Attr("swordsmanIdleAnimationPath", "idle") ?? "idle";
            swordsmanReadyAnimationPath = data?.Attr("swordsmanReadyAnimationPath", swordsmanIdleAnimationPath) ?? swordsmanIdleAnimationPath;
            swordsmanSlashAnimationPath = data?.Attr("swordsmanSlashAnimationPath", "slash") ?? "slash";
            swordsmanRainbowAnimationPath = data?.Attr("swordsmanRainbowAnimationPath", "rainbow") ?? "rainbow";
            swordsmanDefeatAnimationPath = data?.Attr("swordsmanDefeatAnimationPath", "defeat") ?? "defeat";
        }

        private static string NormalizeSpriteRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
                return string.Empty;

            return root.Trim().Trim('/').Replace('\\', '/') + "/";
        }

        private static bool HasAtlasFrames(string root, string animationPath)
        {
            return !string.IsNullOrWhiteSpace(root)
                && !string.IsNullOrWhiteSpace(animationPath)
                && GFX.Game.HasAtlasSubtextures(root + animationPath);
        }

        private static bool TryPlay(Sprite sprite, string animation)
        {
            if (sprite == null || string.IsNullOrWhiteSpace(animation))
                return false;

            try
            {
                sprite.Play(animation);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void AddLoopWithFallback(Sprite sprite, string root, string id, string animationPath, string fallbackPath, float delay)
        {
            string resolvedPath = HasAtlasFrames(root, animationPath)
                ? animationPath
                : fallbackPath;

            if (HasAtlasFrames(root, resolvedPath))
                sprite.AddLoop(id, resolvedPath, delay);
        }

        private static void AddAnimWithFallback(Sprite sprite, string root, string id, string animationPath, string fallbackPath, float delay, string gotoAnimation = null)
        {
            string resolvedPath = HasAtlasFrames(root, animationPath)
                ? animationPath
                : fallbackPath;

            if (!HasAtlasFrames(root, resolvedPath))
                return;

            if (string.IsNullOrWhiteSpace(gotoAnimation))
                sprite.Add(id, resolvedPath, delay);
            else
                sprite.Add(id, resolvedPath, delay, gotoAnimation);
        }

        private Sprite CreateEyeFormSprite()
        {
            if (!HasAtlasFrames(eyeFormSpriteRoot, eyeIdleAnimationPath))
                return null;

            Sprite sprite = new Sprite(GFX.Game, eyeFormSpriteRoot);
            AddLoopWithFallback(sprite, eyeFormSpriteRoot, "dormant", eyeDormantAnimationPath, eyeIdleAnimationPath, 0.15f);
            AddLoopWithFallback(sprite, eyeFormSpriteRoot, "idle", eyeIdleAnimationPath, eyeDormantAnimationPath, 0.1f);
            AddLoopWithFallback(sprite, eyeFormSpriteRoot, "attack", eyeAttackAnimationPath, eyeIdleAnimationPath, 0.06f);
            AddAnimWithFallback(sprite, eyeFormSpriteRoot, "transform", eyeTransformAnimationPath, eyeAttackAnimationPath, 0.08f, "idle");
            AddLoopWithFallback(sprite, eyeFormSpriteRoot, "enraged", eyeEnragedAnimationPath, eyeAttackAnimationPath, 0.05f);
            AddAnimWithFallback(sprite, eyeFormSpriteRoot, "defeat", eyeDefeatAnimationPath, eyeAttackAnimationPath, 0.08f);
            sprite.CenterOrigin();
            sprite.Visible = false;
            return sprite;
        }

        private Sprite CreateSwordsmanFormSprite()
        {
            if (!HasAtlasFrames(swordsmanFormSpriteRoot, swordsmanIdleAnimationPath))
                return null;

            Sprite sprite = new Sprite(GFX.Game, swordsmanFormSpriteRoot);
            AddLoopWithFallback(sprite, swordsmanFormSpriteRoot, "idle", swordsmanIdleAnimationPath, swordsmanReadyAnimationPath, 0.1f);
            AddLoopWithFallback(sprite, swordsmanFormSpriteRoot, "ready", swordsmanReadyAnimationPath, swordsmanIdleAnimationPath, 0.08f);
            AddLoopWithFallback(sprite, swordsmanFormSpriteRoot, "slash", swordsmanSlashAnimationPath, swordsmanReadyAnimationPath, 0.04f);
            AddLoopWithFallback(sprite, swordsmanFormSpriteRoot, "rainbow", swordsmanRainbowAnimationPath, swordsmanSlashAnimationPath, 0.05f);
            AddAnimWithFallback(sprite, swordsmanFormSpriteRoot, "defeat", swordsmanDefeatAnimationPath, swordsmanSlashAnimationPath, 0.08f);
            sprite.CenterOrigin();
            sprite.Visible = false;
            return sprite;
        }

        private bool UsingEyeFormSprite => eyeFormSprite != null && (currentPhase == BossPhase.Awakening || currentPhase == BossPhase.Phase1_Eye);

        private bool UsingSwordsmanFormSprite => swordsmanFormSprite != null && currentPhase == BossPhase.Phase2_Swordsman;

        private Sprite GetPrimaryVisualSprite()
        {
            if (UsingEyeFormSprite)
                return eyeFormSprite;

            if (UsingSwordsmanFormSprite)
                return swordsmanFormSprite;

            return coreSprite;
        }

        private Sprite GetSwordsmanVisualSprite()
        {
            return UsingSwordsmanFormSprite ? swordsmanFormSprite : swordSprite;
        }

        private void UpdateFormSpriteVisibility()
        {
            if (eyeFormSprite != null)
                eyeFormSprite.Visible = UsingEyeFormSprite;

            if (swordsmanFormSprite != null)
                swordsmanFormSprite.Visible = UsingSwordsmanFormSprite;

            if (UsingEyeFormSprite || UsingSwordsmanFormSprite)
            {
                coreSprite.Visible = false;
                eyeSprite.Visible = false;
                swordSprite.Visible = false;
                auraSprite.Visible = false;
            }
            else
            {
                coreSprite.Visible = true;
                eyeSprite.Visible = true;
                auraSprite.Visible = true;
            }
        }

        private void PlaySpriteAnimationWithFallback(Sprite sprite, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                if (TryPlay(sprite, candidate))
                    return;
            }
        }

        private void PlayEyePhaseVisual(string customAnimation, string coreAnimation, string eyeAnimation)
        {
            if (UsingEyeFormSprite)
            {
                PlaySpriteAnimationWithFallback(eyeFormSprite, customAnimation, "idle", "dormant");
                UpdateFormSpriteVisibility();
                return;
            }

            coreSprite.Play(coreAnimation);
            if (!string.IsNullOrWhiteSpace(eyeAnimation))
                eyeSprite.Play(eyeAnimation);

            UpdateFormSpriteVisibility();
        }

        private void PlaySwordsmanPhaseVisual(string customAnimation, string swordAnimation)
        {
            if (UsingSwordsmanFormSprite)
            {
                PlaySpriteAnimationWithFallback(swordsmanFormSprite, customAnimation, "ready", "idle");
                UpdateFormSpriteVisibility();
                return;
            }

            swordSprite.Play(swordAnimation);
            UpdateFormSpriteVisibility();
        }

        private void SetVisualColor(Color color)
        {
            coreSprite.Color = color;
            eyeSprite.Color = color;
            swordSprite.Color = color;
            auraSprite.Color = color;

            if (eyeFormSprite != null)
                eyeFormSprite.Color = color;

            if (swordsmanFormSprite != null)
                swordsmanFormSprite.Color = color;
        }

        private void SetupVisuals()
        {
            // Core/body sprite
            Add(coreSprite = new Sprite(GFX.Game, "characters/darkerdarkmatter/"));
            coreSprite.AddLoop("dormant", "core_dormant", 0.15f);
            coreSprite.AddLoop("idle", "core_idle", 0.1f);
            coreSprite.AddLoop("pulse", "core_pulse", 0.06f);
            coreSprite.AddLoop("transform", "core_transform", 0.08f);
            coreSprite.AddLoop("true", "core_true", 0.05f);
            coreSprite.Play("dormant");
            coreSprite.CenterOrigin();
            
            // Eye sprite (central eye)
            Add(eyeSprite = new Sprite(GFX.Game, "characters/darkerdarkmatter/"));
            eyeSprite.AddLoop("closed", "eye_closed", 0.1f);
            eyeSprite.AddLoop("open", "eye_open", 0.08f);
            eyeSprite.AddLoop("attack", "eye_attack", 0.05f);
            eyeSprite.AddLoop("rage", "eye_rage", 0.04f);
            eyeSprite.CenterOrigin();
            eyeSprite.Position = Vector2.Zero;
            
            // Sword sprite (for swordsman phase)
            Add(swordSprite = new Sprite(GFX.Game, "characters/darkerdarkmatter/"));
            swordSprite.AddLoop("hidden", "sword_hidden", 0.1f);
            swordSprite.AddLoop("ready", "sword_ready", 0.08f);
            swordSprite.AddLoop("slash", "sword_slash", 0.03f);
            swordSprite.AddLoop("rainbow", "sword_rainbow", 0.04f);
            swordSprite.CenterOrigin();
            swordSprite.Position = new Vector2(40f, 0f);
            swordSprite.Visible = false;
            
            // Aura sprite
            Add(auraSprite = new Sprite(GFX.Game, "characters/darkerdarkmatter/"));
            auraSprite.AddLoop("none", "aura_none", 0.1f);
            auraSprite.AddLoop("dark", "aura_dark", 0.08f);
            auraSprite.AddLoop("void", "aura_void", 0.05f);
            auraSprite.CenterOrigin();
            auraSprite.Play("none");
            
            // Core glow
            Add(coreGlow = new VertexLight(darkColor, 1.2f, 64, 128));
            
            // Eye glow
            Add(eyeGlow = new VertexLight(eyeColor, 1f, 32, 56));
            
            // Void ambience
            Add(voidLoop = new SoundSource());

            eyeFormSprite = CreateEyeFormSprite();
            if (eyeFormSprite != null)
            {
                Add(eyeFormSprite);
                PlaySpriteAnimationWithFallback(eyeFormSprite, "dormant", "idle");
            }

            swordsmanFormSprite = CreateSwordsmanFormSprite();
            if (swordsmanFormSprite != null)
            {
                Add(swordsmanFormSprite);
                PlaySpriteAnimationWithFallback(swordsmanFormSprite, "idle", "ready");
            }

            UpdateFormSpriteVisibility();
        }
        #endregion

        #region Scene Management
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(BossRoutine()));
        }

        public override void Update()
        {
            base.Update();
            
            // Rotation for true form
            if (currentPhase == BossPhase.Phase3_TrueForm)
            {
                rotationAngle += Engine.DeltaTime * 2f;
                coreSprite.Rotation = rotationAngle;
            }

            UpdateFormSpriteVisibility();
            
            // Pulsing glow
            coreGlow.Alpha = 1f + (float)Math.Sin(Scene.TimeActive * 2f) * 0.3f;
            eyeGlow.Alpha = 0.8f + (float)Math.Sin(Scene.TimeActive * 3f) * 0.2f;
            
            // Update void particles
            for (int i = voidParticles.Count - 1; i >= 0; i--)
            {
                Vector2 toCenter = (Position - voidParticles[i]).SafeNormalize();
                voidParticles[i] += toCenter * Engine.DeltaTime * 50f;
                
                if (Vector2.Distance(voidParticles[i], Position) < 10f)
                {
                    voidParticles.RemoveAt(i);
                }
            }
            
            // Spawn void particles
            if (currentPhase != BossPhase.Awakening && Calc.Random.Chance(0.05f))
            {
                float angle = Calc.Random.NextAngle();
                voidParticles.Add(Position + Calc.AngleToVector(angle, 100f));
            }
        }

        public override void Render()
        {
            // Render void particles
            foreach (var particle in voidParticles)
            {
                Draw.Rect(particle - Vector2.One * 3f, 6f, 6f, darkColor);
            }
            
            base.Render();
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            yield return AwakeningSequence();
            
            while (currentPhase != BossPhase.Defeated)
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase1_Eye:
                        yield return Phase1Loop();
                        break;
                    case BossPhase.Phase2_Swordsman:
                        yield return Phase2Loop();
                        break;
                    case BossPhase.Phase3_TrueForm:
                        yield return Phase3Loop();
                        break;
                }
                
                CheckPhaseTransition();
            }
        }

        private IEnumerator AwakeningSequence()
        {
            voidLoop.Play("event:/darkerdarkmatter_void_loop");
            
            // Eye opens
            yield return 1f;
            
            PlayEyePhaseVisual("idle", "idle", "open");
            Audio.Play("event:/darkerdarkmatter_awaken", Position);
            
            var level = Scene as Level;
            level?.Shake(1f);
            
            auraSprite.Play("dark");
            
            yield return 1f;
            
            currentPhase = BossPhase.Phase1_Eye;
        }

        private void CheckPhaseTransition()
        {
            float healthPercent = (float)health / maxHealth;
            
            if (healthPercent <= 0)
            {
                currentPhase = BossPhase.Defeated;
            }
            else if (healthPercent <= 0.3f && currentPhase != BossPhase.Phase3_TrueForm)
            {
                currentPhase = BossPhase.Phase3_TrueForm;
                Add(new Coroutine(EnterPhase3()));
            }
            else if (healthPercent <= 0.6f && currentPhase == BossPhase.Phase1_Eye)
            {
                currentPhase = BossPhase.Phase2_Swordsman;
                Add(new Coroutine(EnterPhase2()));
            }
        }
        #endregion

        #region Phase Routines
        private IEnumerator Phase1Loop()
        {
            while (currentPhase == BossPhase.Phase1_Eye && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(0, 4);
                yield return ExecuteAttack(attack);
                yield return 2.2f;
            }
        }

        private IEnumerator EnterPhase2()
        {
            isSwordsmanForm = true;
            PlayEyePhaseVisual("transform", "transform", "rage");
            Audio.Play("event:/darkerdarkmatter_transform", Position);
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Purple * 0.4f, true);
            
            yield return 1f;
            
            // Sword appears
            swordSprite.Visible = true;
            UpdateFormSpriteVisibility();
            PlaySwordsmanPhaseVisual("ready", "ready");
            Audio.Play("event:/darkerdarkmatter_sword_appear", Position);

            eyeColor = Color.Yellow;
            eyeGlow.Color = eyeColor;
            
            yield return 1f;
        }

        private IEnumerator Phase2Loop()
        {
            while (currentPhase == BossPhase.Phase2_Swordsman && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(4, 8);
                yield return ExecuteAttack(attack);
                yield return 1.8f;
            }
        }

        private IEnumerator EnterPhase3()
        {
            coreSprite.Play("true");
            auraSprite.Play("void");
            Audio.Play("event:/darkerdarkmatter_true_form", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(darkColor * 0.6f, true);
            
            // Expand hitbox
            Collider = new Hitbox(64f, 64f, -32f, -32f);
            
            coreGlow.Color = new Color(60, 0, 100);
            coreGlow.StartRadius = 96f;
            coreGlow.EndRadius = 160f;

            eyeSprite.Play("rage");
            eyeColor = Color.Red;
            eyeGlow.Color = eyeColor;
            eyeGlow.Alpha = 2f;

            UpdateFormSpriteVisibility();
            
            yield return 2f;
        }

        private IEnumerator Phase3Loop()
        {
            while (currentPhase == BossPhase.Phase3_TrueForm && health > 0)
            {
                var attack = (AttackType)Calc.Random.Next(8, 12);
                yield return ExecuteAttack(attack);
                yield return 1.5f;
            }
        }
        #endregion

        #region Attack Execution
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            switch (attack)
            {
                // Phase 1
                case AttackType.DarkBeam:
                    yield return DarkBeamAttack();
                    break;
                case AttackType.VoidOrbs:
                    yield return VoidOrbsAttack();
                    break;
                case AttackType.DimensionalTear:
                    yield return DimensionalTearAttack();
                    break;
                case AttackType.GravityPull:
                    yield return GravityPullAttack();
                    break;
                    
                // Phase 2
                case AttackType.RainbowSword:
                    yield return RainbowSwordAttack();
                    break;
                case AttackType.DarkSlash:
                    yield return DarkSlashAttack();
                    break;
                case AttackType.DimensionalCut:
                    yield return DimensionalCutAttack();
                    break;
                case AttackType.BladeBarrage:
                    yield return BladeBarrageAttack();
                    break;
                    
                // Phase 3
                case AttackType.AbsoluteVoid:
                    yield return AbsoluteVoidAttack();
                    break;
                case AttackType.RealityShatter:
                    yield return RealityShatterAttack();
                    break;
                case AttackType.DarkMatterCore:
                    yield return DarkMatterCoreAttack();
                    break;
                case AttackType.FinalOblivion:
                    yield return FinalOblivionAttack();
                    break;
            }
        }
        #endregion

        #region Phase 1 Attacks
        private IEnumerator DarkBeamAttack()
        {
            PlayEyePhaseVisual("attack", "pulse", "attack");
            Audio.Play("event:/darkerdarkmatter_dark_beam", Position);

            eyeGlow.Alpha = 2f;
            
            var level = Scene as Level;
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Track player briefly
            for (float t = 0; t < 0.8f; t += Engine.DeltaTime)
            {
                if (player != null)
                {
                    Vector2 toPlayer = (player.Position - Position).SafeNormalize();
                    // Visual tracking indicator
                }
                yield return null;
            }
            
            // Fire beam
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                for (float dist = 0; dist < 300f; dist += 30f)
                {
                    level?.Displacement.AddBurst(Position + direction * dist, 0.5f, 16f, 40f, 0.3f);
                }
            }
            
            level?.Shake(1f);
            
            yield return 0.5f;
            
            eyeGlow.Alpha = 1f;
            PlayEyePhaseVisual("idle", "idle", "open");
        }

        private IEnumerator VoidOrbsAttack()
        {
            PlayEyePhaseVisual("attack", "pulse", "open");
            Audio.Play("event:/darkerdarkmatter_void_orbs", Position);
            
            var level = Scene as Level;
            
            // Spawn void orbs in a pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 orbPos = Position + Calc.AngleToVector(angle, 60f);
                
                voidParticles.Add(orbPos);
                level?.Displacement.AddBurst(orbPos, 0.5f, 16f, 48f, 0.4f);
                
                yield return 0.1f;
            }
            
            PlayEyePhaseVisual("idle", "idle", "open");
        }

        private IEnumerator DimensionalTearAttack()
        {
            Audio.Play("event:/darkerdarkmatter_dimension_tear", Position);
            
            var level = Scene as Level;
            
            // Create tears in space
            for (int i = 0; i < 5; i++)
            {
                Vector2 tearPos = Position + Calc.Random.Range(Vector2.One * -120f, Vector2.One * 120f);
                
                level?.Displacement.AddBurst(tearPos, 0.8f, 8f, 64f, 0.5f);
                Audio.Play("event:/darkerdarkmatter_tear_open", tearPos);
                
                yield return 0.3f;
            }
        }

        private IEnumerator GravityPullAttack()
        {
            PlayEyePhaseVisual("attack", "pulse", "open");
            Audio.Play("event:/darkerdarkmatter_gravity_pull", Position);
            
            var level = Scene as Level;
            coreGlow.Alpha = 2f;
            
            // Pull effect
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.4f, 120f, 30f, 0.3f);
                
                // Add particles being pulled in
                if (Calc.Random.Chance(0.3f))
                {
                    float angle = Calc.Random.NextAngle();
                    voidParticles.Add(Position + Calc.AngleToVector(angle, 150f));
                }
                
                yield return null;
            }
            
            coreGlow.Alpha = 1f;
            PlayEyePhaseVisual("idle", "idle", "open");
        }
        #endregion

        #region Phase 2 Attacks
        private IEnumerator RainbowSwordAttack()
        {
            Sprite swordsmanSprite = GetSwordsmanVisualSprite();
            PlaySwordsmanPhaseVisual("rainbow", "rainbow");
            Audio.Play("event:/darkerdarkmatter_rainbow_sword", Position);
            
            var level = Scene as Level;
            
            // Rainbow colored slashes
            Color[] rainbowColors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple };
            
            for (int i = 0; i < 6; i++)
            {
                swordsmanSprite.Color = rainbowColors[i];
                
                level?.Displacement.AddBurst(Position + new Vector2(50f, 0f), 0.5f, 24f, 64f, 0.4f);
                level?.Flash(rainbowColors[i] * 0.2f, true);
                
                yield return 0.15f;
            }
            
            swordsmanSprite.Color = Color.White;
            PlaySwordsmanPhaseVisual("ready", "ready");
        }

        private IEnumerator DarkSlashAttack()
        {
            PlaySwordsmanPhaseVisual("slash", "slash");
            Audio.Play("event:/darkerdarkmatter_dark_slash", Position);
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Speed = direction * 400f;
            }
            
            var level = Scene as Level;
            
            for (float t = 0; t < 0.4f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.4f, 20f, 50f, 0.3f);
                yield return null;
            }
            
            Speed = Vector2.Zero;
            level?.Shake(1f);
            
            PlaySwordsmanPhaseVisual("ready", "ready");
        }

        private IEnumerator DimensionalCutAttack()
        {
            PlaySwordsmanPhaseVisual("slash", "slash");
            Audio.Play("event:/darkerdarkmatter_dimension_cut", Position);
            
            var level = Scene as Level;
            
            // Cut through dimensions - creates line of damage
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                
                for (float dist = 0; dist < 250f; dist += 25f)
                {
                    Vector2 cutPos = Position + direction * dist;
                    level?.Displacement.AddBurst(cutPos, 0.6f, 16f, 48f, 0.4f);
                    yield return 0.03f;
                }
            }
            
            level?.Flash(Color.Purple * 0.3f, true);
            
            PlaySwordsmanPhaseVisual("ready", "ready");
        }

        private IEnumerator BladeBarrageAttack()
        {
            Audio.Play("event:/darkerdarkmatter_blade_barrage", Position);
            
            var level = Scene as Level;
            
            // Multiple sword slashes in rapid succession
            for (int i = 0; i < 8; i++)
            {
                PlaySwordsmanPhaseVisual("slash", "slash");
                
                float angle = (i / 8f) * MathHelper.TwoPi;
                Vector2 slashDir = Calc.AngleToVector(angle, 80f);
                
                level?.Displacement.AddBurst(Position + slashDir, 0.5f, 16f, 48f, 0.3f);
                
                yield return 0.12f;
            }
            
            PlaySwordsmanPhaseVisual("ready", "ready");
        }
        #endregion

        #region Phase 3 Attacks
        private IEnumerator AbsoluteVoidAttack()
        {
            coreSprite.Play("true");
            auraSprite.Play("void");
            Audio.Play("event:/darkerdarkmatter_absolute_void", Position);
            
            var level = Scene as Level;
            
            coreGlow.Alpha = 3f;
            
            // Create expanding void zone
            for (int i = 0; i < 5; i++)
            {
                level?.Displacement.AddBurst(Position, 1.2f, i * 50f, i * 50f + 80f, 0.7f);
                level?.Shake(0.5f + i * 0.2f);
                yield return 0.3f;
            }
            
            level?.Flash(darkColor * 0.5f, true);
            
            coreGlow.Alpha = 1f;
        }

        private IEnumerator RealityShatterAttack()
        {
            Audio.Play("event:/darkerdarkmatter_reality_shatter", Position);
            
            var level = Scene as Level;
            level?.Shake(2f);
            
            // Shatter effect - multiple tears at once
            for (int i = 0; i < 12; i++)
            {
                Vector2 shatterPos = Position + Calc.Random.Range(Vector2.One * -150f, Vector2.One * 150f);
                level?.Displacement.AddBurst(shatterPos, 0.8f, 8f, 80f, 0.5f);
            }
            
            level?.Flash(Color.White * 0.3f, true);
            
            yield return 1.5f;
        }

        private IEnumerator DarkMatterCoreAttack()
        {
            coreSprite.Play("pulse");
            eyeSprite.Play("rage");
            Audio.Play("event:/darkerdarkmatter_core_attack", Position);
            
            var level = Scene as Level;
            
            coreGlow.Alpha = 4f;
            eyeGlow.Alpha = 3f;
            
            // Charge up
            for (float t = 0; t < 1.5f; t += Engine.DeltaTime)
            {
                // Particles drawn inward
                for (int i = 0; i < 3; i++)
                {
                    float angle = Calc.Random.NextAngle();
                    voidParticles.Add(Position + Calc.AngleToVector(angle, 120f));
                }
                
                level?.Displacement.AddBurst(Position, 0.3f, 80f, 30f, 0.2f);
                yield return null;
            }
            
            // Release
            level?.Flash(Color.Red * 0.4f, true);
            level?.Shake(2.5f);
            
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                
                for (float dist = 30f; dist < 200f; dist += 30f)
                {
                    level?.Displacement.AddBurst(Position + dir * dist, 0.5f, 20f, 50f, 0.4f);
                }
            }
            
            yield return 0.5f;
            
            coreGlow.Alpha = 1f;
            eyeGlow.Alpha = 1f;
            coreSprite.Play("true");
        }

        private IEnumerator FinalOblivionAttack()
        {
            coreSprite.Play("true");
            auraSprite.Play("void");
            eyeSprite.Play("rage");
            Audio.Play("event:/darkerdarkmatter_final_oblivion", Position);
            
            var level = Scene as Level;
            
            // Ultimate attack sequence
            coreGlow.Alpha = 5f;
            eyeGlow.Alpha = 4f;
            
            // Phase 1: Gathering
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = Calc.Random.NextAngle();
                    voidParticles.Add(Position + Calc.AngleToVector(angle, 200f));
                }
                
                level?.Shake(t * 0.5f);
                yield return null;
            }
            
            // Phase 2: Release
            level?.Flash(Color.White, true);
            level?.Shake(4f);
            
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 10; i++)
                {
                    level?.Displacement.AddBurst(Position, 2f, wave * 80f + i * 20f, wave * 80f + i * 20f + 40f, 0.8f);
                }
                yield return 0.2f;
            }
            
            yield return 1f;
            
            coreGlow.Alpha = 1f;
            eyeGlow.Alpha = 1f;
        }
        #endregion

        #region Damage and Defeat
        public override void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            health -= damage;
            Audio.Play("event:/darkerdarkmatter_damage", Position);
            
            var level = Scene as Level;
            level?.Shake(0.4f);
            
            // Flash red
            SetVisualColor(Color.Red);
            Add(new Coroutine(FlashReset()));
            
            if (health <= 0)
            {
                Defeat();
            }
        }

        private IEnumerator FlashReset()
        {
            yield return 0.1f;
            SetVisualColor(Color.White);
        }

        private void Defeat()
        {
            isDefeated = true;
            currentPhase = BossPhase.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        private IEnumerator DefeatSequence()
        {
            voidLoop.Stop();
            Audio.Play("event:/darkerdarkmatter_defeat", Position);
            
            var level = Scene as Level;

            if (currentPhase == BossPhase.Phase2_Swordsman)
                PlaySwordsmanPhaseVisual("defeat", "slash");
            else
                PlayEyePhaseVisual("defeat", "pulse", "closed");

            Sprite mainSprite = GetPrimaryVisualSprite();
            
            // Destabilizing
            for (float t = 0; t < 3f; t += Engine.DeltaTime)
            {
                mainSprite.Position = Calc.Random.Range(Vector2.One * -5f, Vector2.One * 5f);
                level?.Displacement.AddBurst(Position, 0.5f, 30f, 60f, 0.3f);
                level?.Shake(0.5f);
                
                yield return null;
            }
            
            // Implosion
            level?.Flash(Color.White, true);
            level?.Shake(3f);
            
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                mainSprite.Scale = new Vector2(1f - t);
                coreGlow.Alpha = 1f - t;
                yield return null;
            }
            
            level?.Session.SetFlag("darker_dark_matter_boss_defeated");
            RemoveSelf();
        }
        #endregion

        #region Cleanup
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            voidLoop?.Stop();
        }
        #endregion
    }
}
