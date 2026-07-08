namespace Celeste.Entities;

[CustomEntity(ids: "DZ/DashCodeGate")]
[HotReloadable]
public class DashCodeGate : Solid
{
	public enum IconOrientation
	{
		Auto,
		Horizontal,
		Vertical
	}

	public static ParticleType PBehind = SwitchGate.P_Behind;
	public static ParticleType PDust = SwitchGate.P_Dust;
	public ParticleType PurpleFire;
	public const float ICON_DISTANCE = 2f;

	private ParticleSystem aboveParticles;
	private MTexture[,] nineSlice;
	private MTexture[] inactiveIcons;
	private MTexture[] activeIcons;
	private Vector2 iconScale;
	private Wiggler wiggler;
	private Vector2 node;
	private SoundSource openSfx;
	public string PersistenceFlag;
	private readonly string spriteName;
	private readonly string code;
	private readonly IconOrientation iconOrientation;
	private Color inactiveColor = Calc.HexToColor("5fcde4");
	private Color activeColor = Color.White;
	private Color finishColor = Calc.HexToColor("f141df");
	private DashListener dashListener;
	private List<string> currentInputs = new List<string>();

	public IconOrientation GetIconOrientation() => iconOrientation1;

	public void SetIconOrientation(IconOrientation value) => iconOrientation1 = value;

	public string[] Code;
	public bool Moved = false;
	private readonly Vector2 position;
	private readonly float width;
	private readonly float height;
	public int Columns;
	private IconOrientation iconOrientation1;

	/// <summary>
	/// Converts a dash vector into a dash code.
	/// </summary>
	/// <param name="dir">A dash vector.</param>
	/// <returns>A dash code. (i.e. UL, R, D, DL, R, U)</returns>
	private string dashVectorToCode(Vector2 dir)
	{
		string text = "";

		if (dir.Y < 0f)
			text = "U";
		else if (dir.Y > 0f)
			text = "D";
		if (dir.X < 0f)
			text += "L";
		else if (dir.X > 0f)
			text += "R";

		return text;
	}

	/// <summary>
	/// Convert a dash code to a <see cref="Vector2"/>
	/// </summary>
	/// <param name="code">A dash code. (i.e. "UL, R, D, DL, R, U")</param>
	/// <returns>A normalized vector2.</returns>
	private Vector2 codeToDashVector(string code)
	{
		Vector2 res = new Vector2(0f, 0f);
		foreach (char c in code)
		{
			switch (c)
			{
				case 'U':
					res.Y = -1f;
					break;
				case 'D':
					res.Y = 1f;
					break;
				case 'L':
					res.X = -1f;
					break;
				case 'R':
					res.X = 1f;
					break;
			}
		}
		return res.SafeNormalize();
	}
	/// <summary>
	/// 	500 IQ algorithm right here.
	/// 	Computes how much of the <see cref="Code"/> is activated by dashes.
	/// </summary>
	/// <returns>
	/// 	It outputs a number depending on how much of the <see cref="Code"/> is activated
	/// 	0 is for not activated at all, the <see cref="Code"/>'s length when fully activated.
	/// </returns>
	private int computeCodeCompletion()
	{
		int codeCompletion = 0;
		for (int i = 0; i < currentInputs.Count; i++)
		{
			if (currentInputs[i] == Code[codeCompletion])
			{
				codeCompletion += 1;
			}
			else
			{
				// If the code counter is greater than 0 and the current input does not match the corresponding code item, 
				// the loop variable i is decremented by 1. This is done to ensure that the loop re-evaluates the previous input, 
				// as it may have been the start of a correct code sequence.
				if (codeCompletion > 0)
				{
					i--;
				}
				codeCompletion = 0;
			}
		}
		return codeCompletion;
	}

