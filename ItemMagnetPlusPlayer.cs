using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
    public class ItemMagnetPlusPlayer : ModPlayer
    {
        public int magnetActive = 0;
        public int magnetScreenRadius = 60 + 1;
        public int magnetGrabRadius = 10;
        public int magnetMinGrabRadius = 10;
        public int magnetMaxGrabRadius = 10; //60 half a screen radius
        public int magnetScale = 1;
        public int magnetVelocity = 8;
        public int magnetAcceleration = 8;
        public int[] magnetBlacklist; //only populated when player activates magnet, not changed during gameplay
        private bool hadMagnetActive = false;
        public bool currentlyActive = false;
        public int counter = 30;
        public int clientcounter = 30;

        public struct ClientConf
        {
            public int Range, Scale, Velocity, Acceleration, Buff;
            public string Filter;

            public ClientConf(int p1, int p2, int p3, int p4, int p5, string p6)
            {
                Range = p1;
                Scale = p2;
                Velocity = p3;
                Acceleration = p4;
                Buff = p5;
                Filter = p6;
            }

            public override string ToString()
            {
                return "R: " + Range +
                    ", S: " + Scale +
                    ", V: " + Velocity +
                    ", A: " + Acceleration +
                    ", B: " + Buff +
                    ", F: '" + Filter + "'";
            }
        }

        public ClientConf clientConf = new ClientConf(0, 0, 0, 0, 0, "");

        public override void ResetEffects()
        {
            if (clientConf.Buff == 1)
            {
                magnetActive = 0;
            }

            //magnetGrabRadius = 0;
            //these are changed by config
            magnetMaxGrabRadius = 10; //60 //vvvvvvvvv starting values vvvvvvvvvvvvvv
            magnetScale = 1; //1
            magnetVelocity = 8; //16
            magnetAcceleration = 8; //20
        }

        private void SendClientChangesPacket()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //Main.NewText("Send: " + slots + " " + slotsLast);
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)IMPMessageType.SendClientChanges);
                packet.Write((byte)player.whoAmI);
                packet.Write((int)magnetGrabRadius);
                BitsByte flags = new BitsByte();
                flags[0] = currentlyActive;
                //flags[1] = magnetActive > 0;
                packet.Write((byte)flags);
                packet.Send();
            }
        }

        public override void clientClone(ModPlayer clientClone)
        {
            ItemMagnetPlusPlayer clone = clientClone as ItemMagnetPlusPlayer;
            clone.magnetGrabRadius = magnetGrabRadius;
            clone.currentlyActive = currentlyActive;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            ItemMagnetPlusPlayer clone = clientPlayer as ItemMagnetPlusPlayer;
            if (clone.magnetGrabRadius != magnetGrabRadius || clone.currentlyActive != currentlyActive)
            {
                SendClientChangesPacket();
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            //server sends its config to player
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)IMPMessageType.SyncPlayer);
            packet.Write((byte)player.whoAmI);
            packet.Write((int)ModConf.Range);
            packet.Write((byte)ModConf.Scale);
            packet.Write((int)ModConf.Velocity);
            packet.Write((byte)ModConf.Acceleration);
            packet.Write((byte)1); //ModConf.Buff      //enforce buff in MP
            packet.Write((string)ModConf.Filter);
            player.GetModPlayer<ItemMagnetPlusPlayer>().clientConf = new ClientConf(ModConf.Range, ModConf.Scale, ModConf.Velocity, ModConf.Acceleration, 1, ModConf.Filter);

            //in addition to sending the server config, send all info about the players

            byte[] indexes = new byte[255];
            int[] ranges = new int[255];
            bool[] currentlyActives = new bool[255];
            byte arrayLength = 0;
            for (int i = 0; i < 255; i++)
            {
                if (Main.player[i].active && i != player.whoAmI)
                {
                    ranges[arrayLength] = magnetGrabRadius;
                    currentlyActives[arrayLength++] = currentlyActive;
                }
            }

            packet.Write((byte)arrayLength);
            if (arrayLength > 0)
            {
                Array.Resize(ref ranges, arrayLength + 1);
                Array.Resize(ref currentlyActives, arrayLength + 1);

                for (int i = 0; i < arrayLength; i++)
                {
                    packet.Write((byte)indexes[i]);
                    packet.Write((int)ranges[i]);
                    packet.Write((bool)currentlyActives[i]);
                }
            }

            packet.Send(toWho/*, fromWho*/);
        }

        public int[] MagnetBlacklist()
        {
            //list of item types to ignore
            int[] typeBlacklist = new int[20];
            if (clientConf.Filter == "")
            {
                return typeBlacklist;
            }
            string[] stringBlacklist = clientConf.Filter.Split(new string[] { "," }, 50, StringSplitOptions.RemoveEmptyEntries);
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
            Array.Resize(ref typeBlacklist, j + 1);
            Array.Sort(typeBlacklist,0, typeBlacklist.Length - 1);
            return typeBlacklist;
        }

        public void ActivateMagnet(Player player)
        {
            if (clientConf.Buff != 0) // != 0 is buff
            {
                player.AddBuff(mod.BuffType("ItemMagnetBuff"), 3600);
            }
            else
            {
                magnetActive = 1;
            }
        }

        public void DeactivateMagnet(Player player)
        {
            player.ClearBuff(mod.BuffType("ItemMagnetBuff"));
            if (clientConf.Buff == 0) // == 0 is no buff
            {
                magnetActive = 0;
            }
        }

        public override void OnEnterWorld(Player player)
        {
            DeactivateMagnet(player);
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                clientConf = new ClientConf(ModConf.Range, ModConf.Scale, ModConf.Velocity, ModConf.Acceleration, ModConf.Buff, ModConf.Filter);
                magnetBlacklist = MagnetBlacklist();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                SendClientChangesPacket();
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (player.HasBuff(mod.BuffType("ItemMagnetBuff")) || magnetActive != 0)
            {
                hadMagnetActive = true;
            }
            else
            {
                hadMagnetActive = false;
            }
            DeactivateMagnet(player);
        }

        public override void OnRespawn(Player player)
        {
            if (hadMagnetActive)
            {
                hadMagnetActive = false;
                ActivateMagnet(player);
            }
        }

        public void UpdateMagnetValues(int currentRadius)
        {
            //currentRadius is for creating steps between min and max range, and setting it accordingly
            magnetMaxGrabRadius = clientConf.Range;
            magnetScale = clientConf.Scale;
            magnetVelocity = clientConf.Velocity;
            magnetAcceleration = clientConf.Acceleration;

            if (magnetScale == 2)
            {
                magnetGrabRadius = magnetMaxGrabRadius;
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

            if (magnetScale == 0)
            {
                magnetGrabRadius = magnetMaxGrabRadius;
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
        }

        public override void PreUpdate()
        {
            //doing this only client side causes a small "lag" when the item first gets dragged toward the player
            currentlyActive = (clientConf.Buff == 1) ? player.HasBuff(mod.BuffType("ItemMagnetBuff")) : magnetActive == 1;

            if (magnetActive > 0 && !player.dead)
            {
                UpdateMagnetValues(magnetGrabRadius);

                int grabRadius = (int)(magnetGrabRadius * 16); //16 == to world coordinates
                int fullhdgrabRadius = (int)(grabRadius * 0.5625f);
                for (int j = 0; j < 400; j++)
                {
                    if(Main.item[j].active && Main.item[j].noGrabDelay == 0 && !ItemLoader.GrabStyle(Main.item[j], player) && ItemLoader.CanPickup(Main.item[j], player) /*&& Main.player[Main.item[j].owner].ItemSpace(Main.item[j])*/) {
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
                                    float velo = magnetVelocity; //16 ideal

                                    Vector2 distance = player.Center - Main.item[j].Center;

                                    velo += 2 * (1 - (distance.Length() / grabRadius));

                                    distance.Normalize();
                                    distance *= velo;

                                    //acceleration, higher = more acceleration
                                    if (magnetAcceleration > 40) magnetAcceleration = 40;
                                    int accel = -(magnetAcceleration - 41); //20 ideal

                                    Main.item[j].velocity = (Main.item[j].velocity * (float)(accel - 1) + distance) / (float)accel;

                                    if (Main.rand.NextFloat() < 0.7f)
                                    {
                                        Dust dust = Main.dust[Dust.NewDust(Main.item[j].position, Main.item[j].width, Main.item[j].height, 204, 0f, 0f, 0, new Color(255, 255, 255), 0.8f)];
                                        dust.noGravity = true;
                                        dust.noLight = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //}

            //if (Main.netMode == NetmodeID.Server)
            //{
            //    if (counter == 0)
            //    {
            //        //Console.WriteLine(" " + player.name + " active " + magnetActive);
            //        //Console.WriteLine(" " + player.name + " grabradius " + magnetGrabRadius);
            //        //(clientConf.Buff == 1)? player.HasBuff(mod.BuffType("ItemMagnetBuff")) : magnetActive == 1
            //        NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("server " + player.whoAmI + " " + (clientConf.Buff == 1) +  " " + player.HasBuff(mod.BuffType("ItemMagnetBuff")) + " " +(magnetActive == 1)), Color.Green);
            //        NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("server " + player.whoAmI + " " + currentlyActive), new Color(255, 25, 25));

            //        counter = 120;
            //    }
            //    counter--;
            //}

            //if (Main.netMode == NetmodeID.MultiplayerClient)
            //{
            //    if (clientcounter == 0)
            //    {
            //        for (int players = 0; players < Main.player.Length; players++)
            //        {
            //            if (Main.player[players].active)
            //            {
            //                if (Main.player[players].whoAmI == player.whoAmI)
            //                {
            //                    Main.NewText("SELF: " + currentlyActive);
            //                    //Main.NewText(clientConf);
            //                }
            //                else
            //                {
            //                    Main.NewText("OTHE: " + Main.player[players].GetModPlayer<ItemMagnetPlusPlayer>().currentlyActive);
            //                    //Main.NewText(Main.player[players].GetModPlayer<ItemMagnetPlusPlayer>().clientConf);
            //                }
            //            }
            //        }

            //        clientcounter = 120;
            //    }
            //    clientcounter--;
            //}

            //if (Main.netMode == NetmodeID.SinglePlayer)
            //{
            //    if (clientcounter == 0)
            //    {
            //        //Main.NewText("active " + magnetActive);
            //        //Main.NewText("grabradius " + magnetGrabRadius);
            //        Main.NewText(clientConf);

            //        clientcounter = 30;
            //    }
            //    clientcounter--;
            //}
        }
    }
}
