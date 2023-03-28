using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RomUtilities;
using System.IO;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Security.Cryptography;

namespace FFMQLib
{

    public class ApObject : GameObjectData
    { 
        public Items Content { get; set; }
        public string PlayerName { get; set; }
        public int PlayerId { get; set; }
        public string ItemName { get; set; }

        public ApObject()
        {
            Content = Items.None;
            PlayerName = "";
            PlayerId = 0;
            ItemName = "";
            ObjectId = 0;
            Type = GameObjectType.Dummy;
        }
    }
    
    public class ApConfigs
    { 
        public string ItemPlacementYaml { get; set; }
        public string StartingItemsYaml { get; set; }
        public List<ApObject> ItemPlacement { get; set; }
        public List<Items> StartingItems { get; set; }
        public bool ApEnabled { get; set; }

        public ApConfigs()
        {
            ItemPlacementYaml = "";
            StartingItemsYaml = "";
            ItemPlacement = new();
            StartingItems = new();
            ApEnabled = false;
        }
        public void ProcessItemPlacement()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            try
            {
                ItemPlacement = deserializer.Deserialize<List<ApObject>>(ItemPlacementYaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                StartingItems = deserializer.Deserialize<List<Items>>(StartingItemsYaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    public partial class ItemsPlacement
    {
        public ItemsPlacement(Flags flags, ApConfigs apconfigs, List<GameObject> initialGameObjects, FFMQRom rom, MT19337 rng)
        {
            List<Items> consumableList = rom.GetFromBank(0x01, 0x801E, 0xDD).ToBytes().Select(x => (Items)x).ToList();
            List<Items> finalConsumables = rom.GetFromBank(0x01, 0x80F2, 0x04).ToBytes().Select(x => (Items)x).ToList();

            List<Items> consumables = new() { Items.CurePotion, Items.HealPotion, Items.Refresher, Items.Seed };

            ItemsLocations = new(initialGameObjects.Select(x => new GameObject(x)));
            StartingItems = apconfigs.StartingItems.ToList();

            foreach (var placedObject in apconfigs.ItemPlacement)
            { 
                var currentObject = ItemsLocations.Find(x => x.ObjectId == placedObject.ObjectId && x.Type == placedObject.Type);
                currentObject.Content = placedObject.Content;
                currentObject.IsPlaced = true;
                currentObject.Type = placedObject.Type;
            }

            var unfilledLocations = ItemsLocations.Where(x => x.IsPlaced == false && (x.Type == GameObjectType.NPC || x.Type == GameObjectType.Battlefield || (x.Type == GameObjectType.Chest && x.ObjectId < 0x20))).ToList();

            foreach (var location in unfilledLocations)
            {
                location.Content = rng.PickFrom(consumables);
                location.IsPlaced = true;
                if (location.Type == GameObjectType.Chest || location.Type == GameObjectType.Box)
                {
                    location.Type = GameObjectType.Box;
                }
            }

            // Place consumables
            var consumableBox = ItemsLocations.Where(x => !x.IsPlaced && (x.Type == GameObjectType.Box || x.Type == GameObjectType.Chest) && (x.ObjectId < 0xF2 || x.ObjectId > 0xF5)).ToList();

            foreach (var box in consumableBox)
            {
                if (flags.ShuffleBoxesContent)
                {
                    box.Content = rng.TakeFrom(consumableList);
                }
                else
                {
                    box.Content = consumableList[box.ObjectId - 0x1E];
                }

                box.IsPlaced = true;
                if (box.Type == GameObjectType.Chest || box.Type == GameObjectType.Box)
                {
                    box.Type = GameObjectType.Box;
                }
            }

            // Add the final chests so we can update their properties
            List<GameObject> finalChests = ItemsLocations.Where(x => x.Type == GameObjectType.Chest && x.ObjectId >= 0xF2 && x.ObjectId <= 0xF5).ToList();

            for (int i = 0; i < finalChests.Count; i++)
            {
                finalChests[i].Content = finalConsumables[i];
                finalChests[i].IsPlaced = true;
            }
        }
    }

    public partial class FFMQRom : SnesRom
	{
        public void GenerateFromApConfig(ApConfigs apconfigs, Flags flags, Blob seed, Preferences preferences)
        {
            MT19337 rng;
            MT19337 sillyrng;
            using (SHA256 hasher = SHA256.Create())
            {
                Blob hash = hasher.ComputeHash(seed + flags.EncodedFlagString());
                rng = new MT19337((uint)hash.ToUInts().Sum(x => x));
                sillyrng = new MT19337((uint)hash.ToUInts().Sum(x => x));
            }

            EnemyAttackLinks = new(this);
            Attacks = new(this);
            enemiesStats = new(this);
            GameMaps = new(this);
            MapObjects = new(this);
            Credits credits = new(this);
            GameFlags = new(this);
            TalkScripts = new(this);
            TileScripts = new(this);
            Battlefields = new(this);
            MapChanges = new(this);
            Overworld = new(this);
            Teleporters = new(this);
            MapSpriteSets = new(this);
            GameLogic = new();
            EntrancesData = new(this);
            TitleScreen titleScreen = new(this);

            // General modifications
            ExpandRom();
            FastMovement();
            DefaultSettings();
            RemoveClouds();
            RemoveStrobing();
            SmallFixes();
            BugFixes();
            NonSpoilerDemoplay(flags.MapShuffling != MapShufflingMode.None && flags.MapShuffling != MapShufflingMode.Overworld);
            CompanionRoutines();
            DummyRoom();
            PazuzuFixedFloorRng(rng);
            KeyItemWindow();
            ArchipelagoSupport();

            // AP Configs
            apconfigs.ProcessItemPlacement();

            // Maps Changes
            GameMaps.RandomGiantTreeMessage(rng);
            GameMaps.LessObnoxiousMaps(flags.TweakedDungeons, MapObjects, rng);

            // Enemies
            MapObjects.SetEnemiesDensity(flags.EnemiesDensity, rng);
            MapObjects.ShuffleEnemiesPosition(flags.ShuffleEnemiesPosition, GameMaps, rng);
            EnemyAttackLinks.ShuffleAttacks(flags.EnemizerAttacks, flags.BossesScalingUpper, rng);
            enemiesStats.ScaleEnemies(flags, rng);

            // Overworld
            Overworld.OpenNodes(flags);
            Battlefields.SetBattlesQty(flags.BattlesQuantity, rng);
            Battlefields.SetBattelfieldRewards(flags.ShuffleBattlefieldRewards, apconfigs.ItemPlacement, rng);

            // Locations & Logic
            GameLogic.CrestShuffle(flags.CrestShuffle && !apconfigs.ApEnabled, rng);
            //GameLogic.FloorShuffle(flags.MapShuffling, rng);
            //Overworld.ShuffleOverworld(flags, GameLogic, Battlefields, rng);

            Overworld.UpdateOverworld(flags, Battlefields);

            GameLogic.CrawlRooms(flags, Overworld, Battlefields);

            EntrancesData.UpdateCrests(flags, TileScripts, GameMaps, GameLogic, Teleporters.TeleportersLong, this, rng);
            EntrancesData.UpdateEntrances(flags, GameLogic.Rooms, rng);

            // Items
            ItemsPlacement itemsPlacement = new(flags, apconfigs, GameLogic.GameObjects, this, rng);

            SetStartingWeapons(itemsPlacement);
            MapObjects.UpdateChests(itemsPlacement);
            UpdateScripts(flags, itemsPlacement, Overworld.StartingLocation, rng);
            ChestsHacks(flags, itemsPlacement);
            Battlefields.PlaceItems(itemsPlacement);

            // Doom Castle
            SetDoomCastleMode(flags.DoomCastleMode);
            DoomCastleShortcut(flags.DoomCastleShortcut);

            // Various
            SetLevelingCurve(flags.LevelingCurve);
            ProgressiveGears(flags.ProgressiveGear);
            SkyCoinMode(flags, rng);
            ExitHack(Overworld.StartingLocation);
            ProgressiveFormation(flags.ProgressiveFormations, Overworld, rng);
            credits.Update();

            // Preferences
            Msu1SupportRandom(preferences.RandomMusic, sillyrng);
            RandomBenjaminPalette(preferences.RandomBenjaminPalette, sillyrng);
            WindowPalette(preferences.WindowPalette);

            // Write everything back			
            itemsPlacement.WriteChests(this);
            credits.Write(this);
            EnemyAttackLinks.Write(this);
            Attacks.Write(this);
            enemiesStats.Write(this);
            GameMaps.Write(this);
            MapChanges.Write(this);
            Teleporters.Write(this);
            TileScripts.Write(this);
            TalkScripts.Write(this);
            GameFlags.Write(this);
            EntrancesData.Write(this);
            Battlefields.Write(this);
            Overworld.Write(this);
            MapObjects.Write(this);
            MapSpriteSets.Write(this);
            titleScreen.Write(this, Metadata.Version, seed, flags);


            // Spoilers
            spoilersText = itemsPlacement.GenerateSpoilers(this, titleScreen.versionText, titleScreen.hashText, flags.GenerateFlagString(), seed.ToHex());
            spoilers = flags.EnableSpoilers;

            // Remove header if any
            this.Header = Array.Empty<byte>();

        }
        
        public void ArchipelagoSupport()
		{
			ItemFetcher();
            APItem();
        }
		public void ItemFetcher()
		{
			PutInBank(0x01, 0x82A9, Blob.FromHex("22008015eaea"));
            PutInBank(0x15, 0x8000, Blob.FromHex("eef7199cf81908e220add00ff00fa9018db019a9508dee19a9088def19286b"));

			TileScripts.AddScript(0x50, new ScriptBuilder(new List<string>
			{
				"0FD00F",
				"057F",
				"115F01",
                "0C600101",
                "62",
                "0588D10F",
                "0CD00F00",
				"00"
            }));
        }

        public void APItem()
        {
            // Set sprite bank
            PutInBank(0x00, 0x8525, Blob.FromHex("5c508015eaea"));
            PutInBank(0x15, 0x8050, Blob.FromHex("8d1621ad700029ff00c9f000f007f404005c2b8500f415009c70005c2b85006b"));

            // Set sprite adresss
            PutInBank(0x00, 0xB6B5, Blob.FromHex("5c808015"));
            PutInBank(0x15, 0x8080, Blob.FromHex("9c6500a59e8d7000c9de00b0045cbab600f011a900818562a990038d6400e2305ccbb600a901008562a920008d6400e2305ccbb600"));

            // Set item name
            PutInBank(0x03, 0xB50B, Blob.FromHex("07D080150A13B5"));
            PutInBank(0x15, 0x80D0, Blob.FromHex("0bf0dd80054d0c054320c10c00053df08015"));
            PutInBank(0x15, 0x80F0, Blob.FromHex("03039aa9ffa2c7b8c0030303")); // AP Item

            // New sprite
            PutInBank(0x15, 0x8100, Blob.FromHex("0000010102031c1f3e3f7f7f7f7f7f7f0001031f2341415d")); // First tile
            PutInBank(0x15, 0x8118, Blob.FromHex("0000c0c020e01cfc3ee27fc17fc1ffdd00c0e0fce2c1c1dd")); // Second tile
            PutInBank(0x15, 0x8280, Blob.FromHex("3e227f417e437c473c271c1f020301013e7f7e7c3c1c0201")); // Third tile + 0x180 from first tile
            PutInBank(0x15, 0x8298, Blob.FromHex("2323c1c121e111f112f21cfc20e0c0c03fff3f1f1e1c20c0")); // Fourth tile

            // New Palette
            PutInBank(0x07, 0xDC74, Blob.FromHex("00000f3f9b429e4b79620f62f9410821")); // Palette 0x390

            // Palette hacks
            PutInBank(0x00, 0x8482, Blob.FromHex("22008515ea")); // zero out b part of x register when loading palette selector
            PutInBank(0x00, 0x848A, Blob.FromHex("22108515ea")); // zero out b part of x register when loading palette selector
            PutInBank(0x00, 0x850f, Blob.FromHex("eaeaea")); // don't clear the b part of x register
            PutInBank(0x15, 0x8500, Blob.FromHex("08e230a988aef400286b"));
            PutInBank(0x15, 0x8510, Blob.FromHex("08e230a998aef700286b"));
        }


    }
}
