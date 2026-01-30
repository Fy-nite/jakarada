; Example x86_64 assembly program
; Simple fibonacci calculation

_start:
    ; Initialize registers
    mov rax, 0
    mov rbx, 1
    mov rcx, 10
    
loop_start:
    ; Add rbx to rax
    add rax, rbx
    
    ; Check loop condition
    dec rcx
    cmp rcx, 0
    jne loop_start
    
    ; Exit program
    mov rax, 60          ; sys_exit
    xor rdi, rdi         ; exit code 0
    syscall

fibonacci:
    ; Calculate fibonacci
    push rbp
    mov rbp, rsp
    sub rsp, 16
    
    ; Store variables
    mov qword ptr [rbp-8], 0
    mov qword ptr [rbp-16], 1
    
    ; Get n from parameter
    mov rcx, [rbp+16]
    
fib_loop:
    cmp rcx, 0
    je fib_done
    
    ; Calculate next number
    mov rax, [rbp-8]
    mov rbx, [rbp-16]
    add rax, rbx
    
    ; Update variables
    mov [rbp-8], rbx
    mov [rbp-16], rax
    
    dec rcx
    jmp fib_loop
    
fib_done:
    mov rax, [rbp-16]
    add rsp, 16
    pop rbp
    ret

