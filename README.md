# ItemMagnetPlus

![Banner](https://raw.githubusercontent.com/direwolf420/ItemMagnetPlus/master/banner.png)

Terraria Forum link: https://forums.terraria.org/index.php?threads/itemmagnetplus-customizable-item-magnet.74425/

ItemMagnetPlus adds a single item called "Item Magnet" that does the obvious thing: Sucking in items around you so you don't have to run around and collect them yourself.

Adds:
Item Magnet (and corresponding buff) that
* can be crafted with simple materials (Iron/Lead)
* works while in your inventory (doesn't waste an accessory slot)
* can be toggled on and off
* has adjustable range and item succ speed
* has increased stats after killing bosses
* customizable via config (\Documents\My Games\Terraria\ModLoader\Mod Configs) to fit your playstyle

How to use:
* Left click cycles through its ranges
* Right click shows current range (you can always check the stats on the buff tooltip)
* This right click functionality changes to "turn off" when "buff" flag in the config is set to 0
* Killing bosses improves its stats
* If you want it to either be off or on, there is a config entry called "scale", set it to 0
* If you plan on using this in multiplayer, make sure that everyone has the same config

Progression: (default config)

 Starts with:

 Range = 10 (Blocks in each direction)

 Velocity = 8

 Acceleration = 8


Ends with: (killing Moonlord)

 Range = 120 (one and a half screens)

 Velocity = 32

 Acceleration = 32


 About the config:
* Buff decides if it gives you a corresponding buff icon to show the status of the magnet
* Filter decides what do ignore when using the magnet (only hearts, mana stars and coins supported for now)
* Range starts from 10, but can be as big as you want
* If you increase Vel or Acc too much from those recommended above, items might get "stuck" on you until you deactivate it again
* Beware of lag when increasing these values, especially range
* If the difference between velocity and acceleration is too big, items will go in circles around you or get stuck until you deactivate it
* If you change the version number, your config might get reset, so don't touch it 


 Changelog:

 v0.1.4.1 + 2 + 3 + 4: filter hotfix, fix stuck range when using magnet inside inventory, another filter hotfix
 
 v0.1.4: Added blacklist "filter" to be able to filter hearts, mana stars and coins (for now)

 v0.1.3: Added config flag "buff" to be able to decide if a buff should be applied while the magnet is active

 v0.1.2: Added Buff- and Tooltip to show range, updated icons (Thanks to Harblesnargits!)

 v0.1.1: Fixed incompatibility with Even More Modifiers, changed acceleration values (updating will set it back to default)

 v0.1: Initial release
