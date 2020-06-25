using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AntiGravMovers
{
	public class AntiGravMovers : Mod
	{
		public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Silver Bar", ItemID.SilverBar, ItemID.TungstenBar);

			RecipeGroup.RegisterGroup("AntiGravMovers:AnySilverBar", group);
		}
	}
}