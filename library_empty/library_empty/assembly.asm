.data
	src qword 0
	dst qword 0
	strife qword 0
	dstStrife qword 0
	srcWidth dword 0
	srcHeight dword 0
	dstHeight dword 0
	dstWidth dword 0
	zero dword 0
	comparisonResult dword 0
	refRow dword 0
	refCol dword 0
	nextRefCol dword 0
	nextRefRow dword 0
	f256 dword 256
	f1 dword 1
.code
zwrocNumber PROC
	mov rax, 5
	ret
zwrocNumber ENDP

zwrocPiksel PROC
	mov rax, 0
	mov al, byte ptr[rcx]
	ret
zwrocPiksel ENDP
	


interpolateAsm PROC
	mov src, rcx					;skopiuj 1. argument do src
	mov dst, rdx					;skopiuj 2. argument do dst
	mov rax, r8						;skopiuj 3. argument do rejestru rax
	mov [srcWidth], eax				;skopiuj zawartosc rejestru eax do srcWidth
	mov rax, r9						;skopiuj 4. argument do rejestru rax
	mov [srcHeight], eax			;skopiuj zawartosc rejestru eax do srcHeight
	mov eax, dword ptr[rsp+40]		;skopiuj 5. argument do eax ze stosu
	mov dstWidth, eax				;skopiuj zawartosc rej. eax do dstWidth
	mov eax, dword ptr[rsp+48]		;skopiuj 6. argument do eax ze stosu
	mov dstHeight, eax				;skopiuj zawartosc rej. eax do dstHeight

	mov rax, dst
	sub rax, 4
	mov rcx, 0
	mov ecx, dstWidth
	shl rcx, 2
	sub rax, rcx
	mov dst, rax

	mov rax, src
	sub rax, 4
	mov ecx, srcWidth
	shl rcx, 2
	sub rax, rcx
	mov src, rax

	cvtsi2ss xmm10, dstWidth			;xmm10 = 0 dstWidth
	cvtsi2ss xmm11, dstHeight		;xmm11 = 0 dstHeight
	cvtsi2ss xmm8, srcWidth			;xmm8 = 0 srcWidth
	cvtsi2ss xmm9, srcHeight		;xmm9 = 0 srcHeight
	unpcklps xmm9, xmm8				;xmm9  = srcWidth srcHeight
	unpcklps xmm11, xmm10			;xmm11 = dstWidth dstHeight
	movups xmm8, xmm9
	divps xmm9, xmm11				;xmm9 = srcWidth/dstWidth srcHeight/dstWidth				

	mov rcx, 0
	mov ecx, 0
row_loop:
	add ecx, 1
	;;;;;;;;;;;;;;
	mov rax, rcx				;[rax] = row
		
	cvtsi2ss xmm10, ecx			;refRowPos(xmm10) = row(ecx)
	push rcx
	
	mov rcx,0					
	mov ecx, dstWidth			;[edx] = dstWidth
	shl ecx, 2					;[edx] *= 4
	mul rcx						;[rax] = row * edx
	mov dstStrife, rax			; dstStrife = [rax]


	;;koniec dodania
	mulss xmm10, xmm9			;refRowPos *= scaleH(xmm9L)
	cvttss2si edx, xmm10		;refRow(edx) = floor(refRowPos)
	cmp edx, 1
	jnl row_loop_1
	mov edx, 1
row_loop_1:
	cmp edx, srcHeight
	jng row_loop_2
	mov edx, srcHeight
row_loop_2:
	cvtsi2ss xmm11, edx			;refRow(xmm11)
	mov refRow, edx				;save refRow to memory
	subss xmm10, xmm11			;deltaRow(xmm10) = refRowPos(xmm10) - refRow(xmm11)
	movss xmm12, xmm10			;isLesserThanZero(xmm12) = deltaRow(xmm10)
	cmpss xmm12, zero, 1		;isLesserThanZero = deltaRow >= 0
	movss comparisonResult, xmm12	;comparizonResult = isLesserThanZero
	cmp comparisonResult, 0
	jz row_loop_3				;if comparisonResult == 0 {
	mov ecx, 0					;ecx = 0
	cvtsi2ss xmm10, ecx			;deltaRow(xmm10) = ecx
row_loop_3:						;}
	
	shufps xmm9, xmm13, 00010001b ;xmm9 = scaleH scaleW

	mov rcx, 0
	mov ecx, 0					;col[ecx] = 0
