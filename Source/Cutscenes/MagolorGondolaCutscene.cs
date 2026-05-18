using Celeste.Cutscenes;
using Celeste.Entities;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.NPCs
{
	// Token: 0x02000274 RID: 628
	public class NPC06_Magolor : NPC
	{
		// Token: 0x06001381 RID: 4993 RVA: 0x0006A0C0 File Offset: 0x000682C0
		public NPC06_Magolor(Vector2 position) : base(position)
		{
			base.Add(this.Sprite = GFX.SpriteBank.Create("magolor"));
			this.IdleAnim = "idle";
			this.MoveAnim = "walk";
			this.Visible = false;
			this.Maxspeed = 48f;
			base.SetupTheoSpriteSounds();
		}

		// Token: 0x06001382 RID: 4994 RVA: 0x0006A120 File Offset: 0x00068320
		public override void Update()
		{
			base.Update();
			if (!this.started)
			{
				GondolaMaggy gondola = base.Scene.Entities.FindFirst<GondolaMaggy>();
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (gondola != null && entity != null && entity.X > gondola.Left - 16f)
				{
					this.started = true;
                    CS06_Gondola cutsceneEntity = new CS06_Gondola(this, gondola, entity);
                    base.Scene.Add(cutsceneEntity);
				}
			}
		}

		// Token: 0x04000F55 RID: 3925
		private bool started;
	}
}
