using Celeste.Entities;

namespace Celeste.Cutscenes
{
    public class CS05_OshiroRooftop(CelesteNPC oshiro) : CutsceneEntity
    {
        public const string Flag = "oshiro_05_rooftop";

        private const float playerEndPosition = 170f;

        private global::Celeste.Player player;

        private readonly CelesteNPC oshiro = oshiro;

        private CharaDummy chara;

        private Vector2 bossSpawnPosition;

        private float anxiety;

        private float anxietyFlicker;

        private readonly Sprite bossSprite = GFX.SpriteBank.Create("oshiro_boss");

        private float bossSpriteOffset;

        private bool oshiroRumble;

        public override void OnBegin(Level level)
        {
            bossSpawnPosition = new Vector2(oshiro.X, level.Bounds.Bottom - 40);
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    break;
                }
                yield return null;
            }
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            while (!player.OnGround() || player.Speed.Y < 0f)
            {
                yield return null;
            }
            yield return 0.6f;
            player.Facing = Facings.Left;
            yield return Textbox.Say("MAGGYHELPER_CH5_OSHIRO_START_CHASE", CharaAppear, BadelineFaceChara, KirbyWalkAwayWithGroup, KirbyLookAtChara, OshiroEnterAndAsk, CharaTurnsToOshiro, CharaDisappearsOshiroSnaps, OshiroTransformChase);
            yield return OshiroTransform();
            Add(new Coroutine(AnxietyAndCameraOut()));
            yield return level.ZoomBack(0.5f);
            yield return 0.25f;
            EndCutscene(level);
        }

        // Trigger 0: Chara appears
        private IEnumerator CharaAppear()
        {
            Level level = Scene as Level;
            chara = new CharaDummy(new Vector2(oshiro.X - 40f, level.Bounds.Bottom - 60));
            chara.Sprite.Scale.X = 1f;
            chara.Appear(level);
            level.Add(chara);
            yield return 0.3f;
        }

        // Trigger 1: Badeline faces Chara and yells
        private IEnumerator BadelineFaceChara()
        {
            player.Facing = Facings.Left;
            yield return 0.2f;
        }

        // Trigger 2: Kirby walks away with Badeline, Chara follows, Ralsei appears
        private IEnumerator KirbyWalkAwayWithGroup()
        {
            Level level = Scene as Level;
            Add(new Coroutine(player.DummyWalkTo((float)level.Bounds.Left + 170f)));
            yield return 0.2f;
            Audio.Play("event:/game/pusheen/05_restore/suite_bad_moveroof", chara.Position);
            Add(new Coroutine(chara.FloatTo(chara.Position + new Vector2(80f, 30f))));
            yield return 0.5f;
        }

        // Trigger 3: Kirby looks at Chara and zoom in
        private IEnumerator KirbyLookAtChara()
        {
            yield return 0.25f;
            player.Facing = Facings.Left;
            yield return 0.1f;
            Level level = SceneAs<Level>();
            yield return level.ZoomTo(new Vector2(150f, bossSpawnPosition.Y - (float)level.Bounds.Y - 8f), 2f, 0.5f);
        }

        // Trigger 4: Oshiro enters and asks a question, Chara responds rudely
        private IEnumerator OshiroEnterAndAsk()
        {
            yield return 0.3f;
            bossSpriteOffset = (bossSprite.Justify.Value.Y - oshiro.Sprite.Justify.Value.Y) * bossSprite.Height;
            oshiro.Visible = true;
            oshiro.Sprite.Scale.X = 1f;
            Add(new Coroutine(oshiro.MoveTo(bossSpawnPosition - new Vector2(0f, bossSpriteOffset))));
            oshiro.Add(new SoundSource("event:/char/oshiro/move_07_roof00_enter"));
            float from = Level.ZoomFocusPoint.X;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.7f)
            {
                Level.ZoomFocusPoint.X = from + (126f - from) * Ease.CubeInOut(p);
                yield return null;
            }
            yield return 0.3f;
            player.Facing = Facings.Left;
            yield return 0.1f;
            chara.Sprite.Scale.X = -1f;
        }

        // Trigger 5: Chara turns to Oshiro
        private IEnumerator CharaTurnsToOshiro()
        {
            yield return 0.1f;
            chara.Sprite.Scale.X = 1f;
            yield return 0.2f;
        }

        // Trigger 6: Chara disappears, Oshiro snaps
        private IEnumerator CharaDisappearsOshiroSnaps()
        {
            yield return 0.1f;
            chara.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            chara = null;
            yield return 0.8f;
        }

        // Trigger 7: Oshiro transforms into boss form, chase begins
        private IEnumerator OshiroTransformChase()
        {
            Audio.Play("event:/char/oshiro/boss_transform_begin", oshiro.Position);
            oshiro.Remove(oshiro.Sprite);
            oshiro.Sprite = bossSprite;
            oshiro.Sprite.Play("transformStart");
            oshiro.Y += bossSpriteOffset;
            oshiro.Add(oshiro.Sprite);
            oshiro.Depth = -12500;
            oshiroRumble = true;
            yield return 1f;
        }

        private IEnumerator OshiroTransform()
        {
            yield return 0.2f;
            Audio.Play("event:/char/oshiro/boss_transform_burst", oshiro.Position);
            oshiro.Sprite.Play("transformFinish");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            SceneAs<Level>().Shake(0.5f);
            SetChaseMusic();
            while (anxiety < 0.5f)
            {
                anxiety = Calc.Approach(anxiety, 0.5f, Engine.DeltaTime * 0.5f);
                yield return null;
            }
            yield return 0.25f;
        }

        private IEnumerator AnxietyAndCameraOut()
        {
            Level level = Scene as Level;
            Vector2 from = level.Camera.Position;
            Vector2 to = player.CameraTarget;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                anxiety = Calc.Approach(anxiety, 0f, Engine.DeltaTime * 4f);
                level.Camera.Position = from + (to - from) * Ease.CubeInOut(t);
                yield return null;
            }
        }

        private void SetChaseMusic()
        {
            Level obj = base.Scene as Level;
            obj.Session.Audio.Music.Event = "event:/music/pusheen/lvl5/oshiro_chase";
            obj.Session.Audio.Apply(forceSixteenthNoteHack: false);
        }
        public override void OnEnd(Level level)
        {
            Distort.Anxiety = (anxiety = (anxietyFlicker = 0f));
            if (chara != null)
            {
                level.Remove(chara);
            }
            player = base.Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
                player.X = (float)level.Bounds.Left + 170f;
                player.Speed.Y = 0f;
                while (player.CollideCheck<Solid>())
                {
                    player.Y--;
                }
                level.Camera.Position = player.CameraTarget;
            }
            if (WasSkipped)
            {
                SetChaseMusic();
            }
            oshiro.RemoveSelf();
            base.Scene.Add(new AngyOshiro(bossSpawnPosition, fromCutscene: true));
            level.Session.RespawnPoint = new Vector2((float)level.Bounds.Left + 170f, level.Bounds.Top + 160);
            level.Session.SetFlag("oshiro_05_rooftop");
        }
        public override void Update()
        {
            Distort.Anxiety = anxiety + anxiety * anxietyFlicker;
            if (base.Scene.OnInterval(0.05f))
            {
                anxietyFlicker = -0.2f + Calc.Random.NextFloat(0.4f);
            }
            base.Update();
            if (oshiroRumble)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }
        }
    }
}
