using System.IO;
using Terraria;
using Terraria.ID;

namespace ItemMagnetPlus.Core.Netcode.Packets
{
	public class SendIMPPlayerChangeTogglePacket : PlayerPacket
	{
		readonly bool currentlyActive;

		public SendIMPPlayerChangeTogglePacket() { }

		public SendIMPPlayerChangeTogglePacket(ItemMagnetPlusPlayer mPlayer) : base(mPlayer.player)
		{
			currentlyActive = mPlayer.currentlyActive;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.Write(currentlyActive);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			bool currentlyActive = reader.ReadBoolean();

			ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();

			mPlayer.currentlyActive = currentlyActive;

			if (Main.netMode == NetmodeID.Server)
			{
				new SendIMPPlayerChangeTogglePacket(mPlayer).Send(-1, sender);
			}
		}
	}
}
