namespace MaggyHelper.Entities {
    /// <summary>
    /// Enhanced Dialog NPC with advanced AI and cutscene integration
    /// </summary>
    [CustomEntity("MaggyHelper/EnhancedDialogNPC", "MaggyHelper/DialogNPC")]
    public class EnhancedDialogNpc : Entity {
        public enum AiBehavior {
            Static,
            Patrol,
            FollowPlayer,
            Random,
            Guard,
            Fleeing,
            Aggressive
        }

        private Sprite sprite;
        private AiBehavior aiType;
        private string dialogKey;
        private string cutsceneId;
        private string npcName;
        private bool isActive;
        private bool triggerOnce;
        private bool hasTriggered;
        private Vector2 startPosition;
        private float aiTimer;
        private Level level;

        public EnhancedDialogNpc(EntityData data, Vector2 offset) : base(data.Position + offset) {
            string aiString = data.Attr(nameof(aiType), "static");
            if (!System.Enum.TryParse(aiString, true, out aiType)) {
                aiType = AiBehavior.Static;
            }

            dialogKey = data.Attr(nameof(dialogKey), "NPC_DEFAULT");
            cutsceneId = data.Attr(nameof(cutsceneId), "");
            npcName = data.Attr(nameof(npcName), nameof(NPC));
            isActive = data.Bool(nameof(isActive), true);
            triggerOnce = data.Bool(nameof(triggerOnce), true);
            hasTriggered = false;
            startPosition = Position;
            aiTimer = 0f;

            setupSprite();
            setupCollision();
        }

        private void setupSprite() {
            // Try to load custom sprite based on NPC name
            if (GFX.SpriteBank.Has($"npc_{npcName.ToLower()}")) {
                sprite = GFX.SpriteBank.Create($"npc_{npcName.ToLower()}");
            } else {
                // Fallback to default NPC sprite
                sprite = new Sprite(GFX.Game, "characters/player/");
                sprite.AddLoop("idle", "idle", 0.1f);
                sprite.AddLoop("walk", "walk", 0.1f);
            }

            Add(sprite);
            sprite.Play("idle");
            Depth = -1000;
        }

        private void setupCollision() {
            Collider = new Hitbox(12f, 16f, -6f, -16f);
            Add(new PlayerCollider(OnPlayerInteract));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update() {
            base.Update();

            if (!isActive) return;

            aiTimer += Engine.DeltaTime;
            updateAi();
        }

        private void updateAi() {
            switch (aiType) {
                case AiBehavior.Static:
                    // Do nothing, stay in place
                    break;

                case AiBehavior.Patrol:
                    updatePatrolAi();
                    break;

                case AiBehavior.FollowPlayer:
                    updateFollowPlayerAi();
                    break;

                case AiBehavior.Random:
                    updateRandomAi();
                    break;

                case AiBehavior.Guard:
                    updateGuardAi();
                    break;

                case AiBehavior.Fleeing:
                    updateFleeingAi();
                    break;

                case AiBehavior.Aggressive:
                    updateAggressiveAi();
                    break;
            }
        }

        private void updatePatrolAi() {
            // Simple back and forth patrol
            if (aiTimer > 3f) {
                Position = Vector2.Lerp(Position, startPosition + new Vector2(Calc.Random.Range(-50, 50), 0),
                    Engine.DeltaTime);
                aiTimer = 0f;
            }
        }

        private void updateFollowPlayerAi() {
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null) {
                float distance = Vector2.Distance(Position, player.Position);
                if (distance > 40f && distance < 200f) {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Position += direction * 30f * Engine.DeltaTime;
                    sprite?.Play("walk");
                } else {
                    sprite?.Play("idle");
                }
            }
        }

        private void updateRandomAi() {
            if (aiTimer > 2f) {
                Vector2 randomOffset = new Vector2(Calc.Random.Range(-20, 20), 0);
                Position = Vector2.Lerp(Position, startPosition + randomOffset, Engine.DeltaTime * 0.5f);
                aiTimer = 0f;
            }
        }

        private void updateGuardAi() {
            // Look for player and face them
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null) {
                float distance = Vector2.Distance(Position, player.Position);
                if (distance < 100f) {
                    // Face the player
                    if (player.Position.X > Position.X)
                        sprite.Scale.X = 1f;
                    else
                        sprite.Scale.X = -1f;
                }
            }
        }

        private void updateFleeingAi() {
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null) {
                float distance = Vector2.Distance(Position, player.Position);
                if (distance < 80f) {
                    Vector2 direction = (Position - player.Position).SafeNormalize();
                    Position += direction * 50f * Engine.DeltaTime;
                    sprite?.Play("walk");
                } else {
                    sprite?.Play("idle");
                }
            }
        }

        private void updateAggressiveAi() {
            var player = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null) {
                float distance = Vector2.Distance(Position, player.Position);
                if (distance < 120f) {
                    Vector2 direction = (player.Position - Position).SafeNormalize();
                    Position += direction * 40f * Engine.DeltaTime;
                    sprite?.Play("walk");
                }
            }
        }

        private void OnPlayerInteract(global::Celeste.Player player) {
            if (!isActive || (triggerOnce && hasTriggered))
                return;

            hasTriggered = true;

            if (!string.IsNullOrEmpty(cutsceneId)) {
                startCutscene();
            } else if (!string.IsNullOrEmpty(dialogKey)) {
                startDialog();
            }
        }

        private void startDialog() {
            level?.StartCutscene(showDialogCutscene);
        }

        private void showDialogCutscene(Level obj) {
            if (!string.IsNullOrEmpty(dialogKey))
                // Trigger the dialog sequence using the dialogKey
                obj.Add(new Textbox(dialogKey));

            if (triggerOnce)
                // Mark the NPC as triggered to prevent re-triggering
                hasTriggered = true;
        }

        private void startCutscene() {
            level?.StartCutscene(level1 => showCustomCutscene(level1));
        }

        private IEnumerator showCustomCutscene(Level level) {
            if (string.IsNullOrEmpty(cutsceneId)) yield break;

            if (triggerOnce)
                // Mark the NPC as triggered to prevent re-triggering
                hasTriggered = true;

            // Trigger the custom cutscene sequence using the cutsceneId
            yield return Textbox.Say($"{npcName}_{cutsceneId}");
        }
    }
}



