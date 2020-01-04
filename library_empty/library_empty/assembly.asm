.data
	src qword 0
	dst qword 0
	srcWidth dword 0
	srcHeight dword 0
	dstHeight dword 0
	dstWidth dword 0
	zero dword 0
	comparisonResult dword 0
	refRow dword 0
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
	mov [srcHeight], eax				;skopiuj zawartosc rejestru eax do srcHeight
	mov eax, [rbp+32]				;skopiuj 5. argument do eax ze stosu
	mov dstWidth, eax				;skopiuj zawartosc rej. eax do dstWidth
	mov eax, [rbp+40]				;skopiuj 6. argument do eax ze stosu
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

	cvtsi2ss xmm10, dstWidth			;xmm8 = 0 dstWidth
	cvtsi2ss xmm11, dstHeight		;xmm9 = 0 dstHeight
	cvtsi2ss xmm8, srcWidth			;xmm10 = 0 srcWidth
	cvtsi2ss xmm9, srcHeight		;xmm11 = 0 srcHeight
	unpcklps xmm9, xmm8				;xmm9  = dstWidth srcHeight
	unpcklps xmm11, xmm10			;xmm11 = srcWidth srcHeight
	movups xmm8, xmm9
	divps xmm9, xmm11				;xmm9 = srcWidth/dstWidth srcHeight/dstWidth				

	mov rcx, 0
	mov ecx, 0
row_loop:
	add ecx, 1
	
	cvtsi2ss xmm10, ecx			;refRowPos(xmm10) = row(ecx)
	push rcx
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
	cmpss xmm12, zero, 1		;isLesserThanZero = deltaRow < 0
	movss comparisonResult, xmm12	;comparizonResult = isLesserThanZero
	cmp comparisonResult, 0
	jz row_loop_3				;if comparisonResult == 0 {
	mov ecx, 0					;ecx = 0
	cvtsi2ss xmm10, ecx			;deltaRow(xmm10) = ecx
row_loop_3:						;}
	

	pop rcx
	cmp ecx, dstHeight
	jl row_loop


	mov rax, 5
	ret
interpolateAsm ENDP


end