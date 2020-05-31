using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ItemMagnetPlus.Buffs;
using ItemMagnetPlus.Dusts;

namespace ItemMagnetPlus.Items
{
    public class ItemMagnet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Item Magnet");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 32;
            item.value = Item.sellPrice(silver: 36);
            item.rare = ItemRarityID.Green;
            item.useAnimation = 10;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.HoldingUp;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ItemMagnetPlusPlayer mPlayer = Main.LocalPlayer.GetModPlayer<ItemMagnetPlusPlayer>();

            float alpha = Main.mouseTextColor / 255f;
            string color1 = (new Color(128, 255, 128) * alpha).Hex3();
            string color2 = (new Color(159, 159, 255) * alpha).Hex3();
            string color3 = (new Color(255, 128, 128) * alpha).Hex3();
            tooltips.Add(new TooltipLine(mod, "Buffa", "Left Click to " + (Config.Instance.Scale == Config.ScaleModeBosses ? "[c/" + color1 + ":change range ]" : "[c/" + color1 + ":toggle on/off ]")));
            tooltips.Add(new TooltipLine(mod, "Buffb", "Right Click to " + (Config.Instance.Buff ? "[c/" + color2 + ":show current range ]" : "[c/" + color3 + ":turn off ]")));

            if (Main.LocalPlayer.HasBuff(mod.BuffType("ItemMagnetBuff")) || mPlayer.magnetActive == 1)
            {
                mPlayer.UpdateMagnetValues(mPlayer.magnetGrabRadius);
                tooltips.Add(new TooltipLine(mod, "Range", "Current Range: " + mPlayer.magnetGrabRadius));
                tooltips.Add(new TooltipLine(mod, "Velocity", "Current Velocity: " + mPlayer.magnetVelocity));
                tooltips.Add(new TooltipLine(mod, "Acceleration", "Current Acceleration: " + mPlayer.magnetAcceleration));
            }
            else if (Main.LocalPlayer.HasItem(item.type))
            {
                tooltips.Add(new TooltipLine(mod, "Range", "Magnet is off"));
            }
            // If player has buff, then he automatically also has the item
            // If player doesn't have the buff, he can still have the item, just not activated
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddRecipeGroup("IronBar", 12);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public static Dust QuickDust(Vector2 pos, Color color)
        {
            int type = ModContent.DustType<ColorableDustAlphaFade>();
            Dust dust = Dust.NewDustDirect(pos, 4, 4, type, 0f, 0f, 100, color, 1f);
            dust.customData = new InAndOutData(reduceScale: false);
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

        public static void DrawRectangle(Player player, int radius, Color color)
        {
            float fullhdradius = radius * 0.5625f;

            Vector2 pos = player.position;
            float leftx = pos.X - radius;
            float topy = pos.Y - fullhdradius;
            float rightx = leftx + player.width + radius * 2;
            float boty = topy + player.height + fullhdradius * 2;

            QuickDustLine(new Vector2(leftx, topy), new Vector2(rightx, topy), radius / 16f, color); //clock wise starting top left
            QuickDustLine(new Vector2(rightx, topy), new Vector2(rightx, boty), fullhdradius / 16f, color);
            QuickDustLine(new Vector2(rightx, boty), new Vector2(leftx, boty), radius / 16f, color);
            QuickDustLine(new Vector2(leftx, boty), new Vector2(leftx, topy), fullhdradius / 16f, color);
        }

        public override bool UseItem(Player player)
        {
            ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();
            mPlayer.UpdateMagnetValues(mPlayer.magnetGrabRadius);

            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                // Right click feature only shows the range
                if (player.altFunctionUse == 2)
                {
                    if(mPlayer.magnetActive == 0)
                    {
                        // Nothing
                        CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, "magnet is off");
                    }
                    else if(Config.Instance.Buff && player.HasBuff(ModContent.BuffType<ItemMagnetBuff>()))
                    {
                        // Shows the range
                        DrawRectangle(player, mPlayer.magnetGrabRadius * 16, CombatText.HealMana);
                        CombatText.NewText(player.getRect(), CombatText.HealMana, "range:" + mPlayer.magnetGrabRadius);
                    }
                    else
                    {
                        // Deactivates
                        mPlayer.DeactivateMagnet(player);
                        CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, "magnet off");
                    }
                }
                else //if (player.altFunctionUse != 2)
                {
                    int divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5;
                    int radius = mPlayer.magnetGrabRadius;

                    if (mPlayer.magnetActive == 0)
                    {
                        mPlayer.ActivateMagnet();

                        Main.PlaySound(SoundID.MaxMana, player.Center, 1);
                        mPlayer.magnetActive = 1;
                        mPlayer.UpdateMagnetValues(mPlayer.magnetMinGrabRadius);
                        radius = mPlayer.magnetGrabRadius;
                        divider = (Main.hardMode || mPlayer.magnetGrabRadius >= mPlayer.magnetScreenRadius) ? 10 : 5; //duplicate because need updated value
                        DrawRectangle(player, mPlayer.magnetGrabRadius * 16, new Color(200, 255, 200));

                        string ranges = "range:" + radius;
                        if (radius + divider > mPlayer.magnetMaxGrabRadius)
                        {
                            ranges += "| next:off";
                        }
                        else
                        {
                            ranges += "| next:" + (radius + divider);
                        }
                        
                        CombatText.NewText(player.getRect(), CombatText.HealLife, ranges);
                    }
                    else
                    {
                        radius += divider;

                        if (radius > mPlayer.magnetMaxGrabRadius)
                        {
                            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, "magnet off");
                            Main.PlaySound(SoundID.MaxMana, player.Center, 1);
                            mPlayer.DeactivateMagnet(player);
                            return true;
                        }

                        mPlayer.UpdateMagnetValues(radius);
                        DrawRectangle(player, mPlayer.magnetGrabRadius * 16, new Color(200, 255, 200));

                        // Here radius is already + divider
                        string ranges = "range:" + radius;
                        if (radius + divider > mPlayer.magnetMaxGrabRadius)
                        {
                            ranges += "| next:off";
                        }
                        else
                        {
                            ranges += "| next:" + (radius + divider);
                        }
                        CombatText.NewText(player.getRect(), new Color(128, 255, 128), ranges);
                    }
                }
            }
            return true;
        }
    }
}
