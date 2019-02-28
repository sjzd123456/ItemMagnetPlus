using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ItemMagnetPlus.Items
{
    //public class IMPGlobalItem : GlobalItem //unused
    //{
    //    public IMPGlobalItem()
    //    {

    //    }

    //    public byte? closestPlayer = 0;

    //    public override bool InstancePerEntity
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public override GlobalItem Clone(Item item, Item itemClone)
    //    {
    //        IMPGlobalItem myClone = (IMPGlobalItem)base.Clone(item, itemClone);
    //        return myClone;
    //    }

    //    private byte? ClosestPlayerForItem(Item item)
    //    {
    //        byte? closestPlayerTemp = null;

    //        float oldDistance = 16000;
    //        float newDistance = 16000;
    //        for (byte i = 0; i < 255; i++)
    //        {
    //            if (Main.player[i].active)
    //            {
    //                Player player = Main.player[i];
    //                ItemMagnetPlusPlayer mPlayer = player.GetModPlayer<ItemMagnetPlusPlayer>();

    //                if (mPlayer.currentlyActive)
    //                {
    //                    int grabRadius = (int)(mPlayer.magnetGrabRadius * 16); //16 == to world coordinates
    //                    int fullhdgrabRadius = (int)(grabRadius * 0.5625f);
    //                    Rectangle rect = new Rectangle((int)player.position.X - grabRadius, (int)player.position.Y - fullhdgrabRadius, player.width + grabRadius * 2, player.height + fullhdgrabRadius * 2);

    //                    if (rect.Intersects(new Rectangle((int)item.position.X, (int)item.position.Y, item.width, item.height)))
    //                    {
    //                        newDistance = Vector2.Distance(player.Center, item.Center);
    //                        if (newDistance < oldDistance)
    //                        {
    //                            oldDistance = newDistance;
    //                            closestPlayerTemp = i;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        return closestPlayerTemp;
    //    }

    //    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    //    {
    //        closestPlayer = ClosestPlayerForItem(item);
    //    }
    //}
}
