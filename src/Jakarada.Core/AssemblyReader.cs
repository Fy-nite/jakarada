using Jakarada.Core.AST;
using Jakarada.Core.Lexer;
using Jakarada.Core.Parser;

namespace Jakarada.Core;

/// <summary>
/// Main entry point for parsing x86_64 assembly code
/// </summary>
public class AssemblyReader
{
    /// <summary>
    /// Parses x86_64 assembly code and returns an AST
    /// </summary>
    /// <param name="assemblyCode">The assembly code to parse</param>
    /// <returns>A ProgramNode representing the parsed assembly</returns>
    public static ProgramNode Parse(string assemblyCode)
    {
        // Tokenize
        var lexer = new AssemblyLexer(assemblyCode);
        var tokens = lexer.Tokenize();
        
        // Parse
        var parser = new AssemblyParser(tokens);
        return parser.Parse();
    }

    /// <summary>
    /// Parses x86_64 assembly code from a file and returns an AST
    /// </summary>
    /// <param name="filePath">Path to the assembly file</param>
    /// <returns>A ProgramNode representing the parsed assembly</returns>
    public static ProgramNode ParseFile(string filePath)
    {
        var code = File.ReadAllText(filePath);
        return Parse(code);
    }
}
