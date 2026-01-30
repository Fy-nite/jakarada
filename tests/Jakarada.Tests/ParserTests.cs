using Xunit;
using Jakarada.Core;
using Jakarada.Core.Visitors;

namespace Jakarada.Tests;

public class ParserTests
{
    [Fact]
    public void Should_Parse_Complete_Function()
    {
        var code = @"
main:
    push rbp
    mov rbp, rsp
    sub rsp, 16
    mov qword ptr [rbp-8], 42
    mov rax, [rbp-8]
    add rsp, 16
    pop rbp
    ret
";
        var ast = AssemblyReader.Parse(code);
        
        Assert.Single(ast.Labels);
        Assert.Equal("main", ast.Labels[0].Name);
        Assert.True(ast.Instructions.Count > 0);
    }

    [Fact]
    public void Should_Parse_Jump_Instructions()
    {
        var code = @"
start:
    cmp rax, rbx
    je equal
    jmp end
equal:
    mov rcx, 1
end:
    ret
";
        var ast = AssemblyReader.Parse(code);
        
        Assert.Equal(3, ast.Labels.Count);
        Assert.Contains(ast.Labels, l => l.Name == "start");
        Assert.Contains(ast.Labels, l => l.Name == "equal");
        Assert.Contains(ast.Labels, l => l.Name == "end");
    }

    [Fact]
    public void Should_Print_AST()
    {
        var code = @"
main:
    mov rax, 10
    add rax, rbx
";
        var ast = AssemblyReader.Parse(code);
        var printer = new AstPrinterVisitor();
        var output = printer.Visit(ast);
        
        Assert.NotNull(output);
        Assert.Contains("Program:", output);
        Assert.Contains("MOV", output);
        Assert.Contains("ADD", output);
    }
}
