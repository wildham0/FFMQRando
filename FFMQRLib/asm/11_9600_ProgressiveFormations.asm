
lutLocationLevels = $119780
lutFormations = $119800
lutAnimations = $119880
lutSprites = $119900

 .ORG $119600

LoadValue:
  PHX
  PHP
  REP #$30
  AND #$00FF
  CLC
  ADC $19B5
  TAX
  PLA
  SEP #$20
  REP #$10
  LDA $11B100,X
  PLP
  PLX
  RTS

 .ORG $119620
GetFormation:
  INC $1935
  LDA $0E88
  CMP #$35
  BEQ NotFormation
    LDA $1935
    CLC
    ADC #$04
    JSR LoadValue
    AND #$18
    CMP #$08
    BEQ Formation:
NotFormation:
  LDA #$00
  STA $0520
  LDA $1935
  JSR LoadValue
  RTL
Formation:
  LDA #$01
  STA $0520
  JSR LoadFormation
  RTL

 .ORG $119650
GetAnimation:
  INC $1935
  LDA $0520
  BNE Formation
    LDA $1935
    JSR LoadValue
    RTL
Formation:
  LDA $1935
  JSR LoadValue
  AND #$E0
  STA $0522
  JSR LoadAnimation
  ORA $0522
  RTL

 .ORG $119680
GetSprite:
  INC $1935
  LDA $0520
  BNE Formation
    LDA $1935
    JSR LoadValue
    RTL
Formation:
  JSR LoadSprite
  RTL

 .ORG $1196A0
LoadFormation:
  PHX
  CLC
  LDA $1935
  ADC #$05
  JSR LoadValue
  AND #$1F
  ASL
  ASL
  STA $0521
  LDA $0E88
  TAX
  LDA lutLocationLevels, X
  ADC $0521
  TAX
  LDA lutFormations, X
  PLX
  RTS

 .ORG $1196D0
LoadAnimation:
  PHX
  CLC
  LDA $0E88
  TAX
  LDA lutLocationLevels, X
  ADC $0521
  TAX
  LDA lutAnimations, X
  PLX
  RTS

 .ORG $1196F0
LoadAnimation:
  PHX
  LDA $0E88
  TAX
  LDA lutLocationLevels, X
  ADC $0521
  TAX
  LDA lutSprites, X
  CLC
  ADC #$60
  PLX
  RTS