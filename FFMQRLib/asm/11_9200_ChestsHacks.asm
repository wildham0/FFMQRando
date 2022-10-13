SetFlag = $00974E

 .ORG $DB82
  JSL KeyItemRoutine
  NOP
  NOP
  NOP
  NOP
  NOP
  NOP

 .ORG $9200

KeyItemRoutine
  CMP #$05 ; NOP these parts instead
  BEQ Mask
  CMP #$06
  BEQ Mirror
  CMP #$0F
  BEQ SkyFragments
NormalGiveItem:
  PHD
  PEA #$0EA6
  PLD
  JSL SetFlag 
  PLD
  RTL
Mask:
  PHD 
  PEA #$00D0
  PLD
  LDA #$92
  JSL SetFlag
;  PEA #$00D0
;  PLD
;  LDA #$94
;  JSL SetFlag
  PLD
  LDA $009E
  BRA NormalGiveItem
Mirror:
  PHD 
  PEA #$00D0
  PLD
  LDA #$92
  JSL SetFlag
  PLD
  LDA $009E
  BRA NormalGiveItem
SkyFragments:
  INC $0E93
  RTL