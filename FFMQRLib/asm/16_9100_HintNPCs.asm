price = #$FFFF
hintflag = #$00C1
CheckGameFlag = #$009776
CheckFlag = #$00975A

item = $1500
price = $1501 (3 bytes)
floor = $1504 (3 bytes)
hintaddress = $1507 (3 bytes)

BANK = $16

 .ORG $9100
; Compute Price
ComputePrice:
  PHP
  REP #$30
  STZ $1503
  STZ $1506
  LDA price
  STA $1501
  JMP FloorRoutine ; skip if progressive  
  LDA hintflag
PriceLoop:
  PHA
  JSL CheckGameFlag
  BEQ NotSet
    LDA $1501
    CLC
    ADC price
    STA $1501
NotSet:
  PLA
  INC A
  CMP #$00C5
  BCC PriceLoop

 .ORG $9130
; Compute Floor
ComputeFloor:
  LDA #$0000
  STA $1504
  BRA WindiaItemBough ; skip all this if density > 0
  LDA #$00D0
  JSL CheckGameFlag
  BNE AquariaItemBought
    LDA $1504
    CLC
    ADC #00C8
    STA $1504
AquariaItemBought:
  LDA #$00D1
  JSL CheckGameFlag
  BNE FireburgItemBought
    LDA $1504
    CLC
    ADC #01F4
    STA $1504
FireburgItemBought:
  LDA #$00D2
  JSL CheckGameFlag
  BNE WindiaItemBought
    LDA $1504
    CLC
    ADC #012C
    STA $1504
WindiaItemBought:
  LDA $1501
  CLC
  ADC $1504
  STA $1504
; Check with current GP
  SEP #$20
  REP #$10
  LDA $0E86
  BNE EnoughMoney
    REP #$30
    LDA $0E84
    CMP $1504
    BCS EnoughMoney
      LDA $0E84
      CMP $1501
      BCS BuyMoreItems
        LDA #$00FF
        BRA StoreResult
BuyMoreItems
  LDA #$0001
  BRA StoreResult
EnoughMoney:
  REP #$30
  LDA #$0000
StoreResult:
  STA $009e
  PLP
  RTL

 .ORG $91C0
; Remove Money
TakeMoney:
  PHP
  REP #$30
  LDA $0E84
  SEC
  SBC $1501
  STA $0E84
  SEP #$20
  REP #$10
  LDA $0E86
  SBC #$00
  STA $0E86
  PLP
  RTL


 .ORG $91E0
; Move hint address to memory
JumpToHint:
  PHP
  SEP #$20
  REP #$10
  LDA $1507
  STA $009E
  LDA $1508
  STA $009F
  LDA $1509
  STA $00A0
  PLP
  RTL  

; Seeking inventory to find the right hint in the list
 .ORG $9200  
CheckItem:
  PHD
  PEA #$0EA6
  PLD
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

 .ORG $9210
CheckWeapon:
  PHD
  PEA #$1032
  PLD
  SEC
  SBC #$20
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

 .ORG $9220
CheckArmor:
  PHD
  PEA #$1035
  PLD
  SEC
  SBC #$2F  
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

 .ORG $9230  
CheckSpell:  
  PHD
  PEA #$1038
  PLD
  SEC
  SBC #$14  
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

 .ORG $9240  
Check3WeaponsG3:
  DEC
Check3WeaponsG2:
  DEC
Check3WeaponsG1:
  TAX
  JSR CheckWeapon
  BNE HaveWeapon
  TXA
  INC
  TAX
  JSR CheckWeapon
  BNE HaveWeapon
  TXA
  INC
  JSR CheckWeapon
HaveWeapon:
  RTS

 .ORG $9260
Check3ArmorsG3:
  DEC
Check3ArmorsG2:
  DEC
Check3Armors:
  TAX
  JSR CheckArmor
  BNE HaveArmor
  TXA
  INC
  TAX
  JSR CheckArmor
  BNE HaveArmor
  TXA
  INC
  JSR CheckArmor
HaveArmor:  
  RTS

   .ORG $9230  
CheckLocationChest:
  PHD
  PEA #$0ec8
  PLD
  TAX
  LDA lut_LocationFlag,X
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

   .ORG $9230  
CheckLocationNPCOn:
  PHD
  PEA #$0EA8
  PLD
  TAX
  LDA lut_LocationFlag, X
  JSL CheckFlag
  PLD
  INC
  DEC
  RTS

  .ORG $9230  
CheckLocationNPCOff:
  PHD
  PEA #$0EA8
  PLD
  TAX
  LDA lut_LocationFlag, X
  JSL CheckFlag
  PLD
  INC
  DEC
  BEQ AtZero
    LDA #$00
	RTS
AtZero:
  LDA #RFF
  RTS

     .ORG $9230  
CheckLocationBattlefield:
  TAX
  LDA lut_LocationFlag,X
  TAX
  LDA $0FD4, X
  BEQ AtZero
    LDA #$00
	RTS
AtZero:
  LDA #RFF
  RTS

 .ORG #92E0
lut_LocationFlag:
; #$20 bytes


 .ORG $9300
lut_CheckRoutines:
; pointers to all check routines

 .ORG $9290
; entry point for the full check routine, update y to the actual start address
  PHP
  PHB
  SEP #$20
  REP #$10
  LDA #$16
  PHA
  PLB
  LDA #$00
  XBA
  LDY #$0000 ; update this for each npcs
  JMP StartCheckRoutine
  
 .ORG $9310  
; Main Item Check Routine
NextCheck:
  INY
  INY
  INY
  INY
  INY  
StartCheckRoutine:
  LDA lut_ChecksToDo+1, Y  ; Get Check Routine
  CMP #$FF                 ; If #$FF
  BEQ EndOfline            ; no hint left, do a useless hint
  CMP #$FE
  BEQ JumpTable
  ASL
  TAX
  LDA lut_ChecksToDo, Y
  JSR lut_CheckRoutines, X
  BNE NextCheck
EndOfLine:
  LDA lut_ChecksToDo, Y  
  STA $1500
  LDA lut_ChecksToDo+2, Y  
  STA $1507
  LDA lut_ChecksToDo+3, Y  
  STA $1508
  LDA lut_ChecksToDo+4, Y  
  STA $1509
  PLB
  PLP
  RTL
JumpTab:
  PHP
  REP #$30
  LDA lut_ChecksToDo+2, Y  
  TAY
  PLP
  BRA StartCheckRoutine

 
 .ORG $9380
lut_ChecksToDo:
  
 