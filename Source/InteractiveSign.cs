namespace MaggyHelper.Entities
{
    /// <summary>
    /// Interactive sign that displays dialog when player approaches or interacts
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InteractiveSign")]
    public class InteractiveSign : Entity
    {
        public enum SignType
        {
            Wooden,
            Stone,
            Ancient,
            Magical
        }

        private SignType signType;
        private string dialogId;
        private bool requiresInteraction;
        private bool onlyOnce;
        private bool hasTriggered;
        private float detectionRange;
        private Sprite sprite;
        private TalkComponent talk;
        private Wiggler wiggler;
        private SoundSource ambientSound;
        private VertexLight light;

        public InteractiveSign(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;

            string typeString = data.Attr(nameof(signType), "wooden");
            if (!System.Enum.TryParse(typeString, true, out signType))
            {
                signType = SignType.Wooden;
            }

            dialogId = data.Attr(nameof(dialogId), "sign_default");
            requiresInteraction = data.Bool(nameof(requiresInteraction), true);
            onlyOnce = data.Bool(nameof(onlyOnce), false);
            detectionRange = data.Float(nameof(detectionRange), 32f);
            hasTriggered = false;

            setupSprite();
            setupComponents();
            setupColliders();

            Depth = 8999;
        }

        private void setupSprite()
        {
            string spriteName = $"sign_{signType.ToString().ToLower()}";

            if (GFX.SpriteBank.Has(spriteName))
            {
                sprite = GFX.SpriteBank.Create(spriteName);
            }
            else
            {
                // Fallback sprite
                sprite = new Sprite(GFX.Game, "objects/temple/portal/");
                sprite.AddLoop("idle", "portal", 0.1f);
                sprite.AddLoop("glow", "portal", 0.05f);
            }

            Add(sprite);
            sprite.Play("idle");

            // Apply color based on sign type
            sprite.Color = getSignColor();
        }

        private Color getSignColor()
        {
            switch (signType)
            {
                case SignType.Wooden: return Color.SaddleBrown;
                case SignType.Stone: return Color.Gray;
                case SignType.Ancient: return Color.DarkGreen;
                case SignType.Magical: return Color.Purple;
                default: return Color.White;
            }
        }

        private void setupComponents()
        {
            // Talk component for interaction
            if (requiresInteraction)
            {
                talk = new TalkComponent(
                    new Rectangle(-8, -8, 16, 16),
                    new Vector2(0f, -8f),
                    OnTalk
                );
                Add(talk);
            }

            // Wiggler for visual feedback
            wiggler = Wiggler.Create(0.4f, 4f, null, false, false);
            Add(wiggler);

            // Setup lighting for magical signs
            if (signType == SignType.Magical)
            {
                light = new VertexLight(Color.Purple, 1f, 32, 48);
                Add(light);

                ambientSound = new SoundSource();
                Add(ambientSound);
                ambientSound.Play("event:/game/05_mirror_temple/crystaltheo_pulse", "pulse", 0f);
            }

            // Setup lighting for ancient signs
            if (signType == SignType.Ancient)
            {
                light = new VertexLight(Color.Green, 0.8f, 24, 32);
                Add(light);
            }
        }

        private void setupColliders()
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
        }

        public override void Update()
        {
            base.Update();

            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float distance = Vector2.Distance(Position, player.Position);

                // Auto-trigger for non-interaction signs
                if (!requiresInteraction && distance <= detectionRange)
                {
                    if (!hasTriggered || !onlyOnce)
                    {
                        triggerDialog();
                    }
                }

                // Visual feedback when player is near
                if (distance <= detectionRange + 16f)
                {
                    sprite?.Play("glow");
                    updateLightIntensity(1f);
                }
                else
                {
                    sprite?.Play("idle");
                    updateLightIntensity(0.5f);
                }
            }

            // Update wiggler scale
            if (wiggler != null)
            {
                sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.1f);
            }
        }

        private void updateLightIntensity(float targetAlpha)
        {
            if (light != null)
            {
                light.Alpha = Calc.Approach(light.Alpha, targetAlpha, Engine.DeltaTime * 2f);
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            triggerDialog();
        }

        private void triggerDialog()
        {
            if (onlyOnce && hasTriggered)
                return;

            var level = Scene as Level;
            if (level != null)
            {
                // Start dialog
                level.StartCutscene((Level l) => OnDialogStart(l));
                hasTriggered = true;

                // Visual feedback
                wiggler?.Start();

                // Play sound based on sign type
                Audio.Play(getSignSoundEvent(), Position);
            }
        }

        private string getSignSoundEvent()
        {
            switch (signType)
            {
                case SignType.Wooden: return "event:/game/general/thing_booped";
                case SignType.Stone: return "event:/game/04_cliffside/arrowblock_activate";
                case SignType.Ancient: return "event:/game/06_reflection/badeline_disappear";
                case SignType.Magical: return "event:/game/05_mirror_temple/crystaltheo_pulse";
                default: return "event:/game/general/thing_booped";
            }
        }

        private System.Collections.IEnumerator OnDialogStart(Level level)
        {
            global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StDummy;
                yield return Textbox.Say(dialogId);
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
        }

        public void SetDialogId(string newDialogId)
        {
            dialogId = newDialogId;
        }

        public void Reset()
        {
            hasTriggered = false;
        }
    }
}




