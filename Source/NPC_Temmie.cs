using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Temmie NPC for Chapter 12 - Shop owner
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/NPC_Temmie")]
    public class NPC_Temmie : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting;
        private int shopStage = 1;

        public NPC_Temmie(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            shopStage = data.Int("shopStage", 1);
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            try
            {
                // Use Theo sprite as placeholder for Temmie
                Add(sprite = GFX.SpriteBank.Create("theo"));
                sprite.Play("idle");
                
                // Make smaller to represent Temmie
                sprite.Scale = Vector2.One * 0.8f;
                
                IngesteLogger.Debug($"NPC_Temmie sprite setup complete for shop stage {shopStage}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Temmie sprite");
                sprite = null;
            }
        }

        private void SetupCollision()
        {
            try
            {
                Add(talker = new TalkComponent(
                    new Rectangle(-20, -8, 40, 16),
                    new Vector2(0f, -24f),
                    OnTalk
                ));
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Temmie collision");
                talker = null;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            if (talker != null)
            {
                talker.Enabled = true;
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (isInteracting)
                return;

            isInteracting = true;
            
            Level level = Scene as Level;
            
            // Different dialogue based on shop progression
            switch (shopStage)
            {
                case 1:
                    level?.Session.SetFlag("temmie_shop_1");
                    // MultiCharacterCutscene doesn't exist
                    // Scene.Add(new MultiCharacterCutscene(player, "CH12_TEMMIE_ROOM_TEM_SHOP_1"));
                    break;
                case 2:
                    level?.Session.SetFlag("temmie_shop_2");
                    // MultiCharacterCutscene doesn't exist
                    // Scene.Add(new MultiCharacterCutscene(player, "CH12_TEMMIE_ROOM_TEM_SHOP_2"));
                    break;
                default:
                    // MultiCharacterCutscene doesn't exist
                    // Scene.Add(new MultiCharacterCutscene(player, "CH12_TEMMIE_ROOM_TEM_SHOP_END"));
                    break;
            }
            
            isInteracting = false;
        }

        public override void Update()
        {
            base.Update();
            
            // Temmie-like bouncing animation
            if (sprite != null)
            {
                float bounce = (float)Math.Sin(Scene.TimeActive * 3f) * 2f;
                sprite.Position.Y = bounce;
            }
        }
    }
}



