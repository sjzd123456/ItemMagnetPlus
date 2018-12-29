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
            bool hasBuff;
            int magnetGrabRadius;
            int magnetScale;
            bool magnetActive;
            byte blacklistLength;
            Player magnetPlayer;
            ItemMagnetPlusPlayer mPlayer;
            switch (msgType)
            {
                // This message syncs ItemMagnetPlusPlayer.Magnet values
                case ItemMagnetPlusMessageType.Magnet:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        Console.WriteLine("echo recieved a Magnet");
                    }
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    hasBuff = reader.ReadBoolean();
                    magnetGrabRadius = reader.ReadInt32();
                    magnetScale = reader.ReadInt32();
                    magnetActive = reader.ReadBoolean();
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        //Console.WriteLine(" " + magnetPlayer.name + " radius " + magnetGrabRadius);
                        //Console.WriteLine(" " + magnetPlayer.name + " scale " + magnetScale);
                        //Console.WriteLine(" " + magnetPlayer.name + " active " + magnetActive);
                    }
                    if(hasBuff) Main.player[playernumber].AddBuff(BuffType("ItemMagnetBuff"), 3600, true);
                    mPlayer.magnetGrabRadius = magnetGrabRadius;
                    mPlayer.magnetScale = magnetScale;
                    mPlayer.magnetActive = magnetActive;
                    break;

                case ItemMagnetPlusMessageType.MagnetPlayerSyncPlayer:
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    magnetActive = reader.ReadBoolean();
                    mPlayer.magnetActive = magnetActive;
                    break;

                case ItemMagnetPlusMessageType.MagnetBlacklist:
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    blacklistLength = reader.ReadByte();
                    for (int i = 0; i < blacklistLength; i++)
                    {
                        mPlayer.magnetBlacklist[i] = reader.ReadInt32();
                    }
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
        MagnetPlayerSyncPlayer,
        MagnetBlacklist
    }
}