	/// <summary>
	/// Gets the (centered) draw position of an arrow.
	/// </summary>
	/// <param name="arrowId">The id of the arrow</param>
	/// <returns>The draw position.</returns>
	private Vector2 getArrowDrawPos(int arrowId)
	{
		//! WARNING: Bad code ahead!
		// Viewer discretion is advised.

		IconOrientation orientation = this.iconOrientation;
		if (orientation == IconOrientation.Auto)
		{
			// In the `Auto` case, we check which orientation is better, 
			// and convert the orientation into that one.
			orientation =
				this.Width > this.Height ?
				IconOrientation.Horizontal :
				IconOrientation.Vertical;
		}

		//*I apologize in advance for anyone trying to read this code.
		// I'm sorry. I did my best to tell you what it does but...
		// I just used trial and error for this one.
		if (orientation == IconOrientation.Horizontal)
		{
			int columnSize = (int)Math.Ceiling((double)Code.Length / (double)Columns);
			int currentColumn = (arrowId) / columnSize; // Stores the column we should be in.
			return
				this.Position + // Get our positon
				new Vector2(this.Width / 2f, this.Height / 2f) + // Center it to our block
				new Vector2(
					(arrowId - (currentColumn * columnSize)) * ((activeIcons[0].Width + ICON_DISTANCE)),
					currentColumn * (activeIcons[0].Height + ICON_DISTANCE)
				) + // Go right depending on arrow_id
				new Vector2(
					((columnSize - 1) / 2f) * -(float)(activeIcons[0].Width + ICON_DISTANCE),
					(Columns - 1) * -((activeIcons[0].Height + ICON_DISTANCE) / 2f)
				); // Shift all arrows left depending on how many arrows and columns we can have.
		}
		else
		{
			float currentColumn = arrowId % Columns; // Stores the column we should be in.
			return
				this.Position + // Get our positon
				new Vector2(this.Width / 2f, this.Height / 2f) + // Center it to our block
				new Vector2(
					currentColumn * (activeIcons[0].Width + ICON_DISTANCE),
					(arrowId - currentColumn) * ((activeIcons[0].Height + ICON_DISTANCE) / Columns)
				) + // Go down depending on arrow_id
				new Vector2(
					(Columns - 1) * -((activeIcons[0].Width + ICON_DISTANCE) / 2f),
					(Code.Length / Columns - 1) * -((activeIcons[0].Height + ICON_DISTANCE) / 2f)
				); // Shift all arrows up depending on how many arrows and columns we can have.
		}

	}

