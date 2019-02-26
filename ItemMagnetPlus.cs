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
            int buff;

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
                        Console.WriteLine("echo recieved a Magnet");
                    }
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    hasBuff = reader.ReadBoolean();
                    magnetGrabRadius = reader.ReadInt32();
                    magnetScale = reader.ReadByte();
                    magnetVelocity = reader.ReadInt32();
                    magnetAcceleration = reader.ReadInt32();
                    magnetActive = reader.ReadBoolean();
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();
                    if (Main.netMode == NetmodeID.Server)
                    {
                        Console.WriteLine(" " + magnetPlayer.name + " radius " + magnetGrabRadius);
                        Console.WriteLine(" " + magnetPlayer.name + " scale " + magnetScale);
                        Console.WriteLine(" " + magnetPlayer.name + " active " + magnetActive);
                    }
                    if (hasBuff) Main.player[playernumber].AddBuff(BuffType("ItemMagnetBuff"), 3600, true);
                    mPlayer.magnetGrabRadius = magnetGrabRadius;
                    mPlayer.magnetScale = magnetScale;
                    mPlayer.magnetVelocity = magnetVelocity;
                    mPlayer.magnetAcceleration = magnetAcceleration;
                    mPlayer.magnetActive = magnetActive;

                    //tell everyone including the player itself, what his magnet status is
                    mPlayer.SendActive();

                    break;

                //This message syncs magnetActive regularly
                case ItemMagnetPlusMessageType.MagnetPlayerSyncPlayer:
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    Main.NewText("recv Player " + magnetPlayer.name);
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


                //This message recieves the server config (if necessary)
                case ItemMagnetPlusMessageType.Override:
                    Main.NewText("recv Override packet");
                    playernumber = reader.ReadByte();
                    magnetPlayer = Main.player[playernumber];
                    mPlayer = magnetPlayer.GetModPlayer<ItemMagnetPlusPlayer>();

                    magnetGrabRadius = reader.ReadInt32();
                    magnetScale = reader.ReadByte();
                    magnetVelocity = reader.ReadInt32();
                    magnetAcceleration = reader.ReadInt32();
                    buff = reader.ReadByte();

                    blacklistLength = reader.ReadByte();
                    mPlayer.magnetBlacklist = mPlayer.MagnetBlacklist();
                    for (int i = 0; i < blacklistLength; i++)
                    {
                        mPlayer.magnetBlacklist[i] = reader.ReadInt32();
                        //Console.WriteLine(" " + mPlayer.magnetBlacklist[i]);
                    }
                    Array.Sort(mPlayer.magnetBlacklist, 0, mPlayer.magnetBlacklist.Length);

                    //ErrorLogger.Log("Previous Config::::::");
                    //ErrorLogger.Log("ModConf.Range " + mPlayer.tempConf.Range);
                    //ErrorLogger.Log("ModConf.Velocity " + mPlayer.tempConf.Velocity);
                    //ErrorLogger.Log("ModConf.Acceleration " + mPlayer.tempConf.Acceleration);
                    //ErrorLogger.Log("ModConf.Buff " + mPlayer.tempConf.Buff);

                    mPlayer.OverrideConfig(magnetGrabRadius, magnetScale, magnetVelocity, magnetAcceleration, buff);

                    mPlayer.clientHasBuff = mPlayer.tempConf.Buff == 1 ? true : false;

                    //ErrorLogger.Log("After Config::::::");
                    //ErrorLogger.Log("ModConf.Range " + ModConf.Range);
                    //ErrorLogger.Log("ModConf.Velocity " + ModConf.Velocity);
                    //ErrorLogger.Log("ModConf.Acceleration " + ModConf.Acceleration);
                    //ErrorLogger.Log("ModConf.Buff " + ModConf.Buff);

                    //ModConf.OverrideConfig(magnetGrabRadius, magnetScale, magnetVelocity, magnetAcceleration, buff);

                    mPlayer.clientHasBuff = mPlayer.tempConf.Buff == 1 ? true : false;

                    //ErrorLogger.Log("After Config::::::");
                    //ErrorLogger.Log("ModConf.Range " + ModConf.Range);
                    //ErrorLogger.Log("ModConf.Velocity " + ModConf.Velocity);
                    //ErrorLogger.Log("ModConf.Acceleration " + ModConf.Acceleration);
                    //ErrorLogger.Log("ModConf.Buff " + ModConf.Buff);
                    break;

                case ItemMagnetPlusMessageType.RequestOverride:
                    Console.WriteLine("recv RequestOverride packet");
                    playernumber = reader.ReadByte();
                    //if force server config is set to 1 on the server side, .SendOverrideData();
                    if (ModConf.ForceServerConf == 1) Main.player[playernumber].GetModPlayer<ItemMagnetPlusPlayer>().SendOverrideData();

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
        MagnetInitialData,
        RequestOverride,
        Override
    }
}
