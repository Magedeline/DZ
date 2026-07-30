using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
	[CustomEntity("DZ/TesseractGateway")]
	[Tracked]
	public class TesseractGateway : Solid
	{
		private const int OpenHeight = 0;
		private const float HoldingWaitTime = 0.2f;
		private const float HoldingOpenDistSq = 4096f;
		private const float HoldingCloseDistSq = 6400f;
		private const int MinDrawHeight = 4;
		public string LevelID;
		public Types Type;
		public bool ClaimedByASwitch;
		private bool maddyGate;
		private int closedHeight;
		private Sprite sprite;
		private Shaker shaker;
		private float drawHeight;
		private float drawHeightMoveSpeed;
		private bool open;
		private float holdingWaitTimer = 0.2f;
		private Vector2 holdingCheckFrom;
		private bool lockState;

		public TesseractGateway(
			Vector2 position,
			int height,
			Types type,
			string spriteName,
			string levelID)
			: base(position, 8f, height, true)
		{
			Type = type;
			closedHeight = height;
			LevelID = levelID;
			Add(sprite = GFX.SpriteBank.Create("tesseractgate_" + spriteName));
			sprite.X = Collider.Width / 2f;
			sprite.Play("idle");
			Add(shaker = new Shaker(false));
			Depth = -9000;
			maddyGate = spriteName.Equals("maddy", StringComparison.InvariantCultureIgnoreCase);
			holdingCheckFrom = Position + new Vector2(Width / 2f, height / 2);
		}

		public TesseractGateway(EntityData data, Vector2 offset, string levelID)
			: this(data.Position + offset, data.Height, data.Enum<Types>("type"), data.Attr(nameof(sprite), "default"), levelID)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (Type == Types.CloseBehindPlayer)
			{
				Player entity = Scene.Tracker.GetEntity<Player>();
				if (entity != null && entity.Left < (double)Right && entity.Bottom >= (double)Top && entity.Top <= (double)Bottom)
				{
					StartOpen();
					Add(new Coroutine(CloseBehindPlayer()));
				}
			}
			else if (Type == Types.CloseBehindPlayerAlways)
			{
				StartOpen();
				Add(new Coroutine(CloseBehindPlayer()));
			}
			else if (Type == Types.CloseBehindPlayerAndMaddy)
			{
				StartOpen();
				Add(new Coroutine(CloseBehindPlayerAndMaddy()));
			}
			else if (Type == Types.HoldingMaddy)
			{
				if (MaddyIsNearby())
					StartOpen();
				Hitbox.Width = 16f;
			}
			else if (Type == Types.TouchSwitches)
				Add(new Coroutine(CheckTouchSwitches()));
			drawHeight = Math.Max(4f, Height);
		}

		public bool CloseBehindPlayerCheck()
		{
			Player entity = Scene.Tracker.GetEntity<Player>();
			return entity != null && entity.X < (double)X;
		}

		public void SwitchOpen()
		{
			sprite.Play("open");
			Alarm.Set(this, 0.2f, () =>
			{
				shaker.ShakeFor(0.2f, false);
				Alarm.Set(this, 0.2f, Open);
			});
		}

		public void Open()
		{
			Audio.Play(maddyGate ? "event:/game/05_mirror_temple/gate_maddy_open" : "event:/game/05_mirror_temple/gate_main_open", Position);
			holdingWaitTimer = 0.2f;
			drawHeightMoveSpeed = 200f;
			drawHeight = Height;
			shaker.ShakeFor(0.2f, false);
			SetHeight(0);
			sprite.Play("open");
			open = true;
		}

		public void StartOpen()
		{
			SetHeight(0);
			drawHeight = 4f;
			open = true;
		}

		public void Close()
		{
			Audio.Play(maddyGate ? "event:/game/05_mirror_temple/gate_maddy_close" : "event:/game/05_mirror_temple/gate_main_close", Position);
			holdingWaitTimer = 0.2f;
			drawHeightMoveSpeed = 300f;
			drawHeight = Math.Max(4f, Height);
			shaker.ShakeFor(0.2f, false);
			SetHeight(closedHeight);
			sprite.Play("hit");
			open = false;
		}

		private IEnumerator CloseBehindPlayer()
		{
			TesseractGateway templeGate = this;
			while (true)
			{
				Player entity = templeGate.Scene.Tracker.GetEntity<Player>();
				if (templeGate.lockState || entity == null || entity.Left <= templeGate.Right + 4.0)
					yield return null;
				else
					break;
			}
			templeGate.Close();
		}

		private IEnumerator CloseBehindPlayerAndMaddy()
		{
			TesseractGateway tesseractGateway = this;
			while (true)
			{
				Player entity1 = tesseractGateway.Scene.Tracker.GetEntity<Player>();
				if (entity1 != null && entity1.Left > tesseractGateway.Right + 4.0)
				{
					MaddyCrystal entity2 = tesseractGateway.Scene.Tracker.GetEntity<MaddyCrystal>();
					if (!tesseractGateway.lockState && entity2 != null && entity2.Left > tesseractGateway.Right + 4.0)
						break;
				}
				yield return null;
			}
			tesseractGateway.Close();
		}

		private IEnumerator CheckTouchSwitches()
		{
			TesseractGateway tesseractGateway = this;
			while (!Switch.Check(tesseractGateway.Scene))
				yield return null;
			tesseractGateway.sprite.Play("open");
			yield return 0.5f;
			tesseractGateway.shaker.ShakeFor(0.2f, false);
			yield return 0.2f;
			while (tesseractGateway.lockState)
				yield return null;
			tesseractGateway.Open();
		}

		public bool MaddyIsNearby()
		{
			MaddyCrystal entity = Scene.Tracker.GetEntity<MaddyCrystal>();
			return entity == null || entity.X > X + 10.0 || Vector2.DistanceSquared(holdingCheckFrom, entity.Center) < (open ? 6400.0 : 4096.0);
		}

		private void SetHeight(int height)
		{
			if (height < (double)Collider.Height)
			{
				Collider.Height = height;
			}
			else
			{
				float y = Y;
				int height1 = (int)Collider.Height;
				if (Collider.Height < 64.0)
				{
					Y -= 64f - Collider.Height;
					Collider.Height = 64f;
				}
				MoveVExact(height - height1);
				Y = y;
				Collider.Height = height;
			}
		}

		public override void Update()
		{
			base.Update();
			if (Type == Types.HoldingMaddy)
			{
				if (holdingWaitTimer > 0.0)
					holdingWaitTimer -= Engine.DeltaTime;
				else if (!lockState)
				{
					if (open && !MaddyIsNearby())
					{
						Close();
						CollideFirst<Player>(Position + new Vector2(8f, 0.0f))?.Die(Vector2.Zero);
					}
					else if (!open && MaddyIsNearby())
						Open();
				}
			}
			float target = Math.Max(4f, Height);
			if (drawHeight != (double)target)
			{
				lockState = true;
				drawHeight = Calc.Approach(drawHeight, target, drawHeightMoveSpeed * Engine.DeltaTime);
			}
			else
				lockState = false;
		}

		public override void Render()
		{
			Vector2 vector2 = new Vector2(Math.Sign(shaker.Value.X), 0.0f);
			Draw.Rect(X - 2f, Y - 8f, 14f, 10f, Color.Black);
			sprite.DrawSubrect(Vector2.Zero + vector2, new Rectangle(0, (int)(sprite.Height - (double)drawHeight), (int)sprite.Width, (int)drawHeight));
		}

		public enum Types
		{
			NearestSwitch,
			CloseBehindPlayer,
			CloseBehindPlayerAlways,
			HoldingMaddy,
			TouchSwitches,
			CloseBehindPlayerAndMaddy,
		}
	}
}