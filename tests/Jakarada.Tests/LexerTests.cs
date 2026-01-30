using Xunit;
using Jakarada.Core;
using Jakarada.Core.AST;

namespace Jakarada.Tests;

public class LexerTests
{
    [Fact]
    public void Should_Tokenize_Simple_Instruction()
    {
        var code = "mov rax, rbx";
        var ast = AssemblyReader.Parse(code);
        
        Assert.Single(ast.Instructions);
        Assert.Equal("MOV", ast.Instructions[0].Mnemonic);
        Assert.Equal(2, ast.Instructions[0].Operands.Count);
    }

    [Fact]
    public void Should_Parse_Register_Operands()
    {
        var code = "mov rax, rbx";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        Assert.IsType<RegisterOperand>(instruction.Operands[0]);
        Assert.IsType<RegisterOperand>(instruction.Operands[1]);
        
        var reg1 = (RegisterOperand)instruction.Operands[0];
        var reg2 = (RegisterOperand)instruction.Operands[1];
        
        Assert.Equal("rax", reg1.Name);
        Assert.Equal("rbx", reg2.Name);
    }

    [Fact]
    public void Should_Parse_Immediate_Operands()
    {
        var code = "mov rax, 42";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        Assert.IsType<RegisterOperand>(instruction.Operands[0]);
        Assert.IsType<ImmediateOperand>(instruction.Operands[1]);
        
        var imm = (ImmediateOperand)instruction.Operands[1];
        Assert.Equal(42, imm.Value);
        Assert.False(imm.IsHex);
    }

    [Fact]
    public void Should_Parse_Hex_Immediate_Operands()
    {
        var code = "mov rax, 0x10";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        var imm = (ImmediateOperand)instruction.Operands[1];
        
        Assert.Equal(16, imm.Value);
        Assert.True(imm.IsHex);
    }

    [Fact]
    public void Should_Parse_Memory_Operand_With_Base_Register()
    {
        var code = "mov rax, [rbx]";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        Assert.IsType<MemoryOperand>(instruction.Operands[1]);
        
        var mem = (MemoryOperand)instruction.Operands[1];
        Assert.Equal("rbx", mem.BaseRegister);
    }

    [Fact]
    public void Should_Parse_Memory_Operand_With_Displacement()
    {
        var code = "mov rax, [rbp+8]";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        var mem = (MemoryOperand)instruction.Operands[1];
        
        Assert.Equal("rbp", mem.BaseRegister);
        Assert.Equal(8, mem.Displacement);
    }

    [Fact]
    public void Should_Parse_Memory_Operand_With_Index_And_Scale()
    {
        var code = "mov rax, [rbx+rcx*4]";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        var mem = (MemoryOperand)instruction.Operands[1];
        
        Assert.Equal("rbx", mem.BaseRegister);
        Assert.Equal("rcx", mem.IndexRegister);
        Assert.Equal(4, mem.Scale);
    }

    [Fact]
    public void Should_Parse_Label()
    {
        var code = "main:\n    mov rax, rbx";
        var ast = AssemblyReader.Parse(code);
        
        Assert.Single(ast.Labels);
        Assert.Equal("main", ast.Labels[0].Name);
    }

    [Fact]
    public void Should_Parse_Label_Reference()
    {
        var code = "jmp main";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        Assert.IsType<LabelReferenceOperand>(instruction.Operands[0]);
        
        var labelRef = (LabelReferenceOperand)instruction.Operands[0];
        Assert.Equal("main", labelRef.LabelName);
    }

    [Fact]
    public void Should_Parse_Multiple_Instructions()
    {
        var code = @"
mov rax, 10
add rax, rbx
sub rsp, 16
";
        var ast = AssemblyReader.Parse(code);
        
        Assert.Equal(3, ast.Instructions.Count);
        Assert.Equal("MOV", ast.Instructions[0].Mnemonic);
        Assert.Equal("ADD", ast.Instructions[1].Mnemonic);
        Assert.Equal("SUB", ast.Instructions[2].Mnemonic);
    }

    [Fact]
    public void Should_Parse_Comments()
    {
        var code = "mov rax, rbx ; move rbx to rax";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        Assert.NotNull(instruction.Comment);
        Assert.Contains("move rbx to rax", instruction.Comment);
    }

    [Fact]
    public void Should_Parse_Size_Specifiers()
    {
        var code = "mov qword ptr [rbp-8], rax";
        var ast = AssemblyReader.Parse(code);
        
        var instruction = ast.Instructions[0];
        var mem = (MemoryOperand)instruction.Operands[0];
        
        Assert.Equal(64, mem.Size);
    }
}
