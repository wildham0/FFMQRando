using RomUtilities;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Security.Cryptography;


namespace FFMQLib
{
	public class Flags
	{
		public EnemiesDensity EnemiesDensity { get; set; } = EnemiesDensity.All;
		public ItemShuffleChests ChestsShuffle { get; set; } = ItemShuffleChests.Prioritize;
		public bool ShuffleBoxesContent { get; set; } = false;
		public ItemShuffleNPCsBattlefields NpcsShuffle { get; set; } = ItemShuffleNPCsBattlefields.Prioritize;
		public ItemShuffleNPCsBattlefields BattlefieldsShuffle { get; set; } = ItemShuffleNPCsBattlefields.Prioritize;
		public LogicOptions LogicOptions { get; set; } = LogicOptions.Standard;
		public bool ShuffleEnemiesPosition { get; set; } = false;
		public EnemiesScaling EnemiesScalingLower { get; set; } = EnemiesScaling.Normal;
		public EnemiesScaling EnemiesScalingUpper { get; set; } = EnemiesScaling.Normal;
		public EnemiesScaling BossesScalingLower { get; set; } = EnemiesScaling.Normal;
		public EnemiesScaling BossesScalingUpper { get; set; } = EnemiesScaling.Normal;
		public EnemizerAttacks EnemizerAttacks { get; set; } = EnemizerAttacks.Normal;
		public EnemizerGroups EnemizerGroups
		{
			get => EnemizerAttacks == EnemizerAttacks.Normal ? EnemizerGroups.MobsOnly : internalEnemizerGroups;
			set => internalEnemizerGroups = value;
		}
        public bool ShuffleResWeakType { get; set; } = false;
		public LevelingCurve LevelingCurve { get; set; } = LevelingCurve.Normal;
		public LevelingType CompanionLevelingType { get; set; } = LevelingType.Quests;
		public SpellbookType CompanionSpellbookType { get; set; } = SpellbookType.Standard;
		public StartingCompanionType StartingCompanion { get; set; } = StartingCompanionType.None;
        public AvailableCompanionsType AvailableCompanions { get; set; } = AvailableCompanionsType.Four;
		public CompanionsLocationType CompanionsLocations { get; set; } = CompanionsLocationType.Standard;
        public bool KaelisMomFightMinotaur { get; set; } = false;
        public BattlesQty BattlesQuantity { get; set; } = BattlesQty.Ten;
		public bool ShuffleBattlefieldRewards { get; set; } = false;
		public bool RandomStartingWeapon { get; set; } = false;
		public bool ProgressiveGear { get; set; } = false;
		public bool TweakedDungeons { get; set; } = false;
		public DoomCastleModes DoomCastleMode { get; set; } = DoomCastleModes.Standard;
		public bool DoomCastleShortcut { get; set; } = false;
		public SkyCoinModes SkyCoinMode { get; set; } = SkyCoinModes.Standard;
		public SkyCoinFragmentsQty SkyCoinFragmentsQty {
			get => SkyCoinMode == SkyCoinModes.ShatteredSkyCoin ? internalSkyCoinFragmentsQty : SkyCoinFragmentsQty.Mid24;
			set => internalSkyCoinFragmentsQty = value; }
		public bool DisableSpoilers { get; set; } = false;
		public ProgressiveFormationsModes ProgressiveFormations { get; set; } = ProgressiveFormationsModes.Disabled;
		public MapShufflingMode MapShuffling { get; set; } = MapShufflingMode.None;
		public bool CrestShuffle { get; set; } = false;
        //public bool KaelisMomIsKaeli { get; set; } = false;

    private SkyCoinFragmentsQty internalSkyCoinFragmentsQty = SkyCoinFragmentsQty.Mid24;
    private EnemizerGroups internalEnemizerGroups = EnemizerGroups.MobsOnly;

