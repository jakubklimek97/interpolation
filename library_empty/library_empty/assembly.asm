.data
	dstHeight dword 0
	dstWidth dword 0
	zero dword 0
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
	movd xmm5, rcx								;skopiuj 1. argument (src) do [xmm5]
	movd xmm6, rdx								;skopiuj 2. argument (dst) do [xmm6]
	mov rax, r8									;skopiuj 3. argument (srcWidth) do rejestru rax
	movd xmm7, eax								;xmm7 = srcWidth
	mov rax, r9									;skopiuj 4. argument (srcHeight) do rejestru rax
	movd xmm0, eax								;xmm0 = srcHeight
	movlhps xmm0, xmm7							;xmm0 = srcWidth srcHeight
	mov eax, dword ptr[rsp+40]					;skopiuj 5. argument (dstWidth) do eax ze stosu
	mov dstWidth, eax							;skopiuj zawartosc rej. eax do dstWidth
	mov eax, dword ptr[rsp+48]					;skopiuj 6. argument (dstHeight) do eax ze stosu
	mov dstHeight, eax							;skopiuj zawartosc rej. eax do dstHeight
									
												;sekcja przesuniecia wskaznika dst tak, by wiersze i kolumny zaczynaly sie od 1
	movd rax, xmm6								;rax = xmm6(dst)
	sub rax, 4									;rax -= 4 //przesuniecie o 1 piksel
	mov rcx, 0									;rcx = 0
	mov ecx, dstWidth							;ecx = dstWidth
	shl rcx, 2									;ecx << 2 //na kazda 1 kolumne przypadaja 4 wartosci (1 piksel)
	sub rax, rcx								;rax = rax - rcx				
	movd xmm6, rax								;xmm6(dst) = rax
									
												;sekcja przesuniecia wskaznika src tak, by wiersze i kolumny zaczynaly sie od 1
	movd rax, xmm5								;rax = xmm5(src)
	sub rax, 4									;rax -= 4 //przesuniecie o 1 piksel	
	mov rcx, 0									;rcx = 0
	movd ecx, xmm7								;ecx = xmm7(srcWidth)
	shl rcx, 2									;ecx << 2 //na kazda 1 kolumne przypadaja 4 wartosci (1 piksel)
	sub rax, rcx								;rax = rax - rcx
	movd xmm5, rax								;xmm5(src) = rax

												;ustalenie wspolczynnika skali (xmm9)
	cvtsi2ss xmm10, dstWidth					;xmm10 = dstWidth
	cvtsi2ss xmm11, dstHeight					;xmm11 = dstHeight
	mov rax, 0									;rax = 0
	movd eax, xmm7								;eax = xmm7(srcWidth)
	cvtsi2ss xmm8, eax							;xmm8 = eax(srcWidth)
	movd eax, xmm0								;eax = xmm0(srcHeight)
	cvtsi2ss xmm9, eax							;xmm9 = eax(srcHeight)
	
	unpcklps xmm9, xmm8							;xmm9  = xmm8(srcWidth) xmm9(srcHeight)
	unpcklps xmm11, xmm10						;xmm11 = xmm10(dstWidth) xmm11(dstHeight)
	;movups xmm8, xmm9							
	divps xmm9, xmm11							;xmm9 = srcWidth/dstWidth srcHeight/dstWidth			

	mov rcx, 0									;ecx(row) = 0
