using MaggyHelper.Cutscenes;

namespace MaggyHelper.NPCs
{
    /// <summary>
    /// Axis - Royal Robot Guard NPC for Chapters 13 & 14
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/NPC_Axis")]
    public class NPC_Axis : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        private bool isInteracting;
        private string axisForm; // "normal", "giant", "defeated"
        private bool isActive;
        private string cutsceneId;

        public NPC_Axis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            axisForm = data.Attr("axisForm", "normal");
            isActive = data.Bool("isActive", true);
            
            SetupSprite();
            SetupCollision();
            Depth = 100;
        }

        private void SetupSprite()
        {
            try
            {
                // Use Oshiro sprite as placeholder for robotic look
                Add(sprite = GFX.SpriteBank.Create("maggy_oshiro"));
                sprite.Play("idle");
                
                switch (axisForm)
                {
                    case "normal":
                        sprite.Color = Color.Gray;
                        sprite.Scale = Vector2.One * 1.2f; // Larger for robot
                        cutsceneId = "CH13_AXIS_APPROACH";
                        break;
                    case "giant":
                        sprite.Color = Color.DarkGray;
                        sprite.Scale = Vector2.One * 2.0f; // Much larger
                        cutsceneId = "CH14_GIANT_AXIS_APPROACH";
                        break;
                    case "defeated":
                        sprite.Color = Color.LightGray;
                        sprite.Scale = Vector2.One * 0.8f;
                        isActive = false;
                        break;
                }
                
                IngesteLogger.Debug($"NPC_Axis sprite setup complete - Form: {axisForm}");
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Axis sprite");
                sprite = null;
            }
        }

        private void SetupCollision()
        {
            try
            {
                // Larger interaction radius for robot
                Rectangle talkBounds = axisForm == "giant" 
                    ? new Rectangle(-48, -16, 96, 32) 
                    : new Rectangle(-24, -8, 48, 16);
                
                Vector2 talkOffset = axisForm == "giant"
                    ? new Vector2(0f, -48f)
                    : new Vector2(0f, -32f);
                
                Add(talker = new TalkComponent(
                    talkBounds,
                    talkOffset,
                    OnTalk
                ));
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error(ex, "Error setting up NPC_Axis collision");
                talker = null;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            if (talker != null)
            {
                talker.Enabled = isActive;
            }
        }

        private void OnTalk(global::Celeste.Player player)
        {
            if (isInteracting || !isActive)
                return;

            isInteracting = true;
            
            Level level = Scene as Level;
            
            switch (axisForm)
            {
                case "normal":
                    if (level?.Session.GetFlag("axis_defeated") != true)
                    {
                        Scene.Add(new CS13_AxisBossBattle(player));
                        level?.Session.SetFlag("axis_defeated");
                    }
                    break;
                case "giant":
                    if (level?.Session.GetFlag("giant_axis_defeated") != true)
                    {
                        Scene.Add(new CS14_GiantAxisBattle(player));
                        level?.Session.SetFlag("giant_axis_defeated");
                    }
                    break;
            }
            
            isInteracting = false;
        }

        public override void Update()
        {
            base.Update();
            
            if (sprite != null && isActive)
            {
                // Robot-like mechanical sounds and movements
                if (Scene.OnInterval(3f))
                {
                    Audio.Play("event:/game/general/touchswitch_any");
                }
                
                // Heavy footstep shake for giant form
                if (axisForm == "giant" && Scene.OnInterval(2f))
                {
                    (Scene as Level)?.Shake(0.5f);
                }
            }
        }
    }
}



