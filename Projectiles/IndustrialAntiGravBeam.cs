using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace AntiGravMovers.Projectiles
{
	public class IndustrialAntiGravBeam : ModProjectile
	{
		public const float MAX_SPEED = 250.0f;
		public const float MAX_NPC_SPEED = 500.0f;
		public const float SPEED_DECAY = 0.95f;
		public const float MAX_DIST = 600.0f;
		public const float MIN_DIST = 300.0f;
		public const float WEAPON_OFFSET_X = -40.0f;
		public const float WEAPON_OFFSET_Y = -40.0f;
		public const float NPC_SPEED_DECAY = 0.98f;
		public const int MODE_NONE = -1, MODE_GROUND = 0, MODE_PLAYER = 1, MODE_NPC = 2;

		public Player owner;
		public NPC target;
		public Player ptarget;
		public bool isOwner;
		public float oDist;
		private float aRot;
		private float cx, cy, px, py, ox, oy, dcx, dcy, ncx, ncy, dpx, dpy, cdist, pdist, ang, nx, ny, tox, toy;
		private float halfPlayerHeight, halfPlayerWidth;
		private int mode;

		public override void SetDefaults()
		{
			projectile.Size = new Vector2(10);
			projectile.aiStyle = -1;
			projectile.alpha = 255;
			projectile.timeLeft = 3600;
			projectile.light = 0.8f;
			projectile.penetrate = 1;
			projectile.tileCollide = false;
			projectile.magic = true;
		}

		public override void AI()
		{
			projectile.netUpdate = true;

			if (projectile.ai[0] == 0)
			{
				Initialize();
				projectile.ai[0] = 1;
			}

			projectile.velocity.X = 0.0f;
			projectile.velocity.Y = 0.0f;

			if (Main.netMode != NetmodeID.Server)
			{
				owner.itemAnimation = 5;
				owner.itemTime = 10;
			}

			float speed;

			switch (mode)
			{
				case MODE_NPC:
					UpdateAltPoints();
					target.velocity.X += (nx - px) / 250.0f;
					target.velocity.Y += (ny - py) / 250.0f;

					speed = (float)Math.Sqrt(target.velocity.X * target.velocity.X + target.velocity.Y * target.velocity.Y);
					if (speed > MAX_NPC_SPEED)
					{
						target.velocity.X = (target.velocity.X / speed) * MAX_NPC_SPEED;
						target.velocity.Y = (target.velocity.Y / speed) * MAX_NPC_SPEED;
					}
					target.velocity.X *= NPC_SPEED_DECAY;
					target.velocity.Y *= NPC_SPEED_DECAY;
					break;
				case MODE_PLAYER:
					if (Main.netMode != NetmodeID.Server)
					{
						UpdatePlayerPoints();
						ptarget.velocity.X += (nx - px) / 250.0f;
						ptarget.velocity.Y += (ny - py) / 250.0f;

						speed = (float)Math.Sqrt(ptarget.velocity.X * ptarget.velocity.X + ptarget.velocity.Y * ptarget.velocity.Y);
						if (speed > MAX_SPEED)
						{
							ptarget.velocity.X = (ptarget.velocity.X / speed) * MAX_SPEED;
							ptarget.velocity.Y = (ptarget.velocity.Y / speed) * MAX_SPEED;
						}
						ptarget.velocity.X *= SPEED_DECAY;
						ptarget.velocity.Y *= SPEED_DECAY;
					}
					break;
				case MODE_GROUND:
					if (Main.netMode != NetmodeID.Server)
					{
						UpdatePoints();
						owner.velocity.X += (nx - ox) / 350.0f;
						owner.velocity.Y += (ny - oy) / 350.0f;

						speed = (float)Math.Sqrt(owner.velocity.X * owner.velocity.X + owner.velocity.Y * owner.velocity.Y);
						if (speed > MAX_SPEED)
						{
							owner.velocity.X = (owner.velocity.X / speed) * MAX_SPEED;
							owner.velocity.Y = (owner.velocity.Y / speed) * MAX_SPEED;
						}
						owner.velocity.X *= SPEED_DECAY;
						owner.velocity.Y *= SPEED_DECAY;
					}
					break;
			}
			if (!Main.player[projectile.owner].channel)
				projectile.Kill();
		}

		private float GetCursorX() => Main.MouseWorld.X;

		private float GetCursorY() => Main.MouseWorld.Y;

		public void Initialize()
		{
			owner = Main.player[projectile.owner];
			isOwner = (projectile.owner == Main.myPlayer);

			if (isOwner)
			{
				oDist = 0.0f;
				mode = MODE_NONE;
				float projSpeed = (float)Math.Sqrt(projectile.velocity.X * projectile.velocity.X + projectile.velocity.Y * projectile.velocity.Y);
				if (projSpeed < 8.0f)
				{
					projSpeed = 8.0f;
				}
				while ((mode == MODE_NONE) && ((oDist += projSpeed) < MAX_DIST))
				{
					mode = HitTest();
					projectile.position.X += projectile.velocity.X;
					projectile.position.Y += projectile.velocity.Y;
				}

				if (oDist < MIN_DIST)
					oDist = MIN_DIST;
			}

			aRot = 0.0f;
			projectile.velocity.X = 0.0f;
			projectile.velocity.Y = 0.0f;
			halfPlayerWidth = owner.width / 2.0f;
			halfPlayerHeight = owner.height / 2.0f;
		}

		private int HitTest()
		{
			int projX = (int)(projectile.position.X / 16);
			int projY = (int)(projectile.position.Y / 16);
			if ((projX > 0) && (projY > 0) && (projX < Main.maxTilesX) && (projY < Main.maxTilesY))
			{
				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					for (int i = 0; i < Main.maxPlayers; i++)
					{
						if (i != projectile.owner)
						{
							Player n = Main.player[i];
							float tx = projectile.position.X - n.position.X;
							float ty = projectile.position.Y - n.position.Y;
							if ((tx > 0) && (ty > 0) && (tx < n.width) && (ty < n.height) && n.active)
							{
								ptarget = n;
								return MODE_PLAYER;
							}
						}
					}
				}
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC n = Main.npc[i];
					float tx = projectile.position.X - n.position.X;
					float ty = projectile.position.Y - n.position.Y;
					if ((tx > 0) && (ty > 0) && (tx < n.width) && (ty < n.height) && n.CanBeChasedBy())
					{
						target = n;
						return MODE_NPC;
					}
				}
				if (Main.tile[projX, projY] != null)
					return (Main.tileSolid[Main.tile[projX, projY].type] && Main.tile[projX, projY].active()) ? MODE_GROUND : MODE_NONE;
				else
					return MODE_NONE;
			}
			return MODE_GROUND;
		}

		private void UpdatePlayerPoints()
		{
			cx = GetCursorX();
			cy = GetCursorY();
			px = ptarget.position.X + halfPlayerWidth;
			py = ptarget.position.Y + halfPlayerHeight;
			ox = owner.position.X + halfPlayerWidth;
			oy = owner.position.Y + halfPlayerHeight;

			dcx = cx - ox;
			dcy = cy - oy;
			dpx = px - ox;
			dpy = py - oy;

			cdist = (float)Math.Sqrt(dcx * dcx + dcy * dcy);
			pdist = (float)Math.Sqrt(dpx * dpx + dpy * dpy);

			ang = (float)Math.Atan2(cy - oy, cx - ox);

			nx = (float)Math.Cos(ang) * oDist + ox;
			ny = (float)Math.Sin(ang) * oDist + oy;

			tox = (float)Math.Cos(ang + 3.1415f) * WEAPON_OFFSET_X;
			toy = (float)Math.Sin(ang + 3.1415f) * WEAPON_OFFSET_Y;

			ncx = (dcx / cdist) * pdist;
			ncy = (dcy / cdist) * pdist;
		}
		private void UpdatePoints()
		{
			cx = GetCursorX();
			cy = GetCursorY();
			px = projectile.position.X;
			py = projectile.position.Y;
			ox = owner.position.X + halfPlayerWidth;
			oy = owner.position.Y + halfPlayerHeight;

			dcx = cx - ox;
			dcy = cy - oy;
			dpx = px - ox;
			dpy = py - oy;

			cdist = (float)Math.Sqrt(dcx * dcx + dcy * dcy);
			pdist = (float)Math.Sqrt(dpx * dpx + dpy * dpy);

			ang = (float)Math.Atan2(cy - oy, cx - ox) + 3.141592654f;

			nx = (float)Math.Cos(ang) * oDist + px;
			ny = (float)Math.Sin(ang) * oDist + py;

			tox = (float)Math.Cos(ang) * WEAPON_OFFSET_X;
			toy = (float)Math.Sin(ang) * WEAPON_OFFSET_Y;

			ncx = (dcx / cdist) * pdist;
			ncy = (dcy / cdist) * pdist;
		}
		private void UpdateAltPoints()
		{
			cx = GetCursorX();
			cy = GetCursorY();
			px = target.position.X + target.width / 2.0f;
			py = target.position.Y + target.height / 2.0f;
			ox = owner.position.X + halfPlayerWidth;
			oy = owner.position.Y + halfPlayerHeight;

			dcx = cx - ox;
			dcy = cy - oy;
			dpx = px - ox;
			dpy = py - oy;

			cdist = (float)Math.Sqrt(dcx * dcx + dcy * dcy);
			pdist = (float)Math.Sqrt(dpx * dpx + dpy * dpy);

			ang = (float)Math.Atan2(cy - oy, cx - ox);

			nx = (float)Math.Cos(ang) * oDist + ox;
			ny = (float)Math.Sin(ang) * oDist + oy;

			tox = (float)Math.Cos(ang + 3.1415f) * WEAPON_OFFSET_X;
			toy = (float)Math.Sin(ang + 3.1415f) * WEAPON_OFFSET_Y;

			ncx = (dcx / cdist) * pdist;
			ncy = (dcy / cdist) * pdist;
		}

		private float GetScale(float x)
		{
			return -((x - 1.0f) * (x - 1.0f)) + 1.25f;
		}

		private void DrawBit(SpriteBatch s, float x, float y, float ang = 0.0f, float scale = 1.0f, float fade = 1.0f)
		{
			float ts = scale - 0.25f;
			ts = 1.0f - ts;
			ts /= 2.0f;
			ts += 0.5f;
			Texture2D t = Main.projectileTexture[projectile.type];
			s.Draw(t, new Vector2(x - Main.screenPosition.X, y - Main.screenPosition.Y),
									 new Rectangle?(new Rectangle(0, 0, t.Width, t.Height)),
									 new Color((int)(scale * 100.0f), (int)(fade * 150.0f * ts), 0xFF), ang, new Vector2(t.Width >> 1, t.Height >> 1), scale, SpriteEffects.None, 0f);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			aRot -= 0.05f;
			float cap = 100.0f;

			float dx, dy, prec, iprec;
			for (float i = 0.0f; i < cap; i += 1.0f)
			{
				prec = i / cap;
				iprec = 1.0f - prec;

				dx = (dpx * prec) * prec + (ncx * prec + tox) * iprec + ox;
				dy = (dpy * prec) * prec + (ncy * prec + toy) * iprec + oy;
				DrawBit(spriteBatch, dx, dy, aRot + prec * 4.0f, GetScale(prec * 2.0f), (float)Math.Sin(aRot * 3.0f + prec * 4.0f) / 4.0f + 0.75f);
			}
		}
	}
}