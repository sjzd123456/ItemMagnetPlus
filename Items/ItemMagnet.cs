using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ItemMagnetPlus.Items
{
    public class ItemMagnet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Item Magnet");
            Tooltip.SetDefault("Use to change radius");
        }

        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.scale = 1f;
            item.value = 100;
            item.rare = 12;
            item.useAnimation = 10;
            item.useTime = 10;
            item.useStyle = 4;
            item.consumable = false;
            item.buffType = mod.BuffType("ItemMagnetBuff");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.IronBar, 6);
            recipe.AddIngredient(ItemID.Sapphire, 1);
            recipe.AddIngredient(ItemID.Ruby, 1);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.IronBar, 12);
            recipe2.SetResult(this, 1);
            recipe2.AddRecipe();
        }

        public override bool CanUseItem(Player player)
        {
            return true;
        }

        public override bool OnPickup(Player player)
        {
            //player.AddBuff(item.buffType, 3600, true);
            //player.ClearBuff(mod.BuffType("ItemMagnetBuff")); //not needed since buff time is only 2 sec
            //ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>(mod);
            //Main.NewText("deactivated Magnet", Color.Red.R, Color.Red.G, Color.Red.B);
            //Main.PlaySound(SoundID.MaxMana, player.position, 1);
            //mPlayer.magnetActive = 0;
            //SendMagnetData(mPlayer);

            //for (int j = 0; j < 400; j++)
            //{
            //    if (Main.item[j].beingGrabbed)
            //    {
            //        Main.NewText("reset item " + Main.item[j].Name);
            //        Main.item[j].beingGrabbed = false;
            //    }
            //}

            return true;
        }

        public static Dust QuickDust(Vector2 pos, Color color)
        {
            int type = 1;
            Dust dust = Main.dust[Dust.NewDust(pos, 4, 4, type, 0f, 0f, 120, color, 2f)];
            dust.position = pos;
            dust.velocity = Vector2.Zero;
            dust.fadeIn = 3f;
            dust.noLight = true;
            dust.noGravity = true;
            return dust;
        }

        public static void QuickDustLine(Vector2 start, Vector2 end, float splits, Color color)
        {
            QuickDust(start, color);
            float num = 1f / splits;
            for (float num2 = 0f; num2 < 1f; num2 += num)
            {
                QuickDust(Vector2.Lerp(start, end, num2), color);
            }
        }

        private void DrawRectangle(ItemMagnetPlusPlayer mPlayer, int radius, Color color)
        {
            int stage = radius / (mPlayer.magnetScreenRadius * 16);
            radius = radius - mPlayer.magnetScreenRadius * 16 * stage;
            float fullhdradius = radius * 0.5625f;
            color = new Color(color.R + stage * 40, color.G, color.B - stage * 40);
            //Main.NewText("DrawRec radius " + radius);

            //radius in world coordinates
            Vector2 pos = mPlayer.player.position;
            //radius in tile coordinates
            float leftx = pos.X - radius;
            float topy = pos.Y - fullhdradius;
            float rightx = leftx + mPlayer.player.width + radius * 2;
            float boty = topy + mPlayer.player.height + fullhdradius * 2;
            //Main.NewText("leftx " + leftx);
            //Main.NewText("topy " + topy);
            //Main.NewText("rightx " + rightx);
            //Main.NewText("boty " + boty);

            QuickDustLine(new Vector2(leftx, topy), new Vector2(rightx, topy), radius / 16f, color); //clock wise starting top left
            QuickDustLine(new Vector2(rightx, topy), new Vector2(rightx, boty), radius / 16f, color);
            QuickDustLine(new Vector2(rightx, boty), new Vector2(leftx, boty), radius / 16f, color);
            QuickDustLine(new Vector2(leftx, boty), new Vector2(leftx, topy), radius / 16f, color);
        }

        public void SendMagnetData(ItemMagnetPlusPlayer mPlayer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                //Main.NewText("sent Magnet packet");
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)ItemMagnetPlusMessageType.Magnet);
                packet.Write((byte)mPlayer.player.whoAmI);
                packet.Write(mPlayer.magnetGrabRadius);
                packet.Write(mPlayer.magnetScale);
                packet.Write(mPlayer.magnetActive);
                packet.Send();
            }
        }

        public override bool UseItem(Player player)
        {
            ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>(mod);
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {

                int divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5;
                //int steps = (mPlayer.magnetMaxGrabRadius - divider) / divider;
                int radius = mPlayer.magnetGrabRadius;

                if (mPlayer.magnetActive == 0)
                {
                    player.AddBuff(item.buffType, 3600, true);

                    //CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.HealLife, "magnet on");
                    //Main.NewText("activated Magnet", Color.Green.R, Color.Green.G, Color.Green.B);
                    Main.PlaySound(SoundID.MaxMana, player.position, 1);
                    mPlayer.magnetActive = 1;
                    radius = mPlayer.magnetMinGrabRadius;
                    mPlayer.UpdateMagnetValues(mPlayer, mPlayer.magnetMinGrabRadius);
                    radius = mPlayer.magnetGrabRadius;
                    divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5; //duplicate because need updated value
                    //Main.NewText("grab radius after update: " + mPlayer.magnetGrabRadius);
                    DrawRectangle(mPlayer, mPlayer.magnetGrabRadius * 16, new Color(128, 255, 128));

                    string ranges = "range:" + radius;
                    if (radius + divider > mPlayer.magnetMaxGrabRadius)
                    {
                        ranges += "| next:off";
                    }
                    else
                    {
                        ranges += "| next:" + (radius + divider);
                    }

                    //Main.NewText("radius " + radius);
                    //Main.NewText("divider " + divider);
                    CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.HealLife, ranges);
                }
                else
                {
                    radius += divider;

                    if (radius > mPlayer.magnetMaxGrabRadius)
                    {
                        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.DamagedFriendly, "magnet off");
                        //Main.NewText("deactivated Magnet", Color.Red.R, Color.Red.G, Color.Red.B);
                        Main.PlaySound(SoundID.MaxMana, player.position, 1);
                        player.ClearBuff(mod.BuffType("ItemMagnetBuff"));
                        //DrawRectangle(mPlayer, 16 * 1, new Color(255, 128, 128));

                        SendMagnetData(mPlayer);

                        for (int j = 0; j < 400; j++)
                        {
                            if (Main.item[j].beingGrabbed)
                            {
                                Main.NewText("reset item " + Main.item[j].Name);
                                Main.item[j].beingGrabbed = false;
                            }
                        }
                        return true;
                    }

                    mPlayer.UpdateMagnetValues(mPlayer, radius);
                    DrawRectangle(mPlayer, mPlayer.magnetGrabRadius * 16, new Color(128, 255, 128));

                    //here radius is already + divider
                    string ranges = "range:" + radius;
                    if (radius + divider > mPlayer.magnetMaxGrabRadius)
                    {
                        ranges += "| next:off";
                    }
                    else
                    {
                        ranges += "| next:" + (radius + divider);
                    }
                    CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), new Color(128, 255, 128), ranges);
                    //Main.NewText("divider " + divider);
                    //Main.NewText("steps " + steps);
                    //Main.NewText("Radius " + radius);
                    //Main.NewText("mPlayer.magnetGrabRadius " + mPlayer.magnetGrabRadius);
                }

                SendMagnetData(mPlayer);

            }

            return true;
        }
    }
}