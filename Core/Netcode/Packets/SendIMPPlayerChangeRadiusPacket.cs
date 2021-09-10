using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace ItemMagnetPlus.Core.Netcode.Packets
{
	public class SendIMPPlayerChangeRadiusPacket : PlayerPacket
	{
		readonly int magnetGrabRadius;

		public SendIMPPlayerChangeRadiusPacket() { }

		public SendIMPPlayerChangeRadiusPacket(ItemMagnetPlusPlayer mPlayer) : base(mPlayer.Player)
		{
			magnetGrabRadius = mPlayer.magnetGrabRadius;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.WriteVarInt(magnetGrabRadius);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			int magnetGrabRadius = reader.ReadVarInt();

			ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();

			mPlayer.magnetGrabRadius = magnetGrabRadius;

			if (Main.netMode == NetmodeID.Server)
            {
				new SendIMPPlayerChangeRadiusPacket(mPlayer).Send(-1, sender);
            }
		}
	}
}
