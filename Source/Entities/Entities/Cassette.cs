using DZ;
using FMOD.Studio;

namespace Celeste.Entities {

	[CustomEntity(ids: "DZ/Cassette")]
	[HotReloadable]
	public class Cassette : Entity {
		public static ParticleType P_Shine;

		// Note: Removed [CustomEntity] - this class is created programmatically, not placed in maps
		private class Unlocked1Cutscene : Entity {
			private float alpha, textAlpha;
			public string[] text;
			private bool waitForKeyPress;
			private float timer;
			public int textIndex = 0;
			string menuSprite;

			public Unlocked1Cutscene(string[] unlockText, string menuSprite) {
				text = unlockText;
				this.menuSprite = menuSprite;
			}

			public override void Added(Scene scene) {
				base.Added(scene);
				Tag = Tags.HUD | Tags.PauseUpdate;
				for(int i = 0; i < text.Length; i++)
					text[i] = ActiveFont.FontSize.AutoNewline(Dialog.Clean(text[i]), 900);
				Depth = -10000;
			}

			public IEnumerator EaseIn() {
				_ = Scene;
				while((textAlpha = (alpha += Engine.DeltaTime / 0.5f)) < 1f) {
					yield return null;
				}

				alpha = 1f;
				yield return 1.5f;
				waitForKeyPress = true;
			}

			public IEnumerator EaseOut() {
				waitForKeyPress = false;
				while((textAlpha = (alpha -= Engine.DeltaTime / 0.5f)) > 0f) {
					yield return null;
				}

				alpha = 0f;
				RemoveSelf();
			}

			public IEnumerator NextText() {
				while((textAlpha -= Engine.DeltaTime / 0.5f) > 0f) {
					yield return null;
				}
				textIndex++;
				while((textAlpha += Engine.DeltaTime / 0.5f) < 1f) {
					yield return null;
				}
			}

			public override void Update() {
				timer += Engine.DeltaTime;
				base.Update();
			}

