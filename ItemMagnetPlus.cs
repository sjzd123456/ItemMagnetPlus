using ItemMagnetPlus.Core.Netcode;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
	class ItemMagnetPlus : Mod
	{
		public static bool JPANsLoaded = false;

		public static LocalizedText AcceptClientChangesText { get; private set; }

		// Mod Helpers compat
		public static string GithubUserName { get { return "direwolf420"; } }
		public static string GithubProjectName { get { return "ItemMagnetPlus"; } }

		public override void Load()
		{
			ConfigWrapper.Load();
			NetHandler.Load();

			string category = $"Configs.Common.";
			AcceptClientChangesText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}AcceptClientChanges"));
		}

		public override void PostSetupContent()
		{
			JPANsLoaded = ModLoader.TryGetMod("JPANsBagsOfHoldingMod", out _);
		}

		public override void Unload()
		{
			ConfigWrapper.Unload();
			NetHandler.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetHandler.HandlePackets(reader, whoAmI);
		}
	}
}
