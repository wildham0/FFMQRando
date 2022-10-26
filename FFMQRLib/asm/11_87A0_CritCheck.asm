
ExitRoutine = $9F34

 .ORG $9F24
 JSL CritExclusionCheck
 BCC ExitRoutine
 NOP
 NOP
 NOP
 NOP
 
 .ORG $87A0

CritExclusionCheck:  
  LDA $3A
  CMP #$49
  BCC DoCrit
  
  CMP #$50
  BCC NoCrit
  
  CMP #$CA
  BCC DoCrit
  
  CMP #$D7
  BCC NoCrit
DoCrit:  
  SEC
  RTL
NoCrit:
  CLC
  RTL