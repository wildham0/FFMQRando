; Ap Item - Allow Archipelago custom icon + name


; Sprite Bank Hack
 .ORG $008525
    JML $158050
    NOP
    NOP

 .ORG $158050
    STA $2116
    LDA $0070           ; Check current item
    AND #$00FF          ; Remove auxiliary byte
    CMP #$00F0          ; Is it Ap Item?
    BEQ :+              ; If not
        PEA #$0004      ;   normal bank
        JML $00852B
:   PEA $0015           ; If it is, use AP bank
    STZ $0070           ; Clear current item
    JML $00852B
    RTL

; Sprite Address Hack
 .ORG $00B6B5
    JML $158080

 .ORG $158080
    STZ $0065
    LDA $9E, dp         ; Get current item and store it at $70
    STA $0070
    CMP #$00DE          ; Is it a standard item?
    BCS :+              ; Continue normally
        JML $00B6BA     
:   BEQ :+              ; Is it an ap item?
        LDA #$8100
        STA $62, dp
        LDA #$0390
        STA $0064
        SEP #$30
        JML $00B6CB
:   LDA #$0001          ; It's a projectile refill
    STA $62, dp
    LDA #$0020
    STA $0064
    SEP #$30
    JML $00B6CB

; Set item name
 .ORG $03B50B ;Script
    GOSUB $1580D0       ; 07 D08115
    GOTO $B513          ; 0A 13B5

 .ORG $1580D0
    IF s[9E] == $F0 GOTO a              ; 0B F0 DD80        if ap item, new name address
    [9E] *= $0C                         ; 05 4D 0C          else normal name address
    [9E] += $0CC120                     ; 05 43 20C10C
A:
    END                                 ; 00
    [9E] = $1580F0                      ; 08 3D F08015
    END                                 ; 00

 .ORG $1580F0
    .BYTE 03 03 9A A9 FF A2 C7 B8 C0 03 03 03   ; "__AP ITEM___"

; Palette Hack
 .ORG $0008482
    JSL 158500          ; zero out b part of x register when loading palette selector
    NOP

 .ORG $000848A
    JSL 158510          ; zero out b part of x register when loading palette selector
    NOP

 .ORG $00850F
    NOP                 ; don't clear the b part of x register
    NOP
    NOP

 .ORG $158500
    PHP
    SEP #$30
    LDA #$88
    LDX $00F4
    PLP
    RTL

 .ORG $158510
    PHP
    SEP #$30
    LDA #$98
    LDX $00F7
    PLP
    RTL