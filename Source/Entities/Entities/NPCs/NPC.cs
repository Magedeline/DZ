using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Celeste.Cutscenes;
using Celeste.NPCs;
using Celeste.Triggers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    [CustomEntity(ids:"DZ/NPC,DZ/NPC")]
    [Tracked(true)]
    public partial class NPC : Entity
    {
        private const string DefaultSpriteDirectory = "characters/DZ/theo/";

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

        private string cutsceneClass;
        private bool onlyOnce;
        private bool unskippable;
        private bool shouldDisable;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NPC(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            DialogKey = data.Attr("dialogKey", string.Empty);
            FlagName = data.Attr("flagName", string.Empty);
            EventId = data.Attr("eventId", string.Empty);
            cutsceneClass = data.Attr("cutsceneClass", string.Empty);
            onlyOnce = data.Bool("onlyOnce", false);
            unskippable = data.Bool("unskippable", false);
            shouldDisable = false;

            InitializeBaseComponents();
            string spriteId = ResolveSpriteId(data.Attr("spriteId", string.Empty));
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create(spriteId));
            Sprite.CenterOrigin();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NPC(Vector2 position) : base(position)
        {
            DialogKey = string.Empty;
            FlagName = string.Empty;
            EventId = string.Empty;

            InitializeBaseComponents();
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }

        private void InitializeBaseComponents()
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Add(Talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -16f), OnTalk));
            Add(Light = new VertexLight(Color.White, 1f, 16, 32));
            Depth = 1000;
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
                Logger.Log(LogLevel.Error, nameof(NPC), $"Failed to run eventId '{EventId}' for {GetType().Name}: {ex}");
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

            if (!string.IsNullOrWhiteSpace(cutsceneClass))
            {
                if (TryInstantiateAndAddCutscene(level, player))
                {
                    if (onlyOnce)
                    {
                        shouldDisable = true;
                    }
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(EventId))
            {
                player.StateMachine.State = Player.StDummy;
                level.StartCutscene(ctx => {
                    Player p = ctx.Tracker.GetEntity<Player>();
                    if (p != null) p.StateMachine.State = Player.StNormal;
                });

                if (!string.IsNullOrWhiteSpace(FlagName))
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

        private bool TryInstantiateAndAddCutscene(Level level, global::Celeste.Player player)
        {
            if (string.IsNullOrWhiteSpace(cutsceneClass))
            {
                return false;
            }

            Type cutsceneType = FindType(cutsceneClass);
            if (cutsceneType == null)
            {
                Logger.Log(LogLevel.Warn, nameof(NPC), $"Cutscene class not found: '{cutsceneClass}'");
                return false;
            }

            if (!typeof(CutsceneEntity).IsAssignableFrom(cutsceneType))
            {
                Logger.Log(LogLevel.Warn, nameof(NPC), $"Type '{cutsceneClass}' is not a CutsceneEntity.");
                return false;
            }

            CutsceneEntity cutscene = null;
            try
            {
                var ctor = cutsceneType.GetConstructor(new[] { typeof(NPC), typeof(global::Celeste.Player) });
                if (ctor != null)
                {
                    cutscene = (CutsceneEntity)ctor.Invoke(new object[] { this, player });
                }
                else
                {
                    ctor = cutsceneType.GetConstructor(new[] { typeof(global::Celeste.Player) });
                    if (ctor != null)
                    {
                        cutscene = (CutsceneEntity)ctor.Invoke(new object[] { player });
                    }
                    else
                    {
                        ctor = cutsceneType.GetConstructor(Type.EmptyTypes);
                        if (ctor != null)
                        {
                            cutscene = (CutsceneEntity)ctor.Invoke(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, nameof(NPC), $"Failed to instantiate cutscene '{cutsceneClass}': {ex}");
                return false;
            }

            if (cutscene == null)
            {
                Logger.Log(LogLevel.Warn, nameof(NPC), $"No suitable constructor found for cutscene '{cutsceneClass}'. Expected (NPC, Player), (Player), or ().");
                return false;
            }

            return TryAddCutscene(cutscene);
        }

        private static Type FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = asm.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private static string ResolveSpriteId(string spriteId)
        {
            if (string.IsNullOrWhiteSpace(spriteId))
            {
                return "theo";
            }

            return spriteId switch
            {
                "theo" => "theo",
                "chara" => "chara",
                "kirby" => "kirby",
                "ralsei" => "ralsei",
                "madeline" => "madeline",
                "badeline" => "badeline",
                "DZ" => "magolor",
                "magolor" => "magolor",
                "magalor" => "magolor",
                "toriel" => "toriel",
                "asriel" => "asriel",
                "oshiro" => "oshiro",
                "granny" => "granny",
                "meta_knight" => "metaknight",
                "metaknight" => "metaknight",
                "roxus" => "roxus",
                "temmie" => "temmie",
                "axis" => "axis",
                "els" => "els",
                "digital_guide" => "digitalguide",
                "digitalguide" => "digitalguide",
                "phone" => "phone",
                "titan_council_member" => "titancouncil",
                "titancouncil" => "titancouncil",
                _ => spriteId
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
            if (shouldDisable && Talker != null && Talker.Enabled)
            {
                Talker.Enabled = false;
            }

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
                    Audio.Play("event:/DZ/char/kirby/footstep", this.Position);
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
                    Audio.Play("event:/DZ/char/kirby/footstep", this.Position);
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
                    Audio.Play("event:/DZ/char/kirby/footstep", this.Position);
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
                    Audio.Play("event:/DZ/char/kirby/footstep", this.Position);
                }
            };
        }
    }

    // Add NPC07_Badeline as a specialized NPC Event type
    // Generic NPC implementations from NPCs folder
    
    [CustomEntity("DZ/NPC_Theo")]
    [Tracked(true)]
    public partial class Npc_Theo : NPC
    {
        public Npc_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPCDZ_CHara")]
    [Tracked(true)]
    public partial class NpcDZ_CHara : NPC
    {
        public NpcDZ_CHara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("chara"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Kirby")]
    [Tracked(true)]
    public partial class Npc_Kirby : NPC
    {
        public Npc_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("kirby"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Ralsei")]
    [Tracked(true)]
    public partial class Npc_Ralsei : NPC
    {
        public Npc_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("ralsei"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_MetaKnight")]
    [Tracked(true)]
    public partial class Npc_MetaKnight : NPC
    {
        public Npc_MetaKnight(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("metaknight"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_DigitalGuide")]
    [Tracked(true)]
    public partial class Npc_DigitalGuide : NPC
    {
        public Npc_DigitalGuide(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("digitalguide"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Phone")]
    [Tracked(true)]
    public partial class Npc_Phone : NPC
    {
        public Npc_Phone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("phone"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Roxus")]
    [Tracked(true)]
    public partial class Npc_Roxus : NPC
    {
        public Npc_Roxus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("roxus"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Temmie")]
    [Tracked(true)]
    public partial class Npc_Temmie : NPC
    {
        public Npc_Temmie(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("temmie"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Axis")]
    [Tracked(true)]
    public partial class Npc_Axis : NPC
    {
        public Npc_Axis(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("axis"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_Els")]
    [Tracked(true)]
    public partial class Npc_Els : NPC
    {
        public Npc_Els(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("els"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC_TitanCouncilMember")]
    [Tracked(true)]
    public partial class Npc_TitanCouncilMember : NPC
    {
        public Npc_TitanCouncilMember(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("titancouncil"));
            Sprite.CenterOrigin();
        }
    }

    // Chapter-specific NPC implementations

    [CustomEntity("DZ/NPC00_Theo")]
    [Tracked(true)]
    public partial class Npc00_Theo : NPC
    {
        public Npc00_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC01_DZ")]
    [Tracked(true)]
    public partial class Npc01_DZ : NPC
    {
        public Npc01_DZ(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("DZ"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC02_DZ")]
    [Tracked(true)]
    public partial class Npc02_DZ : NPC
    {
        public Npc02_DZ(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.CenterOrigin();
        }
    }


    [CustomEntity("DZ/NPC03_Theo")]
    [Tracked(true)]
    public partial class Npc03_Theo : NPC
    {
        public Npc03_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC05_Magolor_Vents")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Vents : NPC
    {
        public Npc05_Magolor_Vents(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC05_MagolorEscape")]
    [Tracked(true)]
    public partial class Npc05_Magolor_Escape : NPC
    {
        public Npc05_Magolor_Escape(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC05_Oshiro_Breakdown")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Breakdown : NPC
    {
        public Npc05_Oshiro_Breakdown(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC05_Oshiro_Clutter")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Clutter : NPC
    {
        private int index;
        #pragma warning disable CS0649
            private NPC05_Oshiro_Clutter sectionsComplete;
        #pragma warning restore CS0649

        public Npc05_Oshiro_Clutter(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
            this.index = data.Int("index", 0);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS05_OshiroClutter(player, this.sectionsComplete, this.index));
        }
    }

    [CustomEntity("DZ/NPC05_Oshiro_Hallway1")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway1 : NPC
    {
        public Npc05_Oshiro_Hallway1(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC05_Oshiro_Hallway2")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Hallway2 : NPC
    {
        public Npc05_Oshiro_Hallway2(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC05_Oshiro_Lobby")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Lobby : NPC
    {
        public Npc05_Oshiro_Lobby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC05_Oshiro_Rooftop")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Rooftop : NPC
    {
        public Npc05_Oshiro_Rooftop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC05_Oshiro_Suite")]
    [Tracked(true)]
    public partial class Npc05_Oshiro_Suite : NPC
    {
        public Npc05_Oshiro_Suite(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC06_Magolor")]
    [Tracked(true)]
    public partial class Npc06_Magolor : NPC
    {
        public Npc06_Magolor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<NPC06_Magolor>();
            var gondola = Scene.Tracker.GetEntity<GondolaDZ>();
            if (magolor != null && gondola != null)
            {
                TryAddCutscene(new CS06_Gondola(magolor, gondola, player));
            }
        }
    }

    [CustomEntity("DZ/NPC06_Theo")]
    [Tracked(true)]
    public partial class Npc06_Theo : NPC
    {
        public Npc06_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC07DZ_CHara")]
    [Tracked(true)]
    public partial class Npc07DZ_CHara : NPC
    {
        public Npc07DZ_CHara(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("chara"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS07_Darker(player));
        }
    }

    [CustomEntity("DZ/NPC07_Maddy_Mirror")]
    [Tracked(true)]
    public partial class Npc07_Maddy_Mirror : NPC
    {
        public Npc07_Maddy_Mirror(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo_crystal"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new Cs07MaddyMirror(player));
        }
    }

    [CustomEntity("DZ/NPC08DZ_CHara_Crying")]
    [Tracked(true)]
    public partial class Npc08DZ_CHara_Crying : NPC
    {
        public Npc08DZ_CHara_Crying(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("chara"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC08_Maddy_and_Theo_Ending")]
    [Tracked(true)]
    public partial class Npc08_Maddy_and_Theo_Ending : NPC
    {
        public Npc08_Maddy_and_Theo_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("player"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var madelineBandage = Scene.Tracker.GetEntity<Npc08MadelineEndingBandage>();
            if (madelineBandage != null)
            {
                TryAddCutscene(new Cs08End(player));
            }
        }
    }

    [CustomEntity("DZ/NPC08_Madeline_Plateau")]
    [Tracked(true)]
    public partial class Npc08_Madeline_Plateau : NPC
    {
        public Npc08_Madeline_Plateau(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("player"));
            Sprite.CenterOrigin();
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

    [CustomEntity("DZ/NPC08_DZ_Ending")]
    [Tracked(true)]
    public partial class Npc08_DZ_Ending : NPC
    {
        public Npc08_DZ_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("magolor"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var magolor = Scene.Tracker.GetEntity<Npc08DZEnding>();
            if (magolor != null)
            {
                TryAddCutscene(new Cs08End(player));
            }
        }
    }
    
    public partial class Npc08_Theo_Ending : NPC
    {
        public Npc08_Theo_Ending(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            var theo = Scene.Tracker.GetEntity<Npc08TheoEnding>();
            if (theo != null)
            {
                TryAddCutscene(new Cs08End(player));
            }
        }
    }

    [CustomEntity("DZ/NPC17_Kirby")]
    [Tracked(true)]
    public partial class Npc17_Kirby : NPC
    {
        public Npc17_Kirby(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("kirby"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DZ/NPC17_Oshiro")]
    [Tracked(true)]
    public partial class Npc17_Oshiro : NPC
    {
        public Npc17_Oshiro(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("oshiro"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DZ/NPC17_Ralsei")]
    [Tracked(true)]
    public partial class Npc17_Ralsei : NPC
    {
        public Npc17_Ralsei(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("ralsei"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DZ/NPC17_Theo")]
    [Tracked(true)]
    public partial class Npc17_Theo : NPC
    {
        public Npc17_Theo(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DZ/NPC17_Toriel")]
    [Tracked(true)]
    public partial class Npc17_Toriel : NPC
    {
        public Npc17_Toriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("toriel"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS17_EndingMod());
        }
    }

    [CustomEntity("DZ/NPC18_Toriel_Inside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Inside : NPC
    {
        public Npc18_Toriel_Inside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("toriel"));
            Sprite.CenterOrigin();
        }
        
    }

    [CustomEntity("DZ/NPC18_Toriel_Outside")]
    [Tracked(true)]
    public partial class Npc18_Toriel_Outside : NPC
    {
        public Npc18_Toriel_Outside(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("toriel"));
            Sprite.CenterOrigin();
        }
    }

    [CustomEntity("DZ/NPC19_Gravestone")]
    [Tracked(true)]
    public partial class Npc19_Gravestone : NPC
    {
        public Npc19_Gravestone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("gravestone"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            // This class is a partial wrapper, don't use it directly
            // The actual NPC19_Gravestone entity should be used instead
        }
    }

    [CustomEntity("DZ/NPC19_DZ_Loop")]
    [Tracked(true)]
    public partial class Npc19_DZ_Loop : NPC
    {
        public Npc19_DZ_Loop(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("DZ"));
            Sprite.CenterOrigin();
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS19_TrapinLoop(player));
        }
    }

    [CustomEntity("DZ/NPC20_Asriel")]
    [Tracked(true)]
    public partial class Npc20_Asriel : NPC
    {
        public Npc20_Asriel(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("asriel"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float targetX, float targetY, bool waitForCompletion)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS21_RestorationAndFarewell(player));
        }
    }

    [CustomEntity("DZ/NPC20_Granny")]
    [Tracked(true)]
    public partial class Npc20_Granny : NPC
    {
        public Npc20_Granny(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float targetX)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS21_RestorationAndFarewell(player));
        }
    }

    [CustomEntity("DZ/NPC20_Madeline")]
    [Tracked(true)]
    public partial class Npc20_Madeline : NPC
    {
        public Npc20_Madeline(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (Sprite != null)
            {
                Remove(Sprite);
            }
            Add(Sprite = GFX.SpriteBank.Create("player"));
            Sprite.CenterOrigin();
        }

        internal IEnumerator MoveTo(float targetX)
        {
            yield return new WaitForSeconds(0.5f);
        }
        protected override void OnTalk(global::Celeste.Player player)
        {
            TryAddCutscene(new CS21_RestorationAndFarewell(player));
        }
    }
    [CustomEntity("DZ/NPCEventInteract")]
    [Tracked(true)]
    public class NPCEventInteract : NPC
    {
        public NPCEventInteract(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}

