namespace Jakarada.Core;

/// <summary>
/// Exception thrown when the lexer encounters invalid assembly syntax
/// </summary>
public class LexerException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public LexerException(string message, int line, int column)
        : base($"{message} at {line}:{column}")
    {
        Line = line;
        Column = column;
    }
}

/// <summary>
/// Exception thrown when the parser encounters invalid assembly structure
/// </summary>
public class ParserException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public ParserException(string message, int line, int column)
        : base($"{message} at {line}:{column}")
    {
        Line = line;
        Column = column;
    }
}
