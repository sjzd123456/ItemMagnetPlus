using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
            IMPMessageType msgType = (IMPMessageType)reader.ReadByte();
            byte playernumber;
            int range;
            byte scale;
            int velocity;
            byte acceleration;
            byte buff;
            string filter;

            byte arrayLength;

            bool currentlyActive;

            ItemMagnetPlusPlayer mPlayer;
            switch (msgType)
            {
                case IMPMessageType.SyncPlayer:
                    if(Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        playernumber = reader.ReadByte();  //byte
                        range = reader.ReadInt32();        //int
                        scale = reader.ReadByte();         //byte
                        velocity = reader.ReadInt32();     //int
                        acceleration = reader.ReadByte();  //byte
                        buff = reader.ReadByte();          //byte
                        filter = reader.ReadString();      //string
                        mPlayer = Main.player[playernumber].GetModPlayer<ItemMagnetPlusPlayer>();
                        mPlayer.clientConf = new ItemMagnetPlusPlayer.ClientConf(range, scale, velocity, acceleration, buff, filter);
                        mPlayer.magnetBlacklist = mPlayer.MagnetBlacklist();

                        //in addition to recieving the server config, get all info about the players

                        arrayLength = reader.ReadByte();
                        //Main.NewText("arrayLength is " + arrayLength);
                        if (arrayLength > 0)
                        {
                            byte[] indexes = new byte[arrayLength];
                            int[] ranges = new int[arrayLength];
                            bool[] currentlyActives = new bool[arrayLength];

                            for (int i = 0; i < arrayLength; i++)
                            {
                                indexes[i] = reader.ReadByte();
                                ranges[i] = reader.ReadInt32();
                                currentlyActives[i] = reader.ReadBoolean();
                            }

                            for (int i = 0; i < arrayLength; i++)
                            {
                                //Main.NewText("recv with " + indexes[i] + " " + ranges[i] + " " + currentlyActives[i]);
                                mPlayer = Main.player[indexes[i]].GetModPlayer<ItemMagnetPlusPlayer>();
                                mPlayer.magnetGrabRadius = ranges[i];
                                mPlayer.currentlyActive = currentlyActives[i];
                            }
                        }
                    }
                    break;
                case IMPMessageType.SendClientChanges:
                    playernumber = reader.ReadByte();

                    //if (Main.netMode == NetmodeID.MultiplayerClient)
                    //{
                    //    Main.NewText("recv sendclientchanges from " + playernumber);
                    //}
                    range = reader.ReadInt32();        //int
                    currentlyActive = reader.ReadBoolean();

                    mPlayer = Main.player[playernumber].GetModPlayer<ItemMagnetPlusPlayer>();
                    mPlayer.magnetGrabRadius = range;
                    mPlayer.currentlyActive = currentlyActive;
                    if (Main.netMode == NetmodeID.Server)
                    {
                        //NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("server send SendClientChanges from " + playernumber), new Color(255, 25, 25));
                        ModPacket packet = GetPacket();
                        packet.Write((byte)IMPMessageType.SendClientChanges);
                        packet.Write(playernumber);
                        packet.Write((int)range);
                        packet.Write((bool)currentlyActive);
                        packet.Send(-1, playernumber);
                    }
                    break;
                default:
                    ErrorLogger.Log("ItemMagnetPlus: Unknown Message type: " + msgType);
                    break;
            }
        }
    }

    enum IMPMessageType : byte
    {
        Magnet,
        SyncPlayer,
        SendClientChanges
    }
}
