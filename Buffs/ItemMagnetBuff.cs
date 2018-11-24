using Terraria;
using Terraria.ModLoader;

namespace ItemMagnetPlus.Buffs
{
    public class ItemMagnetBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Item Magnet");
            Description.SetDefault("A magnetic aura surrounds you!");
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HasItem(mod.ItemType("ItemMagnet")))
            {
                player.buffTime[buffIndex] = 60;
                player.GetModPlayer<ItemMagnetPlusPlayer>(mod).magnetActive = 1;
            }
        }
    }
}