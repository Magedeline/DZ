using Celeste.Entities;
using Celeste.NPCs;
using FMOD.Studio;
using BirdNPC = Celeste.Entities.BirdNPC;
using DZ;
using TimeRateModifier = Celeste.Mod.Entities.TimeRateModifier;

namespace Celeste.Cutscenes
{
    public class CS19_TrapinLoop : CutsceneEntity
    {
        // Existing fields
        private readonly global::Celeste.Player player;
        private CharaDummy chara;
        private BirdNPC bird;
        private EventInstance snapshot;
        private Vector2 playerSpeed;

        // Additional entities for the cutscene
        private CharaDummy charaEntity;
        private CharaBoost charabooster;
        private Npc19MaggyLoop magolorEntity;
        private TimeRateModifier timeRateModifier;

        // Constructor implementation
        public CS19_TrapinLoop(global::Celeste.Player sourceTrapinLoop)
        {
            // Initialize the player with the provided Player object
            player = sourceTrapinLoop;
            bird = null; // BirdNPC may also appear depending on the logic inside cutscene
            snapshot = null; // May need initialization later if FMOD Events are used

            // Set up other properties as required for CutsceneEntity
            Tag = Tags.TransitionUpdate; // Ensure it updates during scene transitions
            playerSpeed = player.Speed;
            Add(timeRateModifier = new TimeRateModifier(1f, false));
        }

        // Replace coroutine override with a void override that starts the coroutine
        public override void OnBegin(Level level)
        {
            // Put player in dummy state (StDummy = 11) so they appear during the cutscene
            this.player.StateMachine.State = Player.StDummy;
            this.player.StateMachine.Locked = true;
            this.player.Visible = true;
            Add(new Coroutine(CutsceneRoutine(level)));
        }

        // extracted routine from the old IEnumerator OnBegin
        private IEnumerator CutsceneRoutine(Level level)
        {
            var boost = Scene.Entities.FindFirst<CharaBoost>();
            if (boost != null)
                boost.Active = boost.Visible = boost.Collidable = false;

            yield return MovePlayerToGround();

            timeRateModifier.SetTimeRateMultiplier(0.65f);
            player.Dashes = 1;
            player.DummyGravity = false;
            player.DummyFriction = false;
            player.Speed.X = 200f;
            playerSpeed.X = 200f;
            player.DummyAutoAnimate = false;

            yield return MovePlayerAndBird();

            // Restore normal player physics and continue the cutscene logic
            playerSpeed.X = 0.0f;
            player.Speed.X = 0.0f;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            yield return 0.25f;

            yield return RestoreTimeRate();

            player.ForceCameraUpdate = false;
            yield return 0.6f;
            yield return 0.8f;

            yield return Textbox.Say(
                "DZ_CH19_TRAP_IN_LOOP",
                CameraZoomin,
                CharaAppear,
                CharaTurnLeft,
                KirbyTurnLeft,
                MagolorWalkin,
                DreamFriendsArrive,
                KirbyandCharaTurnRight,
                ShiftCameraToPowergen
            );

            yield return level.ZoomBack(0.5f);

            // Remove Chara and Magolor before charabooster appears
            if (charaEntity != null)
            {
                Level.Displacement.AddBurst(charaEntity.Position, 0.5f, 8f, 32f, 0.5f);
                Audio.Play("event:/char/badeline/disappear", charaEntity.Position);
                charaEntity.RemoveSelf();
            }
            
            if (magolorEntity != null)
            {
                magolorEntity.RemoveSelf();
            }

            yield return 0.3f;

            // Make charabooster appear
            if (boost != null)
            {
                Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", boost.Center);
                boost.Active = boost.Visible = boost.Collidable = true;
            }

            yield return 0.3f;

            // Unlock player movement
            player.StateMachine.Locked = false;

            EndCutscene(level);
        }

        private IEnumerator MovePlayerToGround()
        {
            while (!player.OnGround())
                player.MoveVExact(1);
            while (player.CollideCheck<Solid>())
                player.MoveVExact(-1);
            yield return null;
        }

        private IEnumerator MovePlayerAndBird()
        {
            yield return null;
        }

        private IEnumerator RestoreTimeRate()
        {
            while (timeRateModifier.CurrentTimeRate() < 1.0)
            {
                timeRateModifier.SetTimeRateMultiplier(Calc.Approach(timeRateModifier.CurrentTimeRate(), 1f, 4f * Engine.DeltaTime));
                yield return null;
            }

            timeRateModifier.ResetTimeRateMultiplier();
        }

        // Trigger -1: Camera zoom in
        private IEnumerator CameraZoomin()
        {
            // Zoom to position between player and where Chara will appear
            // Convert world position to screen-space coordinates
            Vector2 focusPoint = (player.Position + new Vector2(-16f, -8f)) - Level.Camera.Position;
            yield return Level.ZoomTo(focusPoint, 2f, 0.5f);
        }

        // Trigger 0: Chara appears
        private IEnumerator CharaAppear()
        {
            Vector2 position = player.Position + new Vector2(-32f, 0f);
            Level.Displacement.AddBurst(position, 0.5f, -8f, 32f, 0.5f);
            Level.Add(charaEntity = new CharaDummy(position));
            Audio.Play("event:/char/badeline/maddy_split", position);
            charaEntity.Sprite.Scale.X = 1f;
            yield return 0.2f;
        }

