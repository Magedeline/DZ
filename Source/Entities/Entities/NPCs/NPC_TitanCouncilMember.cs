using Celeste.Cutscenes;

namespace Celeste.NPCs
{
    /// <summary>
    /// Titan Council Member NPC for Chapter 15 - Roaring Titan Council
    /// </summary>
    [Tracked(true)]
    [CustomEntity(ids: "DZ/NPC_TitanCouncilMember")]
    public class NPC_TitanCouncilMember : Entity
    {
      private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting;
        private string titanRole; // "leader", "member1", "member2", "king"
     private bool isMajestic;
#pragma warning disable CS0414
 private string cutsceneId;
#pragma warning restore CS0414

   public NPC_TitanCouncilMember(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
    titanRole = data.Attr("titanRole", "member1");
       isMajestic = titanRole == "leader" || titanRole == "king";
            
      SetupSprite();
            SetupCollision();
            Depth = 100;
        }

      private void SetupSprite()
  {
       try
      {
         // Use Badeline sprite for imposing titan look
 Add(sprite = GFX.SpriteBank.Create("maggy_badeline"));
                sprite.Play("idle");
  
         switch (titanRole)
         {
            case "leader":
         sprite.Color = Color.Gold;
        sprite.Scale = Vector2.One * 1.5f;
   cutsceneId = "DZ_CH15_ROARING_TITAN_COUNCIL_ENTRANCE";
     break;
         case "king":
        sprite.Color = Color.Purple;
         sprite.Scale = Vector2.One * 1.8f;
     cutsceneId = "DZ_CH15_CEREMONY_OF_FLAME";
             break;
              case "member1":
               case "member2":
   sprite.Color = Color.Silver;
  sprite.Scale = Vector2.One * 1.3f;
    cutsceneId = "DZ_CH15_COUNCIL_JUDGMENT_RESULTS";
       break;
    }
     
        IngesteLogger.Debug($"NPC_TitanCouncilMember sprite setup complete - Role: {titanRole}");
      }
       catch (System.Exception ex)
          {
    IngesteLogger.Error(ex, "Error setting up NPC_TitanCouncilMember sprite");
    sprite = null;
            }
        }

        private void SetupCollision()
        {
     try
      {
    // Larger interaction radius for titan
  Add(talker = new TalkComponent(
         new Rectangle(-32, -8, 64, 16),
           new Vector2(0f, -32f),
         OnTalk
            ));
      }
     catch (System.Exception ex)
            {
   IngesteLogger.Error(ex, "Error setting up NPC_TitanCouncilMember collision");
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
    
            switch (titanRole)
        {
          case "leader":
     Scene.Add(new CS15_TitanCouncilJudgment(player));
    break;
     case "king":
       if (level?.Session.GetFlag("council_judgment_complete") == true)
               {
          Scene.Add(new CS15_RoaringTitanKingBattle(player));
      }
 else
         {
           // MultiCharacterCutscene doesn't exist
            // Scene.Add(new MultiCharacterCutscene(player, "DZ_CH15_KING_NOT_READY"));
            }
  break;
 default:
     // MultiCharacterCutscene doesn't exist
     // Scene.Add(new MultiCharacterCutscene(player, "DZ_CH15_COUNCIL_MEMBER_TALK"));
  break;
   }
          
            isInteracting = false;
        }

    public override void Update()
        {
  base.Update();
       
            if (sprite != null && isMajestic)
  {
                // Majestic glowing effect for leader and king
              float pulse = ((float)Math.Sin(Scene.TimeActive * 2f) + 1f) / 2f;
     Color baseColor = sprite.Color;
          sprite.Color = Color.Lerp(baseColor * 0.7f, baseColor, pulse);
        
                // Gentle floating for divine presence
       float float_y = (float)Math.Sin(Scene.TimeActive * 1.5f) * 3f;
        sprite.Position.Y = float_y;
    }
      }
    }
}
