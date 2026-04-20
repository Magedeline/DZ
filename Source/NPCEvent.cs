using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.Cutscenes;
using Celeste.NPCs;
using Celeste.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids:"DesoloZantas/NPC_Event,MaggyHelper/NPC_Event")]
    [Tracked(true)]
    public partial class NpcEvent : Entity
    {
        private const string DefaultSpriteDirectory = "characters/theo/";

        public const string MET_THEO = "MetMagolor";
        public const string THEO_KNOWS_NAME = "MagolorKnowsName";
        public const float THEO_MAX_SPEED = 48f;
        public Sprite Sprite;
        public TalkComponent Talker;
        public VertexLight Light;
        public Level Level;
        public SoundSource PhoneTapSfx;
        public float Maxspeed = 80f;
        public string MoveAnim = "";
        public string IdleAnim = "";
        public bool MoveY = true;
        public bool UpdateLight = true;
        public List<Entity> Temp = new List<Entity>();
        public Session Session => this.Level?.Session;
        protected string DialogKey { get; }
        protected string FlagName { get; }
        protected string EventId { get; }

        private bool configuredInteractionRunning;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NpcEvent(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            DialogKey = data.Attr("dialogKey", string.Empty);
            FlagName = data.Attr("flagName", string.Empty);
            EventId = data.Attr("eventId", string.Empty);

            InitializeBaseComponents();
            SetSpriteDirectory(ResolveSpriteDirectory(data.Attr("spriteId", string.Empty)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NpcEvent(Vector2 position) : base(position)
        {
            DialogKey = string.Empty;
            FlagName = string.Empty;
            EventId = string.Empty;

            InitializeBaseComponents();
            SetSpriteDirectory(DefaultSpriteDirectory);
        }

        private void InitializeBaseComponents()
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -16f), OnTalk));
            Add(Light = new VertexLight(Color.White, 1f, 16, 32));
            Depth = 1000;
        }

        protected void SetSpriteDirectory(string spriteDirectory)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }

            Add(Sprite = new Sprite(GFX.Game, spriteDirectory));
            Sprite.CenterOrigin();
        }

        protected bool TryAddCutscene(CutsceneEntity cutscene)
        {
            if (Level == null || cutscene == null)
            {
                return false;
            }

            Level.Add(cutscene);
            return true;
        }

        private bool RunNpcActionOnce(Level level, string flag, Func<bool> action)
        {
            if (!string.IsNullOrWhiteSpace(flag) && level.Session.GetFlag(flag))
            {
                return true;
            }

            try
            {
                if (!action())
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(flag))
                {
                    level.Session.SetFlag(flag, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(NpcEvent), $"Failed to run eventId '{EventId}' for {GetType().Name}: {ex}");
                return false;
            }
        }

        private bool TriggerNpcEvent(Level level, string flag, Func<CutsceneEntity> cutsceneFactory)
        {
            return RunNpcActionOnce(level, flag, () => {
                var cutscene = cutsceneFactory();
                if (cutscene == null)
                {
                    return false;
                }

                level.Add(cutscene);
                return true;
            });
        }

        protected virtual void OnTalk(global::Celeste.Player player)
        {
            if (configuredInteractionRunning || Scene is not Level level)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(FlagName) && level.Session.GetFlag(FlagName))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(EventId))
            {
                bool dispatched = CutsceneEventDispatcher.TryDispatch(
                    level,
                    player,
                    EventId,
                    (flag, factory) => TriggerNpcEvent(level, flag, factory),
                    (flag, action) => RunNpcActionOnce(level, flag, action));

                if (dispatched && !string.IsNullOrWhiteSpace(FlagName))
                {
                    level.Session.SetFlag(FlagName, true);
                }

                return;
            }

            string dialogKey = DialogKey;
            if (!string.IsNullOrWhiteSpace(dialogKey))
            {
                Add(new Coroutine(RunConfiguredDialogue(level, player, dialogKey)));
            }
        }

        private IEnumerator RunConfiguredDialogue(Level level, global::Celeste.Player player, string dialogKey)
        {
            configuredInteractionRunning = true;
            level.StartCutscene(EndConfiguredDialogue);

            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StDummy;
            }

            yield return Textbox.Say(dialogKey);
            EndConfiguredDialogue(level);
        }

        private void EndConfiguredDialogue(Level level)
        {
            if (!configuredInteractionRunning)
            {
                return;
            }

            configuredInteractionRunning = false;

            if (!string.IsNullOrWhiteSpace(FlagName))
            {
                level.Session.SetFlag(FlagName, true);
            }

            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }

            if (level.InCutscene)
            {
                level.EndCutscene();
            }
        }

        private static string ResolveSpriteDirectory(string spriteId)
        {
            if (string.IsNullOrWhiteSpace(spriteId))
            {
                return DefaultSpriteDirectory;
            }

            return spriteId switch
            {
                "theo" => "characters/theo/",
                "chara" => "characters/chara/",
                "kirby" => "characters/kirby/",
                "ralsei" => "characters/ralsei/",
                "madeline" => "characters/madeline/",
                "badeline" => "characters/badeline/",
                "maggy" => "characters/maggy/",
                "magolor" => "characters/magolor/",
                "magalor" => "characters/magolor/",
                "toriel" => "characters/toriel/",
                "asriel" => "characters/asriel/",
                "oshiro" => "characters/oshiro/",
                "granny" => "characters/granny/",
                "meta_knight" => "characters/metaknight/",
                "metaknight" => "characters/metaknight/",
                "roxus" => "characters/roxus/",
                "temmie" => "characters/temmie/",
                "axis" => "characters/axis/",
                "els" => "characters/els/",
                "digital_guide" => "characters/digitalguide/",
                "digitalguide" => "characters/digitalguide/",
                "phone" => "characters/phone/",
                "titan_council_member" => "characters/titancouncil/",
                "titancouncil" => "characters/titancouncil/",
                _ when spriteId.Contains("/") => spriteId.EndsWith("/") ? spriteId : $"{spriteId}/",
                _ => $"characters/{spriteId.TrimEnd('/')}/"
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.Level = scene as Level;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            if (this.UpdateLight && this.Light != null)
            {
                Rectangle bounds = this.Level?.Bounds ?? default;
                this.Light.Alpha = Calc.Approach(this.Light.Alpha,
                    (this.X <= bounds.Left - 16 || this.Y <= bounds.Top - 16 ||
                     this.X >= bounds.Right + 16 || this.Y >= bounds.Bottom + 16 ||
                     (this.Level?.Transitioning ?? false))
                        ? 0.0f
                        : 1f, Engine.DeltaTime * 2f);
            }
            if (this.Sprite != null && this.Sprite.CurrentAnimationID == "usePhone")
            {
                if (this.PhoneTapSfx == null)
                    this.Add(this.PhoneTapSfx = new SoundSource());
                if (!this.PhoneTapSfx.Playing)
                    this.PhoneTapSfx.Play("event:/char/theo/phone_taps_loop");
            }
            else
            {
                this.PhoneTapSfx?.Stop();
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            if (this.Light != null && this.UpdateLight) this.Light.Position = this.Position + new Vector2(4f, 4f);
            base.Render();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void SetupTheoSpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) ||
                    (anim == "run" && (currentAnimationFrame == 0 || currentAnimationFrame == 4)))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "crawl" && currentAnimationFrame == 0)
                {
                    if (!(this.Level?.Transitioning ?? false))
                        Audio.Play("event:/char/theo/resort_crawl", this.Position);
                }
                else if (anim == "pullVent" && currentAnimationFrame == 0)
                {
                    Audio.Play("event:/char/theo/resort_vent_tug", this.Position);
                }
            };
        }
        public virtual void SetupGrannySpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "walk" && currentAnimationFrame == 2)
                {
                    Audio.Play("event:/char/granny/cane_tap", this.Position);
                }
            };
        }
        public virtual void SetupMadelineSpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "walk" && currentAnimationFrame == 2)
                {
                    Audio.Play("event:/desolozantas/char/kirby/footstep", this.Position);
                }
            };
        }
        public virtual void SetupTorielSpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "walk" && currentAnimationFrame == 2)
                {
                    Audio.Play("event:/desolozantas/char/kirby/footstep", this.Position);
                }
            };
        }
        public virtual void SetupMagolorSpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "walk" && currentAnimationFrame == 2)
                {
                    Audio.Play("event:/desolozantas/char/kirby/footstep", this.Position);
                }
            };
        }
        public virtual void SetupMadNTheoSpriteSounds()
        {
            this.Sprite.OnFrameChange = anim =>
            {
                int currentAnimationFrame = this.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
                {
                    Platform platformByPriority =
                        SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.Temp));
                    if (platformByPriority != null)
                    {
                        Audio.Play(
                            SurfaceIndex.GetPathFromIndex(platformByPriority.GetStepSoundIndex(this)) + "/footstep",
                            this.Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
                    }
                }
                else if (anim == "walk" && currentAnimationFrame == 2)
                {
                    Audio.Play("event:/desolozantas/char/kirby/footstep", this.Position);
                }
            };
        }
    }

    // Add NPC07_Badeline as a specialized NPC Event type
    // Generic NPC implementations from NPCs folder
    
    [CustomEntity("DesoloZantas/NPC_Theo")]
    [Tracked(true)]
    public partial class Npc_Theo : NpcEvent
    {
        public Npc_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theo/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Chara")]
    [Tracked(true)]
    public partial class Npc_Chara : NpcEvent
    {
        public Npc_Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/chara/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Kirby")]
    [Tracked(true)]
    public partial class Npc_Kirby : NpcEvent
    {
        public Npc_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/kirby/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Ralsei")]
    [Tracked(true)]
    public partial class Npc_Ralsei : NpcEvent
    {
        public Npc_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/ralsei/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_MetaKnight")]
    [Tracked(true)]
    public partial class Npc_MetaKnight : NpcEvent
    {
        public Npc_MetaKnight(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/metaknight/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_DigitalGuide")]
    [Tracked(true)]
    public partial class Npc_DigitalGuide : NpcEvent
    {
        public Npc_DigitalGuide(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/digitalguide/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Phone")]
    [Tracked(true)]
    public partial class Npc_Phone : NpcEvent
    {
        public Npc_Phone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/phone/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Roxus")]
    [Tracked(true)]
    public partial class Npc_Roxus : NpcEvent
    {
        public Npc_Roxus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/roxus/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Temmie")]
    [Tracked(true)]
    public partial class Npc_Temmie : NpcEvent
    {
        public Npc_Temmie(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/temmie/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Axis")]
    [Tracked(true)]
    public partial class Npc_Axis : NpcEvent
    {
        public Npc_Axis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/axis/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_Els")]
    [Tracked(true)]
    public partial class Npc_Els : NpcEvent
    {
        public Npc_Els(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/els/");
        }
    }

    [CustomEntity("DesoloZantas/NPC_TitanCouncilMember")]
    [Tracked(true)]
    public partial class Npc_TitanCouncilMember : NpcEvent
    {
        public Npc_TitanCouncilMember(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/titancouncil/");
        }
    }

    // Chapter-specific NPC implementations

    [CustomEntity("DesoloZantas/NPC00_Theo")]
    [Tracked(true)]
    public partial class Npc00_Theo : NpcEvent
    {
        public Npc00_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theo/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new Cs00Theo(player));
        }
    }

    [CustomEntity("DesoloZantas/NPC01_Maggy")]
    [Tracked(true)]
    public partial class Npc01_Maggy : NpcEvent
    {
        public Npc01_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/maggy/");
        }
    }

    [CustomEntity("DesoloZantas/NPC02_Maggy")]
    [Tracked(true)]
    public partial class Npc02_Maggy : NpcEvent
    {
        public Npc02_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/maggy/");
        }
    }

    [CustomEntity("DesoloZantas/NPC03_Maggy")]
    [Tracked(true)]
    public partial class Npc03_Maggy : NpcEvent
    {
        public Npc03_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/maggy/");
        }
    }

    [CustomEntity("DesoloZantas/NPC03_Theo")]
    [Tracked(true)]
    public partial class Npc03_Theo : NpcEvent
    {
        public Npc03_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theo/");
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Magolor_Vents")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Vents : NpcEvent
    {
        public Npc05_Magolor_Vents(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/magolor/");
        }
    }

    [CustomEntity("DesoloZantas/NPC05_MagolorEscape")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Escape : NpcEvent
    {
        public Npc05_Magolor_Escape(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/magolor/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<NPC05_Magolor_Escaping>();
            if (magolor != null)
            {
                TryAddCutscene(new CS05_MagolorEscape(magolor, player));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Breakdown")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Breakdown : NpcEvent
    {
        public Npc05_Oshiro_Breakdown(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Clutter")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Clutter : NpcEvent
    {
        private int index;
        #pragma warning disable CS0649
            private NPC03_Oshiro_Cluttter sectionsComplete;
        #pragma warning restore CS0649

        public Npc05_Oshiro_Clutter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
            this.index = data.Int("index", 0);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS05_OshiroClutter(player, this.sectionsComplete, this.index));
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Hallway1")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway1 : NpcEvent
    {
        public Npc05_Oshiro_Hallway1(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Hallway1>();
            if (oshiro != null)
            {
                TryAddCutscene(new CS05_OshiroHallway1(player, oshiro));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Hallway2")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway2 : NpcEvent
    {
        public Npc05_Oshiro_Hallway2(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Hallway2>();
            if (oshiro != null)
            {
                TryAddCutscene(new CS05_OshiroHallway2(player, oshiro));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Lobby")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Lobby : NpcEvent
    {
        public Npc05_Oshiro_Lobby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Lobby>();
            if (oshiro != null)
            {
                TryAddCutscene(new CS05_OshiroLobby(player, oshiro));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Rooftop")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Rooftop : NpcEvent
    {
        public Npc05_Oshiro_Rooftop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<global::Celeste.NPC>();
            if (oshiro != null)
            {
                TryAddCutscene(new CS05_OshiroRooftop(oshiro));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Suite")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Suite : NpcEvent
    {
        public Npc05_Oshiro_Suite(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Suite>();
            if (oshiro != null)
            {
                TryAddCutscene(new CS05_OshiroMasterSuite(oshiro));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC06_Magolor")]
    [Tracked(true)]
    public partial class Npc06_Magolor : NpcEvent
    {
        public Npc06_Magolor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/magolor/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<NPC06_Magolor>();
            var gondola = Scene.Tracker.GetEntity<GondolaMaggy>();
            if (magolor != null && gondola != null)
            {
                TryAddCutscene(new CS06_Gondola(magolor, gondola, player));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC06_Theo")]
    [Tracked(true)]
    public partial class Npc06_Theo : NpcEvent
    {
        public Npc06_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theo/");
        }
    }

    [CustomEntity("DesoloZantas/NPC07_Chara")]
    [Tracked(true)]
    public partial class Npc07_Chara : NpcEvent
    {
        public Npc07_Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/chara/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS07_Darker(player));
        }
    }

    [CustomEntity("DesoloZantas/NPC07_Maddy_Mirror")]
    [Tracked(true)]
    public partial class Npc07_Maddy_Mirror : NpcEvent
    {
        public Npc07_Maddy_Mirror(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theoCrystal/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new Cs07MaddyMirror(player));
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Chara_Crying")]
    [Tracked(true)]
    public partial class Npc08_Chara_Crying : NpcEvent
    {
        public Npc08_Chara_Crying(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/chara/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var chara = Scene.Tracker.GetEntity<Npc08CharaCrying>();
            if (chara != null)
            {
                TryAddCutscene(new Cs08CharaBossEnd(player, chara));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Maddy_and_Theo_Ending")]
    [Tracked(true)]
    public partial class Npc08_Maddy_and_Theo_Ending : NpcEvent
    {
        public Npc08_Maddy_and_Theo_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/madeline/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<Npc08MaggyEnding>();
            var theoNMaddy = Scene.Tracker.GetEntity<Npc08MaddyAndTheoEnding>();
            if (theoNMaddy != null && magolor != null)
            {
                TryAddCutscene(new Cs08End(player, theoNMaddy, magolor));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Madeline_Plateau")]
    [Tracked(true)]
    public partial class Npc08_Madeline_Plateau : NpcEvent
    {
        public Npc08_Madeline_Plateau(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/madeline/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var madelineNPC = Scene.Tracker.GetEntity<Npc08MadelinePlateau>();
            var madeline = Scene.Tracker.GetEntity<CelesteNPC>();
            if (madelineNPC != null && madeline != null)
            {
                TryAddCutscene(new Cs08Campfire(madelineNPC, player, madeline));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Maggy_Ending")]
    [Tracked(true)]
    public partial class Npc08_Maggy_Ending : NpcEvent
    {
        public Npc08_Maggy_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/maggy/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var theoNMaddy = Scene.Tracker.GetEntity<Npc08MaddyAndTheoEnding>();
            var magolor = Scene.Tracker.GetEntity<Npc08MaggyEnding>();
            if (theoNMaddy != null && magolor != null)
            {
                TryAddCutscene(new Cs08End(player, theoNMaddy, magolor));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Kirby")]
    [Tracked(true)]
    public partial class Npc17_Kirby : NpcEvent
    {
        public Npc17_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/kirby/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Oshiro")]
    [Tracked(true)]
    public partial class Npc17_Oshiro : NpcEvent
    {
        public Npc17_Oshiro(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/oshiro/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Ralsei")]
    [Tracked(true)]
    public partial class Npc17_Ralsei : NpcEvent
    {
        public Npc17_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/ralsei/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Theo")]
    [Tracked(true)]
    public partial class Npc17_Theo : NpcEvent
    {
        public Npc17_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/theo/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Toriel")]
    [Tracked(true)]
    public partial class Npc17_Toriel : NpcEvent
    {
        public Npc17_Toriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/toriel/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DesoloZantas/NPC18_Toriel_Inside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Inside : NpcEvent
    {
        public Npc18_Toriel_Inside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/toriel/");
        }
        
    }

    [CustomEntity("DesoloZantas/NPC18_Toriel_Outside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Outside : NpcEvent
    {
        public Npc18_Toriel_Outside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/toriel/");
        }
    }

    [CustomEntity("DesoloZantas/NPC19_Gravestone")]
    [Tracked(true)]
    public partial class Npc19_Gravestone : NpcEvent
    {
        public Npc19_Gravestone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/gravestone/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var gravestone = Scene.Tracker.GetEntity<NPC19_Gravestone>();
            if (gravestone != null)
            {
                TryAddCutscene(new CS19_Gravestone(player, gravestone, Position));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC19_Maggy_Loop")]
    [Tracked(true)]
    public partial class Npc19_Maggy_Loop : NpcEvent
    {
        public Npc19_Maggy_Loop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/maggy/");
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            CharaDummy charaDummy = Level.Tracker.GetEntity<CharaDummy>();
            if (charaDummy != null)
            {
                TryAddCutscene(new Cs19TrapinLoop(player, charaDummy));
            }
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Asriel")]
    [Tracked(true)]
    public partial class Npc20_Asriel : NpcEvent
    {
        public Npc20_Asriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/asriel/");
        }

        internal IEnumerator MoveTo(float v1, float v2, bool v3)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS20_Saved(player));
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Granny")]
    [Tracked(true)]
    public partial class Npc20_Granny : NpcEvent
    {
        public Npc20_Granny(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/granny/");
        }

        internal IEnumerator MoveTo(float v)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS20_Saved(player));
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Madeline")]
    [Tracked(true)]
    public partial class Npc20_Madeline : NpcEvent
    {
        public Npc20_Madeline(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SetSpriteDirectory("characters/madeline/");
        }

        internal IEnumerator MoveTo(float v)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS20_Saved(player));
        }
    }
    [CustomEntity("DesoloZantas/NPCEventInteract")]
    [Tracked(true)]
    public class NPCEventInteract : NpcEvent
    {
        public NPCEventInteract(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}