        // Trigger 1: Chara turns left
        private IEnumerator CharaTurnLeft()
        {
            if (charaEntity != null)
            {
                charaEntity.Sprite.Scale.X = -1f;
            }
            yield return null;
        }

        // Trigger 2: Player turns left
        private IEnumerator KirbyTurnLeft()
        {
            if (player != null)
            {
                player.Facing = Facings.Left;
            }
            yield return null;
        }

        // Trigger 3: Magolor walks in
        private IEnumerator MagolorWalkin()
        {
            Vector2 MagolorStartPos = player.Position + new Vector2(-120f, 0f);
            Vector2 MagolorEndPos = player.Position + new Vector2(-64f, 0f);
            
            // Create Magolor NPC with EntityData
            var entityData = new EntityData();
            entityData.Position = MagolorStartPos;
            magolorEntity = new Npc19MaggyLoop(entityData, Vector2.Zero);
            Level.Add(magolorEntity);
            
            // Animate Magolor walking to end position
            float duration = 1.5f;
            for (float t = 0; t < duration; t += Engine.DeltaTime)
            {
                magolorEntity.Position = Vector2.Lerp(MagolorStartPos, MagolorEndPos, t / duration);
                yield return null;
            }
            magolorEntity.Position = MagolorEndPos;
            yield return 0.2f;
        }

        // Trigger 5: Dream Friends arrive
        private IEnumerator DreamFriendsArrive()
        {
            // Flash the screen to signify the Dream Friends arriving en masse
            Level.Flash(Color.White * 0.5f, drawPlayerOver: true);
            Audio.Play("event:/new_content/game/10_farewell/glitch_short", player.Position);
            yield return 0.6f;
        }

        // Trigger 6: Player and Chara turn right
        private IEnumerator KirbyandCharaTurnRight()
        {
            if (player != null)
            {
                player.Facing = Facings.Right;
            }
            if (charaEntity != null)
            {
                charaEntity.Sprite.Scale.X = 1f;
            }
            yield return null;
        }

        // Trigger 5: Shift camera to power generator
        private IEnumerator ShiftCameraToPowergen()
        {
            // Pan camera to show the power generator/scene element
            // Convert world position to screen-space coordinates (relative to camera)
            Vector2 targetPos = (player.Position + new Vector2(150f, -130f)) - Level.Camera.Position;
            
            // Pan camera to the target
            yield return Level.ZoomTo(targetPos, 2f, 1.2f);
            
            // Sine wave camera movement while viewing the power generator
            float duration = 1.2f;
            float amplitude = 3f;
            float frequency = 4f;
            Vector2 baseOffset = Level.CameraOffset;
            
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                float sineOffset = (float)Math.Sin(t * frequency * Math.PI * 2) * amplitude;
                Level.CameraOffset = baseOffset + new Vector2(sineOffset * 0.5f, sineOffset);
                yield return null;
            }
            
            Level.CameraOffset = baseOffset;
        }

        // Other methods remain unchanged...

        public override void OnEnd(Level level)
        {
            Add(new Coroutine(OnEndRoutine(level)));
        }

        private IEnumerator OnEndRoutine(Level level)
        {
            timeRateModifier.ResetTimeRateMultiplier();
            // Player is already unlocked in CutsceneRoutine, so no need to unlock again
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
            if (WasSkipped)
            {
                var eventTrigger = Scene.Entities.FindFirst<MaggyEventTrigger>();
                if (eventTrigger != null)
                {
                    // use pattern matching instead of 'as' + null check
                    if (player.Sprite is global::Celeste.PlayerSprite playerSprite)
                        playerSprite.Play(global::Celeste.PlayerSprite.Idle);
                    player.Position = eventTrigger.Position.Floor();
                    level.Camera.Position = player.BottomCenter;
                }
                foreach (var lightning in Scene.Entities.FindAll<Lightning>())
                    lightning.ToggleCheck();
                Scene.Tracker.GetEntity<LightningRenderer>()?.ToggleEdges(true);
                level.Session.Audio.Ambience.Event = "event:/pusheen/new_content/env/19_rushingvoid";
                level.Session.Audio.Apply();
            }
            if (chara != null)
            {
                chara.Vanish();
            }
            yield return 0.5f;
            if (charabooster != null)
            {
                Level.Displacement.AddBurst(charabooster.Center, 0.5f, 8f, 32f, 0.5f);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", charabooster.Center);
                bool visible = true;
                charabooster.Collidable = true;
                charabooster.Active = (charabooster.Visible = visible);
                yield return 0.2f;
            }
            playerSpeed = Vector2.Zero;
            player.DummyGravity = true;
            player.DummyFriction = true;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            player.StateMachine.State = Player.StDummy; // Return to normal state
            player.StateMachine.Locked = false;
            var first = Scene.Entities.FindFirst<CharaBoost>();
            if (first != null)
                first.Active = first.Visible = first.Collidable = true;
            chara?.RemoveSelf();
            Vector2 birdWaitPosition = default;
            if (WasSkipped)
            {
                bird?.RemoveSelf();
                Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
                bird.Facing = Facings.Right;
                bird.FlyAwayUp = false;
                bird.WaitForLightningPostDelay = 1f;
                level.SnapColorGrade("none");
            }
            level.ResetZoom();
        }
    }
}

