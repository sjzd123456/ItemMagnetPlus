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
                        try
                        {
                            Console.WriteLine("echo recieved a magnet packet");
                        }
                        catch (Exception)
                        {
                            playernumber = 0; //dummy
                        }
                    }
                    playernumber = reader.ReadByte();
                    Player magnetPlayer = Main.player[playernumber];
                    int magnetGrabRadius = reader.ReadInt32();
                    int magnetScale = reader.ReadInt32();
                    int magnetActive = reader.ReadInt32();
                    ItemMagnetPlusPlayer mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    try
                    {
                        Console.WriteLine("radius " + magnetGrabRadius);
                        Console.WriteLine("scale " + magnetScale);
                        Console.WriteLine("active " + magnetActive);
                    }
                    catch(Exception)
                    {
                        playernumber = 0; //dummy
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
}