.data											;segment data
	dstHeight dword 0							;wysokosc obrazu docelowego
	dstWidth dword 0							;szerokosc obrazu docelowego
	zero dword 0								;zdefiniowane 0
	f256 dword 256								;zdefiniowane 256
	f1 dword 1									;zdefiniowana 1
.code

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;	Funkcja skaluje obraz wykorzystujac metode interpolacji dwuliniowej
;
;	@param rcx  - src - wskaznik na tablice pikseli zrodlowego obrazu
;	@param rdx  - dst - wskaznik na tablice pikseli docelowego obrazu
;	@param r8   - srcWidth - szeroko�� obrazu �r�d�owego
;	@param r9   - srcHeight - wysoko�� obrazu �r�d�owego
;	@param stos - dstWidth - szeroko�� obrazu docelowego
;	@param stos - dstHeight - wysoko�� obrazu docelowego
;	@return int - 0 gdy proces si� zako�czy
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
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

	subss xmm12, xmm8							;xmm12(deltaCol) -= xmm8(refCol)
	movss xmm13, xmm12							;xmm13 = xmm12(deltaCol)
	cmpss xmm13, zero, 1						;xmm13 = xmm12(deltaCol) >= 0
	movd ecx, xmm13								;ecx(comparisonResult) = xmm13
	cmp ecx, 0									;if(ecx(comparisonResult) == 0)
	jz col_loop_4								;{
	mov ecx, 0									;	ecx(deltaCol) = 0
	cvtsi2ss xmm12, ecx							;	xmm12(deltaCol) = ecx(deltaCol)
col_loop_4:										;}
	movhlps xmm7, xmm0							;xmm7 = xmm0H(srcWidth)
	movd edx, xmm7								;edx = xmm7(srcWidth)

	movd eax, xmm15								;eax = xmm15(refCol)
	cmp eax, edx								;if eax(refCol)eax < edx(srcWidth)
	jnl col_loop_5								;{
	add eax, 1									;	eax = eax + 1 
col_loop_5:										;}

	movd xmm7, eax								;xmm7 = eax(nextRefCol)
	movlhps xmm15, xmm7							;xmm15H = xmm7L(nextRefCol)

	movd eax, xmm14								;eax = xmm14(refRow)

	movd edx, xmm0								;edx = xmm0(srcHeight)
	cmp eax, edx								;if eax(refRow) < edx(srcHeight)
	jnl col_loop_6								;{
	add eax, 1									;	eax = eax + 1
col_loop_6:										;}
	movd xmm7, eax								;xmm7 = eax(nextRefRow)
	movlhps xmm14, xmm7							;xmm14H = xmm7L(nextRefRow)
	
	mov rax, 0									;rax = 0
	movhlps xmm7, xmm0							;xmm7 = xmm0H(srcWidth)
	movd eax, xmm7								;eax = xmm7(srcWidth)
	shl eax, 2									;eax << 2 //4 bajty na piksel
	mov rbx, 0									;rbx = 0

	;mov ebx, refRow					;[ebx] = refRow
	movd ebx, xmm14								;ebx = xmm14(refRow)

	mul rbx										;rax = rax * rbx
	mov rbx, rax								;rbx(strife) = rax

	movd rax, xmm5								;rax = xmm5(src)
	add rax, rbx								;rax(src) += strife(rbx)
	mov rbx, 0									;rbx = 0
	;mov ebx, refCol						;[ebx] = refCol;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movd ebx, xmm15								;ebx = xmm15(refCol)
	shl rbx, 2									;rbx << 2

	movapd xmm8, xmm0							;xmm8 = xmm0(srcWidth srcHeight)
	movd xmm0, dword ptr[rax+rbx]				;xmm0 = src[refCol*4 + strife]
	
	mov rbx, 0									;rbx = 0

	movhlps xmm7, xmm15							;xmm7 = xmm15H(nextRefCol)
	movd ebx, xmm7								;ebx = xmm7(nextRefCol)

	;mov ebx, nextRefCol				;[ebx] = nextRefCol;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	shl rbx, 2									;nextRefCol(ebx) *= 4
	movd xmm1, dword ptr[rax+rbx]				;xmm1 = src[ebx(nextRefCol*4) + strife]
	punpckldq xmm0, xmm1						;xmm0 = 0 0 xmm1[31:0] xmm0[31:0]


	mov rax, 0									;rax = 0				
	;mov eax, srcWidth				;[eax] = srcWidth;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movhlps xmm7, xmm8							;xmm7 = xmm8H(srcWidth)
	movd eax, xmm7								;eax = xmm7(srcWidth)
	
	shl rax, 2									;eax(srcWidth) *= 4
	mov rbx, 0									;rbx = 0
	;mov ebx, nextRefRow				;[ebx] = nextRefRow;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movhlps xmm7, xmm14							;xmm7 = xmm14H(nextRefRow)
	movd ebx, xmm7								;ebx = xmm7(nextRefRow)

	mul rbx										;rax = rax * rbx
	mov rbx, rax								;rbx = rax(strife)

	movd rax, xmm5								;rax = xmm5(src)
	add rax, rbx								;rax += rbx(strife)
	mov rbx, 0									;rbx = 0
	;mov ebx, refCol					;[ebx] = refCol;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movd ebx, xmm15								;ebx = xmm15L(refCol)

	shl rbx, 2									;rbx(refCol) *= 4
	movd xmm1, dword ptr[rax+rbx]				;xmm1 = src[refCol*4 + strife]

	mov rbx, 0									;rbx = 0
	;mov ebx, nextRefCol				;[ebx] = nextRefCol;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movhlps xmm7, xmm15							;xmm7 = xmm15H(nextRefCol)
	movd ebx, xmm7								;ebx = xmm7(nextRefCol)
	
	shl rbx, 2									;rbx *= 4
	movd xmm2, dword ptr[rax+rbx]				;xmm2 = src[nextRefCol*4 + strife]
	punpckldq xmm1, xmm2						;xmm1 = 0 0 xmm2[31:0] xmm1[31:0]

	punpcklbw xmm0, xmm1						;wstawiaj na zmiane bajty z xmm0 i xmm1 do xmm0
	movd xmm2, zero								;xmm2 = 0
	movapd xmm3, xmm0							;xmm3 = xmm0
	punpckhqdq xmm3, xmm2						;ustawienie wartosci kolorow z wyzszych na nizsze 64 bity rejestru xmm3
	punpcklbw xmm0, xmm3						;wstawiaj na zmiane bajty z xmm0 i xmm3 do xmm0 - w efekcie otrzymaj AAAA RRRR GGGG BBBB


	movapd xmm1, xmm0							;xmm1 = xmm0
	punpcklbw xmm0, xmm2						;konwersja kanalow kolorow z 8 na 16bit - w efekcie xmm0 = G4 G3 G2 G1 B4 B3 B2 B1
	punpckhbw xmm1, xmm2						;konwersja kanalow kolorow z 8 na 16bit(bity 127-64) - w efekcie xmm1 = A4 A3 A2 A1 R4 R3 R2 R1

	cvtsi2ss xmm2, f1							;xmm2 = 0 0 0 1
	movapd xmm3, xmm2							;xmm3 = 0 0 0 1
	subss xmm2, xmm12							;xmm2 = 0 0 0 1-deltaCol
	punpckldq xmm2, xmm12						;xmm2 = 0 0 deltaCol 1-deltaCol
	punpcklqdq xmm2, xmm2						;xmm2 = deltaCol 1-deltaCol deltaCol 1-deltaCol

	subss xmm3, xmm10							;xmm3 = 0 0 0 1-deltaRow
	punpckldq xmm3, xmm3						;xmm3 = 0 0 1-deltaRow 1-deltaRow
	movapd xmm4, xmm10							;xmm4 = 0 0 0 deltaRow
	punpckldq xmm4, xmm4						;xmm4 = 0 0 deltaRow deltaRow
	punpcklqdq xmm3, xmm4						;xmm3 = deltaRow deltaRow	1-deltaRow 1-deltaRow		


	mulps xmm2, xmm3							;xmm2 = xmm2 * xmm3 - wektor pomnozonych przez siebie wag

	cvtsi2ss xmm3, f256							;xmm3 = 0 0 0 256
	punpckldq xmm3, xmm3						;xmm3 = 0 0 256 256
	punpcklqdq xmm3, xmm3						;xmm3 = 256 256 256 256

	mulps xmm2, xmm3							;xmm2 *= xmm3 - wagi pomnozyc przez 256
	cvtps2dq xmm2, xmm2							;konwertuj wartosci w xmm2 ze zmiennioprzecinkowych 32bit na calkowite 32bit
	packssdw xmm2, xmm2							;konwertuj 4 32bit wartosci w xmm2 na 8 wartosci 16bit

	pmaddwd xmm0, xmm2							;xmm0 *= xmm2 - pomnozenie wartosci pikseli przez odpowiadajace im wagi,
												;nastepnie dodanie wartosci sasiednich - w efekcie xmm0 = G G B B
	pmaddwd xmm1, xmm2							;xmm1 *= xmm2 - pomnozenie wartosci pikseli przez odpowiadajace im wagi,
												;nastepnie dodanie wartosci sasiednich - w efekcie xmm1 = A A R R

	phaddd xmm0, xmm1							 ;dodanie sasiednich wartosci w xmm0 i xmm1, a nastepnie wstawienie na zmiane
												;do xmm0 - efekt - [xmm0] = A R G B
	psrld xmm0, 8								;xmm0 /= 256 - otrzymanie wart z zakresu 0-255

	movd xmm1, zero								;xmm1 = 0 0 0 0
	packusdw xmm0, xmm1							;konwersja 32bit na 16bit w xmm0
	packuswb xmm0, xmm1							;konwersja 16bit na 8 bit w xmm0

												;obliczenie pozycji nowego koloru
	pop rax										;rax <- col(stos)
	mov rbx, rax								;rbx = rax
	push rax									;col(rax) -> stos
	shl rbx, 2									;rbx(col) *= 4
	
	movhlps xmm7, xmm6							;xmm7 = xmm6H(dstStrife)
	movd rax, xmm6								;rax = xmm6(dst)
	add rbx, rax								;rbx = rax(dst) + rbx(col*4)
	;add rbx, dstStrife				;[rbx] += dstStrife(mem);;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movd rax, xmm7								;rax = xmm7(dstStrife)
	add rbx, rax								;rbx = dst + dstStrife + col*4 - pozycja docelowego pikselu w pamieci
	;add rbx, dst					;[rbx] += dst;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	movd dword ptr [rbx], xmm0					;dst[col*4 + dstStrife] = xmm0

	movapd xmm0, xmm8							;xmm0 = xmm8(srcWidth srcHeight)
	pop rcx										;rcx <- col(stos)
	cmp ecx, dstWidth							;if col(ecx) < dstWidth {
	jl column_loop								;	skoncz na pocz�tek p�tli }
		

	shufps xmm9, xmm13, 00010001b				;xmm9 = scaleW scaleH 
	pop rcx										;rcx <- row(stos)
	cmp ecx, dstHeight							;if ecx(row) < dstHeight {
	jl row_loop									; skocz na poczatek petli }


	mov rax, 0									;rax = 0
	ret											;return rax(0)
interpolateAsm ENDP

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;	Funkcja sprawdza, czy procesur obsluguje instrukcje SSE4.1
;
;	@return int - 1, gdy procesor obs�ugje instrukcje
;			      0, gdy nie obs�uguje
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
checkForSSE PROC
	mov rax, 1
	cpuid										;pobranie informacji o procesorze
	shr ecx, 19									;przesuniecie ecx w taki sposob, zeby sse4.1 bylo na bicie 0
	and ecx, 1									;ecx = ecx & sse4.1bit
	mov eax, ecx
	ret
checkForSSE ENDP

end