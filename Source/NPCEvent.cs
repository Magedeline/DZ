using System.Runtime.CompilerServices;
using MaggyHelper.Entities;
using MaggyHelper.Cutscenes;
using MaggyHelper.NPCs;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities
{
    [CustomEntity(ids:"DesoloZantas/NPC_Event")]
    [Tracked(true)]
    public partial class NpcEvent : Entity
    {
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NpcEvent(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
            Add(Talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -16f), OnTalk));
            Add(Light = new VertexLight(Color.White, 1f, 16, 32));
            Depth = 1000;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NpcEvent(Vector2 position) : base(position)
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
            Add(Talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -16f), OnTalk));
            Add(Light = new VertexLight(Color.White, 1f, 16, 32));
            Depth = 1000;
        }

        protected virtual void OnTalk(global::Celeste.Player player) {
            // Default talk behavior - override in derived classes
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
                    Audio.Play("event:/char/madeline/footstep", this.Position);
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
                    Audio.Play("event:/char/madeline/footstep", this.Position);
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
                    Audio.Play("event:/char/madeline/footstep", this.Position);
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
                    Audio.Play("event:/char/madeline/footstep", this.Position);
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
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Chara")]
    [Tracked(true)]
    public partial class Npc_Chara : NpcEvent
    {
        public Npc_Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/chara/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Kirby")]
    [Tracked(true)]
    public partial class Npc_Kirby : NpcEvent
    {
        public Npc_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/kirby/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Ralsei")]
    [Tracked(true)]
    public partial class Npc_Ralsei : NpcEvent
    {
        public Npc_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/ralsei/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_MetaKnight")]
    [Tracked(true)]
    public partial class Npc_MetaKnight : NpcEvent
    {
        public Npc_MetaKnight(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/metaknight/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_DigitalGuide")]
    [Tracked(true)]
    public partial class Npc_DigitalGuide : NpcEvent
    {
        public Npc_DigitalGuide(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/digitalguide/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Phone")]
    [Tracked(true)]
    public partial class Npc_Phone : NpcEvent
    {
        public Npc_Phone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/phone/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Roxus")]
    [Tracked(true)]
    public partial class Npc_Roxus : NpcEvent
    {
        public Npc_Roxus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/roxus/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Temmie")]
    [Tracked(true)]
    public partial class Npc_Temmie : NpcEvent
    {
        public Npc_Temmie(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/temmie/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Axis")]
    [Tracked(true)]
    public partial class Npc_Axis : NpcEvent
    {
        public Npc_Axis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/axis/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_Els")]
    [Tracked(true)]
    public partial class Npc_Els : NpcEvent
    {
        public Npc_Els(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/els/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC_TitanCouncilMember")]
    [Tracked(true)]
    public partial class Npc_TitanCouncilMember : NpcEvent
    {
        public Npc_TitanCouncilMember(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/titancouncil/"));
            Sprite.CenterOrigin();
        }
    }

    // Chapter-specific NPC implementations

    [CustomEntity("DesoloZantas/NPC00_Theo")]
    [Tracked(true)]
    public partial class Npc00_Theo : NpcEvent
    {
        public Npc00_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new Cs00Theo(player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC01_Maggy")]
    [Tracked(true)]
    public partial class Npc01_Maggy : NpcEvent
    {
        public Npc01_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/maggy/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC02_Maggy")]
    [Tracked(true)]
    public partial class Npc02_Maggy : NpcEvent
    {
        public Npc02_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/maggy/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC03_Maggy")]
    [Tracked(true)]
    public partial class Npc03_Maggy : NpcEvent
    {
        public Npc03_Maggy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/maggy/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC03_Theo")]
    [Tracked(true)]
    public partial class Npc03_Theo : NpcEvent
    {
        public Npc03_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Magolor_Vents")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Vents : NpcEvent
    {
        public Npc05_Magolor_Vents(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/magolor/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC05_MagolorEscape")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Escape : NpcEvent
    {
        public Npc05_Magolor_Escape(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/magolor/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<NPC05_Magolor_Escaping>();
            Level.StartCutscene(new CS05_MagolorEscape(magolor, player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Breakdown")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Breakdown : NpcEvent
    {
        public Npc05_Oshiro_Breakdown(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
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
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
            this.index = data.Int("index", 0);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS05_OshiroClutter(player, this.sectionsComplete, this.index).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Hallway1")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway1 : NpcEvent
    {
        public Npc05_Oshiro_Hallway1(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Hallway1>();
            Level.StartCutscene(new CS05_OshiroHallway1(player, oshiro).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Hallway2")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway2 : NpcEvent
    {
        public Npc05_Oshiro_Hallway2(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Hallway2>();
            Level.StartCutscene(new CS05_OshiroHallway2(player, oshiro).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Lobby")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Lobby : NpcEvent
    {
        public Npc05_Oshiro_Lobby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Lobby>();
            Level.StartCutscene(new CS05_OshiroLobby(player, oshiro).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Rooftop")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Rooftop : NpcEvent
    {
        public Npc05_Oshiro_Rooftop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<Celeste.NPC>();
            if (oshiro != null)
            {
                Scene.Add(new CS05_OshiroRooftop(oshiro));
            }
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC05_Oshiro_Suite")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Suite : NpcEvent
    {
        public Npc05_Oshiro_Suite(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var oshiro = Scene.Tracker.GetEntity<NPC05_Oshiro_Suite>();
            Level.StartCutscene(new CS05_OshiroMasterSuite(oshiro).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC06_Magolor")]
    [Tracked(true)]
    public partial class Npc06_Magolor : NpcEvent
    {
        public Npc06_Magolor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/magolor/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<NPC06_Magolor>();
            var gondola = Scene.Tracker.GetEntity<GondolaMaggy>();
            Level.StartCutscene(new CS06_Gondola(magolor, gondola, player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC06_Theo")]
    [Tracked(true)]
    public partial class Npc06_Theo : NpcEvent
    {
        public Npc06_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC07_Chara")]
    [Tracked(true)]
    public partial class Npc07_Chara : NpcEvent
    {
        public Npc07_Chara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/chara/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS07_Darker(player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC07_Maddy_Mirror")]
    [Tracked(true)]
    public partial class Npc07_Maddy_Mirror : NpcEvent
    {
        public Npc07_Maddy_Mirror(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/theoCrystal/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new Cs07MaddyMirror(player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Chara_Crying")]
    [Tracked(true)]
    public partial class Npc08_Chara_Crying : NpcEvent
    {
        public Npc08_Chara_Crying(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/chara/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var chara = Scene.Tracker.GetEntity<Npc08CharaCrying>();
            Level.StartCutscene(new Cs08CharaBossEnd(player, chara).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Maddy_and_Theo_Ending")]
    [Tracked(true)]
    public partial class Npc08_Maddy_and_Theo_Ending : NpcEvent
    {
        public Npc08_Maddy_and_Theo_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/madeline/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<Npc08MaggyEnding>();
            var theoNMaddy = Scene.Tracker.GetEntity<Npc08MaddyAndTheoEnding>();
            Level.StartCutscene(new Cs08End(player, theoNMaddy, magolor).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Madeline_Plateau")]
    [Tracked(true)]
    public partial class Npc08_Madeline_Plateau : NpcEvent
    {
        public Npc08_Madeline_Plateau(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/madeline/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var madelineNPC = Scene.Tracker.GetEntity<Npc08MadelinePlateau>();
            var madeline = Scene.Tracker.GetEntity<CelesteNPC>();
            Level.StartCutscene(new Cs08Campfire(madelineNPC, player, madeline).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC08_Maggy_Ending")]
    [Tracked(true)]
    public partial class Npc08_Maggy_Ending : NpcEvent
    {
        public Npc08_Maggy_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/maggy/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var theoNMaddy = Scene.Tracker.GetEntity<Npc08MaddyAndTheoEnding>();
            var magolor = Scene.Tracker.GetEntity<Npc08MaggyEnding>();
            Level.StartCutscene(new Cs08End(player, theoNMaddy, magolor).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Kirby")]
    [Tracked(true)]
    public partial class Npc17_Kirby : NpcEvent
    {
        public Npc17_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/kirby/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS17_EndingMod().OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Oshiro")]
    [Tracked(true)]
    public partial class Npc17_Oshiro : NpcEvent
    {
        public Npc17_Oshiro(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/oshiro/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS17_EndingMod().OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Ralsei")]
    [Tracked(true)]
    public partial class Npc17_Ralsei : NpcEvent
    {
        public Npc17_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/ralsei/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS17_EndingMod().OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Theo")]
    [Tracked(true)]
    public partial class Npc17_Theo : NpcEvent
    {
        public Npc17_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/theo/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS17_EndingMod().OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC17_Toriel")]
    [Tracked(true)]
    public partial class Npc17_Toriel : NpcEvent
    {
        public Npc17_Toriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/toriel/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS17_EndingMod().OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC18_Toriel_Inside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Inside : NpcEvent
    {
        public Npc18_Toriel_Inside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/toriel/"));
            Sprite.CenterOrigin();
        }
        
    }

    [CustomEntity("DesoloZantas/NPC18_Toriel_Outside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Outside : NpcEvent
    {
        public Npc18_Toriel_Outside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/toriel/"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DesoloZantas/NPC19_Gravestone")]
    [Tracked(true)]
    public partial class Npc19_Gravestone : NpcEvent
    {
        public Npc19_Gravestone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/gravestone/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var gravestone = Scene.Tracker.GetEntity<NPC19_Gravestone>();
            Level.StartCutscene(new CS19_Gravestone(player, gravestone, Position).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC19_Maggy_Loop")]
    [Tracked(true)]
    public partial class Npc19_Maggy_Loop : NpcEvent
    {
        public Npc19_Maggy_Loop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/maggy/"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            CharaDummy charaDummy = Level.Tracker.GetEntity<CharaDummy>();
            if (charaDummy != null)
            {
                Level.StartCutscene(new Cs19TrapinLoop(player, charaDummy).OnEnd);
            }
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Asriel")]
    [Tracked(true)]
    public partial class Npc20_Asriel : NpcEvent
    {
        public Npc20_Asriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/asriel/"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float v1, float v2, bool v3)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS20_Saved(player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Granny")]
    [Tracked(true)]
    public partial class Npc20_Granny : NpcEvent
    {
        public Npc20_Granny(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/granny/"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float v)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS20_Saved(player).OnEnd);
            base.OnTalk(player);
        }
    }

    [CustomEntity("DesoloZantas/NPC20_Madeline")]
    [Tracked(true)]
    public partial class Npc20_Madeline : NpcEvent
    {
        public Npc20_Madeline(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Sprite = new Sprite(GFX.Game, "characters/madeline/"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float v)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            Level.StartCutscene(new CS20_Saved(player).OnEnd);
            base.OnTalk(player);
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




