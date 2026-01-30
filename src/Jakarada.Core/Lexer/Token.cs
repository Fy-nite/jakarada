namespace Jakarada.Core.Lexer;

/// <summary>
/// Represents a token in assembly source code
/// </summary>
public class Token
{
    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public TokenType Type { get; set; }

    /// <summary>
    /// Gets or sets the token value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the column number
    /// </summary>
    public int Column { get; set; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Type}('{Value}') at {Line}:{Column}";
    }
}
