# Tables

Here you can find all the related knowledge on the tables found in the ROM. Currently incomplete and also doesn't incorporate some knowledge present in the code.

## Raw Byte Info

### Enemies

#### Enemy Attack General Info

This table controls which attacks specific enemies use

Start address = 0xC6FF
Bank = 2
File address = 0x146FF
Amount = 82
Length = 9

| Id | Description |
| -- | -- |
| 0 | Unknown |
| 1 | Attack 1 |
| 2 | Attack 2 |
| 3 | Attack 3 |
| 4 | Attack 4 |
| 5 | Attack 5 |
| 6 | Attack 6 |
| 7 | Unknown |
| 8 | Unknown |


#### Enemy Attack Table Entries


| Id | Enemy Name |
| --  | -- |
| 6 | Slime |
| 16 | Land Worm |
| 32 | Centaur |
| 64 | Skullrus Rex |
| 65 | Stone Golem |
| 75 | Ice Golem |
| 76 | Twinhead Hydra |
| 77 | Twinhead Wyvern |
| 78 | Pazuzu |
| 79 | Zuh |
| 80 | Dark Knight Phase 1 |
| 81 | Dark Knight Phase 2 |
| 82 | Dark Knight Phase 3 |

Phase 4 copies the stats from the previous phase (if you skip a phase, it copies whatever phase you were on)
Dark Knight phases 2, 3 and 4 do not use the attack ids in the table, but rather use whatever is in 0x1509E with length 0x0C, phase 2 uses first 4 (maybe 6?), phase 3 uses last 6, phase 4 probably copies the previous phase as mentioned above.

#### Enemy Attack Options (bytes 1 through X)

Shamelessly copied from Guard_Master's pro action replay guide:
19 = [Skips Turn]           73 = Sting                A8 = Body Odor
1B = Bare Hands             74 = Tail                 A9 = Para-stare
40 = Sword                  75 = Psudopod             AA = Poison Fluid
41 = Scimitar               76 = Bite                 AB = Poison Flour
42 = Dragon Cut             77 = Hydro Acid           AC = Hypno-sleep
43 = Rapier                 78 = Branch               AD = Lullaby
44 = Axe                    79 = Fin                  AE = Sleep Lure
45 = Beam                   7A = Scissor              AF = Sleep Powder
46 = Bone Missile           7B = Whip Tongue          B0 = Blind Flash
47 = Bow & Arrow            7C = Horn                 B1 = Smoke Screen
48 = Blow Dart              7D = Giant Blade          B2 = Muffle
49 = Cure (Weak)            7E = Headoomerang         B3 = Silence Song
4A = Heal                   7F = Chew Off             B4 = Stone Gas
4B = Quake (Weak)           80 = Quake (Strong)       B5 = Stone Gaze
4C = Blizzard               81 = Flame                B6 = Double Sword
4D = Fire                   82 = Flame Sweep          B7 = Double Hit
4E = Thunder (Weak)         83 = Fireball             B8 = Triple Fang
4F = Reflectant             84 = Flame Pillar         B9 = Double Kick
50 = Electrapulse           85 = Heatwave (Strong)    BA = Twin Shears
51 = Power Drain            86 = Watergun             BB = Three Heads
52 = Spark                  87 = Coldness             BC = 6 Psudopods
53 = Iron Nail              88 = Icy Foam             BD = Snake Head
54 = Scream                 89 = Ice Block            BE = Drain
55 = Quicksand              8A = Snow Storm           BF = Dissolve
56 = Doom Gaze              8B = Whirlwater           C0 = Sucker Stick
57 = Doom Powder            8C = Ice Breath           C1 = Selfdestruct
58 = Cure (Strong)          8D = Tornado              C2 = Multiply
59 = Fire Breath (Strong)   8E = Typhoon              C3 = Para Gas
5A = Punch                  8F = Hurricane (Strong)   C4 = Rip Earth
5B = Kick                   90 = Thunder (Strong)     C5 = Stone Block
5C = Uppercut               91 = Thunder Beam         C6 = Windstorm
5D = Stab                   92 = Corrode Gas          C7 = Twin Fang
5E = Head Butt              93 = Doom Dance           C8 = Psychshield (Weak)
5F = Body Slam              94 = Sonic Boom           C9 = Psychshield (Strong)
60 = Scrunch                95 = Bark                 CA = Dark Cane
61 = Full Nelson            96 = Screechvoice         CB = Dark Saber
62 = Neck choke             97 = Para-needle          CC = Ice Sword
63 = Dash                   98 = Para-claw            CD = Fire Sword
64 = Roundhouse             99 = Para-snake           CE = Mirror Sword
65 = Choke Up               9A = Para-breath          CF = Quake Axe
66 = Stomp Stomp            9B = Poison Sting         D0 = Cure Arrow
67 = Mega Punch             9C = Poison Thorn         D1 = Lazer
68 = Bearhug                9D = Rotton Mucus         D2 = Spider Kids
69 = Axe Bomber             9E = Poison Snake         D3 = Silver Web
6A = Piledriver (Strong)    9F = Poison Breath        D4 = Golden Web
6B = Sky Attack (Strong)    A0 = Blinder              D5 = Mega Flare
6C = Wrap Around            A1 = Blackness            D6 = Mega White
6D = Dive                   A2 = Stone Beak           D7 = Fire Breath (Weak)
6E = Attach                 A3 = Gaze                 D8 = Sky Attack (Weak)
6F = Mucus                  A4 = Stare                D9 = Piledriver (Weak)
70 = Claw                   A5 = Spooky Laugh         DA = Hurricane (Weak)
71 = Fang                   A6 = Riddle               DB = Heatwave (Weak)
72 = Beak                   A7 = Bad Breath           FF = None (Empty Slot)

