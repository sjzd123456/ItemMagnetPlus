﻿using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ItemMagnetPlus.Core.Netcode.Packets
{
	public class SyncIMPPlayerPacket : PlayerPacket
	{
		readonly int magnetGrabRadius;
		readonly bool currentlyActive;

		public SyncIMPPlayerPacket() { }

		public SyncIMPPlayerPacket(ItemMagnetPlusPlayer mPlayer) : base(mPlayer.player)
		{
			magnetGrabRadius = mPlayer.magnetGrabRadius;
			currentlyActive = mPlayer.currentlyActive;
		}

		protected override void PostSend(BinaryWriter writer, Player player)
		{
			writer.WriteVarInt(magnetGrabRadius);
			writer.Write(currentlyActive);
		}

		protected override void PostReceive(BinaryReader reader, int sender, Player player)
		{
			int magnetGrabRadius = reader.ReadVarInt();
			bool currentlyActive = reader.ReadBoolean();

			ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();

			mPlayer.magnetGrabRadius = magnetGrabRadius;
			mPlayer.currentlyActive = currentlyActive;

			//No rebroadcast needed, SyncPlayer handles it
		}
	}
}