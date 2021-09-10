using ItemMagnetPlus.Core.Netcode;
using System.IO;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
    class ItemMagnetPlus : Mod
    {
        public static bool JPANsLoaded = false;

        // Mod Helpers compat
        public static string GithubUserName { get { return "direwolf420"; } }
        public static string GithubProjectName { get { return "ItemMagnetPlus"; } }

        public override void Load()
        {
            ConfigWrapper.Load();
            NetHandler.Load();
        }

        public override void PostSetupContent()
        {
            JPANsLoaded = ModLoader.GetMod("JPANsBagsOfHoldingMod") != null;
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
