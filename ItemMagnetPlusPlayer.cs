using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ItemMagnetPlus
{
    public class ItemMagnetPlusPlayer : ModPlayer
    {
        public bool magnetActive = false;
        public int magnetScreenRadius = 60 + 1;
        public int magnetGrabRadius = 10;
        public int magnetMinGrabRadius = 10;
        public int magnetMaxGrabRadius = 10; //60 half a screen radius
        public int magnetScale = 1;
        public int magnetVelocity = 8;
        public int magnetAcceleration = 8;
        public int[] magnetBlacklist = new int[10]; //only populated when player enters world, not changed during gameplay
        private bool hadMagnetActive = false;
        public bool clientHasBuff = false;
        public int counter = 30;
        public int clientcounter = 30;

        //ErrorLogger.Log("ModConf.Range " + ModConf.Range);
        //ErrorLogger.Log("ModConf.Velocity " + ModConf.Velocity);
        //ErrorLogger.Log("ModConf.Acceleration " + ModConf.Acceleration);
        //ErrorLogger.Log("ModConf.Buff " + ModConf.Buff);

        public struct TempConf
        {
            public int Range, Scale, Velocity, Acceleration, Buff;

            public TempConf(int p1, int p2, int p3, int p4, int p5)
            {
                Range = p1;
                Scale = p2;
                Velocity = p3;
                Acceleration = p4;
                Buff = p5;
            }
        }

        public TempConf tempConf;

        public override void ResetEffects()
        {
            if (clientHasBuff)
            {
                magnetActive = false;
            }
            //magnetGrabRadius = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) //using server config
            {
                //if (/*ModConf.Buff == 1 &&*/ clientHasBuff)
                //{
                //    magnetActive = false;
                //}

                //these are changed by config
                //magnetMaxGrabRadius = 10; //60 //vvvvvvvvv starting values vvvvvvvvvvvvvv
                //magnetScale = 1; //1
                //magnetVelocity = 8; //16
                //magnetAcceleration = 8; //20
            }
        }

        public override void clientClone(ModPlayer clientClone)
        {
            ItemMagnetPlusPlayer clone = clientClone as ItemMagnetPlusPlayer;
        }

        public int[] MagnetBlacklist()
        {
            //list of item types to ignore
            //TODO make this more efficient with LINQ stuff
            //also make this more general
            int[] typeBlacklist = new int[10];
            if (ModConf.Filter == null || ModConf.Filter == "")
            {
                return typeBlacklist;
            }
            string[] stringBlacklist = ModConf.Filter.Split(new string[] { "," }, 50, StringSplitOptions.RemoveEmptyEntries);
            string[] lowerCase = new string[stringBlacklist.Length];
            for (int i = 0; i < stringBlacklist.Length; i++)
            {
                lowerCase[i] = stringBlacklist[i].Trim();
            }
            string[] lowerCaseDistinct = lowerCase.Distinct().ToArray();
            int j = -1;
            for (int i = 0; i < lowerCaseDistinct.Length; i++)
            {
                if (lowerCaseDistinct[i] == "heart")
                {
                    typeBlacklist[++j] = 58;
                    typeBlacklist[++j] = 1734;
                    typeBlacklist[++j] = 1867;
                }
                if (lowerCaseDistinct[i] == "mana")
                {
                    typeBlacklist[++j] = 184;
                    typeBlacklist[++j] = 1735;
                    typeBlacklist[++j] = 1868;
                }
                if (lowerCaseDistinct[i] == "coin")
                {
                    typeBlacklist[++j] = 330;
                    typeBlacklist[++j] = 331;
                    typeBlacklist[++j] = 332;
                    typeBlacklist[++j] = 333;
                }
            }
            //if no things added to the list then return empty list
            if (j < 0) return typeBlacklist;
            Array.Sort(typeBlacklist,0, typeBlacklist.Length);
            return typeBlacklist;
        }

        public void SendMagnetData()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Main.NewText("sent Magnet packet with " + magnetActive + " and range " + magnetGrabRadius);
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)ItemMagnetPlusMessageType.Magnet);
                packet.Write((byte)player.whoAmI);
                packet.Write(player.HasBuff(mod.BuffType("ItemMagnetBuff")));
                packet.Write(magnetGrabRadius);
                packet.Write((byte)magnetScale);
                packet.Write(magnetVelocity);
                packet.Write(magnetAcceleration);
                packet.Write(magnetActive);
                packet.Send();
            }
        }

        private void SendInitialData()
        {
            //client tells the server his buff and filter config
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //Main.NewText("sent MagnetBlacklist packet");
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)ItemMagnetPlusMessageType.MagnetInitialData);
                packet.Write((byte)player.whoAmI);
                packet.Write(clientHasBuff);
                packet.Write((byte)magnetBlacklist.Length);
                for (int i = 0; i < magnetBlacklist.Length; i++)
                {
                    packet.Write(magnetBlacklist[i]);
                }
                packet.Send();
            }
        }
        
        public void SendActive()
        {
            //gets only called from server
            //Main.NewText("syncplayer");
            Console.WriteLine("syncplayer " + player.name);
            //from server to client
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)ItemMagnetPlusMessageType.MagnetPlayerSyncPlayer);
            packet.Write((byte)player.whoAmI);
            packet.Write(magnetActive);
            packet.Send(/*toClient: player.whoAmI*/);
        }

        public void ActivateMagnet(bool sendData = true)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (clientHasBuff) // != 0 is buff
                {
                    player.AddBuff(mod.BuffType("ItemMagnetBuff"), 3600, true);
                }
                else
                {
                    ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>(mod);
                    mPlayer.magnetActive = true;
                }
            }

            UpdateMagnetValues(magnetGrabRadius);
            if (sendData) SendMagnetData();
        }

        public void DeactivateMagnet(bool sendData = true)
        {
            player.ClearBuff(mod.BuffType("ItemMagnetBuff"));
            if (Main.netMode != NetmodeID.Server && !clientHasBuff) // == 0 is no buff
            {
                ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>(mod);
                mPlayer.magnetActive = false;
            }

            for (int j = 0; j < 400; j++)
            {
                if (Main.item[j].active && Main.item[j].beingGrabbed)
                {
                    //Main.NewText("reset item " + Main.item[j].Name);
                    Main.item[j].beingGrabbed = false;
                }
            }
            UpdateMagnetValues(magnetGrabRadius);
            if (sendData) SendMagnetData();
        }

        public void SendOverrideData()
        {
            ErrorLogger.Log("SendOverrideData()");
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)ItemMagnetPlusMessageType.Override);
            packet.Write((byte)player.whoAmI);
            packet.Write(ModConf.Range);
            packet.Write((byte)ModConf.Scale);
            packet.Write(ModConf.Velocity);
            packet.Write(ModConf.Acceleration);
            packet.Write((byte)ModConf.Buff);
            packet.Write((byte)magnetBlacklist.Length);
            for (int i = 0; i < magnetBlacklist.Length; i++)
            {
                packet.Write(magnetBlacklist[i]);
            }

            packet.Send(toClient: player.whoAmI);
        }


        private void SendRequestOverride()
        {
            //client tells the server if it can override
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Main.NewText("sent SendRequestOverride packet");
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)ItemMagnetPlusMessageType.RequestOverride);
                packet.Write((byte)player.whoAmI);
                packet.Send();
            }
        }

        public void OverrideConfig(int a, int b, int c, int d, int e)
        {
            tempConf = new TempConf(a, b, c, d, e);
        }

        public override void OnEnterWorld(Player player)
        {
            //initial, might be overridden, might  be not
            tempConf = new TempConf(ModConf.Range, ModConf.Scale, ModConf.Velocity, ModConf.Acceleration, ModConf.Buff);

            clientHasBuff = tempConf.Buff == 1 ? true : false;
            magnetBlacklist = MagnetBlacklist();
            SendInitialData();

            //request an override
            SendRequestOverride();

            DeactivateMagnet();
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (player.HasBuff(mod.BuffType("ItemMagnetBuff")) || !magnetActive)
            {
                hadMagnetActive = true;
            }
            else
            {
                hadMagnetActive = false;
            }
            DeactivateMagnet();
        }

        public override void OnRespawn(Player player)
        {
            if (hadMagnetActive)
            {
                hadMagnetActive = false;
                ActivateMagnet(true);
            }
        }

        public void UpdateMagnetValues(int currentRadius)
        {
            //only apply changes client side, but sync them up once a second to server
            if (Main.netMode != NetmodeID.Server)
            {
                //currentRadius is for creating steps between min and max range, and setting it accordingly
                magnetMaxGrabRadius = tempConf.Range;
                magnetScale = tempConf.Scale;
                magnetVelocity = tempConf.Velocity;
                magnetAcceleration = tempConf.Acceleration;
                if (magnetScale == 2)
                {
                    magnetGrabRadius = magnetMaxGrabRadius;
                    //SendMagnetData();
                    return;
                }
                if (NPC.downedSlimeKing)
                {
                    //Starts at
                    //magnetMaxGrabRadius = 10;
                    //magnetVelocity = 8;
                    //magnetAcceleration = 8;

                    magnetVelocity += 4;
                    magnetAcceleration += 2;
                }
                if (NPC.downedBoss1) //Eye of Cthulhu
                {
                    magnetMaxGrabRadius += 5;
                }
                if (NPC.downedBoss2) //Eater/Brain
                {
                    magnetMaxGrabRadius += 5;
                }
                if (NPC.downedQueenBee)
                {
                    magnetVelocity += 4;
                    magnetAcceleration += 10;
                }
                if (NPC.downedBoss3) //Skeletron
                {
                    magnetMaxGrabRadius += 5;
                }
                if (Main.hardMode) //Wall of flesh
                {
                    //Ideal at
                    //magnetMaxGrabRadius = 30; //quarter screen
                    //magnetVelocity = 16;
                    //magnetAcceleration = 20;

                    magnetMaxGrabRadius += 5; //quarter of a screen range if ^ satisfied
                }
                if (NPC.downedMechBoss1) //Destroyer
                {
                    magnetMaxGrabRadius += 10;
                }
                if (NPC.downedMechBoss2) //Twins
                {
                    magnetMaxGrabRadius += 10;
                }
                if (NPC.downedMechBoss3) //Skeletron prime
                {
                    magnetMaxGrabRadius += 10;
                }
                if (NPC.downedPlantBoss)
                {
                    magnetMaxGrabRadius += 10;
                    magnetVelocity += 4;
                    magnetAcceleration += 2;
                }
                if (NPC.downedGolemBoss)
                {
                    magnetMaxGrabRadius += 10;
                    magnetVelocity += 4;
                    magnetAcceleration += 2;
                }
                if (NPC.downedFishron)
                {
                    magnetMaxGrabRadius += 10;
                    magnetVelocity += 4;
                    magnetAcceleration += 2;
                }
                if (NPC.downedAncientCultist)
                {
                    magnetMaxGrabRadius += 10;
                }
                if (NPC.downedMoonlord)
                {
                    //Final at
                    //magnetMaxGrabRadius = 120; //one screen
                    //magnetVelocity = 32;
                    //magnetAcceleration = 32;
                    magnetMaxGrabRadius += 20;
                    magnetVelocity += 4;
                    magnetAcceleration += 6;
                }

                if (magnetAcceleration > ModConf.maxAcceleration) magnetAcceleration = ModConf.maxAcceleration; //hard cap

                if (magnetScale == 0)
                {
                    magnetGrabRadius = magnetMaxGrabRadius;
                    //SendMagnetData();
                    return;
                }

                if (currentRadius <= magnetMaxGrabRadius + 1)
                {
                    magnetGrabRadius = currentRadius;
                }
                else
                {
                    magnetGrabRadius = magnetMinGrabRadius;
                }

                //SendMagnetData();
            }
        }

        public override void PreUpdate()
        {
            //Main.NewText("+++++++++++");
            //Main.NewText("rad " + magnetGrabRadius);
            //Main.NewText("max " + magnetMaxGrabRadius);
            //Main.NewText("min " +  magnetMinGrabRadius);
            //Main.NewText("active " + magnetActive);

            //if (Main.netMode != NetmodeID.MultiplayerClient)
            //{
            if (magnetActive)
            {
                //UpdateMagnetValues(magnetGrabRadius);

                int grabRadius = (int)(magnetGrabRadius * 16); //16 == to world coordinates
                int fullhdgrabRadius = (int)(grabRadius * 0.5625f);
                //Main.NewText("grabradius: " + grabRadius);
                for (int j = 0; j < 400; j++)
                {
                    //Main.NewText(j);
                    //if (j ==0) Main.NewText("start for loop");
                    if (Main.item[j].active && Main.item[j].noGrabDelay == 0 && !ItemLoader.GrabStyle(Main.item[j], player) && ItemLoader.CanPickup(Main.item[j], player) /*&& Main.player[Main.item[j].owner].ItemSpace(Main.item[j])*/) {
                        //Main.NewText("position " + player.position);
                        //Main.NewText("Tile " + player.position.ToTileCoordinates());
                        Rectangle rect = new Rectangle((int)player.position.X - grabRadius, (int)player.position.Y - fullhdgrabRadius, player.width + grabRadius * 2, player.height + fullhdgrabRadius * 2);
                        if (rect.Intersects(new Rectangle((int)Main.item[j].position.X, (int)Main.item[j].position.Y, Main.item[j].width, Main.item[j].height)))
                        {
                            if (player.inventory[player.selectedItem].type != 0 || player.itemAnimation <= 0)
                            {
                                if (Array.BinarySearch(magnetBlacklist, Main.item[j].type) < 0)
                                {
                                    //so it can go through walls
                                    Main.item[j].beingGrabbed = true;
                                    //velocity, higher = more speed
                                    int velo = magnetVelocity; //16 default

                                    Vector2 vector = new Vector2(Main.item[j].position.X + (float)(Main.item[j].width / 2), Main.item[j].position.Y + (float)(Main.item[j].height / 2));
                                    float distanceX = player.Center.X - vector.X;
                                    float distanceY = player.Center.Y - vector.Y;
                                    float normalDistance = (float)Math.Sqrt((double)(distanceX * distanceX + distanceY * distanceY));
                                    normalDistance = ((float)velo) / normalDistance;
                                    distanceX *= normalDistance;
                                    distanceY *= normalDistance;

                                    //acceleration, higher = more acceleration
                                    if (magnetAcceleration > 40) magnetAcceleration = 40;
                                    int accel = -(magnetAcceleration - 41); //20 default

                                    // num1 goes linear, num2 goes inverse
                                    Main.item[j].velocity.X = (Main.item[j].velocity.X * (float)(accel - 1) + distanceX) / (float)accel;
                                    Main.item[j].velocity.Y = (Main.item[j].velocity.Y * (float)(accel - 1) + distanceY) / (float)accel;

                                    if (Main.rand.NextFloat() < 0.7f)
                                    {
                                        Dust dust = Main.dust[Dust.NewDust(Main.item[j].position, 30, 30, 204, 0f, 0f, 0, new Color(255, 255, 255), 0.8f)];
                                        dust.noGravity = true;
                                        dust.noLight = true;
                                    }
                                    //Main.NewText("drawing in item " + Main.item[j].Name);
                                }
                            }
                        }
                    }
                       // }
                    else
                    {
                        Main.item[j].beingGrabbed = false;
                    }
                    //Main.item[j].beingGrabbed = false;
                    //if (j == 399) Main.NewText("end for loop");
                }
            }
            //}

            if (Main.time % 60 == 34 && Main.netMode == NetmodeID.Server)
            {
                if (false) SendActive();
            }

            if (Main.netMode == NetmodeID.Server)
            {
                if (counter == 0)
                {
                    Console.WriteLine(player.name + " active " + magnetActive + " buff " + clientHasBuff);
                    Console.WriteLine(player.name + "  scale " + magnetScale + " vel " + magnetVelocity);
                    //for (int i = 0; i < magnetBlacklist.Length; i++)
                    //{
                    //    if(magnetBlacklist[i] !=0)Console.Write( " " + magnetBlacklist[i]);
                    //}
                    //Console.WriteLine("");
                    counter = 240;
                }
                counter--;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (clientcounter == 0)
                {
                    for (int players = 0; players < Main.player.Length; players++)
                    {
                        if (Main.player[players].active)
                        {
                            if(Main.player[players].whoAmI == player.whoAmI)
                            {
                                Main.NewText("SELF: " + " active " + magnetActive + " buff " + clientHasBuff);
                                Main.NewText("SELF: " + "  scale " + magnetScale + " vel " + magnetVelocity);
                            }
                            else
                            {
                                Main.NewText("OTHE: " + " active " + magnetActive + " buff " + clientHasBuff);
                                Main.NewText("OTHE: " + "  scale " + magnetScale + " vel " + magnetVelocity);
                            }
                        }
                    }
                    //Main.NewText("active " + magnetActive + " buff " + clientHasBuff);
                    //Main.NewText("scale " + magnetScale + " vel " + magnetVelocity);
                    //for (int i = 0; i < magnetBlacklist.Length; i++)
                    //{
                    //    if (magnetBlacklist[i] != 0) Main.NewText(magnetBlacklist[i]);
                    //}

                    clientcounter = 240;
                }
                clientcounter--;
            }

            //if (Main.netMode == NetmodeID.SinglePlayer)
            //{
            //    if (clientcounter == 0)
            //    {
            //        Main.NewText("active " + magnetActive);
            //        Main.NewText("grabradius " + magnetGrabRadius);

            //        clientcounter = 30;
            //    }
            //    clientcounter--;
            //}
        }
    }
}
