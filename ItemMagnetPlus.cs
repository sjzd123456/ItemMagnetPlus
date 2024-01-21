using ItemMagnetPlus.Core.Netcode;
using System.IO;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
	public class ItemMagnetPlus : Mod
	{
		public static bool JPANsLoaded = false;

		// Mod Helpers compat
		public static string GithubUserName { get { return "direwolf420"; } }
		public static string GithubProjectName { get { return "ItemMagnetPlus"; } }

		public override void Load()
		{
			NetHandler.Load();
			JPANsLoaded = ModLoader.HasMod("JPANsBagsOfHoldingMod");
		}

		public override void Unload()
		{
			NetHandler.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}

