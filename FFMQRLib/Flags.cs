using RomUtilities;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;



namespace FFMQLib
{
    public class Flags
    { 
        public EnemiesDensity EnemiesDensity { get; set; } = EnemiesDensity.All;
        public ItemShuffleChests ChestsShuffle { get; set; } = ItemShuffleChests.Prioritize;
        public ItemShuffleBoxes BoxesShuffle { get; set; } = ItemShuffleBoxes.Exclude;
        public bool ShuffleBoxesContent { get; set; } = false;
        public ItemShuffleNPCsBattlefields NPCsShuffle { get; set; } = ItemShuffleNPCsBattlefields.Prioritize;
        public ItemShuffleNPCsBattlefields BattlefieldsShuffle { get; set; } = ItemShuffleNPCsBattlefields.Prioritize;
        public bool ShuffleEnemiesPosition { get; set; } = false;
        public EnemiesScaling EnemiesScaling { get; set; } = EnemiesScaling.Normal;
        public EnemiesScalingSpread EnemiesScalingSpread { get; set; } = EnemiesScalingSpread.None;
        public LevelingCurve LevelingCurve { get; set; } = LevelingCurve.Normal;
        public BattlesQty BattlesQuantity { get; set; } = BattlesQty.Ten;
        public bool ShuffleBattlefieldRewards { get; set; } = false;
        public bool RandomStartingWeapon { get; set; } = false;

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

            string flagstring = Convert.ToBase64String(BitConverter.GetBytes(flagstrinvalue));
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
            // Throw an error if the settings don't offer enough locations.
            if ((NPCsShuffle == ItemShuffleNPCsBattlefields.Exclude || BattlefieldsShuffle == ItemShuffleNPCsBattlefields.Exclude) && BoxesShuffle == ItemShuffleBoxes.Exclude)
            {
                throw new Exception("Selected flags don't allow enough locations to place all Quest Items. Change flags to include more locations.");
            }
        }
    }

}
