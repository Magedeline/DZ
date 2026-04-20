using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Monocle;

namespace Celeste.Cutscenes
{
	/// <summary>
	/// Cutscene that plays the Undertale Respite flashback video before transitioning to the final ending.
	/// This shows the "last reality" as a flashback before Madeline or Kirby's ending cinematics.
	/// NOTE: This is the CS19 version - CS20 version should be used instead for the updated VidPlayer implementation.
	/// </summary>
	public class CS19_WhiteCymbalFadeTeleportVideoOld : CutsceneEntity
	{
		private Player player;
		private EventInstance whiteCymbalSound;
		private EventInstance undertaleAmbienceSound;
		private float fadeAlpha = 0f;
		private Color fadeColor = Color.White;
		
		// Video player for MP4 playback
		private VideoPlayer videoPlayer;
		private Video video;
		private Texture2D videoTexture;
		
		// Frame-based fallback
		private Atlas videoAtlas;
		private List<MTexture> videoFrames;
		private int currentFrame = 0;
		private float videoTimer = 0f;
		private const float FRAME_DELAY = 1f / 30f; // 30 FPS video playback
		
		private bool videoPlaying = false;
		private bool usingMp4 = false;
		private Vector2 videoCenter = CelesteGame.TargetCenter;

		public CS19_WhiteCymbalFadeTeleportVideoOld(Player player) : base(false, true)
		{
			base.Tag = Tags.HUD;
			this.player = player;
			player.StateMachine.State = 11;
			player.DummyAutoAnimate = false;
			player.Sprite.Rate = 0f;
			this.RemoveOnSkipped = false;
			
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
			level.PauseLock = true;
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

			// White cymbal fade in effect
			this.whiteCymbalSound = Audio.Play("event:/desolozantas/final_content/music/lvl20/cinematic/end_outro");
			Audio.SetParameter(this.whiteCymbalSound, "cymbal_modulation", 1f);
			
			this.fadeColor = Color.White;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 2f)
			{
				this.fadeAlpha = Ease.CubeIn(p);
				yield return null;
			}
			this.fadeAlpha = 1f;

			yield return 1f;

			// Try to load MP4 video first, then fall back to frames
			bool videoLoaded = this.LoadMp4Video() || this.LoadVideoFrames();
			
			if (videoLoaded)
			{
				// Fade to video
				this.fadeColor = Color.Black;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1f)
				{
					this.fadeAlpha = 1f - Ease.SineOut(p);
					yield return null;
				}
				this.fadeAlpha = 0f;

				// Play Undertale ambience
				this.undertaleAmbienceSound = Audio.Play("event:/desolozantas/final_content/env/21_beaches");
				
				// Play the video
				this.videoPlaying = true;
				
				if (this.usingMp4 && this.videoPlayer != null)
				{
					// Play MP4 video
					this.videoPlayer.Play(this.video);
					this.videoPlayer.Volume = 0f; // Use FMOD for audio instead
					
					// Wait for video duration
					while (this.videoPlayer.State == MediaState.Playing)
					{
						yield return null;
					}
					
					yield return 2f;
				}
				else if (this.videoFrames != null)
				{
					// Play frame sequence
					float totalVideoTime = this.videoFrames.Count * FRAME_DELAY;
					yield return totalVideoTime + 2f;
				}
				
				this.videoPlaying = false;

				// Fade out
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1.5f)
				{
					this.fadeAlpha = Ease.CubeIn(p);
					yield return null;
				}
				this.fadeAlpha = 1f;

				yield return 0.5f;
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

			// Transition to the final ending cutscene
			level.Add(new CS20_Ending(this.player));
			this.RemoveSelf();
			
