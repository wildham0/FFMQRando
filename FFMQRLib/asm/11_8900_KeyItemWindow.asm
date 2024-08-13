; Timer Routine Hack (asm)
.ORG $118900

  lda #$0000
  tcd
  inc $0e97
  bne :+
    inc $0e99
: lda $0e61
  bne :+
    stz $0e60
    rtl
: dec $0e61
  rtl

; Give Item Routine (Script)
.ORG $118940

RUN $00DB28                ; 09 28db00
IF [$9E] < 10 GOTO L_LOAD  ; 05 06 10 5389
IF [$9E] < 14 GOTO L_END   ; 05 06 14 6b86
IF [$9E] >= 40 GOTO L_END  ; 05 04 40 6b86
L_LOAD:
[$9E] = [$0E60]            ; 11 600e
[$0E61] = #$012C           ; 0d 610e 2c01
L_END:
END                        ; 00

; Draw Empty Companion Stat Window (Script)
.ORG $118970

LOOP                       ; 05 30
PRINT [EA]                 ; ea
PRINT [ED]                 ; ed
GOSUB $8990                ; 08 9089
[$1D] = #$00               ; 28 00
END                        ; 00

; Draw Complete Companion Stat Window (Script)
.ORG $118980

GOSUB $8990                ; 08 9089
[$18] = #$071E2E01         ; 24 012e1e07
END                        ; 00

; Box drawing script (Script)
.ORG $118990

[$9E] = [$0E00]                 ; 0f 000e  
IF [$9E] = #$55 GOTO L_END      ; 0b 55 dc89
[$9E] = [$0E61]                 ; 10 610e  
IF [$9E] != #$0000 GOTO L_DRAWBOX    ; 05 c1 0000 aa89
[$9E] = [$10A0]                 ; 0f a010
IF [$9E] = #$FF GOTO L_END      ; 0b ff dc89
GOTO L_DRAWITEM                 ; 0a d989
L_DRAWBOX:
[$18] = #$0504301B              ; 24 1b300405
[$25] = #$311C                  ; 15 1c31
DRAW_PLAIN_BOX                  ; 18
PRINT [FE]                      ; fe
PRINT [FE]                      ; fe
LINEFEED                        ; 01
PRINT [FE]                      ; fe
PRINT [FE]                      ; fe
LINEFEED                        ; 01
PRINT [FE]                      ; fe
PRINT [FE]                      ; fe
[9E] = b[0E60]                  ; 0f 600e
IF [9E] != #$0F GOTO L_DRAWITEM ; 05090f d989 > changed to GOTO L_DRAWITEM 0ad989 if not sky fragment
LINEFEED                        ; 01
[9E] = b[0E93]                  ; 0f 930e
[006C] = #$31                   ; 0c 6c00 31
UNKNOWN_6D                      ; 056d
[9E] = w[006C]                  ; 10 6c00
[9E] -= #$1010                  ; 0548 1010
UNKNOWN_18 #$02009E             ; 0518 9e0002
L_DRAWITEM:
RUN $008D29                     ; 09 298d00
L_END:
END                             ; 00

; Companion Weapon Drawing Routine (asm)
.ORG $118A00

SelectIconToDraw:
  PHP
  REP #$30
  LDX $0e61
  BEQ showWeapon
    LDX $0e60
    BRA done
showWeapon:
    LDX $10b1
done:
  PLP
  CPX #$ff
  RTL

.ORG $1189E0

  JSL SelectIconToDraw
  PHX
  LDA $049800, X
  ASL
  ASL
  STA $00F7
  REP #$10
  PLA
  RTL