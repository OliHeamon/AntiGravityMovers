using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using AntiGravMovers.Projectiles;

namespace AntiGravMovers.Items
{
	public class AntiGravityMover : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Anti-Gravity Mover");
			Tooltip.SetDefault("WARNING: Slippery when wet");
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 18;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAnimation = 10;
			item.useTime = 6;
			item.shoot = ModContent.ProjectileType<AntiGravBeam>();
			item.shootSpeed = 10;
			item.channel = true;
			item.magic = true;
			item.noMelee = true;
			item.rare = ItemRarityID.Pink;
			item.value = 200000;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("AntiGravMovers:AnySilverBar", 10);
			recipe.AddIngredient(ItemID.Emerald, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UseStyle(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 vector = new Vector2(player.position.X + player.width * 0.5f, player.position.Y + player.height * 0.5f);
				float num27 = Main.MouseWorld.X - vector.X;
				float num28 = Main.MouseWorld.Y - vector.Y;

				if (Main.MouseWorld.X > player.position.X)
					player.direction = 1;
				else
					player.direction = -1;

				float ang = (float)Math.Atan2((num28 * player.direction), (num27 * player.direction));

				float aOffset = ang + ((player.direction == -1) ? 3.14f : 0.0f);
				float xOffset = (float)Math.Cos(aOffset) * -15.0f;
				float yOffset = (float)Math.Sin(aOffset) * -10.0f;

				player.itemLocation.X = player.position.X + player.width * 0.5f - Main.itemTexture[player.inventory[player.selectedItem].type].Width * 0.5f + xOffset;
				player.itemLocation.Y = player.position.Y + player.height * 0.5f - Main.itemTexture[player.inventory[player.selectedItem].type].Height * 0.5f + yOffset;
				player.itemRotation = ang;
			}
		}
	}
}