			yield break;
		}

		private bool LoadMp4Video()
		{
			try
			{
				// Look for MP4 files in the Video folder
				string modPath = Everest.Loader.PathMods;
				string videoFolderPath = Path.Combine(modPath, "MaggyHelper", "Video");
				
				if (!Directory.Exists(videoFolderPath))
				{
					Logger.Log(LogLevel.Info, "MaggyHelper", $"Video folder not found: {videoFolderPath}");
					return false;
				}

				// Look for MP4 files
				string[] mp4Files = Directory.GetFiles(videoFolderPath, "*.mp4");
				
				if (mp4Files.Length == 0)
				{
					Logger.Log(LogLevel.Info, "MaggyHelper", "No MP4 files found in Video folder");
					return false;
				}

				// Use the first MP4 file found (or look for specific name)
				string videoFile = mp4Files[0];
				
				// Check for specific file name first
				string preferredFile = Path.Combine(videoFolderPath, "undertale_respite.mp4");
				if (File.Exists(preferredFile))
				{
					videoFile = preferredFile;
				}

				Logger.Log(LogLevel.Info, "MaggyHelper", $"Loading MP4 video: {videoFile}");

				// Load the video using FNA's Video.FromUriEXT for Ogg Theora videos
				this.video = Video.FromUriEXT(new Uri(videoFile, UriKind.Absolute), CelesteGame.Instance.GraphicsDevice);

				if (this.video != null)
				{
					this.videoPlayer = new VideoPlayer();
					this.usingMp4 = true;
					Logger.Log(LogLevel.Info, "MaggyHelper", "MP4 video loaded successfully");
					return true;
				}
			}
			catch (Exception ex)
			{
				Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to load MP4 video: {ex.Message}");
			}
			
			return false;
		}

		private bool LoadVideoFrames()
		{
			try
			{
				string videoPath = Path.Combine("Graphics", "Atlases", "UndertaleRespiteVideo");
				
				// Try to load the video atlas
				if (Directory.Exists(Path.Combine(Engine.ContentDirectory, videoPath)))
				{
					this.videoAtlas = Atlas.FromAtlas(videoPath, Atlas.AtlasDataFormat.PackerNoAtlas);
					this.videoFrames = new List<MTexture>();
					
					// Load frames sequentially (frame_0000, frame_0001, etc.)
					int frameIndex = 0;
					while (true)
					{
						string frameName = $"frame_{frameIndex:D4}";
						MTexture frame = this.videoAtlas[frameName];
						
						if (frame == null || frame.Width == 0)
						{
							break;
						}
						
						this.videoFrames.Add(frame);
						frameIndex++;
					}

					return this.videoFrames.Count > 0;
				}
			}
			catch (Exception ex)
			{
				Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to load video frames: {ex.Message}");
			}
			
			return false;
		}

		private IEnumerator ShowTextBasedFlashback(Level level)
		{
			// Fallback: show text-based flashback if video isn't available
			this.fadeColor = Color.Black;
			this.fadeAlpha = 1f;
			
			this.undertaleAmbienceSound = Audio.Play("event:/env/amb/00_prologue");
			
			yield return 1f;

			// Show flashback text
			yield return Textbox.Say("CH19_UNDERTALE_RESPITE_FLASHBACK");

			yield return 2f;

			// Fade out text
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 1.5f)
			{
				Audio.SetParameter(this.undertaleAmbienceSound, "fade", 1f - p);
				yield return null;
			}

			yield break;
		}

		private void CleanupAudio()
		{
			if (this.whiteCymbalSound != null)
			{
				Audio.Stop(this.whiteCymbalSound, true);
				this.whiteCymbalSound = null;
			}
			
			if (this.undertaleAmbienceSound != null)
			{
				Audio.Stop(this.undertaleAmbienceSound, true);
				this.undertaleAmbienceSound = null;
			}
		}

		private void CleanupVideo()
		{
			// Dispose video player
			if (this.videoPlayer != null)
			{
				this.videoPlayer.Stop();
				this.videoPlayer.Dispose();
				this.videoPlayer = null;
			}
			
			if (this.video != null)
			{
				this.video = null;
			}
			
			if (this.videoTexture != null)
			{
				this.videoTexture.Dispose();
				this.videoTexture = null;
			}
			
			// Dispose video atlas
			if (this.videoAtlas != null)
			{
				this.videoAtlas.Dispose();
				this.videoAtlas = null;
			}
		}

		public override void Update()
		{
			// Update video texture from player
			if (this.usingMp4 && this.videoPlayer != null && this.videoPlayer.State == MediaState.Playing)
			{
				this.videoTexture = this.videoPlayer.GetTexture();
			}
			else if (this.videoPlaying && this.videoFrames != null)
			{
				// Update frame-based playback
				this.videoTimer += Engine.DeltaTime;
				this.currentFrame = (int)(this.videoTimer / FRAME_DELAY) % this.videoFrames.Count;
			}
			
			base.Update();
		}

		public override void Render()
		{
			// Render MP4 video if playing
			if (this.videoPlaying && this.usingMp4 && this.videoTexture != null)
			{
				Draw.Rect(-100f, -100f, 2120f, 1280f, Color.Black);
				
				// Scale and center the video
				float scaleX = 1920f / this.videoTexture.Width;
				float scaleY = 1080f / this.videoTexture.Height;
				float scale = Math.Min(scaleX, scaleY);
				
				Vector2 position = this.videoCenter;
				Vector2 origin = new Vector2(this.videoTexture.Width / 2f, this.videoTexture.Height / 2f);
				
				Draw.SpriteBatch.Draw(
					this.videoTexture,
					position,
					null,
					Color.White,
					0f,
					origin,
					scale,
					SpriteEffects.None,
					0f
				);
			}
			// Render frame-based video if playing
			else if (this.videoPlaying && this.videoFrames != null && this.currentFrame < this.videoFrames.Count)
			{
				Draw.Rect(-100f, -100f, 2120f, 1280f, Color.Black);
				
				MTexture frame = this.videoFrames[this.currentFrame];
				if (frame != null)
				{
					// Scale and center the video frame
					float scale = Math.Min(1920f / frame.Width, 1080f / frame.Height);
					Vector2 position = this.videoCenter;
					
					frame.DrawJustified(
						position,
						new Vector2(0.5f, 0.5f),
						Color.White,
						scale
					);
				}
			}

			// Render fade overlay
			if (this.fadeAlpha > 0f)
			{
				Draw.Rect(0f, 0f, 1920f, 1080f, this.fadeColor * this.fadeAlpha);
			}

			// Render pause overlay
			if ((base.Scene as Level)?.Paused == true)
			{
				Draw.Rect(0f, 0f, 1920f, 1080f, Color.Black * 0.5f);
			}

			base.Render();
		}

		public override void OnEnd(Level level)
		{
			this.CleanupAudio();
			this.CleanupVideo();
			
			// Transition to ending
			level.Add(new CS20_Ending(this.player));
		}
	}
}
