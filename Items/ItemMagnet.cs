using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ItemMagnetPlus.Items
{
    public class ItemMagnet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Item Magnet");
            Tooltip.SetDefault("Left Click to [c/80FF80:change range ]" + "\nRight Click to [c/9999FF:show current range ]");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 32;
            item.scale = 1f;
            item.value = 100;
            item.rare = 2;
            item.useAnimation = 10;
            item.useTime = 10;
            item.useStyle = 4;
            item.consumable = false;
            //item.buffType = mod.BuffType("ItemMagnetBuff");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.HasBuff(mod.BuffType("ItemMagnetBuff")) || Main.LocalPlayer.GetModPlayer<ItemMagnetPlusPlayer>(mod).magnetActive == 1)
            {
                ItemMagnetPlusPlayer mPlayer = Main.LocalPlayer.GetModPlayer<ItemMagnetPlusPlayer>(mod);
                mPlayer.UpdateMagnetValues(mPlayer, mPlayer.magnetGrabRadius);
                tooltips.Add(new TooltipLine(mod, "Range", "Current Range: " + mPlayer.magnetGrabRadius));
                tooltips.Add(new TooltipLine(mod, "Velocity", "Current Velocity: " + mPlayer.magnetVelocity));
                tooltips.Add(new TooltipLine(mod, "Acceleration", "Current Acceleration: " + mPlayer.magnetAcceleration));
            }
            else if (Main.LocalPlayer.HasItem(mod.ItemType("ItemMagnet")))
            {
                tooltips.Add(new TooltipLine(mod, "Range", "Magnet is off"));
            }
            //If player has buff, then he automatically also has the item
            //If player doesn't have the buff, he can still have the item, just not activated
        }

        public override void AddRecipes()
        {
            //6 iron / lead, 1 sapphire, 1 ruby
            //ModRecipe recipe = new ModRecipe(mod);
            //recipe.AddRecipeGroup("IronBar", 6);
            //recipe.AddIngredient(ItemID.Sapphire, 1);
            //recipe.AddIngredient(ItemID.Ruby, 1);
            //recipe.AddTile(TileID.Anvils);
            //recipe.SetResult(this, 1);
            //recipe.AddRecipe();

            //12 iron/lead
            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddRecipeGroup("IronBar", 12);
            recipe2.AddTile(TileID.Anvils);
            recipe2.SetResult(this, 1);
            recipe2.AddRecipe();
        }

        public override bool CanUseItem(Player player)
        {
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
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
            color = new Color(color.R + stage * 30, color.G, color.B - stage * 30);
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
            QuickDustLine(new Vector2(rightx, topy), new Vector2(rightx, boty), fullhdradius / 16f, color);
            QuickDustLine(new Vector2(rightx, boty), new Vector2(leftx, boty), radius / 16f, color);
            QuickDustLine(new Vector2(leftx, boty), new Vector2(leftx, topy), fullhdradius / 16f, color);
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
            mPlayer.UpdateMagnetValues(mPlayer, mPlayer.magnetGrabRadius);

            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                //Main.NewText("alt "+ player.altFunctionUse);

                //right click feature only shows the range
                if (player.altFunctionUse == 2)
                {
                    if(mPlayer.magnetActive == 0)
                    {
                        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.DamagedFriendly, "magnet is off");
                    }
                    else if(player.HasBuff(mod.BuffType("ItemMagnetBuff")))
                    {
                        DrawRectangle(mPlayer, mPlayer.magnetGrabRadius * 16, CombatText.HealMana);
                        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.HealMana, "range:" + mPlayer.magnetGrabRadius);
                    }
                    else
                    {
                        mPlayer.DeactivateMagnet(player);
                        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.DamagedFriendly, "magnet off");
                    }
                }
                else //if (player.altFunctionUse != 2)
                {
                    int divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5;
                    //int steps = (mPlayer.magnetMaxGrabRadius - divider) / divider;
                    int radius = mPlayer.magnetGrabRadius;

                    if (mPlayer.magnetActive == 0)
                    {
                        mPlayer.ActivateMagnet(player);

                        //CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.HealLife, "magnet on");
                        //Main.NewText("activated Magnet", Color.Green.R, Color.Green.G, Color.Green.B);
                        Main.PlaySound(SoundID.MaxMana, player.position, 1);
                        mPlayer.magnetActive = 1;
                        mPlayer.UpdateMagnetValues(mPlayer, mPlayer.magnetMinGrabRadius);
                        radius = mPlayer.magnetGrabRadius;
                        divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5; //duplicate because need updated value
                        //Main.NewText("grab radius after update: " + mPlayer.magnetGrabRadius);
                        DrawRectangle(mPlayer, mPlayer.magnetGrabRadius * 16, new Color(200, 255, 200));

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
                            mPlayer.DeactivateMagnet(player);
                            //DrawRectangle(mPlayer, 16 * 1, new Color(255, 128, 128));

                            SendMagnetData(mPlayer);

                            for (int j = 0; j < 400; j++)
                            {
                                if (Main.item[j].beingGrabbed)
                                {
                                    //Main.NewText("reset item " + Main.item[j].Name);
                                    Main.item[j].beingGrabbed = false;
                                }
                            }
                            return true;
                        }

                        mPlayer.UpdateMagnetValues(mPlayer, radius);
                        DrawRectangle(mPlayer, mPlayer.magnetGrabRadius * 16, new Color(200, 255, 200));

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
                }

                SendMagnetData(mPlayer);
            }

            return true;
        }
    }
}