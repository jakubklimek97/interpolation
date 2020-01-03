.data

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
	
end