    public string GenerateFlagString()
		{
			var flaglist = this.GetType().GetProperties();
			var orderedflaglist = flaglist.OrderBy(x => x.Name).ToList();
			long flagstrinvalue = 0;
			foreach (var flag in orderedflaglist)
			{
				if (flag.PropertyType == typeof(bool))
				{
					flagstrinvalue *= 2;
					flagstrinvalue += Convert.ToInt32(flag.GetValue(this, null));
				}
				else if (flag.PropertyType.IsEnum)
				{
					var specificenum = flag.PropertyType.GetEnumNames();
					flagstrinvalue *= specificenum.Count();
					flagstrinvalue += (int)flag.GetValue(this, null);
				}
			}

			var actualbytes = BitConverter.GetBytes(flagstrinvalue);
			string flagstring = Convert.ToBase64String(BitConverter.GetBytes(flagstrinvalue).Concat(new byte[] { 0 }).ToArray());
			return flagstring.Replace('+', '-').Replace('/', '_').Replace('=', '~');
		}

		public Blob EncodedFlagString()
		{
			return Encoding.UTF8.GetBytes(GenerateFlagString());
		}

		public void ReadFlagString(string flagstring)
		{
			flagstring = flagstring.Replace('-', '+').Replace('_', '/').Replace('~', '=');
			var flaglist = this.GetType().GetProperties();
			var orderedflaglist = flaglist.OrderByDescending(x => x.Name).ToList();
			long numflagstring = BitConverter.ToInt64(Convert.FromBase64String(flagstring), 0);

			foreach (var flag in orderedflaglist)
			{
				if (flag.PropertyType == typeof(bool))
				{
					var value = numflagstring % 2;
					flag.SetValue(this, Convert.ToBoolean(value));
					numflagstring /= 2;
				}
				else if (flag.PropertyType.IsEnum)
				{
					var enumValues = flag.PropertyType.GetEnumValues();
					var value = numflagstring % enumValues.Length;
					foreach (var enumValue in enumValues)
					{
						if (Convert.ToInt32(enumValue) == value)
						{
							flag.SetValue(this, enumValue);
						}
					}
					
					numflagstring /= enumValues.Length;
				}
			}
		}

