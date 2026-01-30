# Jakarada

Jakarada is an x86_64 assembly code reader and parser that converts assembly language into an Abstract Syntax Tree (AST). This AST can be used for virtual machines, emulators, code analysis, and other tools.

## Features

- **Lexical Analysis**: Tokenizes x86_64 assembly code
- **Parsing**: Builds an AST from assembly instructions
- **Comprehensive Support**:
  - All common x86_64 registers (64-bit, 32-bit, 16-bit, 8-bit)
  - Register operands
  - Immediate values (decimal and hexadecimal)
  - Memory operands with base, index, scale, and displacement
  - Labels and label references
  - Size specifiers (byte ptr, word ptr, dword ptr, qword ptr)
  - Comments
- **Visitor Pattern**: Easy traversal and transformation of the AST
- **Clean API**: Simple interface for parsing assembly code

## Project Structure

```
jakarada/
├── src/
│   ├── Jakarada.Core/         # Core library with lexer, parser, and AST
│   └── Jakarada.CLI/          # Command-line interface
├── tests/
│   └── Jakarada.Tests/        # Unit tests
└── Jakarada.sln               # Solution file
```

## Getting Started

### Prerequisites

- .NET 10.0 or later

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Using the CLI

Run the demo:
```bash
dotnet run --project src/Jakarada.CLI/Jakarada.CLI.csproj -- --demo
```

Parse an assembly file:
```bash
dotnet run --project src/Jakarada.CLI/Jakarada.CLI.csproj -- mycode.asm
```

## Usage

### Basic Example

```csharp
using Jakarada.Core;
using Jakarada.Core.Visitors;

// Parse assembly code
var assemblyCode = @"
main:
    mov rax, 0x10
    add rax, rbx
    ret
";

var ast = AssemblyReader.Parse(assemblyCode);

// Print the AST
var printer = new AstPrinterVisitor();
Console.WriteLine(printer.Visit(ast));

// Access instructions programmatically
foreach (var instruction in ast.Instructions)
{
    Console.WriteLine($"Mnemonic: {instruction.Mnemonic}");
    foreach (var operand in instruction.Operands)
    {
        Console.WriteLine($"  Operand: {operand.GetType().Name}");
    }
}
```

### AST Structure

The AST consists of several node types:

- **ProgramNode**: Root node containing all instructions and labels
- **InstructionNode**: Represents a single assembly instruction
- **RegisterOperand**: Register operand (e.g., RAX, RBX)
- **ImmediateOperand**: Immediate value (e.g., 42, 0x10)
- **MemoryOperand**: Memory reference (e.g., [RAX], [RBP+8])
- **LabelNode**: Label definition
- **LabelReferenceOperand**: Label reference in jump/call instructions

### Implementing a Custom Visitor

```csharp
using Jakarada.Core.AST;

public class MyCustomVisitor : IAstVisitor<string>
{
    public string Visit(ProgramNode node)
    {
        // Process program
        foreach (var instruction in node.Instructions)
        {
            instruction.Accept(this);
        }
        return "Processed";
    }

    public string Visit(InstructionNode node)
    {
        // Process instruction
        return $"Instruction: {node.Mnemonic}";
    }

    // Implement other Visit methods...
}
```

## Supported Instructions

Jakarada can parse any x86_64 instruction mnemonic. Common examples include:

- Data movement: MOV, PUSH, POP, LEA
- Arithmetic: ADD, SUB, MUL, DIV, INC, DEC
- Logical: AND, OR, XOR, NOT
- Control flow: JMP, JE, JNE, CALL, RET
- And many more...

## Use Cases

- **Virtual Machines**: Build emulators that execute x86_64 assembly
- **Code Analysis**: Analyze assembly code for patterns or vulnerabilities
- **Disassemblers**: Convert machine code to assembly and then to AST
- **Debuggers**: Parse and display assembly instructions
- **Education**: Learn about assembly language structure and parsing

## License

This project is open source. See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