#### Attack General Info

This table controls individual attacks, players and enemies alike use various entries here.

Start address = 0xBC78
Bank = 2
File address = 0x13C78
Amount = 169
Length = 7

| Id | Description |
| --  | -- |
| 0 | Unknown |
| 1 | Unknown |
| 2 | power |
| 3 | attack type |
| 4 | attack sound |
| 5 | Unknown |
| 6 | attack animation on target |

#### Attack Table Entries

| Id | Enemy Name |
| --  | -- |
| 6 | Flamesaurus Rex T-Bone |
| 26 | Brownie Stab |
| 27 | Brownie Kick |
| 29 | Slime Stab |
| 63 | Flamesaurus Rex Chew Off |
| 107 | Flamesaurus Rex Poison |
| 111 | Flamesaurus Rex Sleep |
| 129 | Self Destruct |
| 132 | Flamesaurus Rex Rip Earth |
| 135 | Flamesaurus Rex Twin Fang |
| 157 | Player Exit (no effect if used by enemies) |
| 158 | Player Cure |
| 159 | Player Heal |
| 160 | Player Life |
| 161 | Player Quake (multi target earth attack with feather animation if used by enemies) |
| 162 | Player Blizzard (multi target water attack with feather animation if used by enemies) |
| 163 | Player Fire (or multi target fire attack with DK spider animation if used by enemies)  |
| 164 | Player Aero (or multi target wind attack with DK spider animation if used by enemies)  |
| 165 | Player Thunder (or single target DK spider attack if used by enemies) |
| 166 | Player White (or multi target DK spider attack if used by enemies) |
| 167 | Player Meteor (or multi target DK spider attack if used by enemies) |
| 168 | Player Flare (or multi target DK spider attack if used by enemies) |

#### Byte 3: Attack Type Ids (byte id 3)

For the Slime and Brownie, setting every raw byte for enemies to 0:
| Id  | Attack Description |
| --  | -- |
| 14 | Some sort of cure target |
| 18 | regular? |
| 28 | regular? |
| 73 | regular? |
| 136 | regular? |
| 137 | regular? |
| 194 | regular? |
| 195 | regular? |
| 197 | regular? |
| 198 | regular? |
| 200 | regular? |
| 201 | regular? |
| 202 | Attack Down |
| 203 | "No Effect" |
| 204 | "No Effect" |
| 205 | Probably Doom (entire party??) |
| 206 | Another cure |
| 207 | regular? |
| 209 | regular? |
| 210 | regular? |
| 211 | Another Doom |
| 212 | Always 1 damage, regardless of power |
| 213 | Some attack that doesn't display damage (silence?) |
| 214 | Always "Missed" |
| 215 | "Hit Once" |
| 216 | Always 1 damage |
| 217 | Self destruct |
| 218 | Multiply |
| 221 | A high critical chance regular attack |