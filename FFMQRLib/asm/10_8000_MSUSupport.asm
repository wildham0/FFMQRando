; Patch by Conn
; apply on Final Fantasy - Mystic Quest (USA) Rev1 or Pal versions expanded to 1MB
; modified to check for MSU chips
; last modification: 2022-10-06


 .ORG $8186 ; 0x0D
	JML PlayTrack
	NOP
	NOP

 .ORG $85D2 ; 0x0D
	JML mute
	NOP
	
 .ORG $81F4 ; 0x0D, prevent overwrite of original track e.g., interrupt from join party
	JSL preventOverwrite

 .ORG $81FA ; 0x0D
	JSL storePreviousTrack
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	
; freespace 
 .ORG $8000 ; 0x10

MSU_ID		=		$2002
lut_RandomSongs = 		$108120
lut_ReverseRandomSongs =	$108140
	
PlayTrack:
  JSR LoadRandomTrack
  LDA $01	 
  CMP $05
  BNE NewTrack
    JML $0d818a
NewTrack:
  JSR CheckForMSU
  BCC PlaySpc
    LDA $7ffff0
    CMP $01
    BNE NewTrackMsu
      STZ $01
      JML $0d818a	
NewTrackMsu:	
  STZ $2006
  LDA $01
  STA $7ffff0
  STA $2004
  STZ $2005
loop:
  LDA #$01
  BIT $2000
  BVS loop

  LDA $2000
  AND #$08
  BNE PlaySpcMSU ; error bit set?	
    LDA #$FF
    STA $2006
    LDA $01
    CMP #$15 ;noloop joined party 
    BNE LoopTrack
      LDA #$01
      BRA NoLoop
LoopTrack:	
   JSR SaveTrack
   LDA #$03
NoLoop:
   STA $2007
   STZ $01 ;mute
PlaySpc:	
   JML $0D81ED
PlaySpcMSU:
   LDA #$00
   STA $7ffff0
   JML $0D81ED		


mute:
  STA $2140
  CMP #$F0
  BNE NotMuted
    STZ $2007
    JML $0d85d9
NotMuted:	
  JML $0D85ED
	
preventOverwrite:
  LDX $06
  PHA
  JSR CheckForMSU
  BCC error
    LDA $2000
    AND #$08
    BNE error ; error bit set?	
      PLA
      RTL
error:
  LDA $7ffff0
  BEQ nomsubefore
    PLA
    LDA $01
    JSR SaveTrack
    LDA #$00
    STA $7ffff0
    RTL
nomsubefore:	
  PLA
  JSR SaveTrackNoLoad
  RTL

storePreviousTrack:
  LDA $01
  BNE notzero
    JSR CheckForMSU
    BCC nomsu
      LDA $7ffff0
      BEQ nomsu
        STZ $2141
        STZ $4202
        STA $05
        RTL
nomsu:
  LDA $01
notzero:
  STA $2141
  STA $05
  STA $4202
  RTL
  	
CheckForMSU:
	lda MSU_ID
	cmp #$53	; 'S'
	bne NoMSU	; Stop checking if it's wrong
	lda MSU_ID+1
	cmp #$2D	; '-'
	bne NoMSU
	lda MSU_ID+2
	cmp #$4D	; 'M'
	bne NoMSU
	lda MSU_ID+3
	cmp #$53	; 'S'
	bne NoMSU
	lda MSU_ID+4
	cmp #$55	; 'U'
	bne NoMSU
	lda MSU_ID+5
	cmp #$31	; '1'
	bne NoMSU
MSUFound:
	SEC
	RTS
NoMSU:
	CLC
	RTS

LoadRandomTrack:
  PHX
  PHP
  SEP #$30
  LDA $01
  TAX                      
  LDA lut_RandomSongs, X   
  AND #$1F	          
  STA $01                 
  PLP
  PLX
  RTS

SaveRandomTrack:
  PHX
  PHP
  SEP #$30
  TAX
  LDA lut_ReverseRandomSongs, X 
  AND #$1F	  
  PLP
  PLX
  RTS
  
SaveTrack:
  LDA $01
SaveTrackNoLoad:
  JSR SaveRandomTrack ; NOP NOP NOP
  STA $09
  RTS       
