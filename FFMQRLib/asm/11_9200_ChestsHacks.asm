SetFlag = $00974E

 .ORG $DB82
  JSL KeyItemRoutine
  NOP
  NOP
  NOP
  NOP
  NOP
  NOP

 .ORG $9050
ItemQuantityRoutine:
  sep #$20
  lda $015f
  cmp #$F2
  bcc normal_box 
  cmp #$F6
  bcs normal_box
  lda #$18
  sta $0166
  bra finalize
normal_box:  
  lda $9e
  cmp #$10
  bcc key_item
  cmp #$14
  bcc consumable
  cmp #$dd
  bcc key_item
  cmp #$f0
  bcc projectile
key_item:
  stz $0166
  lda #$00
  bra finalize_nocount
consumable:
  lda #$02
  sta $0166
  bra finalize
projectile:
  lda $#09
  sta $0166
finalize:
  lda #$80
finalize_nocount:
  sta $0165  
  rtl

 .ORG $9200

KeyItemRoutine
  CMP #$05
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
  LDA $0E88                    ; Check if we're in volcano
  CMP #$29
  BNE NormalGiveItem
  LDA $0E91
  BEQ NormalGiveItem
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
  LDA $0E88                     ; Check if we're in Ice Pyramid
  CMP #$21
  BNE NormalGiveItem
  LDA $0E91
  BEQ NormalGiveItem
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
  BRA NormalGiveItem