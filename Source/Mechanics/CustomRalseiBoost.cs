// Celeste.CS10_FinalLaunch

namespace Celeste.Entities;
[CustomEntity(ids: "MaggyHelper/RalseiBoost")]
[Tracked]
[HotReloadable]
public class CustomRalseiBoost : Entity
{
	public static ParticleType AmbienceParticle;
	public static ParticleType MoveParticle;

    private const float move_speed = 320f;

    private Sprite sprite;
	private Image stretch;
	private Wiggler wiggler;
	private VertexLight light;
	private BloomPoint bloom;
	private bool canSkip;
	private Vector2[] nodes;
	private int nodeIndex;
	private bool travelling;
	private global::Celeste.Player holding;
	private SoundSource relocateSfx;
	public SoundSource ch9FinalBoostSfx;
	private string preLaunchDialog;
	private string cutsceneTeleport;
	private string goldenTeleport;
	private bool cutsceneBird;
	private Color transitionColor;
	private readonly TimeRateModifier timeRateModifier;

	// Add chapter completion tracking
	private bool chapterCompleted = false;
	public bool AreaComplete => chapterCompleted;

	public CustomRalseiBoost(
		Vector2[] nodes,
		bool lockCamera,
		bool canSkip = false,
		string preLaunchDialog = "",
		string cutsceneTeleport = "",
		string goldenTeleport = "",
		bool cutsceneBird = true,
		Color? ambientParticleColor1 = null,
		Color? ambientParticleColor2 = null,
		Color? transitionColor = null,
		Image transitionImage = null,
		Color? moveParticleColor = null
	) : base(nodes[0])
	{
		base.Depth = -1000000;
		this.nodes = nodes;
		this.canSkip = canSkip;
		this.preLaunchDialog = preLaunchDialog;
		this.cutsceneTeleport = cutsceneTeleport;
		this.goldenTeleport = goldenTeleport;
		this.cutsceneBird = cutsceneBird;
		this.transitionColor = transitionColor ?? Calc.HexToColor("ff6def");
		base.Collider = new Circle(16f);
		Add(new PlayerCollider(OnPlayer));
        Add(sprite = GFX.SpriteBank.Create("ralseiboost"));
        Add(stretch =
	        transitionImage is not null
            ? transitionImage
            : new Image(GFX.Game["objects/ralseiboost/stretch"])
        );
        stretch.Visible = false;
        stretch.CenterOrigin();
        Add(light = new VertexLight(Color.White, 0.7f, 12, 20));
        Add(bloom = new BloomPoint(0.5f, 12f));
        Add(wiggler = Wiggler.Create(0.4f, 3f, (_) => sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.4f)));
        MoveParticle = new ParticleType
		{
			Source = GFX.Game["particles/shard"],
			Color = Color.White,
			Color2 = moveParticleColor ?? Calc.HexToColor("e0a8d8"),
			ColorMode = ParticleType.ColorModes.Blink,
			FadeMode = ParticleType.FadeModes.Late,
			RotationMode = ParticleType.RotationModes.Random,
			Size = 0.8f,
			SizeRange = 0.4f,
			SpeedMin = 20f,
			SpeedMax = 40f,
			SpeedMultiplier = 0.2f,
			LifeMin = 0.4f,
			LifeMax = 0.6f,
			DirectionRange = (float)Math.PI * 2f
		};
		AmbienceParticle = new ParticleType
		{
			Color = ambientParticleColor1 ?? Calc.HexToColor("f78ae7"),
			Color2 = ambientParticleColor2 ?? Calc.HexToColor("ffccf7"),
			ColorMode = ParticleType.ColorModes.Blink,
			FadeMode = ParticleType.FadeModes.Late,
			Size = 1f,
			DirectionRange = (float)Math.PI * 2f,
			SpeedMin = 20f,
			SpeedMax = 40f,
			SpeedMultiplier = 0.2f,
			LifeMin = 0.6f,
			LifeMax = 1f
		};
		if (lockCamera)
		{
			Add(new CameraLocker(Level.CameraLockModes.BoostSequence, 0f, 160f));
		}
		Add(timeRateModifier = new TimeRateModifier(1f, false));
		Add(relocateSfx = new SoundSource());
	}

	private void OnPlayer(global::Celeste.Player player)
	{
		Add(new Coroutine(boostRoutine(player)));
	}

	public CustomRalseiBoost(EntityData data, Vector2 offset) : this(
		data.NodesWithPosition(offset),
		data.Bool("lockCamera", defaultValue: true),
		data.Bool(nameof(canSkip), defaultValue: false),
		data.Attr(nameof(preLaunchDialog), defaultValue: ""),
		data.Attr(nameof(cutsceneTeleport), defaultValue: ""),
		data.Attr(nameof(goldenTeleport), defaultValue: ""),
		data.Bool(nameof(cutsceneBird), defaultValue: true),
		data.HexColor("ambientParticle1", defaultValue: Calc.HexToColor("f78ae7")),
		data.HexColor("ambientParticle2", defaultValue: Calc.HexToColor("ffccf7")),
		data.HexColor("moveColor", defaultValue: Calc.HexToColor("ff6def")),
		new Image(GFX.Game[
			data.Attr("moveImage", defaultValue: "objects/ralseiboost/stretch")
		]),
		data.HexColor("moveParticleColor", defaultValue: Calc.HexToColor("e0a8d8"))
	)
	{
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		if (CollideCheck<FakeWall>())
		{
			base.Depth = -12500;
		}
	}

	private IEnumerator boostRoutine(global::Celeste.Player player)
	{
		holding = player;
		travelling = true;
		nodeIndex++;
		sprite.Visible = false;
		sprite.Position = Vector2.Zero;
		Collidable = false;
		bool finalBoost = nodeIndex >= nodes.Length;
		Level level = Scene as Level;

		if (finalBoost)
		{
			if (!string.IsNullOrWhiteSpace(preLaunchDialog) || !string.IsNullOrWhiteSpace(cutsceneTeleport))
			{
				Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", Position);
			}
			else
			{
				Audio.Play("event:/char/badeline/booster_final", Position);
			}
		}
		else
		{
			Audio.Play("event:/char/badeline/booster_begin", Position);
		}

		if (player.Holding is not null)
		{
			player.Drop();
		}

		player.StateMachine.State = 11;
		player.DummyAutoAnimate = false;
		player.DummyGravity = false;
		if (player.Inventory.Dashes > 1)
		{
			player.Dashes = 1;
		}
		else
		{
			player.RefillDash();
		}
		player.RefillStamina();
		player.Speed = Vector2.Zero;
		int num = Math.Sign(player.X - X);
		if (num == 0)
		{
			num = -1;
		}

        RalseiDummy ralsei = new RalseiDummy(Position, 0);
		Scene.Add(ralsei);
		player.Facing = (Facings)(-num);
		ralsei.Sprite.Scale.X = num;
		Vector2 playerFrom = player.Position;
		Vector2 playerTo = Position + new Vector2(num * 4, -3f);
		Vector2 ralseiFrom = ralsei.Position;
		Vector2 ralseiTo = Position + new Vector2(-num * 4, 3f);

		for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
		{
			Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
			if (player.Scene is not null)
			{
				player.MoveToX(vector.X);
			}
			if (player.Scene is not null)
			{
				player.MoveToY(vector.Y);
			}
			ralsei.Position = Vector2.Lerp(ralseiFrom, ralseiTo, p);
			yield return null;
		}

		if (finalBoost)
		{
			Vector2 screenSpaceFocusPoint = new Vector2(
				Calc.Clamp(player.X - level.Camera.X, 120f, 200f),
				Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f)
			);
			Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
			timeRateModifier.SetTimeRateMultiplier(0.5f);
		}
		else
		{
			Audio.Play("event:/char/badeline/booster_throw", Position);
		}

		ralsei.Sprite.Play("ralseiboost");
		yield return 0.1f;
		if (!player.Dead)
		{
			player.MoveV(5f);
		}
		yield return 0.1f;

		if ((!string.IsNullOrWhiteSpace(preLaunchDialog) || !string.IsNullOrWhiteSpace(cutsceneTeleport)) && finalBoost)
		{
			Scene.Add(new CustomRalseiBoostCutscene(
				player,
				this,
				preLaunchDialog,
				cutsceneTeleport,
				goldenTeleport,
				cutsceneBird
			));

			player.Active = false;
			ralsei.Active = false;
			Active = false;
			yield return null;
			player.Active = true;
			ralsei.Active = true;
		}
		Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
		{
			if (player.Dashes < player.Inventory.Dashes)
			{
				player.Dashes++;
			}
			Scene.Remove(ralsei);
			(Scene as Level).Displacement.AddBurst(ralsei.Position, 0.25f, 8f, 32f, 0.5f);
		}, 0.15f, start: true));
		(Scene as Level).Shake();
		holding = null;

		if (!finalBoost)
		{
			player.BadelineBoostLaunch(CenterX);
			Vector2 from = Position;
			Vector2 to = nodes[nodeIndex];
			float val = Vector2.Distance(from, to) / 320f;
			val = Math.Min(3f, val);
			stretch.Visible = true;
			stretch.Rotation = (to - from).Angle();
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
			tween.OnUpdate = (Tween t) =>
			{
				Position = Vector2.Lerp(from, to, t.Eased);
				stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
				stretch.Scale.Y =
					(1f - Calc.YoYo(t.Eased) * 0.75f) *
					(Math.Abs(stretch.Rotation) < (Math.PI / 2f) ? 1f : -1f);
				if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
				{
					TrailManager.Add(this, transitionColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
					level.ParticlesFG.Emit(MoveParticle, 1, Center, Vector2.One * 4f);
				}
			};
			tween.OnComplete = (Tween _) =>
			{
				if (X >= (float)level.Bounds.Right)
				{
					RemoveSelf();
				}
				else
				{
					travelling = false;
					stretch.Visible = false;
					sprite.Visible = true;
					Collidable = true;
					Audio.Play("event:/char/badeline/booster_reappear", Position);
				}
			};

			Add(tween);
			relocateSfx.Play("event:/char/badeline/booster_relocate");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.DirectionalShake(-Vector2.UnitY);
			level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
		}
		else
		{
			if (!string.IsNullOrWhiteSpace(preLaunchDialog) || !string.IsNullOrWhiteSpace(cutsceneTeleport))
			{
				ch9FinalBoostSfx = new SoundSource();
				Add(ch9FinalBoostSfx);
				ch9FinalBoostSfx.Play("event:/new_content/char/badeline/booster_finalfinal_part2");
			}
			Engine.FreezeTimer = 0.1f;
			yield return null;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			level.Flash(Color.White * 0.5f, drawPlayerOver: true);
			level.DirectionalShake(-Vector2.UnitY, 0.6f);
			level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
			level.ResetZoom();
			player.SummitLaunch(X);
			timeRateModifier.ResetTimeRateMultiplier();
			finish();
		}
		yield return null;
	}

	private void skip()
	{
		travelling = true;
		nodeIndex++;
		Collidable = false;
		Level level = (base.Scene as Level);
		Vector2 from = Position;
		Vector2 to = nodes[nodeIndex];
		float val = Vector2.Distance(from, to) / 320f;
		val = Math.Min(3f, val);
		stretch.Visible = true;
		stretch.Rotation = (to - from).Angle();
		Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
		tween.OnUpdate = (Tween t) =>
		{
			Position = Vector2.Lerp(from, to, t.Eased);
			stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
			stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
			if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
			{
				TrailManager.Add(this, transitionColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
				level.ParticlesFG.Emit(MoveParticle, 1, Center, Vector2.One * 4f);
			}
		};
		tween.OnComplete = (Tween _) =>
		{
			if (X >= (float)level.Bounds.Right)
			{
				RemoveSelf();
			}
			else
			{
				travelling = false;
				stretch.Visible = false;
				sprite.Visible = true;
				Collidable = true;
				Audio.Play("event:/char/badeline/booster_reappear", Position);
			}
		};
		Add(tween);
		relocateSfx.Play("event:/char/badeline/booster_relocate");
		level.Displacement.AddBurst(base.Center, 0.4f, 8f, 32f, 0.5f);
	}

	public void Wiggle()
	{
		wiggler.Start();
		(Scene as Level).Displacement.AddBurst(Position, 0.3f, 4f, 16f, 0.25f);
		Audio.Play("event:/game/general/crystalheart_pulse", Position);
	}

	public override void Update()
	{
		if (sprite.Visible && base.Scene.OnInterval(0.05f))
		{
			(base.Scene as Level).ParticlesBG.Emit(AmbienceParticle, 1, base.Center, Vector2.One * 3f);
		}
		if (holding is not null)
		{
			holding.Speed = Vector2.Zero;
		}
		if (!travelling)
		{
			global::Celeste.Player player = base.Scene.Tracker.GetEntity<global::Celeste.Player>();
			if (player is not null)
			{
				float num = Calc.ClampedMap((player.Center - Position).Length(), 16f, 64f, 12f, 0f);
				Vector2 vector = (player.Center - Position).SafeNormalize();
				sprite.Position = Calc.Approach(sprite.Position, vector * num, 32f * Engine.DeltaTime);
				if (canSkip && player.Position.X - base.X >= 100f && nodeIndex + 1 < nodes.Length)
				{
					skip();
				}
			}
		}
		light.Visible = (bloom.Visible = sprite.Visible || stretch.Visible);
		base.Update();
	}

	// Add CompleteArea method
	public void CompleteArea()
	{
		if (!chapterCompleted)
		{
			finish(true, false, false);
		}
	}

	private void finish()
	{
		chapterCompleted = true;
		SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
		SceneAs<Level>().Particles.Emit(AmbienceParticle, 12, base.Center, Vector2.One * 6f);
		SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
		SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
		RemoveSelf();
	}

	private void finish(bool completed, bool skipCutscene, bool teleportGolden)
	{
		if (chapterCompleted) return;

		chapterCompleted = true;

		if (completed)
		{
			// Trigger the final boost sound effect
			Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", Position);

			// Handle cutscene teleportation
			if (!string.IsNullOrEmpty(cutsceneTeleport) && !skipCutscene)
			{
				var level = SceneAs<Level>();
				level.Session.RespawnPoint = level.GetSpawnPoint(nodes[nodes.Length - 1]);
				level.OnEndOfFrame += () => level.TeleportTo(null, null, global::Celeste.Player.IntroTypes.Transition, null);
			}

			// Handle golden teleportation
			if (teleportGolden && !string.IsNullOrEmpty(goldenTeleport))
			{
				var level = SceneAs<Level>();
				level.OnEndOfFrame += () => level.TeleportTo(null, null, global::Celeste.Player.IntroTypes.Transition, null);
			}
		}

		RemoveSelf();
	}
}




