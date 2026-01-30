namespace Jakarada.Core.Lexer;

/// <summary>
/// Token types for assembly language
/// </summary>
public enum TokenType
{
    // Literals and identifiers
    Identifier,
    Register,
    Number,
    HexNumber,
    
    // Operators and delimiters
    Comma,
    Colon,
    LeftBracket,
    RightBracket,
    Plus,
    Minus,
    Asterisk,
    
    // Special
    Comment,
    NewLine,
    EndOfFile,
    
    // Size specifiers
    BytePtr,
    WordPtr,
    DwordPtr,
    QwordPtr
}
