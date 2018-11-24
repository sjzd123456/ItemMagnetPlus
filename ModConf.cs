using System;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace ItemMagnetPlus
{
    // Tutorial by goldenapple: https://forums.terraria.org/index.php?threads/modders-guide-to-config-files-and-optional-features.48581/
    public static class ModConf
    {
        public const int configVersion = 1;
        private readonly static string modName = "ItemMagnetPlus";

        //private const string scaleField = "scale";
        //internal static int scale = 10;
        //public static int Scale
        //{
        //    get
        //    {
        //        if (scale <= 0)
        //        {
        //            return 1;
        //        }
        //        return scale;
        //    }
        //}

        private const string velocityField = "velocity";
        internal static int velocity = 8;
        public static int Velocity
        {
            get
            {
                return velocity;
            }
        }

        private const string accelerationField = "acceleration";
        internal static int acceleration = 30;
        public static int Acceleration
        {
            get
            {
                if (acceleration <= 0)
                {
                    return 1;
                }
                return acceleration;
            }
        }

        static readonly string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs/" + modName + ".json");

        static Preferences ModConfig = new Preferences(ConfigPath);

        internal static void Load()
        {
            bool success = ReadConfig();
            if (!success)
            {
                ErrorLogger.Log("ItemMagnetPlus: Couldn't load config file, creating new file.");
                CreateConfig();
            }
        }

        // true if loaded successfully
        internal static bool ReadConfig()
        {
            if (ModConfig.Load())
            {
                int readVersion = 0;
                ModConfig.Get("version", ref readVersion);
                if (readVersion != configVersion)
                {
                    bool canUpdate = false;
                    if (readVersion == 0)
                    {
                        ErrorLogger.Log("ItemMagnetPlus: updated Version");
                        canUpdate = true;
                        ModConfig.Put("version", 1);
                        ModConfig.Put(accelerationField, acceleration);
                        ModConfig.Save();
                    }

                    if (!canUpdate) return false;
                }

                //ModConfig.Get(scaleField, ref scale);
                ModConfig.Get(velocityField, ref velocity);
                ModConfig.Get(accelerationField, ref acceleration);

                return true;
            }
            return false;
        }

        // Create a new config file for the player to edit. 
        internal static void CreateConfig()
        {
            ModConfig.Clear();
            ModConfig.Put("version", configVersion);

            //ModConfig.Put(scaleField, scale);
            ModConfig.Put(velocityField, velocity);
            ModConfig.Put(accelerationField, acceleration);

            ModConfig.Put("readme", "First off, make sure to reload before the configs will take any effect. Scale: factor for how big the radius of the magnet is (default 10, currently unsupported), Velocity: how fast items move towards you (default 16), Acceleration: how fast items speed up when moving towards you (default 20). WARNING: Clients will desync if their local config is different to the server - this cannot be fixed without forcing the clients to download the server's mods and forcing the mods to reload. So don't mess with this too much outside of singleplayer unless you know what you're doing. And no I'm too lazy to find out how to even fix this behaviour, though a simple server mismatch warning might be a good idea. Feel free to delete this.");

            ModConfig.Save();
        }
    }
}
