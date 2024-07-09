 .ORG $8600
 
  LDA $14
  CLC
  ADC $0477             ; Add Current HP + Healing
  BCS Overflow          ; Does it overflow?
    CMP $16             ; If not,
    BCC LowerThanMaxHp  ; Is it higher than Max HP?
Overflow:
      LDA $16           ; If yes to one or the other
      SEC               ; Healing = Max HP - Current HP
      SBC $14
      STA $0477
LowerThanMaxHp:
  LDA $0477             ; Get Healing
  BPL NotNegative       ; Is negative bit set?
    LDA #$7FFF          ; If yes, cap healing
    STA $0477
NotNegative: 
  RTL