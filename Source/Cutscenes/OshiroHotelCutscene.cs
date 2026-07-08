namespace Celeste.Cutscenes
{
    [CustomEntity("DesoloZantas/OshiroHotelCutscene")]
    public class OshiroHotelCutscene : CutsceneEntity
    {
        private global::Celeste.Player player;
        private NPC oshiro;
        private string cutscenePhase;

        public OshiroHotelCutscene(EntityData data, Vector2 offset) : base()
        {
            Position = data.Position + offset;
            cutscenePhase = data.Attr("phase", "front_desk");
        }

        public override void OnBegin(Level level)
        {
            player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            oshiro = Scene.Entities.FindFirst<NPC>();
            
            Add(new Coroutine(HotelSequence()));
        }

        public override void OnEnd(Level level)
        {
            // Set completion flag
            level.Session.SetFlag($"oshiro_hotel_{cutscenePhase}_complete");
        }

        private IEnumerator HotelSequence()
        {
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StDummy;
            }

            Level level = Scene as Level;

            switch (cutscenePhase)
            {
                case "front_desk":
                    yield return Textbox.Say("DZ_CH5_OSHIRO_FRONT_DESK");
                    break;
                    
                case "hallway_a":
                    yield return Textbox.Say("DZ_CH5_OSHIRO_HALLWAY_A");
                    break;
                    
                case "hallway_b":
                    yield return Textbox.Say("DZ_CH5_OSHIRO_HALLWAY_B");
                    break;
                    
                case "clutter":
                    yield return Textbox.Say("DZ_CH5_OSHIRO_CLUTTER0");
                    break;
                    
                case "guestbook":
                    yield return Textbox.Say("DZ_CH5_GUESTBOOK");
                    break;
                    
                case "memo":
                    yield return Textbox.Say("DZ_CH5_MEMO_OPENING");
                    yield return 0.5f;
                    yield return Textbox.Say("DZ_CH5_MEMO");
                    break;
                    
                default:
                    yield return Textbox.Say("DZ_CH5_OSHIRO_FRONT_DESK");
                    break;
            }

            EndCutscene(level);
        }

        // Helper method for moving Oshiro around
        private IEnumerator MoveOshiro(Vector2 targetPosition, float duration = 1f)
        {
            if (oshiro == null) yield break;

            Vector2 startPosition = oshiro.Position;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                oshiro.Position = Vector2.Lerp(startPosition, targetPosition, Ease.CubeInOut(progress));
                yield return null;
            }

            oshiro.Position = targetPosition;
        }
    }
}



