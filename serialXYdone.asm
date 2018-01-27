;; program for serial communication and stepper motor control
;; first pc asserts RTS low indicating start of communication 
;; mc detects the logic change in prt 3.4 and initiates the serial communication
;; then it sends the CTS signal low in port 3.5 indicating it is ready for receiving
;; pc then asserts RTS high to indicate the start of transmission
;; as the mc reads in the data and undergoes required processes, it finally asserts the CTS high

org 0000h
jmp 0100h

;; use the user variable register as timer delays
org 0100h

Tmr0H equ 30h	
Tmr0L equ 31h
Tmr1H equ 42h
Tmr1L equ 43h
DRLST equ 07h	;; used for the state of Drill motor

;; initialize the port 3 to high logic for input
mov p3,#0ffh
mov psw,#00h 		;;  set the default register bank to reg 0

;; check the state of RTS signal in port 3.4 if it is low, initiate serial reception
CHKRTS: jb p3.4,CHKRTS	   ;; keep on checking the state of the p3.4 until it goes low
	acall DELAY	   ;; call delay to indicate settling time
	jb p3.4,RTSFALSE   ;; if RTS is still low, it indicates RTS active
	acall INITSERIAL
	clr p3.5 	   ;; assert CTS low to indicate Clear to Send
	acall DATARECV
	acall STEPPERMTR   ;; code block to run the stepper motors
	setb p3.5 	   ;; assert CTS high to indicate completion of operating motors
RTSFALSE: jmp CHKRTS
	
DELAY:    mov psw,#18h ;; use reg bank 3 for calculating delay 
	  mov r3,#255d
	  ;mov r4,#255d
loopd:    djnz r3,loopd
          ;djnz r4,loopd
	  ret

INITSERIAL:mov scon,#50h	;; mode 1 and receive enable 
	   mov th1,#0fah	;; set th1 for baud rate of 9600
	   mov pcon,#80d 	;; setb PCON.7
	   mov tmod,#20h	;; code for 8 bit autoreload mode in timer1
           ret

DATARECV: mov psw,#10h ;; use reg bank 2 for storing the delay and others 
	  jnb ri,$    
	  mov b,sbuf	;; store control word in register B  
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r0,sbuf	;; store Xstep Quotient
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r1,sbuf	;; store Xstep Remainder
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r5,sbuf	;; store Ystep Quotient
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r6,sbuf	;; store Ystep Remainder
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r2,sbuf	;; store Xdelay upper  byte
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r3,sbuf	;; store Xdelay lower  byte
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r4,sbuf	;; store Ydelay upper  byte
	  clr ri      ;; clear ri
	  jnb ri,$    
	  mov r7,sbuf	;; store Ydelay lower  byte
	  clr ri      ;; clear ri
	  mov psw,#00h 	;; reset the default reg bank
ret       



STEPPERMTR:

	mov psw,#10h ;; use reg bank 2 for storing the delay and calculation
	

	;; R0 ...X step Quotient
	;; R1 ...X step rem
	;; R5 ...Y step Quotient
	;; R6 ...Y step Rem
	
	mov Tmr0H,r3	;; r3 used to hold X delay lower
	mov Tmr0L,r2 	;; used for hldin X delay upper
	mov Tmr1H,r7	;; used for hldin Y delay lower
	mov Tmr1L,r4      ;; used for hldin Y delay upper
	
	;; since r2,r3,r4 and r7 are free
	;; we use r2 and r3 for sequencing and r4 and r7 for stepping 255 times for X and Y resp
	
	mov r4,#255d	;; used to hold the value for multiplication i.e. 255 for X
	mov r7,#255d	;; used to hold the value for multiplication i.e. 255 for X
 
	;jb a.1,XClk	;; a.1 contains X motor direction
	;clr b.0
	;jmp ChkY		
;XClk:   setb b.0        ;; used for X.... b.0 set --> Clockwise 
;ChkY:   jb A.2,YClk	;; a.2 contains Y motor direction
;	clr b.1
;	jmp TIMERSET
;YClk:	setb b.1        ;; used for Y.... b.1 set --> Clockwise 

TIMERSET:		
	setb ea;
	setb et1;
	setb et0;
	
	mov th0,Tmr0H	;; set the required delay unit in corresponding timers
	mov tl0,Tmr0L
	mov th1,Tmr1H
	mov tl1,Tmr1L

	acall SEQUENCE	;; jump to motor sequencing
	;; Reg Bank 0 r2 and r3 used for X motro and Y motor sequencing

	mov tcon,#80d ; 01010000
	mov tmod,#17d ; 00010001
	setb tr0	
	setb tr1
	
	;acall CHKDRILL

EXECUTE:
	cjne r1,#0d,R1TRUE	;; check if r1 is zero, jump if not zero	
	cjne r0,#0d,KEEPLOOPIN	;; check if r0 is zero, jump if not zero
	clr tr0			;; stop timer 0 i.e. stop rotating X 
R1TRUE: cjne r6,#0d,R6TRUE	;; check if r6 is zero, jump if not zero
	cjne r5,#0d,KEEPLOOPIN	;; check if r5 is zero, jump if not zero
	clr tr1			;; stop timer 1 i.e. stop rotating Y
	ret			;; when both the timers are cleared, the processing should stop.
R6TRUE: jmp KEEPLOOPIN
KEEPLOOPIN: jmp EXECUTE

;; Timer overflow interrupt service routine
	org 000bh	;; timer 0
	call rotateX
	reti
	org 001bh 	;; timer 1
	call rotateY
	reti

rotateX: clr tf0
         clr tr0
	 ;; reset the timers to implement the required delay
	 mov th0,Tmr0H	
	 mov tl0,Tmr0L
	 ;; use port 2 for motor X   
	 setb tr0
	 mov P2,r2
	 mov a,r2
	 
	 jb b.1,XCLKRT	;; if b.1 set rotate CLK i.e. left
	 rrc a		;; if b.1 clear rotate ANTCLK i.e. right
	 jmp XDATASET
XCLKRT:  rlc a		;; rotate left clockwise
XDATASET:mov r2,a	; store bck in r2

	 djnz r4,retX
	 mov r4,#255d
	 djnz r0,retX	
	 mov a,r1	;; put the remainder in r4
	 mov r4,a
	 mov r0,#1d	;; put a single delay in r0 for one time only
	 mov r1,#0d
retX:	 ret

rotateY: clr tf1
         clr tr1
         mov tl1,Tmr1H 
	 mov th1,Tmr1L
	 setb tr1
	 ;; use port 1 for motor Y
	 mov P1,r3 
	 mov a,r3
	 jb b.2,YCLKRT	;; if b.2 set rotate CLK i.e. left
	 rr a		;; if b.2 clear rotate ANTCLK i.e. right
	 jmp YDATASET
YCLKRT:  rl a	
YDATASET:mov r3,a

	 djnz r7,retY
	 mov r7,#255d
	 djnz r5,retY
	 mov a,r6	;; put the remainder in r7
	 mov r7,a	
	 mov r5,#1d	;; put a single delay in r5 for one time only
	 mov r6,#0d	;; clear register r6
retY:	 ret

SEQUENCE: 
	clr C
	mov r2,#10010010b	;; use for motor X sequnecing 9 bit rotation
	mov r3,#10001000b	;; use for motor Y sequencing 8 bit rotation
	ret
end