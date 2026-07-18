using DZ;
using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// DZ MiniHeart collectible adapted from CollabUtils2.
    /// Break it by touch (default) or by hitting it with a thrown holdable.
    /// Records a mini_heart achievement, optionally registers a vanilla heart gem,
    /// and optionally ends the level with RegisterAreaComplete.
    /// </summary>
    [CustomEntity("DZ/MiniHeart")]
    [Tracked(false)]
    public class MiniHeart : Entity
    {
        private const string CollectedFlagPrefix = "miniheart_collected_";

        private Sprite sprite;
        private Sprite white;
        private VertexLight light;
        private BloomPoint bloom;
        private SineWave sine;
        private HoldableCollider holdableCollider;

        private readonly string heartId;
        private readonly string variant;
        private readonly int chapter;
        private readonly bool flash;
        private readonly bool endLevel;
        private readonly bool recordMiniHeart;
        private readonly bool registerHeartGem;
        private readonly bool breakOnTouch;

        private bool hasBeenBroken;
        private bool collected;
        private Coroutine smashRoutine;
        private EventInstance pauseMusicSnapshot;
        private SoundEmitter collectSound;

        public MiniHeart(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            heartId = id.ID.ToString();
            variant = data.Attr("variant", "beginner");
            flash = data.Bool("flash", true);
            endLevel = data.Bool("endLevel", true);
            recordMiniHeart = data.Bool("recordMiniHeart", true);
            registerHeartGem = data.Bool("registerHeartGem", false);
            breakOnTouch = data.Bool("breakOnTouch", true);
            chapter = data.Int("chapter", 0);

            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -10f);

            if (breakOnTouch)
                Add(new PlayerCollider(OnPlayer));
            Add(holdableCollider = new HoldableCollider(OnHoldable, null));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Level level = SceneAs<Level>();
            if (level != null)
            {
                string flag = CollectedFlagPrefix + GetMiniHeartKey(level);
                if (level.Session.GetFlag(flag) || DZModule.SaveData?.HasAchievement(GetMiniHeartKey(level)) == true)
                {
                    RemoveSelf();
                    return;
                }
            }

            createVisuals();
        }

        private string GetMiniHeartKey(Level level)
        {
            string sid = (level != null && level.Session != null) ? level.Session.Area.SID : "";
            return $"mini_heart:{sid}:{heartId}";
        }

        private void createVisuals()
        {
            string basePath;
            if (variant == "ghost" || variant == "white")
                basePath = $"DZ/miniheart/{variant}/{variant}";
            else
                basePath = $"DZ/miniheart/{variant}/";

            // Fallback to the white sprite if the requested variant is missing.
            if (!GFX.Game.Has(basePath + "00"))
                basePath = "DZ/miniheart/white/white";

            Add(sprite = new Sprite(GFX.Game, basePath));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();

            Add(white = new Sprite(GFX.Game, "DZ/miniheart/white/white"));
            white.AddLoop("idle", "", 0.1f);
            white.Play("idle");
            white.CenterOrigin();
            white.Visible = false;

            Add(light = new VertexLight(Color.White, 1f, 32, 64));
            Add(bloom = new BloomPoint(0.75f, 16f));

            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
        }

        public override void Update()
        {
            base.Update();

            if (sprite != null)
            {
                sprite.Y = sine.Value * 2f;
                light.Y = sprite.Y;
                bloom.Y = sprite.Y;
            }

            if (white != null && sprite != null)
            {
                white.Position = sprite.Position;
                white.Scale = sprite.Scale;
                white.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }

            if (hasBeenBroken)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player == null || player.Dead)
                    interruptCollection();
            }
        }

        private void OnPlayer(Player player)
        {
            if (!hasBeenBroken && breakOnTouch)
                heartBroken(player, null, SceneAs<Level>());
        }

        private void OnHoldable(Holdable h)
        {
            if (hasBeenBroken || !h.Dangerous(holdableCollider))
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
                heartBroken(player, h, SceneAs<Level>());
        }

        private void heartBroken(Player player, Holdable holdable, Level level)
        {
            if (hasBeenBroken || player == null || level == null)
                return;
            hasBeenBroken = true;
            Add(smashRoutine = new Coroutine(SmashRoutine(player, level)));
        }

        private IEnumerator SmashRoutine(Player player, Level level)
        {
            level.CanRetry = false;
            Collidable = false;
            stopMusic();

            // Collect any berries the player is carrying.
            List<IStrawberry> berries = new List<IStrawberry>();
            ReadOnlyCollection<Type> berryTypes = null;
            try
            {
                berryTypes = StrawberryRegistry.GetBerryTypes();
            }
            catch { }

            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is IStrawberry berry)
                {
                    if (berryTypes == null || berryTypes.Contains(follower.Entity.GetType()))
                        berries.Add(berry);
                }
            }
            foreach (IStrawberry berry in berries)
            {
                berry.OnCollect();
            }

            collectSound = SoundEmitter.Play("event:/SC2020_heartShard_get", this);

            if (white != null)
                white.Visible = true;
            if (sprite != null)
                sprite.Visible = false;
            Depth = -2000000;

            yield return null;
            Celeste.Freeze(0.2f);
            yield return null;

            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            for (int i = 0; i < 10; i++)
                Scene.Add(new AbsorbOrb(Position));

            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (flash)
                level.Flash(Color.White);

            if (light != null)
                light.Alpha = 0f;
            if (bloom != null)
                bloom.Alpha = 0f;

            if (level.FormationBackdrop != null)
            {
                level.FormationBackdrop.Display = true;
                level.FormationBackdrop.Alpha = 1f;
            }

            for (float time = 0f; time < 2f; time += Engine.RawDeltaTime)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }

            if (player.Dead)
                yield return 100f;

            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            level.PauseLock = true;
            level.TimerStopped = true;

            string miniHeartKey = GetMiniHeartKey(level);
            if (recordMiniHeart)
            {
                global::DZ.DZProgressionManager.RecordMiniHeart(level, heartId);
                level.Session.SetFlag(CollectedFlagPrefix + miniHeartKey, true);
                if (chapter >= 10 && chapter <= 15)
                    SmallHeartDoor.CollectMiniHeart(level.Session, chapter);
            }

            if (registerHeartGem)
                SaveData.Instance.RegisterHeartGem(level.Session.Area);

            if (endLevel)
                level.RegisterAreaComplete();

            Audio.SetMusic(null);
            Audio.SetAmbience(null);

            yield break;
        }

        private void interruptCollection()
        {
            Level level = Scene as Level;
            if (level != null)
            {
                level.Frozen = false;
                level.CanRetry = true;
                if (level.FormationBackdrop != null)
                    level.FormationBackdrop.Display = false;
            }

            Engine.TimeRate = 1f;

            if (collectSound != null)
            {
                collectSound.RemoveSelf();
                collectSound = null;
            }
            if (smashRoutine != null)
            {
                smashRoutine.RemoveSelf();
                smashRoutine = null;
            }

            hasBeenBroken = false;
            if (sprite != null)
                sprite.Visible = true;
            if (white != null)
                white.Visible = false;
            Collidable = true;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            resumeMusic();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            resumeMusic();
        }

        private void stopMusic()
        {
            if (pauseMusicSnapshot == null)
                pauseMusicSnapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
            Audio.BusStopAll("bus:/gameplay_sfx", true);
        }

        private void resumeMusic()
        {
            if (pauseMusicSnapshot != null)
            {
                Audio.ReleaseSnapshot(pauseMusicSnapshot);
                pauseMusicSnapshot = null;
            }
        }
    }
}
