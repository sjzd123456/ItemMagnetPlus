using ItemMagnetPlus.Items;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ItemMagnetPlus.Buffs
{
	public class ItemMagnetBuff : ModBuff
	{
		public static LocalizedText CurrentRangeText { get; private set; }
		public static LocalizedText CurrentVelocityText { get; private set; }
		public static LocalizedText CurrentAccelerationText { get; private set; }

		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;

			CurrentRangeText = this.GetLocalization("CurrentRange");
			CurrentVelocityText = this.GetLocalization("CurrentVelocity");
			CurrentAccelerationText = this.GetLocalization("CurrentAcceleration");
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			Player player = Main.LocalPlayer;
			ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();
			string add = $"\n{CurrentRangeText.Format(mPlayer.magnetGrabRadius)}";
			mPlayer.UpdateMagnetValues(mPlayer.magnetGrabRadius);
			add += $"\n{CurrentVelocityText.Format(mPlayer.magnetVelocity)}";
			add += $"\n{CurrentAccelerationText.Format(mPlayer.magnetAcceleration)}";
			tip += add;

			if (Main.GameUpdateCount % 50 == 0)
			{
				ItemMagnet.DrawRectangle(player, mPlayer.magnetGrabRadius * 16, CombatText.HealMana);
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.HasItem(ModContent.ItemType<ItemMagnet>()) || player.selectedItem == 58) //when player takes the item out of his inventory
			{
				player.buffTime[buffIndex] = 2;
				player.GetModPlayer<ItemMagnetPlusPlayer>().magnetActive = 1;
			}
		}
	}
}
