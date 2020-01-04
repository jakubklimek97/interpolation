.data
	src qword 0
	dst qword 0
	srcWidth dword 0
	srcHeight dword 0
	newHeight dword 0
	newWidth dword 0
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
	
	mov rax, 5

	ret
interpolateAsm ENDP


end