row_loop:										;petla po wierszach zaczyna sie tutaj
	add ecx, 1									;ecx(row) += 1
	mov rax, rcx								;rax = rcx(row)
		
	cvtsi2ss xmm10, ecx							;xmm10(refRowPos) = ecx(row)
	push rcx									;rcx -> stos
	
	mov rcx,0									;rcx = 0
	mov ecx, dstWidth							;edx = dstWidth
	shl ecx, 2									;edx << 2 //szerokosc * 4 = ilosc miejsca w pamieci jaka zajmuje wiersz
	mul rcx										;rcx(dstStrife) = rax(row) * rcx
	movd xmm7, rax								;xmm7 = rcx(dstStrife)
	movlhps xmm6, xmm7							;xmm6 = rcx(dstStrife) xmm6L(dst)

	mulss xmm10, xmm9							;xmm10(refRowPos) *= xmm9L(scaleHeight)
	cvttss2si edx, xmm10						;refRow(edx) = floor(refRowPos)
	cmp edx, 1									;if(refRow < 1)
	jnl row_loop_1								;{
	mov edx, 1									;	edx(refRow) = 1
row_loop_1:										;}
	movd eax, xmm0								;eax = xmm0L(srcHeight)
	cmp edx, eax								;if(edx(refRow) > eax(srcHeight))
	jng row_loop_2								;{
	movd edx, xmm0								;edx(refRow) = xmm0L(srcHeight)
row_loop_2:
	cvtsi2ss xmm11, edx							;xmm11 = edx(refRow)
	movd xmm14, edx								;xmm14 = edx(refRow)
	
	subss xmm10, xmm11							;xmm10(deltaRow) = xmm10(refRowPos) - xmm11(refRow)
	movss xmm12, xmm10							;xmm12 = xmm10(deltaRow)
	cmpss xmm12, zero, 1						;xmm12 = xmm12(deltaRow) >= 0
	movd ecx, xmm12	;							;ecx(comparisonResult) = xmm12
	cmp ecx, 0									;if(ecx(comparisonResult) == 0)
	jz row_loop_3								;{
	mov ecx, 0									;	ecx(deltaRow) = 0
	cvtsi2ss xmm10, ecx							;   xmm10(deltaRow) = ecx(deltaRow)
row_loop_3:										;}
	shufps xmm9, xmm13, 00010001b				;xmm9 = xmm9L(scaleHeight) xmm9H(scaleWidth)
	
	mov rcx, 0									;rcx(col) = 0;
column_loop:									;petla po kolumnach zaczyna sie tutaj
	add ecx, 1									;ecx(col) += 1
	cvtsi2ss xmm12, ecx							;xmm12(refColPos) = ecx(col)
	push rcx									;rcx -> stos
	mulss xmm12, xmm9							;xmm12(refColPos)= xmm12(refColPos) * xmm9L(scaleWidth)
	cvttss2si edx, xmm12						;edx(refCol) = floor(xmm12(refColPos))
	cmp edx, 1									;if(edx(refCol) < 1)
	jnl col_loop_1								;{
	mov edx, 1									;	refCol = 1
col_loop_1:										;}
	movhlps xmm7, xmm0							;xmm7 = xmm0H(srcWidth)
	movd eax, xmm7								;eax = xmm7(srcWidth)
	cmp edx, eax								;if(edx(refCol) > eax(srcWidth))
	jng col_loop_2								;{
	mov edx, eax								;	edx(refCol) = eax(srcWidth)
col_loop_2:										;}
	cvtsi2ss xmm8, edx							;xmm8 = edx(refCol)
	movd xmm15, edx								;xmm15 = edx(refCol)
	;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;


	subss xmm12, xmm8				;deltaCol(xmm12) -= refCol(xmm8)
	movss xmm13, xmm12				;[xmm13] = deltaCol(xmm12)
	cmpss xmm13, zero, 1			;isLesserThanZero(xmm13) = deltaCol >= 0
	movd ecx, xmm13					;comparisonResult(ecx) = [xmm13]
	cmp ecx, 0						;if comparisonResult == 0
	jz col_loop_4					;{
	mov ecx, 0						;	[ecx] = 0
	cvtsi2ss xmm12, ecx				;	deltaCol(xmm12) = [ecx]
col_loop_4:							;}
	;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movhlps xmm7, xmm0
	movd edx, xmm7					;[edx] = srcWidth

	movd eax, xmm15					;[eax] = refCol(mem)
	cmp eax, edx					;if refCol(eax) < srcWidth
	jnl col_loop_5					;{
	add eax, 1						;	[eax] = [eax] + 1 
col_loop_5:							;}
	;mov nextRefCol, eax				;nextRefCol(mem) = [eax]

	movd xmm7, eax
	movlhps xmm15, xmm7


	;mov eax, refRow					;[eax] = refRow(mem)
	movd eax, xmm14

	movd edx, xmm0
	cmp eax, edx				;if refRow(eax) < srcHeight 
	jnl col_loop_6					;{
	add eax, 1						;	[eax] = [eax] + 1
col_loop_6:							;}
;	mov nextRefRow, eax				;rextRefRow(mem) = [eax]
	movd xmm7, eax
	movlhps xmm14, xmm7
	
	mov rax, 0						;[rax] = 0
	movhlps xmm7, xmm0
	movd eax, xmm7
	;mov eax, srcWidth				;[eax] = srcWidth
	shl eax, 2						;[rax] << 2 //4 bytes per pixel
	mov rbx, 0						;[rbx] = 0

	;mov ebx, refRow					;[ebx] = refRow
	movd ebx, xmm14

	mul rbx							;[rax] = [rax] * [rbx]
	mov rbx, rax					;strife(rbx) = [rax]

	movd rax, xmm5					;[rax] = src
	add rax, rbx					;[rax] += strife(rbx)
	mov rbx, 0						;[rbx] = 0
	;mov ebx, refCol					;[ebx] = refCol
	movd ebx, xmm15
	shl rbx, 2						;[rbx] << 2

	movapd xmm8, xmm0
	movd xmm0, dword ptr[rax+rbx]	;[xmm0] = src[refCol*4 + strife]
	
	mov rbx, 0						;[rbx] = 0

	movhlps xmm7, xmm15
	movd ebx, xmm7

	;mov ebx, nextRefCol				;[ebx] = nextRefCol
	shl rbx, 2						;nextRefCol(ebx) *= 4
	movd xmm1, dword ptr[rax+rbx]	;[xmm1] = src[nextRefCol*4 + strife]
	punpckldq xmm0, xmm1			;[xmm0] = 0 0 xmm1[31:0] xmm0[31:0]


	mov rax, 0						;[rax] = 0				
	;mov eax, srcWidth				;[eax] = srcWidth
	movhlps xmm7, xmm8
	movd eax, xmm7
	
	shl rax, 2						;srcWidth(eax) *= 4
	mov rbx, 0						;[rbx] = 0
	;mov ebx, nextRefRow				;[ebx] = nextRefRow
	movhlps xmm7, xmm14
	movd ebx, xmm7

	mul rbx							;[rax] = [rax] * [rbx]
	mov rbx, rax					;strife(rbx) = [rax]

	movd rax, xmm5					;[rax] = src
	add rax, rbx					;[rax] += strife(rbx)
	mov rbx, 0						;[rbx] = 0
	;mov ebx, refCol					;[ebx] = refCol
	movd ebx, xmm15

	shl rbx, 2						;refCol(rbx) *= 4
	movd xmm1, dword ptr[rax+rbx]	;[xmm1] = src[refCol*4 + strife]

	mov rbx, 0						;[rbx] = 0
	;mov ebx, nextRefCol				;[ebx] = nextRefCol
	movhlps xmm7, xmm15
	movd ebx, xmm7
	
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
	;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movhlps xmm7, xmm6
	movd rax, xmm6
	add rbx, rax
	;add rbx, dstStrife				;[rbx] += dstStrife(mem)
	movd rax, xmm7
	add rbx, rax
	;add rbx, dst					;[rbx] += dst
	movd dword ptr [rbx], xmm0		;dst[col*4 + dstStrife] = [xmm0]

	movapd xmm0, xmm8				;[xmm0] = srcWidth srcHeight
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