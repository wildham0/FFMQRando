SetFlag = $00974E
ComputeAmmoQty = $8E00 ; this is just a copy of the routine in bank $00

DoNothing = $DBD6
GiveSpell = $DB8E
GiveWeapon = $DB9C
GiveArmor = $DBBE

 .ORG $00DACC  ; Modify the GiveItem routine
  PHP
  REP #$30
  PHX
  PHY
  JSL GiveItemQuantity
  LDA $9E
  CMP #$14
  BCC DoNothing
  CMP #$20
  BCC IsSpell
  CMP #$2F
  BCC IsWeapon
  CMP #$DD
  BCC IsArmor
IsConsumableKeyItem:
  JMP DoNothing                 ; We already gave the approriate amount with GiveItemQuantity
IsSpell:
  JMP GiveSpell
Isweapon:
  JMP GiveWeapon
IsArmor:
  JMP GiveArmor

 .ORG $DB82
  JSL KeyItemRoutine
  NOP
  NOP
  NOP
  NOP
  NOP
  NOP

 .ORG $118E30

GiveItemQuantity:
  JSL ItemQuantityRoutine  ; 22509011
  SEP #$30             ; e2
  LDA $9E              ; a59e
  CMP #$10             ; c910
  BCC IsKeyitem        ; 90ff
  CMP #$14             ; c914
  BCC IsConsumable     ; 90ff
  CMP #$DD             ; c9dd
  BCC IsKeyItem        ; 90ff
  BEQ IsBenAmmo        ; f0ff
IsCompanionAmmo:
  LDX #$80             ; a280
  BRA DoAmmo           ; 8002
IsBenAmmo:
  LDX #$00             ; a200
DoAmmo:
  LDA $1030,X          ; bd3010
  JSR ComputeAmmoQty   ; 20008e
  STA $1030,X          ; 9d3010
  RTL                  ; 6b
IsConsumable:
  JSL #$00DA65         ; 2265da00
  LDA $9E              ; a59e
  STA $0E9E,X          ; 9d9e0e
  LDA $0E9F,X          ; bd9e0f
  JSR ComputeAmmoQty   ; 20008e
  STA $0E9F,X          ; 9d9e0f
  RTL                  ; 6b
IsKeyItem:
  JSL KeyItemRoutine   ; 22009211
  RTL                  ; 60

 .ORG $9050
ItemQuantityRoutine:
  sep #$20
  lda $015f
  cmp #$F2
  bcc normal_box 
  cmp #$F6
  bcs normal_box
  lda #$19
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
  lda #$03
  sta $0166
  bra finalize
projectile:
  lda $#0A
  sta $0166
finalize:
  lda #$80
finalize_nocount:
  sta $0165  
  rtl

 .ORG $119200

KeyItemRoutine
  PHA
  CMP #$05
  BEQ Mask
  CMP #$06
  BEQ Mirror
  CMP #$0F
  BEQ SkyFragments
NormalGiveItem:
  PLA
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
