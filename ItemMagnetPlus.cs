using ItemMagnetPlus.Core.Netcode;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
	public class ItemMagnetPlus : Mod
	{
		public static bool JPANsLoaded = false;

		public static LocalizedText AcceptClientChangesText { get; private set; }

		// Mod Helpers compat
		public static string GithubUserName { get { return "direwolf420"; } }
		public static string GithubProjectName { get { return "ItemMagnetPlus"; } }

		public override void Load()
		{
			NetHandler.Load();
			JPANsLoaded = ModLoader.HasMod("JPANsBagsOfHoldingMod");

			string category = $"Configs.Common.";
			AcceptClientChangesText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AcceptClientChanges"));
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

