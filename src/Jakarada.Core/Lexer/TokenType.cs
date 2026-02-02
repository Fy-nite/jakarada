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
    String,
    Dollar,
    
    // Operators and delimiters
    Comma,
    Colon,
    LeftBracket,
    RightBracket,
    LeftParen,
    RightParen,
    Plus,
    Minus,
    Asterisk,
    Slash,
    Percent,
    
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
