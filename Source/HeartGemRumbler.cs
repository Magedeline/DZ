using TestBreathingGame = MaggyHelper.Entities.TestBreathingGame;
using System;
using Monocle;

namespace MaggyHelper
{
	/// <summary>
	/// Controller rumble entity that can optionally track a TestBreathingGame
	/// and adjust rumble intensity based on the game's panic level.
	/// Replaces the vanilla BreathingRumbler.
	/// </summary>
	public class HeartGemRumbler : Entity
	{
		private const float MaxRumble = 0.25f;

		public float Strength = 0.2f;
		private float currentRumble;
		private TestBreathingGame breathingGame;

		public HeartGemRumbler()
		{
			this.currentRumble = this.Strength;
		}

		/// <summary>
		/// Links this rumbler to a TestBreathingGame so rumble intensity
		/// is driven by the game's panic level.
		/// </summary>
		public void TrackBreathingGame(TestBreathingGame game)
		{
			breathingGame = game;
		}

		public override void Update()
		{
			base.Update();

			// If tracking a breathing game, drive strength from its panic level
			if (breathingGame != null)
			{
				if (breathingGame.Completed || breathingGame.Failed || breathingGame.Scene == null)
				{
					// Game finished — wind down rumble
					breathingGame = null;
					Strength = 0f;
				}
				else
				{
					Strength = breathingGame.PanicLevel * 0.8f + 0.1f;
				}
			}

			this.currentRumble = Calc.Approach(this.currentRumble, this.Strength, 2f * Engine.DeltaTime);
			if (this.currentRumble > 0f)
			{
				Input.RumbleSpecific(this.currentRumble * MaxRumble, 0.05f);
			}
		}
	}
}

