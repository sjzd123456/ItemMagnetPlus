using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ItemMagnetPlus
{
#pragma warning disable 0649
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		public const int RangeMin = 10;
		public const int RangeMax = 240;
		public const int VelocityMin = 1;
		public const int VelocityMax = 80;
		public const int AccelerationMin = 1;
		public const int AccelerationMax = 40;

		//Old data and names for reference
		[JsonExtensionData]
		private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();
		
		public const string OldBlacklistName = "Blacklist";
		public const string OldWhitelistName = "Whitelist";

		public const string OldScaleModeBosses = "Increase With Bosses";
		public const string OldScaleModeAlwaysMaxRange = "Bosses + Max Range";
		public const string OldScaleModeOnlyConfig = "Custom + Max Range";

		[Header("PresetItemWhitelist")]

		[DefaultValue(false)]
		public bool Hearts;

		[DefaultValue(false)]
		public bool ManaStars;

		[DefaultValue(true)]
		public bool Coins;

		[DefaultValue(false)]
		public bool PickupEffect;

		[Header("CustomItemFilter")]

		[BackgroundColor(30, 30, 30)]
		public List<ItemDefinition> Blacklist = new List<ItemDefinition>();

		[BackgroundColor(220, 220, 220)]
		public List<ItemDefinition> Whitelist = new List<ItemDefinition>();

		public enum FilterListModeType : byte
		{
			Blacklist = 0,
			Whitelist = 1
		}

		[DrawTicks]
		[DefaultValue(FilterListModeType.Blacklist)]
		public FilterListModeType FilterListMode;

		[Header("General")]

		[DefaultValue(true)]
		public bool Buff;

		[DefaultValue(false)]
		public bool Held;

		[DefaultValue(true)]
		public bool OnEnter;

		[DefaultValue(false)]
		public bool NeedsSpace;

		[DefaultValue(false)]
		public bool Instant;

		[Header("MagnetBehavior")]

		[Slider]
		[SliderColor(255, 255, 50)]
		[Range(RangeMin, RangeMax)]
		[Increment(5)]
		[DefaultValue(10)]
		public int Range;

		[Slider]
		[SliderColor(255, 255, 50)]
		[Range(VelocityMin, VelocityMax)]
		[DefaultValue(8)]
		public int Velocity;

		[Slider]
		[SliderColor(255, 255, 50)]
		[Range(AccelerationMin, AccelerationMax)]
		[DefaultValue(8)]
		public int Acceleration;

		public enum ScaleModeType : byte
		{
			Bosses = 0,
			AlwaysMaxRange = 1,
			OnlyConfig = 2
		}

		[SliderColor(255, 255, 50)]
		[DrawTicks]
		[DefaultValue(ScaleModeType.AlwaysMaxRange)]
		public ScaleModeType ScaleMode;

		[Header("ResultingMagnetStats")]

		[Slider]
		[SliderColor(50, 255, 50)]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(RangeMin, RangeMax + 110)]
		public int CurrentMaxRange
		{
			get
			{
				int range = Range;
				UpdateRange(ref range);
				return range;
			}
		}

		[Slider]
		[SliderColor(50, 255, 50)]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(VelocityMin, VelocityMax)]
		public int CurrentVelocity
		{
			get
			{
				int velocity = Velocity;
				UpdateVelocity(ref velocity);
				return velocity;
			}
		}

		[Slider]
		[SliderColor(50, 255, 50)]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		[Range(AccelerationMin, AccelerationMax)]
		public int CurrentAcceleration
		{
			get
			{
				int acceleration = Acceleration;
				UpdateAcceleration(ref acceleration);
				return acceleration;
			}
		}
		public static bool IsPlayerLocalServerOwner(int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			return NetMessage.DoesPlayerSlotCountAsAHost(whoAmI);
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost");
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			//port "ListMode": "Whitelist"
			//and "Scale": "Increase With Bosses"
			//from strings to enums, which requires (!) a member rename aswell
			JToken token;
			if (_additionalData.TryGetValue("ListMode", out token))
			{
				var oldListMode = token.ToObject<string>();
				if (oldListMode == OldWhitelistName)
				{
					FilterListMode = FilterListModeType.Whitelist;
				}
				else
				{
					FilterListMode = FilterListModeType.Blacklist;
				}
			}

			if (_additionalData.TryGetValue("Scale", out token))
			{
				var oldScale = token.ToObject<string>();
				if (oldScale == OldScaleModeBosses)
				{
					ScaleMode = ScaleModeType.Bosses;
				}
				else if (oldScale == OldScaleModeOnlyConfig)
				{
					ScaleMode = ScaleModeType.OnlyConfig;
				}
				else
				{
					ScaleMode = ScaleModeType.AlwaysMaxRange;
				}
			}

			//Correct invalid values to default fallback
			EnumFallback(ref FilterListMode, FilterListModeType.Blacklist);
			EnumFallback(ref ScaleMode, ScaleModeType.AlwaysMaxRange);

			_additionalData.Clear(); // Clear this or it'll crash.

			// Clamp
			Clamp(ref Range, RangeMin, RangeMax);
			Clamp(ref Velocity, VelocityMin, VelocityMax);
			Clamp(ref Acceleration, AccelerationMin, AccelerationMax);
			if (Main.netMode == NetmodeID.Server) Buff = true; // Enforce buff in multiplayer
		}

		private static void EnumFallback<T>(ref T value, T defaultValue) where T : Enum
		{
			if (!Enum.IsDefined(typeof(T), value))
			{
				value = defaultValue;
			}
		}

		// This is stupid don't do this
		public void UpdateRange(ref int range)
		{
			int a = 0;
			int b = 0;
			Update(ScaleMode, ref range, ref a, ref b);
			Clamp(ref range, RangeMin, RangeMax + 110);
		}

		public void UpdateVelocity(ref int velocity)
		{
			int a = 0;
			int b = 0;
			Update(ScaleMode, ref a, ref velocity, ref b);
			Clamp(ref velocity, VelocityMin, VelocityMax);
		}

		public void UpdateAcceleration(ref int acceleration)
		{
			int a = 0;
			int b = 0;
			Update(ScaleMode, ref a, ref b, ref acceleration);
			Clamp(ref acceleration, AccelerationMin, AccelerationMax);
		}

		public static void Clamp(ref int value, int min, int max)
		{
			value = value < min ? min : (value > max ? max : value);
		}

		public static void Update(ScaleModeType scale, ref int range, ref int velocity, ref int acceleration)
		{
			// Input as default values into here
			if (scale == ScaleModeType.OnlyConfig)
			{
				//magnetGrabRadius = range;
				return;
			}
			if (NPC.downedSlimeKing)
			{
				//Starts at
				//range = 10;
				//velocity = 8;
				//acceleration = 8;

				velocity += 4;
				acceleration += 2;
			}
			if (NPC.downedBoss1) //Eye of Cthulhu
			{
				range += 5;
			}
			if (NPC.downedBoss2) //Eater/Brain
			{
				range += 5;
			}
			if (NPC.downedQueenBee)
			{
				velocity += 2;
				acceleration += 4;
			}
			if (NPC.downedBoss3) //Skeletron
			{
				range += 5;
			}
			if (NPC.downedDeerclops)
			{
				velocity += 2;
				acceleration += 4;
			}
			if (Main.hardMode) //Wall of flesh
			{
				range += 5;

				//Ideal at
				//range = 30; //quarter screen
				//velocity = 18;
				//acceleration = 18;
			}
			if (NPC.downedQueenSlime)
			{
				velocity += 2;
				acceleration += 2;
			}
			if (NPC.downedMechBoss1) //Destroyer
			{
				range += 10;
			}
			if (NPC.downedMechBoss2) //Twins
			{
				range += 10;
			}
			if (NPC.downedMechBoss3) //Skeletron prime
			{
				range += 10;
			}
			if (NPC.downedPlantBoss)
			{
				range += 10;
				velocity += 2;
				acceleration += 2;
			}
			if (NPC.downedGolemBoss)
			{
				range += 10;
				velocity += 2;
			}
			if (NPC.downedEmpressOfLight)
			{
				velocity += 2;
				acceleration += 2;
			}
			if (NPC.downedFishron)
			{
				range += 10;
				velocity += 2;
				acceleration += 2;
			}
			if (NPC.downedAncientCultist)
			{
				range += 10;
			}
			if (NPC.downedMoonlord)
			{
				range += 20;
				velocity += 4;
				acceleration += 6;

				//Final at
				//range = 120; //one screen
				//magnetVelocity = 32;
				//acceleration = 32;
			}
		}
	}

	public class ConfigWrapper : ModSystem
	{
		public static Config Instance { get; private set; }

		public static int[] HeartTypes { get; private set; }

		public static int[] ManaStarTypes { get; private set; }

		public static int[] CoinTypes { get; private set; }

		/// <summary>
		/// If something matches the presets, then let the configuration control the status, otherwise fall back to white/blacklist
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		private static bool? CheckIfItemIsInPresetWhitelist(Item item, Player player)
		{
			//heart: 58, 184, 1734
			//mana: 1735, 1867, 1868
			//nebula: 3453, 3454, 3455
			//IsAPickup = Factory.CreateBoolSet(58, 184, 1734, 1735, 1867, 1868, 3453, 3454, 3455);
			//But we want custom filters:
			if (Array.BinarySearch(HeartTypes, item.type) > -1)
			{
				return Instance.Hearts;
			}
			if (Array.BinarySearch(ManaStarTypes, item.type) > -1)
			{
				return Instance.ManaStars;
			}
			if (Array.BinarySearch(CoinTypes, item.type) > -1)
			{
				return Instance.Coins;
			}
			if (ItemID.Sets.NebulaPickup[item.type] || ItemMagnetPlus.JPANsLoaded && ItemLoader.ItemSpace(item, player))
			{
				return Instance.PickupEffect;
			}
			return null;
		}

		public static bool CanBePulled(Item item, Player player)
		{
			if (CheckIfItemIsInPresetWhitelist(item, player) is bool presetValue) return presetValue;

			ItemDefinition itemDef = new ItemDefinition(item.type);
			if (Instance.FilterListMode == Config.FilterListModeType.Blacklist)
			{
				return !Instance.Blacklist.Contains(itemDef);
			}
			else if (Instance.FilterListMode == Config.FilterListModeType.Whitelist)
			{
				return Instance.Whitelist.Contains(itemDef);
			}
			return false; //Technically unreachable
		}

		public override void Load()
		{
			Instance = Config.Instance;
		}

		public override void PostSetupContent()
		{
			HeartTypes = new int[] { ItemID.Heart, ItemID.CandyApple, ItemID.CandyCane };
			ManaStarTypes = new int[] { ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum };

			var list = new List<int>();
			for (int i = 0; i < ItemLoader.ItemCount; i++)
			{
				if (ItemID.Sets.CommonCoin[i])
				{
					list.Add(i);
				}
			}
			CoinTypes = list.ToArray();

			Array.Sort(HeartTypes);
			Array.Sort(ManaStarTypes);
			Array.Sort(CoinTypes);
		}

		public override void Unload()
		{
			Instance = null;
			HeartTypes = null;
			ManaStarTypes = null;
			CoinTypes = null;
		}
	}
}
