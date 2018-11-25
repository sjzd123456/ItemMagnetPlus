using System.IO;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System;

namespace ItemMagnetPlus
{
    class ItemMagnetPlus : Mod
    {
        public ItemMagnetPlus()
        {
        }

        public override void Load()
        {
            ModConf.Load();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ItemMagnetPlusMessageType msgType = (ItemMagnetPlusMessageType)reader.ReadByte();
            byte playernumber;
            switch (msgType)
            {
                // This message syncs ItemMagnetPlusPlayer.Magnet values
                case ItemMagnetPlusMessageType.Magnet:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        //Console.WriteLine("echo recieved a magnet packet");
                    }
                    playernumber = reader.ReadByte();
                    Player magnetPlayer = Main.player[playernumber];
                    int magnetGrabRadius = reader.ReadInt32();
                    int magnetScale = reader.ReadInt32();
                    int magnetActive = reader.ReadInt32();
                    ItemMagnetPlusPlayer mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        //Console.WriteLine(" " + magnetPlayer.name + " radius " + magnetGrabRadius);
                        //Console.WriteLine(" " + magnetPlayer.name + " scale " + magnetScale);
                        //Console.WriteLine(" " + magnetPlayer.name + " active " + magnetActive);
                    }
                    mPlayer.magnetGrabRadius = magnetGrabRadius;
                    mPlayer.magnetScale = magnetScale;
                    mPlayer.magnetActive = magnetActive;
                    break;
                default:
                    ErrorLogger.Log("ItemMagnetPlus: Unknown Message type: " + msgType);
                    break;
            }
        }
    }

    enum ItemMagnetPlusMessageType : byte
    {
        Magnet,
    }

    /*TODO
     * -somehow display current radius on buff or item tooltip
     * -change player.magnetMaxGrabRange to 10 on final release
     * 
     * 
     * 
     * 
     * 
     * 
     */
}
