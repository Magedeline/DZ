// Celeste.CS10_FinalLaunch

namespace Celeste.Entities;

[HotReloadable]
internal class CustomRalseiBoostCutscene : CutsceneEntity
{
	private global::Celeste.Player player;
	private CustomRalseiBoost boost;
	private BirdNPC bird;
	private float fadeToWhite;
	private Vector2 birdScreenPosition;
	private AscendManager.Streaks streaks;
	private Vector2 cameraWaveOffset;
	private Vector2 cameraOffset;
	private float timer;
	private Coroutine wave;
	private bool hasGolden;
	private readonly TimeRateModifier timeRateModifier;
	private string sayDialog;
	private bool haveBird;
	private string teleportTo;
	private string goldenTeleportTo;

	// Seven Human Goner Heart Souls
	private List<Sprite> humanGonerSouls = new List<Sprite>();
	private List<Vector2> soulPositions = new List<Vector2>();
	private List<float> soulTimers = new List<float>();
	private readonly string[] humanGonerNames = { "cody", "emily", "odin", "robin", "sabel", "clover" };

	public CustomRalseiBoostCutscene(
		global::Celeste.Player player,
        CustomRalseiBoost boost,
		string sayDialog = "DZ_CH9_LAST_BOOST",
		string teleportTo = "",
		string goldenTeleportTo = "",
		bool haveBird = true
	)
	{
		this.player = player;
		this.boost = boost;
		this.sayDialog = sayDialog;
		this.teleportTo = teleportTo;
		this.goldenTeleportTo = goldenTeleportTo;
		this.haveBird = haveBird;
		Add(timeRateModifier = new TimeRateModifier(1f, false));
		base.Depth = 10010;
	}

    public override void OnBegin(Level level)
    {
        if (string.IsNullOrWhiteSpace(teleportTo))
        {
            Add(new Coroutine(CutsceneDialogOnly()));
            return;
        }
        Audio.SetMusic(null);
        ScreenWipe.WipeColor = Color.White;
        hasGolden = false;
        foreach (Follower follower in player.Leader.Followers)
        {
            if (follower.Entity is CelesteStrawberry strawberry && strawberry.Golden)
            {
                hasGolden = true;
                break;
            }
        }
        Add(new Coroutine(cutscene()));
    }

    private IEnumerator CutsceneDialogOnly()
    {
        if (!String.IsNullOrWhiteSpace(sayDialog))
        {
            yield return Textbox.Say(sayDialog);
        }
        EndCutscene(Level);
    }
    private IEnumerator cutscene()
    {
		timeRateModifier.ResetTimeRateMultiplier();
        boost.Active = false;
        yield return null;
        if (!String.IsNullOrWhiteSpace(sayDialog))
        {
            yield return Textbox.Say(sayDialog);
        }
        else
        {
            yield return 0.152f;
        }
        cameraOffset = new Vector2(0f, -20f);
        boost.Active = true;
		player.EnforceLevelBounds = false;
		yield return null;
		BlackholeBG blackholeBg = Level.Background.Get<BlackholeBG>();
		if (blackholeBg is not null)
		{
			blackholeBg.Direction = -2.5f;
			blackholeBg.SnapStrength(Level, BlackholeBG.Strengths.Wild);
			blackholeBg.CenterOffset.Y = 100f;
			blackholeBg.OffsetOffset.Y = -50f;
		}
		Add(wave = new Coroutine(waveCamera()));
		if (haveBird)
		{
			Add(new Coroutine(birdRoutine(0.8f)));
			Add(new Coroutine(humanGonerSoulsRoutine(1.2f))); // Add human goner souls
		}
		Level.Add(streaks = new AscendManager.Streaks(null));
		float p2;
		for (p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 12f)
		{
			fadeToWhite = p2;
			streaks.Alpha = p2;
			foreach (Parallax item in Level.Foreground.GetEach<Parallax>("blackhole"))
			{
				item.FadeAlphaMultiplier = 1f - p2;
			}
			yield return null;
		}
		while (bird is not null || humanGonerSouls.Count > 0)
		{
			yield return null;
		}
		ScreenWipe.WipeColor = Color.White;
		FadeWipe wipe = new FadeWipe(Level, wipeIn: false)
		{
			Duration = 4f
		};
		if (!hasGolden)
		{
			Audio.SetMusic(null); // Stop music (silence)
		}
		p2 = cameraOffset.Y;
		int to = 180;
		for (float p = 0f; p < 1f; p += Engine.DeltaTime / 2f)
		{
			cameraOffset.Y = p2 + ((float)to - p2) * Ease.BigBackIn(p);
			yield return null;
		}
		yield return wipe.Wait();
		EndCutscene(Level);
	}

    public override void OnEnd(Level level)
    {
        if (string.IsNullOrWhiteSpace(teleportTo))
        {
            boost.Active = true;
            return;
        }
        if (WasSkipped && boost?.ch9FinalBoostSfx != null)
        {
	        boost.ch9FinalBoostSfx.Stop();
        }
        string nextLevelName = teleportTo;
        global::Celeste.Player.IntroTypes nextLevelIntro = global::Celeste.Player.IntroTypes.None;
        // Logger.Log(LogLevel.Info, "DoonvHelper", String.Format("goo goo ga ga | {0} - {1} - {2}", hasGolden, goldenTeleportTo, teleportTo));
        if (hasGolden && !String.IsNullOrEmpty(goldenTeleportTo))
        {
	        nextLevelName = goldenTeleportTo;
         }
		timeRateModifier.ResetTimeRateMultiplier();
        Level.OnEndOfFrame += () =>
        {
	        Level.TeleportTo(player, nextLevelName, nextLevelIntro);

	        if (Level.Wipe is not null)
	        {
		        Level.Wipe.Cancel();
	        } 
	        if (hasGolden) {
     Level.SnapColorGrade("golden");
	        }
	        new FadeWipe(level, wipeIn: true)
	        {
		        Duration = 2f
	        };
	        ScreenWipe.WipeColor = Color.White;

	        player.Active = true;
	        player.Speed = Vector2.Zero;
	        player.EnforceLevelBounds = true;
	        player.DummyFriction = true;
	        player.DummyGravity = true;
	        player.DummyAutoAnimate = true;
	        player.ForceCameraUpdate = false;
	        player.StateMachine.State = Player.StNormal;
	        player.StateMachine.Locked = false;
        };
    }