		public void FlagSanityCheck()
		{
			/*
			// Throw an error if the settings don't offer enough LocationIds.
			if ((NpcsShuffle == ItemShuffleNPCsBattlefields.Exclude || BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Exclude) && BoxesShuffle == ItemShuffleBoxes.Exclude)
			{
				throw new Exception("Selected flags don't allow enough locations to place all Quest Items. Change flags to include more Locations.");
			}
			
			// Throw an error if the settings don't offer enough LocationIds.
			if (SkyCoinMode == SkyCoinModes.ShatteredSkyCoin && BoxesShuffle == ItemShuffleBoxes.Exclude)
			{
				throw new Exception("Selected flags don't allow enough locations to place all Sky Coin Fragments. Set Brown Boxes to Include.");
			}*/
		}
		public string GenerateYaml(string name)
		{
			var serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();
			var yaml = serializer.Serialize(this);
			var lineyaml = yaml.Split('\n');

			var flagList = this.GetType().GetProperties().ToList();

			Dictionary<YamlNode, YamlNode> workingNodes = new();

			for (int i = 0; i < flagList.Count; i++)
			{
				Dictionary<YamlNode, YamlNode> tempNode = new();

				if (flagList[i].PropertyType == typeof(bool))
				{
					tempNode.Add(new YamlScalarNode("true"), new YamlScalarNode((bool)flagList[i].GetValue(this, null) ? "1" : "0"));
					tempNode.Add(new YamlScalarNode("false"), new YamlScalarNode((bool)flagList[i].GetValue(this, null) ? "0" : "1"));
				}
				else if (flagList[i].PropertyType.IsEnum)
				{
					foreach (var item in flagList[i].PropertyType.GetEnumNames())
					{
						tempNode.Add(new YamlScalarNode(item), new YamlScalarNode(Enum.GetName(flagList[i].PropertyType, flagList[i].GetValue(this, null)) == item ? "1" : "0"));
					}
				}

				workingNodes.Add(new YamlScalarNode(lineyaml[i].Split(':')[0]), new YamlMappingNode(tempNode));
			}

			var root = new YamlMappingNode(new List<KeyValuePair<YamlNode, YamlNode>>() {
				new KeyValuePair<YamlNode, YamlNode>(new YamlScalarNode("Final Fantasy Mystic Quest"), new YamlMappingNode(workingNodes)),
				new KeyValuePair<YamlNode, YamlNode>(new YamlScalarNode("description"), new YamlScalarNode("Generated by https://ffmqrando.net")),
				new KeyValuePair<YamlNode, YamlNode>(new YamlScalarNode("game"), new YamlScalarNode("Final Fantasy Mystic Quest")),
				new KeyValuePair<YamlNode, YamlNode>(new YamlScalarNode("name"), new YamlScalarNode(name)),
			});

			string finalYaml = serializer.Serialize(new YamlDocument(root).RootNode);

			return "# YAML Preset file for FFMQR\n" + finalYaml;
		}
		public Stream YamlStream(string name)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(GenerateYaml(name));
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
		public string ReadYaml(string yamlfile, Blob seed)
		{
			MT19337 rng;
			using (SHA256 hasher = SHA256.Create())
			{
				Blob hash = hasher.ComputeHash(seed);
				rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
			}

			var input = new StringReader(yamlfile);

			var yaml = new YamlStream();
			yaml.Load(input);

			var mapping = (YamlMappingNode)((YamlMappingNode)yaml.Documents[0].RootNode).Children["Final Fantasy Mystic Quest"];

			string newyaml = "";

			foreach (var entry in mapping.Children)
			{
				if (entry.Value.NodeType == YamlNodeType.Mapping)
				{
					List<string> weightedChildren = new();
					foreach (var child in (YamlMappingNode)entry.Value)
					{
						int weight = Convert.ToInt32(((YamlScalarNode)child.Value).Value);
						string childname = ((YamlScalarNode)child.Key).Value;

						weightedChildren.AddRange(Enumerable.Repeat(childname, weight));
					}

					if (weightedChildren.Any())
					{
						newyaml += ((YamlScalarNode)entry.Key).Value + ": " + rng.PickFrom(weightedChildren) + "\n";
					}
					else
					{
						Console.WriteLine("Yaml Error: No weighted options for:" + ((YamlScalarNode)entry.Key).Value);
					}
				}
				else if (entry.Value.NodeType == YamlNodeType.Scalar)
				{
					newyaml += ((YamlScalarNode)entry.Key).Value + ": " + ((YamlScalarNode)entry.Value).Value + "\n";
				}
			}

			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
				.Build();

			Flags result = new();
			try
			{
				result = deserializer.Deserialize<Flags>(newyaml);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			var yamlFlags = result.GetType().GetProperties().ToList();

			foreach (var flag in yamlFlags)
			{
				flag.SetValue(this, flag.GetValue(result, null));
			}

			var mapping2 = (YamlScalarNode)((YamlMappingNode)yaml.Documents[0].RootNode).Children["name"];
			return ((YamlScalarNode)mapping2.Value).Value;
		}
	}

	public class Preferences
	{
		public bool RandomBenjaminPalette { get; set; } = false;
		public MusicMode MusicMode { get; set; } = MusicMode.Normal;
        public bool DarkKingTrueForm { get; set; } = false;
        public ushort WindowPalette { get; set; } = 0x5140;
		public bool DumpGameInfoScreen { get; set; } = false;
		public bool AutoDownloadRom { get; set; } = false;
		public string PlayerSprite { get; set; } = "default";	
		public byte[] CustomSprites { get; set; } = new byte[0];
		public byte[] DarkKing3 { get; set; } = new byte[0];
        public byte[] DarkKing4 { get; set; } = new byte[0];

        public void ValidateCustomSprites()
		{
			if (CustomSprites[0x00] != 0x42 || CustomSprites[0x01] != 0x4D)
			{
				throw new Exception("Not BMP image format.");
			}

			if (CustomSprites[0x1C] != 0x08)
			{
				throw new Exception("Wrong bitdepth. Bitdepth must be 8 bits.");
			}
		}
	}

}
