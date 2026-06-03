using Celeste.Cutscenes;
using ModOshiroSprite = Celeste.OshiroSprite;

namespace Celeste.NPCs
{
    [CustomEntity(ids: "MaggyHelper/NPC05_Oshiro_Clutter")]
    public class NPC05_Oshiro_Cluttter : NPC
    {
        public const string TalkFlagsA = "oshiro_clutter_";

        public const string TalkFlagsB = "oshiro_clutter_optional_";

        public const string ClearedFlags = "oshiro_clutter_cleared_";

        public const string FinishedFlag = "oshiro_clutter_finished";

        public const string DoorOpenFlag = "oshiro_clutter_door_open";

        public Vector2 HomePosition;

        private int sectionsComplete;

        private bool talked;

        private bool inRoutine;

        private List<Vector2> nodes = new List<Vector2>();

        private Coroutine paceRoutine;

        private Coroutine talkRoutine;

        private SoundSource paceSfx;

        private float paceTimer;

        protected new VertexLight Light;
        protected new TalkComponent Talker;

        public Vector2 ZoomPoint
        {
            get
            {
                if (sectionsComplete < 2)
                {
                    return Position + new Vector2(0f, -30f) - Level.Camera.Position;
                }
                return Position + new Vector2(0f, -15f) - Level.Camera.Position;
            }
        }

        public NPC05_Oshiro_Cluttter(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = new ModOshiroSprite(-1));
            Add(Talker = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(0f, -24f), OnTalk));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
            Vector2[] array = data.Nodes;
            foreach (Vector2 vector in array)
            {
                nodes.Add(vector + offset);
            }
            Add(paceSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (base.Session.GetFlag("oshiro_clutter_finished"))
            {
                RemoveSelf();
            }
            else
            {
                if (base.Session.GetFlag("oshiro_clutter_cleared_0"))
                {
                    sectionsComplete++;
                }
                if (base.Session.GetFlag("oshiro_clutter_cleared_1"))
                {
                    sectionsComplete++;
                }
                if (base.Session.GetFlag("oshiro_clutter_cleared_2"))
                {
                    sectionsComplete++;
                }
                if (sectionsComplete == 0 || sectionsComplete == 3)
                {
                    Sprite.Scale.X = 1f;
                }
                if (sectionsComplete > 0)
                {
                    Position = nodes[sectionsComplete - 1];
                }
                else if (!base.Session.GetFlag("oshiro_clutter_0"))
                {
                    Add(paceRoutine = new Coroutine(Pace()));
                }
                if (sectionsComplete == 0 && base.Session.GetFlag("oshiro_clutter_0") && !base.Session.GetFlag("oshiro_clutter_optional_0"))
                {
                    Sprite.Play("idle_ground");
                }
                if (sectionsComplete == 3 || base.Session.GetFlag("oshiro_clutter_optional_" + sectionsComplete))
                {
                    Remove(Talker);
                }
            }
            HomePosition = Position;
        }

        private void OnTalk(global::Celeste.Player player)
        {
            talked = true;
            if (paceRoutine != null)
            {
                paceRoutine.RemoveSelf();
            }
            paceRoutine = null;
            if (!base.Session.GetFlag("oshiro_clutter_" + sectionsComplete))
            {
                base.Scene.Add(new CS05_OshiroClutter(player, this, sectionsComplete));
                return;
            }
            Level.StartCutscene(EndTalkRoutine);
            base.Session.SetFlag("oshiro_clutter_optional_" + sectionsComplete);
            Add(talkRoutine = new Coroutine(TalkRoutine(player)));
            if (Talker != null)
            {
                Talker.Enabled = false;
            }
        }

        private IEnumerator TalkRoutine(global::Celeste.Player player)
        {
            yield return PlayerApproach(player, turnToFace: true, 24f, (sectionsComplete != 1 && sectionsComplete != 2) ? 1 : (-1));
            yield return Level.ZoomTo(ZoomPoint, 2f, 0.5f);
            yield return Textbox.Say("MAGGYHELPER_CH5_OSHIRO_CLUTTER" + sectionsComplete + "_B", StandUp);
            yield return Level.ZoomBack(0.5f);
            Level.EndCutscene();
            EndTalkRoutine(Level);
        }

        private void EndTalkRoutine(Level level)
        {
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
            }
            talkRoutine = null;
            (Sprite as ModOshiroSprite).Pop("idle", flip: false);
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
        }

        private IEnumerator StandUp()
        {
            Audio.Play("event:/char/oshiro/chat_get_up", Position);
            (Sprite as ModOshiroSprite).Pop("idle", flip: false);
            yield return 0.25f;
        }

        private IEnumerator Pace()
        {
            while (true)
            {
                (Sprite as ModOshiroSprite).Wiggle();
                yield return PaceLeft();
                while (paceTimer < 2.266f)
                {
                    yield return null;
                }
                paceTimer = 0f;
                (Sprite as ModOshiroSprite).Wiggle();
                yield return PaceRight();
                while (paceTimer < 2.266f)
                {
                    yield return null;
                }
                paceTimer = 0f;
            }
        }

        public IEnumerator PaceRight()
        {
            Vector2 homePosition = HomePosition;
            if ((Position - homePosition).Length() > 8f)
            {
                paceSfx.Play("event:/char/oshiro/move_04_pace_right");
            }
            yield return MoveTo(homePosition.X);
        }

        public IEnumerator PaceLeft()
        {
            Vector2 vector = HomePosition + new Vector2(-20f, 0f);
            if ((Position - vector).Length() > 8f)
            {
                paceSfx.Play("event:/char/oshiro/move_04_pace_left");
            }
            yield return MoveTo(vector.X);
        }

        public override void Update()
        {
            base.Update();
            paceTimer += Engine.DeltaTime;
            global::Celeste.Player entity = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (sectionsComplete == 3 && !inRoutine && entity != null && entity.X < base.X + 32f && entity.Y <= base.Y)
            {
                OnTalk(entity);
                inRoutine = true;
            }
            if (sectionsComplete == 0 && !talked)
            {
                Level level = base.Scene as Level;
                if (entity != null && !entity.Dead)
                {
                    float num = Calc.ClampedMap(Vector2.Distance(base.Center, entity.Center), 40f, 128f);
                    level.Session.Audio.Music.Layer(1, num);
                    level.Session.Audio.Music.Layer(2, 1f - num);
                    level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                }
                else
                {
                    level.Session.Audio.Music.Layer(1, value: true);
                    level.Session.Audio.Music.Layer(2, value: false);
                    level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                }
            }
        }
    }
}
