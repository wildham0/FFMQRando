 .ORG $8600
 
  LDA $14
  CLC
  ADC $0477
  BCC NoOverflow
   LDA $16
   SEC
   SBC $14
   STA $0477
NoOverflow:
  CMP #$8000
  BCC NotNegative
    LDA #$7FFF
    STA $0477
NotNegative: 
  RTL