using System;
using System.Collections;
using System.IO;
using System.Reflection;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Cutscenes
{
	/// <summary>
	/// Cutscene that plays the Undertale Respite flashback video before transitioning to the final ending.
	/// This shows the "last reality" as a flashback before Madeline or Kirby's ending cinematics.
	/// Uses VidPlayer mod for performant OGV video playback with audio support.
	/// </summary>
	public class CS20_WhiteCymbalFadeTeleportVideo : CutsceneEntity
	{
		private Player player;
		private EventInstance whiteCymbalSound;
		
		// Start with full black to cover gameplay immediately when transitioning from CS20_Ending
		private float fadeAlpha = 1f;
		private Color fadeColor = Color.Black;
		
		// VidPlayer entity for video playback
		private Entity vidPlayerEntity;
		private bool videoPlaying = false;
#pragma warning disable CS0414
		private bool videoLoaded = false;
#pragma warning restore CS0414
		
		// Black background to cover gameplay during video
		private bool showBlackBackground = true;

		// Video path - use the mod asset path format for VidPlayer
		// VidPlayer only supports OGV (Theora) format, NOT MP4!
		private const string VIDEO_PATH = "MaggyHelper:/Video/Respite-Cinematics_Endscenes.ogv";

		public CS20_WhiteCymbalFadeTeleportVideo(Player player) : base(true, true)
		{
			// Use Tags.HUD to render on top of gameplay
			base.Tag = Tags.HUD;
			this.player = player;
			player.StateMachine.State = 11;
			player.DummyAutoAnimate = false;
			player.Sprite.Rate = 0f;
			this.RemoveOnSkipped = true;
			
			// Set a very low depth so our fade renders on top
			this.Depth = -2000000;
			
			base.Add(new LevelEndingHook(delegate()
			{
				this.CleanupAudio();
			}));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level level = scene as Level;
			level.TimerStopped = true;
			level.TimerHidden = true;
			level.SaveQuitDisabled = true;
			level.PauseLock = false; // Allow pause menu during video cutscene
			level.AllowHudHide = false;
		}

		public override void OnBegin(Level level)
		{
			Audio.SetAmbience(null, true);
			level.AutoSave();
			base.Add(new Coroutine(this.CutsceneSequence(level), true));
		}

		private IEnumerator CutsceneSequence(Level level)
		{
			// Wait for auto-save
			while (level.IsAutoSaving())
			{
				yield return null;
			}

			yield return 0.5f;

			// White cymbal fade in effect - transition from black to white
			this.whiteCymbalSound = Audio.Play("event:/desolozantas/final_content/music/lvl20/cinematic/end_outro");
			Audio.SetParameter(this.whiteCymbalSound, "cymbal_modulation", 1f);
			
			// Fade from black to white (cymbal flash effect)
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1f)
			{
				this.fadeColor = Color.Lerp(Color.Black, Color.White, Ease.CubeIn(p));
				yield return null;
			}
			this.fadeColor = Color.White;
			this.fadeAlpha = 1f;

			yield return 1f;

			// Try to load video using VidPlayer
			bool videoLoaded = this.LoadVidPlayerVideo(level);
			
			if (videoLoaded)
			{
				// Transition from white to black before revealing video
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
				{
					this.fadeColor = Color.Lerp(Color.White, Color.Black, Ease.SineOut(p));
					yield return null;
				}
				this.fadeColor = Color.Black;
				this.fadeAlpha = 1f;
				
				// Enable black background to cover gameplay (video renders on top in 4:3)
				this.showBlackBackground = true;
				
				// Pre-buffer delay: Give VidPlayer time to load and buffer video frames
				yield return 0.5f;
				
				// Force garbage collection to free memory before video playback
				GC.Collect();
				GC.WaitForPendingFinalizers();
				
				// Additional buffer time for the video decoder to prepare frames
				yield return 0.3f;
				
				// Stop the cymbal sound before video starts
				if (this.whiteCymbalSound != null)
				{
					Audio.Stop(this.whiteCymbalSound, true);
					this.whiteCymbalSound = null;
				}
				
				// Now fade out the overlay to reveal video (video has its own audio)
				this.videoPlaying = true;
				
				// Fade out the black overlay to reveal video in 4:3 with black letterbox bars
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1f)
				{
					this.fadeAlpha = 1f - Ease.SineOut(p);
					yield return null;
				}
				this.fadeAlpha = 0f;

				// Wait for video to complete
				while (!IsVideoDone())
				{
					yield return null;
				}
				
				yield return 0.5f;

				// Fade to black at end of video
				this.fadeColor = Color.Black;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1.5f)
				{
					this.fadeAlpha = Ease.CubeIn(p);
					yield return null;
				}
				this.fadeAlpha = 1f;
				
				this.videoPlaying = false;

				yield return 1f;
			}
			else
			{
				// If video not found, show text-based flashback
				yield return this.ShowTextBasedFlashback(level);
			}

			// Clean up and transition to ending
			this.CleanupAudio();
			this.CleanupVideo();

			yield return 0.5f;

			// Complete the area - show the area complete screen
			level.CompleteArea(false, false, false);
			this.RemoveSelf();
			
			yield break;
		}

		private bool LoadVidPlayerVideo(Level level)
		{
			try
			{
				// Use reflection to access VidPlayer's API since we don't have a direct reference
				Type vidPlayerEntityType = null;
				Assembly vidPlayerAssembly = null;
				
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetName().Name == "VidPlayer")
					{
						vidPlayerAssembly = assembly;
						vidPlayerEntityType = assembly.GetType("global::Celeste.Mod.VidPlayer.VidPlayerEntity");
						break;
					}
				}

				if (vidPlayerEntityType == null)
				{
					Logger.Log(LogLevel.Warn, "MaggyHelper", "VidPlayer mod not loaded - cannot play video");
					return false;
				}

				// Find the private constructor with all parameters including depth and unpausable
				var constructors = vidPlayerEntityType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				ConstructorInfo targetCtor = null;
				
				// Look for the constructor with depth and unpausable (17 parameters)
				foreach (var ctor in constructors)
				{
					var parameters = ctor.GetParameters();
					Logger.Log(LogLevel.Info, "MaggyHelper", $"Found VidPlayerEntity constructor with {parameters.Length} parameters");
					if (parameters.Length == 17)
					{
						targetCtor = ctor;
						break;
					}
				}
				
				// Fallback to 15 parameter constructor
				if (targetCtor == null)
				{
					foreach (var ctor in constructors)
					{
						var parameters = ctor.GetParameters();
						if (parameters.Length == 15)
						{
							targetCtor = ctor;
							break;
						}
					}
				}
				
				// Fallback to any constructor with >= 14 params
				if (targetCtor == null)
				{
					foreach (var ctor in constructors)
					{
						var parameters = ctor.GetParameters();
						if (parameters.Length >= 14)
						{
							targetCtor = ctor;
							break;
						}
					}
				}

				if (targetCtor == null)
				{
					Logger.Log(LogLevel.Warn, "MaggyHelper", "Could not find suitable VidPlayerEntity constructor");
					return false;
				}

				// Position at level offset for proper room-relative positioning
				// For hires fullscreen video, we want it centered on screen
				Vector2 position = level.LevelOffset;
				// Use 4:3 aspect ratio size (240x180 fits in 320x180 with letterbox bars on sides)
				Vector2 size = new Vector2(240f, 180f);
				
				var ctorParams = targetCtor.GetParameters();
				object[] args;
				
				if (ctorParams.Length == 17)
				{
					// Full constructor with depth and unpausable:
					// position, size, videoTarget, muted, keepAspectRatio, looping, hires, volumeMult, 
					// globalAlpha, centered, chromaKey, chromaKeyBaseThr, chromaKeyAlphaCorr, chromaKeySpill, 
					// depth, unpausable, offset
					args = new object[] 
					{
						position,           // position
						size,               // entitySize (4:3 aspect ratio)
						VIDEO_PATH,         // videoTarget
						false,              // muted - VIDEO AUDIO ENABLED
						true,               // keepAspectRatio
						false,              // looping - one-shot playback
						true,               // hires - renders at full resolution
						1.0f,               // volumeMult - full volume for video audio
						1.0f,               // globalAlpha
						true,               // centered
						null,               // chromaKey
						0f,                 // chromaKeyBaseThr
						0f,                 // chromaKeyAlphaCorr
						0f,                 // chromaKeySpill
						-1500000,           // depth - render on top of most things
						false,              // unpausable - pause video when game is paused
						Vector2.Zero        // offset
					};
				}
				else if (ctorParams.Length == 15)
				{
					// Constructor without depth/unpausable - video audio enabled
					args = new object[] 
					{
						position, size, VIDEO_PATH, false, true, false, true, 1.0f, 1.0f, true, null, 0f, 0f, 0f, Vector2.Zero
					};
				}
				else
				{
					// Generic fallback - video audio enabled
					args = new object[] 
					{
						position, size, VIDEO_PATH, false, true, false, true, 1.0f, 1.0f, true, null, 0f, 0f, 0f
					};
				}

				this.vidPlayerEntity = (Entity)targetCtor.Invoke(args);
				
				if (this.vidPlayerEntity != null)
				{
					// Ensure depth is set for rendering order
					this.vidPlayerEntity.Depth = -1500000;
					
					// Add Tags.HUD so it renders in the HUD phase (on top of everything)
					this.vidPlayerEntity.AddTag(Tags.HUD);
					
					level.Add(this.vidPlayerEntity);
					this.videoLoaded = true;
					Logger.Log(LogLevel.Info, "MaggyHelper", $"VidPlayer video entity created successfully with {ctorParams.Length} params");
					return true;
				}
			}
			catch (Exception ex)
			{
				Logger.Log(LogLevel.Error, "MaggyHelper", $"Failed to load VidPlayer video: {ex.Message}\n{ex.StackTrace}");
			}
			
			return false;
		}

		private bool IsVideoDone()
		{
			if (this.vidPlayerEntity == null)
				return true;

			try
			{
				// Try to get the Done property via reflection
				var doneProperty = this.vidPlayerEntity.GetType().GetProperty("Done");
				if (doneProperty != null)
				{
					return (bool)doneProperty.GetValue(this.vidPlayerEntity);
				}
				
				// Alternative: check if the Core's Done property
				var coreProperty = this.vidPlayerEntity.GetType().GetProperty("Core");
				if (coreProperty != null)
				{
					var core = coreProperty.GetValue(this.vidPlayerEntity);
					if (core != null)
					{
						var coreDoneProperty = core.GetType().GetProperty("Done");
						if (coreDoneProperty != null)
						{
							return (bool)coreDoneProperty.GetValue(core);
						}
					}
				}

				// If we can't check, assume not done until entity is removed
				return this.vidPlayerEntity.Scene == null;
			}
			catch
			{
				return this.vidPlayerEntity.Scene == null;
			}
		}

		private IEnumerator ShowTextBasedFlashback(Level level)
		{
			// Fallback: show text-based flashback if video isn't available
			this.fadeColor = Color.Black;
			this.fadeAlpha = 1f;
			
			yield return 1f;

			// Show flashback text
			yield return Textbox.Say("CH20_UNDERTALE_RESPITE_FLASHBACK");

			yield return 2f;

			yield break;
		}

		private void CleanupAudio()
		{
			if (this.whiteCymbalSound != null)
			{
				Audio.Stop(this.whiteCymbalSound, true);
				this.whiteCymbalSound = null;
			}
		}

		private void CleanupVideo()
		{
			// Remove the VidPlayer entity if it exists
			if (this.vidPlayerEntity != null && this.vidPlayerEntity.Scene != null)
			{
				this.vidPlayerEntity.RemoveSelf();
			}
			this.vidPlayerEntity = null;
			this.videoLoaded = false;
		}

		public override void Update()
		{
			base.Update();
		}

		public override void Render()
		{
			// Draw solid black background to cover gameplay when video is playing
			// The video (VidPlayer) renders separately on top of this black background
			if (this.showBlackBackground)
			{
				// Cover entire screen area with black (gameplay is hidden, video is visible)
				Draw.Rect(-100f, -100f, 520f, 380f, Color.Black);
			}
			
			// Render fade overlay (white intro fade or black fade transitions)
			if (this.fadeAlpha > 0f)
			{
				Draw.Rect(-100f, -100f, 520f, 380f, this.fadeColor * this.fadeAlpha);
			}

			base.Render();
		}

		public override void OnEnd(Level level)
		{
			this.CleanupAudio();
			this.CleanupVideo();
			
			// Complete the area - show the area complete screen
			level.CompleteArea(false, false, false);
		}
	}
}