column_loop:
	add ecx, 1					;col++

	cvtsi2ss xmm12, ecx			;refColPos(xmm12) = col(ecx)
	push rcx
	mulss xmm12, xmm9			;refColPos[xmm12] = refColPos* scaleW
	cvttss2si edx, xmm12		;refCol[edx] = floor(refColPos)
	cmp edx, 1					;if(refCol < 1)
	jnl col_loop_1				;{
	mov edx, 1					;	refCol = 1
col_loop_1:						;}
	cmp edx, srcWidth			;if(refCol > srcWidth)
	jng col_loop_2				;{
	mov edx, srcWidth			;	refCol = srcWidth
col_loop_2:						;}
	cvtsi2ss xmm8, edx				;[xmm8] = refCol(edx)
	mov refCol, edx					;refCol(mem) = [edx]
	subss xmm12, xmm8				;deltaCol(xmm12) -= refCol(xmm8)
	movss xmm13, xmm12				;[xmm13] = deltaCol(xmm12)
	cmpss xmm13, zero, 1			;isLesserThanZero(xmm13) = deltaCol >= 0
	movss comparisonResult, xmm13	;comparisonResult(mem) = [xmm13]
	cmp comparisonResult, 0			;if comparisonResult == 0
	jz col_loop_4					;{
	mov ecx, 0						;	[ecx] = 0
	cvtsi2ss xmm12, ecx				;	deltaCol(xmm12) = [ecx]
col_loop_4:							;}
	
	mov eax, refCol					;[eax] = refCol(mem)
	cmp eax, srcWidth				;if refCol(eax) < srcWidth
	jnl col_loop_5					;{
	add eax, 1						;	[eax] = [eax] + 1 
col_loop_5:							;}
	mov nextRefCol, eax				;nextRefCol(mem) = [eax]
	mov eax, refRow					;[eax] = refRow(mem)
	cmp eax, srcHeight				;if refRow(eax) < srcHeight 
	jnl col_loop_6					;{
	add eax, 1						;	[eax] = [eax] + 1
col_loop_6:							;}
	mov nextRefRow, eax				;rextRefRow(mem) = [eax]
	mov rax, 0						;[rax] = 0
	mov eax, srcWidth				;[eax] = srcWidth
	shl eax, 2						;[rax] << 2 //4 bytes per pixel
	mov rbx, 0						;[rbx] = 0
	mov ebx, refRow					;[ebx] = refRow
	mul rbx							;[rax] = [rax] * [rbx]
	mov strife, rax					;strife(mem) = [rax]

	mov rax, src					;[rax] = src
	add rax, strife					;[rax] += strife(mem)
	mov rbx, 0						;[rbx] = 0
	mov ebx, refCol					;[ebx] = refCol
	shl rbx, 2						;[rbx] << 2
	movd xmm0, dword ptr[rax+rbx]	;[xmm0] = src[refCol*4 + strife]
	
	mov rbx, 0						;[rbx] = 0
	mov ebx, nextRefCol				;[ebx] = nextRefCol
	shl rbx, 2						;nextRefCol(ebx) *= 4
	movd xmm1, dword ptr[rax+rbx]	;[xmm1] = src[nextRefCol*4 + strife]
	punpckldq xmm0, xmm1			;[xmm0] = 0 0 xmm1[31:0] xmm0[31:0]


	mov rax, 0						;[rax] = 0				
	mov eax, srcWidth				;[eax] = srcWidth
	shl rax, 2						;srcWidth(eax) *= 4
	mov rbx, 0						;[rbx] = 0
	mov ebx, nextRefRow				;[ebx] = nextRefRow
	mul rbx							;[rax] = [rax] * [rbx]
	mov strife, rax					;strife(mem) = [rax]

	mov rax, src					;[rax] = src
	add rax, strife					;[rax] += strife(mem)
	mov rbx, 0						;[rbx] = 0
	mov ebx, refCol					;[ebx] = refCol
	shl rbx, 2						;refCol(rbx) *= 4
	movd xmm1, dword ptr[rax+rbx]	;[xmm1] = src[refCol*4 + strife]

	mov rbx, 0						;[rbx] = 0
	mov ebx, nextRefCol				;[ebx] = nextRefCol
	shl rbx, 2						;[rbx] *= 4
	movd xmm2, dword ptr[rax+rbx]	;[xmm2] = src[nextRefCol*4 + strife]
	punpckldq xmm1, xmm2			;[xmm1] = 0 0 xmm2[31:0] xmm1[31:0]

	punpcklbw xmm0, xmm1			;wstawiaj na zmiane bajty z xmm0 i xmm1 do xmm0
	movd xmm2, zero					;[xmm2] = 0
	movapd xmm3, xmm0				;[xmm3] = [xmm0]
	punpckhqdq xmm3, xmm2			;ustawienie wartosci kolorow z wyzszych na nizsze 64 bity rejestru xmm3
	punpcklbw xmm0, xmm3			;wstawiaj na zmiane bajty z xmm0 i xmm3 do xmm0 - w efekcie otrzymaj AAAA RRRR GGGG BBBB


	movapd xmm1, xmm0				;[xmm1] = [xmm0]
	punpcklbw xmm0, xmm2			;konwersja kanalow kolorow z 8 na 16bit - w efekcie [xmm0] = G4 G3 G2 G1 B4 B3 B2 B1
	punpckhbw xmm1, xmm2			;konwersja kanalow kolorow z 8 na 16bit(bity 127-64) - w efekcie [xmm1] = A4 A3 A2 A1 R4 R3 R2 R1

	cvtsi2ss xmm2, f1				;[xmm2] = 0 0 0 1
	movapd xmm3, xmm2				;[xmm3] = 0 0 0 1
	subss xmm2, xmm12				;[xmm2] = 0 0 0 1-deltaCol
	punpckldq xmm2, xmm12			;[xmm2] = 0 0 deltaCol 1-deltaCol
	punpcklqdq xmm2, xmm2			;[xmm2] = deltaCol 1-deltaCol deltaCol 1-deltaCol

	subss xmm3, xmm10				;[xmm3] = 0 0 0 1-deltaRow
	punpckldq xmm3, xmm3			;[xmm3] = 0 0 1-deltaRow 1-deltaRow
	movapd xmm4, xmm10				;[xmm4] = 0 0 0 deltaRow
	punpckldq xmm4, xmm4			;[xmm4] = 0 0 deltaRow deltaRow
	punpcklqdq xmm3, xmm4			;[xmm3] = deltaRow deltaRow	1-deltaRow 1-deltaRow		


	mulps xmm2, xmm3				;[xmm2] = [xmm2] * [xmm3] - wektor pomnozonych przez siebie wag

	cvtsi2ss xmm3, f256				;[xmm3] = 0 0 0 256
	punpckldq xmm3, xmm3			;[xmm3] = 0 0 256 256
	punpcklqdq xmm3, xmm3			;[xmm3] = 256 256 256 256

	mulps xmm2, xmm3				;[xmm2] =*= [xmm3] - wagi pomnozyc przez 256
	cvtps2dq xmm2, xmm2				;konwertuj wartosci w xmm2 ze zmiennioprzecinkowych 32bit na calkowite 32bit
	packssdw xmm2, xmm2				;konwertuj 4 32bit wartosci w xmm2 na 8 wartosci 16bit

	pmaddwd xmm0, xmm2				;[xmm0] *= [xmm2] - pomnozenie wartosci pikseli przez odpowiadajace im wagi,
									;nastepnie dodanie wartosci sasiednich - w efekcie [xmm0] = G G B B
	pmaddwd xmm1, xmm2				;[xmm1] *= [xmm2] - pomnozenie wartosci pikseli przez odpowiadajace im wagi,
									;nastepnie dodanie wartosci sasiednich - w efekcie [xmm1] = A A R R

	phaddd xmm0, xmm1               ;dodanie sasiednich wartosci w xmm0 i xmm1, a nastepnie wstawienie na zmiane
									;do xmm0 - efekt - [xmm0] = A R G B
	psrld xmm0, 8					;[xmm0]/256 - otrzymanie wart z zakresu 0-255

	movd xmm1, zero					;[xmm1] = 0 0 0 0
	packusdw xmm0, xmm1				;konwersja 32bit na 16bit w xmm0
	packuswb xmm0, xmm1				;konwersja 16bit na 8 bit w xmm0

	;obliczenie pozycji nowego koloru
	pop rax							;[rax] <- stos(col)
	mov rbx, rax					;[rbx] = rax
	push rax						;col(rax) -> stos
	shl rbx, 2						;col(rbx) *= 4
	add rbx, dstStrife				;[rbx] += dstStrife(mem)
	add rbx, dst					;[rbx] += dst
	movd dword ptr [rbx], xmm0		;src[col*4 + dstStrife] = [xmm0]


	pop rcx							;[rcx] <- stos(col)
	cmp ecx, dstWidth				;if col(ecx) < dstWidth {
	jl column_loop					;	skoncz na pocz¹tek pêtli }
		

	shufps xmm9, xmm13, 00010001b	;[xmm9] = scaleW scaleH 
	pop rcx							;[rcx] <- stos(row)
	cmp ecx, dstHeight				;if row(ecx) < stdHeight {
	jl row_loop						; skocz na poczatek petli }


	mov rax, 0						;return 0
	ret
interpolateAsm ENDP


end