			public override void Render() {
				float num = Ease.CubeOut(alpha);
				float textCol = Ease.CubeOut(textAlpha);
				Vector2 value = global::Celeste.Celeste.TargetCenter + new Vector2(0f, 64f);
				Vector2 value2 = Vector2.UnitY * 64f * (1f - num);
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
				GFX.Gui[menuSprite].DrawJustified(value - value2 + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * num);
				ActiveFont.Draw(text[Math.Min(textIndex, text.Length - 1)], value + value2, new Vector2(0.5f, 0f), Vector2.One, Color.White * textCol);
				if(waitForKeyPress) {
					GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((timer % 1f < 0.25f) ? 6 : 0)));
				}
			}
		}

		// vanilla cassette attributes
		public bool IsGhost;
		private Sprite sprite;
		private SineWave hover;
		private BloomPoint bloom;
		private VertexLight light;
		private Wiggler scaleWiggler;
		private bool collected;
		private Vector2[] nodes;
		private EventInstance remixSfx;
		private bool collecting;

	// B-side attributes
	private string spritePath;
	private string menuSprite;
	private string[] unlockText;
	private string bToUnlock;
	
	// Enhanced collectible properties (matching Lua definitions)
	private string onCollect;
	private string customAudio;
	private Color particleColor;
	private float glowStrength;
	private float bloomStrength;
	private float wiggleIntensity;
	private float floatSpeed;
	private float floatRange;
	private float collectDelay;
	private bool persistent;

	public Cassette(Vector2 position, Vector2[] nodes) : base(position) {
		Collider = new Hitbox(16f, 16f, -8f, -8f);
		this.nodes = nodes;
		Add(new PlayerCollider(OnPlayer));
	}

	public Cassette(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesOffset(offset)) {
		spritePath = data.Attr("spritePath", "collectables/cassette/");
		
		// Determine unlock text based on side to unlock or use custom text
		string customUnlockText = data.Attr("unlockText", "");
		if (string.IsNullOrEmpty(customUnlockText)) {
			string sideToUnlock = data.Attr("bToUnlock", "");
			unlockText = DetermineUnlockText(sideToUnlock);
		} else {
			unlockText = customUnlockText.Split(',');
		}

		menuSprite = data.Attr("menuSprite", "collectables/cassette");
		bToUnlock = data.Attr("bToUnlock");
		
		// Load enhanced properties
		onCollect = data.Attr("onCollect", "");
		customAudio = data.Attr("customAudio", "");
		particleColor = Calc.HexToColor(data.Attr("particleColor", "9CFCFF"));
		glowStrength = data.Float("glowStrength", 1.0f);
		bloomStrength = data.Float("bloomStrength", 0.8f);
		wiggleIntensity = data.Float("wiggleIntensity", 0.35f);
		floatSpeed = data.Float("floatSpeed", 2.0f);
		floatRange = data.Float("floatRange", 2.0f);
		collectDelay = data.Float("collectDelay", 0.3f);
		persistent = data.Bool("persistent", true);
	}		public override void Added(Scene scene) {
			base.Added(scene);

			IsGhost = IngesteModule.SaveData.Unlocked1IDs.Contains(bToUnlock);
			string path = IsGhost ? "ghost" : "idle";
			sprite = new Sprite(GFX.Game, spritePath);
			sprite.Add("idle", path, 0.07f, "pulse", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			sprite.Add("spin", path, 0.07f, "spin", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
			sprite.Add("pulse", path, 0.04f, "idle", new int[] { 13, 14, 15, 16, 17, 18 });
			sprite.CenterOrigin();
			Add(sprite);

		sprite.Play("idle");
		Add(scaleWiggler = Wiggler.Create(0.25f, 4f, delegate (float f) {
			sprite.Scale = Vector2.One * (1f + f * wiggleIntensity);
		}));
		Add(bloom = new BloomPoint(bloomStrength, 16f));
		Add(light = new VertexLight(particleColor, glowStrength * 0.4f, 32, 64));
		Add(hover = new SineWave(floatSpeed, 0f));
		hover.OnUpdate = delegate (float f) {
			Sprite obj = sprite;
			VertexLight vertexLight = light;
			float num2 = bloom.Y = f * floatRange;
			float num5 = obj.Y = (vertexLight.Y = num2);
		};
			if(IsGhost) {
				sprite.Color = Color.White * 0.8f;
			}
		}

		public override void SceneEnd(Scene scene) {
			base.SceneEnd(scene);
			Audio.Stop(remixSfx);
		}

		public override void Removed(Scene scene) {
			base.Removed(scene);
			Audio.Stop(remixSfx);
		}

		public override void Update() {
			base.Update();
			if(!collecting && Scene.OnInterval(0.1f)) {
				SceneAs<Level>().Particles.Emit(P_Shine, 1, base.Center, new Vector2(12f, 10f));
			}
		}

	private void OnPlayer(global::Celeste.Player player) {
		if(!collected) {
			player?.RefillStamina();
			
			// Use custom audio if provided, otherwise use default
			string collectSfx = !string.IsNullOrEmpty(customAudio) 
				? customAudio 
				: "event:/game/general/cassette_get";
			Audio.Play(collectSfx, Position);
			
			collected = true;
			CelesteGame.Freeze(0.1f);
			Add(new Coroutine(CollectRoutine(player)));
		}
	}		private IEnumerator CollectRoutine(global::Celeste.Player player) {
			collecting = true;
			Level level = Scene as Level;
			TapeBlockManager cbm = Scene.Tracker.GetEntity<TapeBlockManager>();
			level.PauseLock = true;
			level.Frozen = true;
			Tag = Tags.FrozenUpdate;
			level.Session.Cassette = true;
			level.Session.RespawnPoint = level.GetSpawnPoint(nodes[1]);
			level.Session.UpdateLevelStartDashes();
			if(!string.IsNullOrEmpty(bToUnlock))
				IngesteModule.SaveData.Unlocked1IDs.Add(bToUnlock);
			cbm?.StopBlocks();
			Depth = -1000000;
			level.Shake();
			level.Flash(Color.White);
			level.Displacement.Clear();
			Vector2 camWas = level.Camera.Position;
			Vector2 camTo = (Position - new Vector2(160f, 90f)).Clamp(level.Bounds.Left - 64, level.Bounds.Top - 32, level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
			level.Camera.Position = camTo;
			level.ZoomSnap((Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2f);
			sprite.Play("spin", restart: true);
			sprite.Rate = 2f;
			for(float p3 = 0f; p3 < 1.5f; p3 += Engine.DeltaTime) {
				sprite.Rate += Engine.DeltaTime * 4f;
				yield return null;
			}
			
			sprite.Rate = 0f;
			sprite.SetAnimationFrame(0);
			scaleWiggler.Start();
			yield return 0.25f;
			Vector2 from = Position;
			Vector2 to = new Vector2(X, level.Camera.Top - 16f);
			float duration2 = 0.4f;
			for(float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2) {
				sprite.Scale.X = MathHelper.Lerp(1f, 0.1f, p3);
				sprite.Scale.Y = MathHelper.Lerp(1f, 3f, p3);
				Position = Vector2.Lerp(from, to, Ease.CubeIn(p3));
				yield return null;
			}

			Visible = false;
			remixSfx = Audio.Play("event:/DZ/game/general/cassette_preview", "remix", level.Session.Area.ID);
			Unlocked1Cutscene message = new Unlocked1Cutscene(unlockText, menuSprite);
			Scene.Add(message);
			yield return message.EaseIn();
			while(message.textIndex < message.text.Length) {
				while(!Input.MenuConfirm.Pressed) {
					yield return null;
				}
				if(message.textIndex != message.text.Length - 1)
					yield return message.NextText();
				else
					break;
			}

			Audio.SetParameter(remixSfx, "end", 1f);
			yield return message.EaseOut();
			duration2 = 0.25f;
			Add(new Coroutine(level.ZoomBack(duration2 - 0.05f)));
			for(float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2) {
				level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p3));
				yield return null;
			}

			if(!player.Dead && nodes != null && nodes.Length >= 2) {
				Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
				player.StartCassetteFly(nodes[1], nodes[0]);
			}

			foreach(SandwichLava item in level.Entities.FindAll<SandwichLava>()) {
				item.Leave();
			}

			level.Frozen = false;
			yield return 0.25f;
			cbm?.Finish();
			level.PauseLock = false;
			level.ResetZoom();
			
			// Execute custom onCollect action if specified
			ExecuteOnCollectAction(level, player);
			
			RemoveSelf();
		}

		public static void LoadParticles() {
			P_Shine = new ParticleType {
				Color = Calc.HexToColor("9CFCFF"),
				Color2 = Color.White,
				ColorMode = ParticleType.ColorModes.Fade,
				Size = 1f,
				LifeMin = 0.6f,
				LifeMax = 1.2f,
				SpeedMin = 15f,
				SpeedMax = 30f,
				DirectionRange = (float)Math.PI * 2f
			};
		}

		// Execute custom onCollect action
		private void ExecuteOnCollectAction(Level level, global::Celeste.Player player) {
			if (string.IsNullOrEmpty(onCollect)) return;

			switch (onCollect.ToLower()) {
				case "unlock_door":
					// Unlock doors with matching ID
					level.Session.SetFlag("cassette_unlock_door", true);
					break;
				case "trigger_cutscene":
					// Trigger a cutscene sequence
					level.Session.SetFlag("cassette_cutscene", true);
					break;
				case "activate_switch":
					// Activate switches or mechanisms
					level.Session.SetFlag("cassette_switch", true);
					break;
				case "spawn_entity":
					// Spawn entity at first node if available
					if (nodes != null && nodes.Length > 0) {
						// Entity spawning logic would go here
					}
					break;
				case "teleport_player":
					// Teleport player to first node if available
					if (nodes != null && nodes.Length > 0 && !player.Dead) {
						player.Position = nodes[0];
					}
					break;
				case "change_music":
					// Change background music (would need additional parameters)
					break;
				case "show_dialogue":
					// Show dialogue (would need dialogue ID parameter)
					break;
				case "set_flag":
					// Set a custom session flag
					level.Session.SetFlag("cassette_collected", true);
					break;
			case "complete_level":
				// Complete the current level
				level.CompleteArea(true, false, false);
				break;
				case "custom_script":
					// Custom script execution point
					break;
			}
		}

		// Determine unlock text based on side being unlocked
		private string[] DetermineUnlockText(string sideToUnlock) {
			if (string.IsNullOrEmpty(sideToUnlock)) {
				return new string[] { "DZ_1_unlocked" };
			}

			// Check if it's a specific side unlock
			if (sideToUnlock.ToUpper().Contains("1") || sideToUnlock.ToUpper().Contains("B_SIDE")) {
				return new string[] { "DZ_1_unlocked" };
			} else if (sideToUnlock.ToUpper().Contains("2") || sideToUnlock.ToUpper().Contains("C_SIDE")) {
				return new string[] { "DZ_2_unlocked" };
			} else if (sideToUnlock.ToUpper().Contains("2") || sideToUnlock.ToUpper().Contains("D_SIDE")) {
				return new string[] { "DZ_2_unlocked" };
			} else if (sideToUnlock.ToUpper().Contains("REMIX")) {
				return new string[] { "DZ_RemixExtra_unlocked" };
			}

			// Default to B-Side
			return new string[] { "DZ_1_unlocked" };
		}
	}
}




