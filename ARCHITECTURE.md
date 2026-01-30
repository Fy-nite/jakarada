# Jakarada Architecture

## Overview

Jakarada is a C# library for parsing x86_64 assembly language into an Abstract Syntax Tree (AST). The project follows a clean, modular architecture with clear separation of concerns.

## Project Structure

```
jakarada/
├── src/
│   ├── Jakarada.Core/          # Core parsing library
│   │   ├── AST/                # AST node definitions
│   │   ├── Lexer/              # Tokenization
│   │   ├── Parser/             # AST construction
│   │   ├── Visitors/           # AST traversal
│   │   ├── AssemblyReader.cs   # Public API
│   │   └── Exceptions.cs       # Custom exceptions
│   └── Jakarada.CLI/           # Command-line interface
├── tests/
│   └── Jakarada.Tests/         # Unit tests
└── examples/                   # Example assembly files
```

## Core Components

### 1. Lexer (Tokenization)

**Location**: `src/Jakarada.Core/Lexer/`

The lexer converts raw assembly text into a stream of tokens.

**Files**:
- `TokenType.cs` - Enumeration of all token types
- `Token.cs` - Token data structure with type, value, line, and column
- `AssemblyLexer.cs` - Main lexer implementation

**Supported Tokens**:
- Identifiers (mnemonics, labels)
- Registers (all x86_64 registers)
- Numbers (decimal and hexadecimal)
- Operators (`,`, `[`, `]`, `+`, `-`, `*`, `:`)
- Size specifiers (`byte ptr`, `word ptr`, `dword ptr`, `qword ptr`)
- Comments (`;` or `#`)

### 2. AST (Abstract Syntax Tree)

**Location**: `src/Jakarada.Core/AST/`

The AST represents the semantic structure of assembly code.

**Node Types**:
- `AstNode` - Base class for all nodes
- `ProgramNode` - Root node containing instructions and labels
- `InstructionNode` - Assembly instruction with mnemonic and operands
- `OperandNode` - Base class for operands
  - `RegisterOperand` - Register operand (e.g., RAX, RBX)
  - `ImmediateOperand` - Immediate value (e.g., 42, 0x10)
  - `MemoryOperand` - Memory reference (e.g., [RAX+8])
  - `LabelReferenceOperand` - Label reference (e.g., in JMP main)
- `LabelNode` - Label definition

**Design Patterns**:
- Visitor Pattern: `IAstVisitor<T>` interface for tree traversal
- Composite Pattern: Hierarchical node structure

### 3. Parser

**Location**: `src/Jakarada.Core/Parser/`

The parser builds the AST from the token stream.

**Files**:
- `AssemblyParser.cs` - Recursive descent parser

**Parsing Strategy**:
- Top-down parsing
- Predictive parsing using lookahead
- Context-free grammar for x86_64 assembly

**Supported Constructs**:
- Instructions with multiple operands
- Labels (standalone or inline with instruction)
- Complex memory addressing: `[base + index*scale + displacement]`
- Size specifiers
- Comments

### 4. Public API

**Location**: `src/Jakarada.Core/AssemblyReader.cs`

Simple, high-level API for parsing assembly code:

```csharp
// Parse from string
var ast = AssemblyReader.Parse(assemblyCode);

// Parse from file
var ast = AssemblyReader.ParseFile("code.asm");
```

### 5. Visitors

**Location**: `src/Jakarada.Core/Visitors/`

Implementations of the visitor pattern for AST traversal.

**Files**:
- `AstPrinterVisitor.cs` - Prints AST in human-readable format

**Extensibility**:
Users can implement custom visitors for:
- Code generation
- Optimization
- Analysis
- Transformation

## Data Flow

```
Assembly Code (String)
    ↓
Lexer (Tokenization)
    ↓
Token Stream
    ↓
Parser (Syntax Analysis)
    ↓
AST (Abstract Syntax Tree)
    ↓
Visitor (Traversal/Processing)
    ↓
Output (Code, Analysis, etc.)
```

## Example Usage

```csharp
using Jakarada.Core;
using Jakarada.Core.Visitors;

var code = @"
main:
    mov rax, 0x10
    add rax, rbx
    ret
";

// Parse
var ast = AssemblyReader.Parse(code);

// Print
var printer = new AstPrinterVisitor();
Console.WriteLine(printer.Visit(ast));

// Process
foreach (var instruction in ast.Instructions)
{
    Console.WriteLine($"{instruction.Mnemonic} with {instruction.Operands.Count} operands");
}
```

## Design Decisions

### 1. Separation of Concerns
- Lexer handles tokenization only
- Parser focuses on syntax and AST construction
- Visitors handle AST processing

### 2. Immutability
- Tokens are immutable once created
- AST nodes use property setters but are typically built once

### 3. Error Handling
- Custom exception types (`LexerException`, `ParserException`)
- Include line and column information
- Clear error messages

### 4. Performance
- Single-pass lexing
- Predictive parsing (minimal backtracking)
- Efficient string handling (using Range indexer `[..]`)

### 5. Extensibility
- Visitor pattern for custom processing
- Open/closed principle: easy to add new visitors
- Abstract base classes for extension points

## Testing Strategy

**Location**: `tests/Jakarada.Tests/`

**Coverage**:
- Lexer tests: tokenization of various constructs
- Parser tests: AST construction
- Integration tests: end-to-end parsing

**Test Cases**:
- Simple instructions
- Register operands
- Immediate operands (decimal and hex)
- Memory operands with various addressing modes
- Labels and label references
- Size specifiers
- Comments
- Complete programs

## Future Enhancements

Potential areas for expansion:
1. Support for assembler directives (`.section`, `.data`, etc.)
2. Macro expansion
3. Symbol table construction
4. Type checking for operand sizes
5. Code generation from AST
6. Optimization passes
7. Support for other architectures (ARM, RISC-V)
8. Integration with disassemblers

## Performance Characteristics

- **Time Complexity**: O(n) where n is the number of characters
- **Space Complexity**: O(n) for token storage and AST
- **Typical Performance**: Can parse thousands of instructions per second

## Dependencies

- .NET 10.0 or later
- xUnit (for testing)
- No third-party parsing libraries

## API Stability

The current API is version 1.0 and follows semantic versioning:
- Public classes and methods in `Jakarada.Core` namespace are stable
- Internal implementation details may change
- AST node structure is stable for visitor implementations