	private IEnumerator waveCamera()
	{
		float timer = 0f;
		while (true)
		{
			cameraWaveOffset.X = (float)Math.Sin(timer) * 16f;
			cameraWaveOffset.Y = (float)Math.Sin(timer * 0.5f) * 16f + (float)Math.Sin(timer * 0.25f) * 8f;
			timer += Engine.DeltaTime * 2f;
			yield return null;
		}
	}

	private IEnumerator birdRoutine(float delay)
	{
		yield return delay;
		Level.Add(bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
		bird.Sprite.Play("flyupIdle");
		Vector2 vector = new Vector2(320f, 180f) / 2f;
		Vector2 topCenter = new Vector2(vector.X, 0f);
		Vector2 vector2 = new Vector2(vector.X, 180f);
		Vector2 from2 = vector2 + new Vector2(40f, 40f);
		Vector2 to2 = vector + new Vector2(-32f, -24f);
		for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 4f)
		{
			birdScreenPosition = from2 + (to2 - from2) * Ease.BackOut(t3);
			yield return null;
		}
		bird.Sprite.Play("flyupRoll");
		for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 2f)
		{
			birdScreenPosition = to2 + new Vector2(64f, 0f) * Ease.CubeInOut(t3);
			yield return null;
		}
		to2 = birdScreenPosition;
		from2 = topCenter + new Vector2(-40f, -100f);
		bool playedAnim = false;
		for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 4f)
		{
			if (t3 >= 0.35f && !playedAnim)
			{
				bird.Sprite.Play("flyupRoll");
				playedAnim = true;
			}
			birdScreenPosition = to2 + (from2 - to2) * Ease.BigBackIn(t3);
			birdScreenPosition.X += t3 * 32f;
			yield return null;
		}
		bird.RemoveSelf();
		bird = null;
	}

	private IEnumerator humanGonerSoulsRoutine(float delay)
	{
		yield return delay;

		// Initialize human goner souls
		for (int i = 0; i < humanGonerNames.Length; i++)
		{
			var soul = new Sprite(GFX.Game, $"characters/birdgoner/{humanGonerNames[i]}flyup");
			soul.AddLoop("flyup", "", 0.1f);
			soul.Play("flyup");
			soul.Color = Color.White * 0.8f; // Make them slightly translucent
			humanGonerSouls.Add(soul);
			
			// Arrange them in a circle around the screen
			float angle = (float)(i * 2 * Math.PI / humanGonerNames.Length);
			Vector2 circlePos = new Vector2(
				(float)(Math.Cos(angle) * 80f),
				(float)(Math.Sin(angle) * 60f)
			);
			soulPositions.Add(circlePos);
			soulTimers.Add(i * 0.2f); // Stagger their appearance
		}

		// Animate souls ascending
		float duration = 6f;
		for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
		{
			for (int i = 0; i < humanGonerSouls.Count; i++)
			{
				if (soulTimers[i] <= 0f)
				{
					Vector2 center = new Vector2(160f, 90f);
					Vector2 currentPos = soulPositions[i];
					
					// Move in a spiral upward pattern
					float spiralT = Math.Max(0f, t - (i * 0.1f));
					currentPos.Y -= spiralT * 200f; // Move up
					currentPos.X += (float)(Math.Sin(spiralT * Math.PI * 4) * 20f); // Spiral motion
					
					soulPositions[i] = center + currentPos;
				}
				else
				{
					soulTimers[i] -= Engine.DeltaTime;
				}
			}
			yield return null;
		}

		// Fade out and remove souls
		for (float fadeT = 0f; fadeT < 1f; fadeT += Engine.DeltaTime / 2f)
		{
			for (int i = 0; i < humanGonerSouls.Count; i++)
			{
				humanGonerSouls[i].Color = Color.White * (0.8f * (1f - fadeT));
			}
			yield return null;
		}

		humanGonerSouls.Clear();
		soulPositions.Clear();
		soulTimers.Clear();
	}

	public override void Update()
	{
		base.Update();
		timer += Engine.DeltaTime;
		if (bird is not null)
		{
			bird.Position = Level.Camera.Position + birdScreenPosition;
			bird.Position.X += (float)Math.Sin(timer) * 4f;
			bird.Position.Y += (float)Math.Sin(timer * 0.1f) * 4f + (float)Math.Sin(timer * 0.25f) * 4f;
		}

		// Update human goner souls
		for (int i = 0; i < humanGonerSouls.Count; i++)
		{
			humanGonerSouls[i].Update();
		}

		Level.CameraOffset = cameraOffset + cameraWaveOffset;
	}

	public override void Render()
	{
		Camera camera = Level.Camera;
		Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);

		// Render human goner souls
		for (int i = 0; i < humanGonerSouls.Count; i++)
		{
			Vector2 soulWorldPos = Level.Camera.Position + soulPositions[i];
			humanGonerSouls[i].Position = soulWorldPos;
			humanGonerSouls[i].Render();
		}
	}
}




