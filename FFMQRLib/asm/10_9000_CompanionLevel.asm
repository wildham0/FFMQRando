 .ORG $A000

companionid = $0E92
benlevel = $1010
companionlevel = $1090
companionmaxhp = $1096
companioncurhp = $1094
companion_strbase = $10A6
companion_strback = $10CC
companion_conbase = $10A7
companion_conback = $10CD
companion_magbase = $10A8
companion_magback = $10CE
companion_spdbase = $10A9
companion_spdback = $10CF
companion_whitemp = $109B
companion_blackmp = $109C
companion_wizardmp = $109D
companion_currentmp = $1098
companion_maxmp = $109B
companion_ailments = $10A1
companion_aiattacks = $10C2

ComputeGearStats = #$00FF70


DoLeveling:
  PHP
  REP #$30
  JSR GetCompanionOffset
  JSR SetLevel           ; change given settings
  JSR ComputeStats
  JSR SetEquipSpells
  JSR UpdateActiveStats
  JSL ComputeGearStats
  PLP
  RTS

UpdateActiveStats:
  PHP
  SEP #$20
  REP #$10
  LDX #$0003
UpdateLoop:
  LDA $10CC,X
  STA $10A6,X
  CLC
  ADC $10AA,X
  STA $10A2,X
  DEX
  BPL UpdateLoop
  PLP
  RTS


GetCompanionOffset:
  PHP
  SEP #$30
  LDA #$00
  XBA
  LDA companionid
  SEC
  SBC #$01
  REP #$30
  ASL
  ASL
  ASL
  ASL
  ASL
  ASL
  ASL
  TAX
  PLP
  RTS

SetLevel_Ben:
  PHP
  SEP #$20
  LDA benlevel
  CLC
  ADC #$00 ; change given setting
  STA companionlevel
  PLP
  RTS

SetLevel_Quest:
  PHP
  SEP #$20
  REP #$10
  PHX
  LDA #$00
  XBA
  LDA companionid
  SEC
  SBC #$01
  ASL
  ASL
  TAY
  CLC
  ADC companionid
  SEC
  SBC #$01
  TAX
  TYA
  CLC
  ADC #$A9 ; companion flags offset
  JSR CheckGameFlag ; 009776
  BEQ :+
    INX
: CLC
  ADC #$01
  JSR CheckGameFlag ; 009776
  BEQ :+
    INX
: CLC
  ADC #$01
  JSR CheckGameFlag ; 009776
  BEQ :+
    INX
: CLC
  ADC #$01
  JSR CheckGameFlag ; 009776
  BEQ :+
    INX
: LDA,X lut_CompanionLevel
  STA companionlevel
  PLX
  PLP
  RTS

ComputeStats:
  PHP
  REP #$30
  LDA companionlevel
  STA $4202

  JSR GetStats
  STA companionmaxhp

  JSR GetStats99
  SEP #$20
  STA companion_strbase
  STA companion_strback

  JSR GetStats99
  SEP #$20
  STA companion_conbase
  STA companion_conback

  JSR GetStats99
  SEP #$20
  STA companion_magbase
  STA companion_magback

  JSR GetStats99
  SEP #$20
  STA companion_spdbase
  STA companion_spdback

  LDA companionlevel
  PHA
  JSR GetMP
  STA companion_whitemp

  PLA
  LSR
  PHA
  JSR GetMP
  STA companion_blackmp

  PLA
  LSR
  JSR GetMP
  STA companion_wizardmp

  PLP
  RTS

GetStats:
  PHP
  SEP #$20
  REP #$10
  LDA,X lut_CompanionStats
  STA $4203
  REP #$30
  XBA
  XBA
  INX
  CLC
  LDA $4216
  ADC,X lut_CompanionStats
  INX
  INX
  PLP
  RTS

GetStats99:
  REP #$30
  JSR GetStats
  CMP #$0063
  BCC :+
    LDA #$0063
  : RTS

GetMP:
  CLC
  ADC,X lut_CompanionStats
  INX
  CMP #$63
  BCC :+
    LDA #$63
  : RTS

SetEquipSpells:
  PHP
  PHX
  SEP #$20
  REP #$10
loop:
  LDA companionlevel
  CMP,X lut_CompanionStats  ; level thresholds are in descending order
  BCS LoadEquipSpells
  INX
  INX
  INX
  INX
  INX
  INX
  INX
  BRA loop

LoadEquipSpells:
  REP #$30
  INX
  TXA
  CLC
  ADC #$A300
  TAX
  LDA #$0005
  LDY #$10B4
  MOV $10,$00
  PLP
  JSR ComputeAI
  RTS

ComputeAI:
  PHP
  REP #$30
  PHX
  LDA,X lut_CompanionStats
  AND #$0003
  ASL
  TAX
  LAD,X lut_AiAttacks
  STA companion_aiattacks
  PLX
  REP #$10
  SEP #$20
  LDA #$00
  XBA
  LDA,X lut_CompanionStats
  TAX
  LDA,X lut_OddsValues
  TAY
  LDA #$04
  TAX
  PHD
loop:
  PEA #$10B8
  PLD
  JSL CheckFlag      ; $00975A
  BEQ spellunknown
    TYA
    BRA storeodds
spellunknown:
    LDA #$00
storeodds:
  STA,X $10C0
  INX
  TXA
  CPX #$0B
  BCC loop
  PLD
  PLP
  RTS

HealUp:
  PHP
  REP #$20
  SEP #$10
  LDX companionmaxhp
  STX companioncurhp
  LDX companion_maxpmp
  STX companion_currentmp
  LDA companion_maxpmp+2
  STA companion_currentmp+2
  STZ companion_ailments
  PLP
  RTS

HpUp:
  PHP
  REP #$30
  LDA companioncurhp
  CLC
  ADC #$0028
  STA companioncurhp
  PLP
  RTS

PhoebeResistBadHack:
  PHP
  REP #$30
  LDX $1090 ; companion level
  CPX #$FF
  BEQ NotPhoebe23
  CPX #$17
  BCC NotPhoebe23
  LDA #$03
  CMP $0e92 ; current companion id
  BNE NotPhoebe23
  SEP #$30
  LDA #$3158 ; resist value with aether shield
  STA $10BA ; companion resist
NotPhoebe23:
  PLP
  RTS


lut_CompanionStats:                ; 0x40 bytes
  ; HP Multiplier (1 byte),
  ; HP Base (2 bytes),
  ; Str Multiplier (1 byte),
  ; Str Base (2 bytes),
  ; Con Multiplier (1 byte),
  ; Con Base (2 bytes),
  ; Mag Multiplier (1 byte),
  ; Mag Base (2 bytes),
  ; Spd Multiplier (1 byte),
  ; Spd Base (2 bytes),
  ; Level Threshold (1 byte) (=<) Repeat for each threshold
  ; Equipment (3 bytes)
  ; Spells (2 bytes)
  
lut_CompanionLevel: ; 5 bytes per companion, initial level then level for each flag
  .BYTE $07, $0F, $17, $22, $29, $07, $0F, $17, $22, $29, $07, $0F, $17, $22, $29, $07, $0F, $17, $22, $29

lut_OddsValues:
  .BYTE $00, $64, $32, $22, $19, $14, $11, $0F, $0D