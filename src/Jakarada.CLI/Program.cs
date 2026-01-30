using Jakarada.Core;
using Jakarada.Core.Visitors;

if (args.Length == 0)
{
    Console.WriteLine("Jakarada - x86_64 Assembly Parser");
    Console.WriteLine();
    Console.WriteLine("Usage: jakarada <assembly-file>");
    Console.WriteLine("   or: jakarada --demo");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  jakarada code.asm        Parse assembly file and display AST");
    Console.WriteLine("  jakarada --demo          Run demo with sample assembly code");
    return;
}

if (args[0] == "--demo")
{
    RunDemo();
}
else
{
    ParseFile(args[0]);
}

static void RunDemo()
{
    Console.WriteLine("=== Jakarada Demo ===\n");
    
    var sampleCode = @"
; Sample x86_64 assembly code
main:
    mov rax, 0x10
    add rax, rbx
    sub rsp, 16
    mov qword ptr [rbp-8], rax
    call my_function
    jmp end

my_function:
    push rbp
    mov rbp, rsp
    mov rax, [rbp+16]
    pop rbp
    ret

end:
    xor rax, rax
    ret
";

    Console.WriteLine("Input Assembly Code:");
    Console.WriteLine("-------------------");
    Console.WriteLine(sampleCode);
    Console.WriteLine();
    
    var ast = AssemblyReader.Parse(sampleCode);
    var printer = new AstPrinterVisitor();
    
    Console.WriteLine("Generated AST:");
    Console.WriteLine("-------------");
    Console.WriteLine(printer.Visit(ast));
    
    Console.WriteLine("\nInstructions parsed: " + ast.Instructions.Count);
    Console.WriteLine("Labels found: " + ast.Labels.Count);
}

static void ParseFile(string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Error: File '{filePath}' not found.");
        return;
    }
    
    try
    {
        var ast = AssemblyReader.ParseFile(filePath);
        var printer = new AstPrinterVisitor();
        
        Console.WriteLine(printer.Visit(ast));
        Console.WriteLine($"\nInstructions parsed: {ast.Instructions.Count}");
        Console.WriteLine($"Labels found: {ast.Labels.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing file: {ex.Message}");
    }
}
