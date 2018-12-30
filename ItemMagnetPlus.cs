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
            int magnetVelocity;
            int magnetAcceleration;
            bool magnetActive;
            byte blacklistLength;
            bool clientHasBuff;
            Player magnetPlayer;
            ItemMagnetPlusPlayer mPlayer;
            switch (msgType)
            {
                //This message syncs ItemMagnetPlusPlayer.Magnet values
                case ItemMagnetPlusMessageType.Magnet:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        //Console.WriteLine("echo recieved a Magnet");
                    }
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    hasBuff = reader.ReadBoolean();
                    magnetGrabRadius = reader.ReadInt32();
                    magnetScale = reader.ReadInt32();
                    magnetVelocity = reader.ReadInt32();
                    magnetAcceleration = reader.ReadInt32();
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
                    mPlayer.magnetVelocity = magnetVelocity;
                    mPlayer.magnetAcceleration = magnetAcceleration;
                    mPlayer.magnetActive = magnetActive;
                    break;

                //This message syncs magnetActive regularly
                case ItemMagnetPlusMessageType.MagnetPlayerSyncPlayer:
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    magnetActive = reader.ReadBoolean();
                    mPlayer.magnetActive = magnetActive;
                    break;

                //This message syncs blacklist and buff on world enter
                case ItemMagnetPlusMessageType.MagnetInitialData:
                    //Console.WriteLine("recv MagnetBlacklist packet");
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    clientHasBuff = reader.ReadBoolean();
                    //Console.WriteLine("clientHasBuff " + clientHasBuff);
                    blacklistLength = reader.ReadByte();
                    for (int i = 0; i < blacklistLength; i++)
                    {
                        mPlayer.magnetBlacklist[i] = reader.ReadInt32();
                        //Console.WriteLine(" " + mPlayer.magnetBlacklist[i]);
                    }
                    mPlayer.clientHasBuff = clientHasBuff;
                    Array.Sort(mPlayer.magnetBlacklist, 0, mPlayer.magnetBlacklist.Length);
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
        MagnetInitialData
    }
}