	private DashCodeGate(Vector2 position,
	float width,
	float height,
	Vector2 node,
	string persistenceFlag,
	string spriteName,
	string code,
	IconOrientation iconOrientation = IconOrientation.Auto,
	int columns = 1
) : base(position, width, height, safe: false)
	{
		this.iconOrientation = iconOrientation;
		this.position = position;
		this.width = width;
		this.height = height;
		this.Columns = columns;
		PurpleFire = new ParticleType
		{
			Source = GFX.Game["particles/fire"],
			Color = Calc.HexToColor("f141df"),
			Color2 = Color.White,
			ColorMode = ParticleType.ColorModes.Fade,
			FadeMode = ParticleType.FadeModes.Late,
			Acceleration = new Vector2(0f, -10f),
			LifeMin = 0.8f,
			LifeMax = 1.0f,
			Size = 0.3f,
			SizeRange = 0.2f,
			Direction = -(float)Math.PI / 2f,
			DirectionRange = (float)Math.PI / 6f,
			SpeedMin = 4f,
			SpeedMax = 6f,
			SpeedMultiplier = 0.2f,
			ScaleOut = true
		};
		this.Code = code.ToUpper().Split(',');
		this.inactiveIcons = new MTexture[this.Code.Length];
		this.activeIcons = new MTexture[this.Code.Length];
		for (int i = 0; i < this.Code.Length; i++)
		{
			Vector2 v = codeToDashVector(this.Code[i]);
			string text = "";
			if (v.Y < 0f)
			{
				text = "up-";
			}
			else if (v.Y > 0f)
			{
				text = "down-";
			}
			if (v.X < 0f)
			{
				text += "left";
			}
			else if (v.X > 0f)
			{
				text += "right";
			}
			// TODO: Port DoonvHelper graphics to your mod
			// After porting, change path from "objects/DZ/DZ/DoonvHelper/" to "objects/DZ/DZ/"
			// Graphics to copy: dashcodegate/arrows/inactive-*.png and active-*.png
			string spritePathPrefix = "objects/DZ/DZ/DoonvHelper"; // Change to "objects" after porting
			this.inactiveIcons[i] = GFX.Game[String.Format(
				"{0}/dashcodegate/arrows/inactive-{1}",
			spritePathPrefix, text.Trim('-'))];
			this.activeIcons[i] = GFX.Game[String.Format(
				"{0}/dashcodegate/arrows/active-{1}",
			spritePathPrefix, text.Trim('-'))];
		}
		Logger.Log(LogLevel.Info, "DZ/DashCodeGate", String.Join(", ", this.Code));
		this.node = node;
		this.PersistenceFlag = persistenceFlag;
		this.spriteName = spriteName;
		this.code = code;
		this.iconOrientation = iconOrientation;
		Add(wiggler = Wiggler.Create(0.5f, 4f, (float f) =>
		{
			iconScale = Vector2.One * (1f + f);
		}));
		MTexture mTexture = GFX.Game["objects/DZ/DZ/switchgate/" + spriteName];
		nineSlice = new MTexture[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
			}
		}
		Add(openSfx = new SoundSource());
		Add(new LightOcclude(0.5f));
		Add(dashListener = new DashListener());
		dashListener.OnDash = (Vector2 dir) =>
		{
			// Logger.Log(LogLevel.Info, "DoonvHelper", dir.ToString());
			int oldCompletion = computeCodeCompletion();
			string code = dashVectorToCode(dir);
			currentInputs.Add(code);
			if (currentInputs.Count > this.Code.Length)
			{
				currentInputs.RemoveAt(0);
			}

			int completion = computeCodeCompletion();
			if (completion != 0)
			{
				SceneAs<Level>().Displacement.AddBurst(
					getArrowDrawPos(completion - 1),
					0.3f,
					8f,
					24f,
					0.4f
				);
			}
			else if (completion != oldCompletion)
			{
				for (int i = 0; i < oldCompletion; i++)
				{
					SceneAs<Level>().Displacement.AddBurst(
						getArrowDrawPos(i),
						1f,
						8f,
						24f,
						0.2f
					);
				}
			}
			if (completion == this.Code.Length)
			{
				Add(new Coroutine(sequence(node)));
			}
		};
	}

	public static DashCodeGate CreateInstance(Vector2 position, float width, float height, Vector2 node, string persistenceFlag, string spriteName, string code, IconOrientation iconOrientation, int columns = 1)
	{
		return new DashCodeGate(position, width, height, node, persistenceFlag, spriteName, code, iconOrientation, columns);
	}

	public DashCodeGate(EntityData data, Vector2 offset)
		: this(
			data.Position + offset,
			data.Width, data.Height,
			data.Nodes[0] + offset,
			data.Attr("persistenceFlag", defaultValue: ""),
			data.Attr("TheoSprite", defaultValue: "block"),
			data.Attr(nameof(code), defaultValue: "U,D,L,R"),
			(IconOrientation)Enum.Parse(
				typeof(IconOrientation),
				data.Attr(nameof(iconOrientation), defaultValue: "Auto"),
				true
			),
			data.Int("columns", defaultValue: 1)
		)
	{
	}

	// We have to put this method into the `Added()` method
	// because `Entity.Scene` is only set in `Added()`
	// For more info this celestecord thread is pretty useful:
	// https://discord.com/channels/403698615446536203/908809001834274887/1078730402111434764
	public override void Added(Scene scene)
	{
		base.Added(scene);
		this.aboveParticles = new ParticleSystem(this.Depth - 1, 200);
		(scene as Level).Add(this.aboveParticles);
	}
	// Then we remove the particle system when the gate is removed to not leave useless feces around.
	public override void Removed(Scene scene)
	{
		base.Removed(scene);
		(scene as Level).Remove(this.aboveParticles);
		this.aboveParticles = null;
	}

	public void Awake(Scene scene, object title)
	{
		base.Awake(scene);

		// for (int i = 0; i < code.Length; i++)
		// {
		// 	getArrowDrawPos(i, true);
		// }

		// If we have already activated the gate and the gate has a persistence flag
		// added to it, and the player goes back into the room with the gate.
		// We want to reactivate the gate and put it where it used to be.
		if (
			!String.IsNullOrWhiteSpace(PersistenceFlag) &&
			SceneAs<Level>().Session.GetFlag(PersistenceFlag)
		)
		{
			MoveTo(node);
			Moved = true;
		}
	}

	public override void Render()
	{
		float num = base.Collider.Width / 8f - 1f;
		float num2 = base.Collider.Height / 8f - 1f;
		for (int i = 0; (float)i <= num; i++)
		{
			for (int j = 0; (float)j <= num2; j++)
			{
				int num3 = (((float)i < num) ? Math.Min(i, 1) : 2);
				int num4 = (((float)j < num2) ? Math.Min(j, 1) : 2);
				nineSlice[num3, num4].Draw(Position + base.Shake + new Vector2(i * 8, j * 8));
			}
		}

		for (int i = 0; i < Code.Length; i++)
		{
			Vector2 drawPos = getArrowDrawPos(i);

			if (
				i < computeCodeCompletion() || (
					!String.IsNullOrWhiteSpace(PersistenceFlag) &&
					SceneAs<Level>().Session.GetFlag(PersistenceFlag)
				) || Moved
			)
			{
				if (Scene.OnInterval(0.1f))
				{
					aboveParticles.Emit(
						PurpleFire,
						drawPos + Calc.AngleToVector(Calc.Random.NextAngle(), 3f)
					);
				}
				activeIcons[i].DrawCentered(drawPos);
			}
			else
			{
				inactiveIcons[i].DrawCentered(drawPos);
			}

		}

		// icon.Position = iconOffset + base.Shake;
		// icon.DrawOutline();
		base.Render();
	}

	/// <summary>
	/// Moves and activates the dash gate.
	/// </summary>
	private IEnumerator sequence(Vector2 targetPosition)
	{
		Vector2 start = Position;
		if (Moved)
		{
			yield break;
		}
		if (!String.IsNullOrWhiteSpace(PersistenceFlag))
		{
			if (SceneAs<Level>().Session.GetFlag(PersistenceFlag))
			{
				yield break;
			}
			SceneAs<Level>().Session.SetFlag(PersistenceFlag);
		}
		Moved = true;
		dashListener.Active = false;
		openSfx.Play("event:/game/general/seed_complete_berry");
		yield return 1f;
		openSfx.Play("event:/game/general/touchswitch_gate_open");
		StartShaking(0.5f);
		yield return 0.1f;
		int particleAt = 0;
		Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, start: true);
		tween.OnUpdate = (Tween t) =>
		{
			MoveTo(Vector2.Lerp(start, targetPosition, t.Eased));
			if (Scene.OnInterval(0.1f))
			{
				particleAt++;
				particleAt %= 2;
				for (int n = 0; (float)n < Width / 8f; n++)
				{
					for (int num2 = 0; (float)num2 < Height / 8f; num2++)
					{
						if ((n + num2) % 2 == particleAt)
						{
							SceneAs<Level>().ParticlesBG.Emit(PBehind, Position + new Vector2(n * 8, num2 * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
						}
					}
				}
			}
		};
		Add(tween);
		yield return 1.8f;
		bool collidable = Collidable;
		Collidable = false;
		if (targetPosition.X <= start.X)
		{
			Vector2 vector = new Vector2(0f, 2f);
			for (int i = 0; (float)i < Height / 8f; i++)
			{
				Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(i * 8));
				Vector2 point = vector2 + Vector2.UnitX;
				if (Scene.CollideCheck<Solid>(vector2) && !Scene.CollideCheck<Solid>(point))
				{
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector2 + vector, (float)Math.PI);
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector2 - vector, (float)Math.PI);
				}
			}
		}
		if (targetPosition.X >= start.X)
		{
			Vector2 vector3 = new Vector2(0f, 2f);
			for (int j = 0; (float)j < Height / 8f; j++)
			{
				Vector2 vector4 = new Vector2(Right + 1f, Top + 4f + (float)(j * 8));
				Vector2 point2 = vector4 - Vector2.UnitX * 2f;
				if (Scene.CollideCheck<Solid>(vector4) && !Scene.CollideCheck<Solid>(point2))
				{
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector4 + vector3, 0f);
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector4 - vector3, 0f);
				}
			}
		}
		if (targetPosition.Y <= start.Y)
		{
			Vector2 vector5 = new Vector2(2f, 0f);
			for (int k = 0; (float)k < Width / 8f; k++)
			{
				Vector2 vector6 = new Vector2(Left + 4f + (float)(k * 8), Top - 1f);
				Vector2 point3 = vector6 + Vector2.UnitY;
				if (Scene.CollideCheck<Solid>(vector6) && !Scene.CollideCheck<Solid>(point3))
				{
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector6 + vector5, -(float)Math.PI / 2f);
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector6 - vector5, -(float)Math.PI / 2f);
				}
			}
		}
		if (targetPosition.Y >= start.Y)
		{
			Vector2 vector7 = new Vector2(2f, 0f);
			for (int l = 0; (float)l < Width / 8f; l++)
			{
				Vector2 vector8 = new Vector2(Left + 4f + (float)(l * 8), Bottom + 1f);
				Vector2 point4 = vector8 - Vector2.UnitY * 2f;
				if (Scene.CollideCheck<Solid>(vector8) && !Scene.CollideCheck<Solid>(point4))
				{
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector8 + vector7, (float)Math.PI / 2f);
					SceneAs<Level>().ParticlesFG.Emit(PDust, vector8 - vector7, (float)Math.PI / 2f);
				}
			}
		}
		Collidable = collidable;
		Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
		StartShaking(0.2f);
		wiggler.Start();
		bool collidable2 = Collidable;
		Collidable = false;
		Collidable = collidable2;
	}
}




