
; Item Fetcher - Check if an item is written by the ap script, if so, call the item giving window and give the item

 .ORG $0182A9

Hijacker:           ; Map game loop
    JSL $158000
    JSR $E9B3
    JSR $82F2
    JSL $158010
    NOP

 .ORG $158000
    INC $19F7
    STZ $19F8
    RTL

 .ORG $158010
    LDA $19B0       ; Check if we're on map 
    BEQ :+          ; If not, abort
        RTL
:   PHP             
    SEP #$20
    LDA $701FF0     ; Check if there's an item in the pipeline
    BEQ :+          ; If yes
        LDA #$01    ; Disable check
        STA $19B0
        LDA #$50    ; Set fetcher script to be run
        STA $19EE
        LDA #$08
        STA $19EF
        PLP
        LDA #$01    ; Return $01 to run script
        RTL
:   PLP
    LDA #$00        ; Return $00
    RTL

 .ORG $158550
    PHP
    REP #$30
    LDA $701FF1     ; Get item count from SRAM
    STA $0FD1       ; Store it in ram for saving
    SEP #$30
    LDA #$00        ;
    STA $701FF0     ; Reset item queue
    PLP
    RTL

; Script: (extended tile script $50)
    [9E] = $701FF0                      ; 05 ED F01F70      Get Item
    [9E]--                              ; 05 7F             Minus 1 (actual item idea)
    [015F] = b[9E]                      ; 11 5F01           Put it at 15F
    [0160] = $01                        ; 0C 6001 01        Put message type 
    RUNL $158550                        ; 09 508515         Run Item queue update 
    IF SYSFLAG $92 == FALSE GOTO A      ; 05 FD 92 [A]      Check if we got mirror/mask
    SYSFLAG $92 = FALSE                 ; 17 92                 then run their script
    GAMEFLAG $F2 = FALSE                ; 2B F2
    UNKNOWN_E1 $02                      ; 05 E1 02
    [0505] = $0F                        ; 0C 0505 0F
    IF SYSFLAG $94 == TRUE GOTO A       ; 05 FC 94 [A]
    ACTION $2100                        ; 2C 0021
A:
    END                                 ; 00                Exit
