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

 .ORG $9200

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




GiveItem
 ItemQuantityRoutine  ; 22509011
 sep #$20             ; e2
 lda $9e              ; a59e
 CMP #$10             ; c910
 BCC iskeyitem        ; 90ff
 CMP #$14             ; c914
 BCC IsConsumable     ; 90ff
 CMP #$DD             ; c9dd
 BCC IsKeyItem        ; 90ff
 BEQ isBenAmmo        ; f0ff
isCompanionAmmo:  (dae1  
 LDX #$80             ; a280
 BRA doAmmo           ; 8002
isbenammo:
 Ldx #$00             ; a200
doAmmo:
 lda $1030, x         ; bd3010
 JSR ComputeAmmoQty   ; 2005db
 sta $1030, x         ; 9d3010
 rts                  ; 60
isconsumable:
 JSL code_00da65      ; 2265da00
 lda $9e              ; a59e
 sta $0e9e,x          ; 9d9e0e
 lda $0e9f,x          ; bd9e0f
 JSR computeammo      ; 2005db
 sta $0e9f,x          ; 9d9e0f
 rts
iskeyitem:
 JSL KeyItemRoutine   ; 22009211
 rts                  ; 60
