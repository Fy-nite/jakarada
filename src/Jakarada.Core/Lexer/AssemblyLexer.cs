using System.Text;

namespace Jakarada.Core.Lexer;

/// <summary>
/// Lexer for x86_64 assembly language
/// </summary>
public class AssemblyLexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;
    
    private static readonly HashSet<string> X86Registers = new(StringComparer.OrdinalIgnoreCase)
    {
        // 64-bit registers
        "rax", "rbx", "rcx", "rdx", "rsi", "rdi", "rbp", "rsp",
        "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15",
        "rip",
        
        // 32-bit registers
        "eax", "ebx", "ecx", "edx", "esi", "edi", "ebp", "esp",
        "r8d", "r9d", "r10d", "r11d", "r12d", "r13d", "r14d", "r15d",
        
        // 16-bit registers
        "ax", "bx", "cx", "dx", "si", "di", "bp", "sp",
        "r8w", "r9w", "r10w", "r11w", "r12w", "r13w", "r14w", "r15w",
        
        // 8-bit registers
        "al", "bl", "cl", "dl", "sil", "dil", "bpl", "spl",
        "ah", "bh", "ch", "dh",
        "r8b", "r9b", "r10b", "r11b", "r12b", "r13b", "r14b", "r15b",
        
        // Segment registers
        "cs", "ds", "es", "fs", "gs", "ss"
    };

    public AssemblyLexer(string source)
    {
        _source = source;
        _position = 0;
    }

    /// <summary>
    /// Tokenizes the entire source code
    /// </summary>
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        
        while (!IsAtEnd())
        {
            SkipWhitespace();
            
            if (IsAtEnd())
                break;
                
            var token = NextToken();
            if (token != null)
            {
                tokens.Add(token);
            }
        }
        
        tokens.Add(new Token(TokenType.EndOfFile, "", _line, _column));
        return tokens;
    }

    private Token? NextToken()
    {
        var startColumn = _column;
        var currentChar = Peek();

        // Comments
        if (currentChar == ';' || currentChar == '#')
        {
            return ReadComment();
        }

        // Newline
        if (currentChar == '\n')
        {
            Advance();
            var token = new Token(TokenType.NewLine, "\n", _line, startColumn);
            _line++;
            _column = 1;
            return token;
        }

        // Hex numbers starting with 0x (check before regular numbers)
        if (currentChar == '0' && (Peek(1) == 'x' || Peek(1) == 'X'))
        {
            return ReadHexNumber();
        }

        // Numbers
        if (char.IsDigit(currentChar))
        {
            return ReadNumber();
        }

        // Identifiers and keywords
        if (char.IsLetter(currentChar) || currentChar == '_')
        {
            return ReadIdentifier();
        }

        // Operators and delimiters
        return ReadOperator();
    }

    private Token ReadComment()
    {
        var startColumn = _column;
        var sb = new StringBuilder();
        
        while (!IsAtEnd() && Peek() != '\n')
        {
            sb.Append(Advance());
        }
        
        return new Token(TokenType.Comment, sb.ToString(), _line, startColumn);
    }

    private Token ReadNumber()
    {
        var startColumn = _column;
        var sb = new StringBuilder();
        
        while (!IsAtEnd() && (char.IsDigit(Peek()) || Peek() == '_'))
        {
            if (Peek() != '_')
                sb.Append(Peek());
            Advance();
        }
        
        return new Token(TokenType.Number, sb.ToString(), _line, startColumn);
    }

    private Token ReadHexNumber()
    {
        var startColumn = _column;
        var sb = new StringBuilder();
        
        // Skip '0x'
        sb.Append(Advance()); // '0'
        sb.Append(Advance()); // 'x'
        
        while (!IsAtEnd() && (char.IsDigit(Peek()) || "abcdefABCDEF_".Contains(Peek())))
        {
            if (Peek() != '_')
                sb.Append(Peek());
            Advance();
        }
        
        return new Token(TokenType.HexNumber, sb.ToString(), _line, startColumn);
    }

    private Token ReadIdentifier()
    {
        var startColumn = _column;
        var sb = new StringBuilder();
        
        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Advance());
        }
        
        var value = sb.ToString();
        var lowerValue = value.ToLower();
        
        // Check for size specifiers
        if (lowerValue == "byte" && MatchNext("ptr"))
        {
            SkipWhitespace();
            if (Peek() == 'p' && Peek(1) == 't' && Peek(2) == 'r')
            {
                Advance(); Advance(); Advance();
                return new Token(TokenType.BytePtr, "byte ptr", _line, startColumn);
            }
        }
        else if (lowerValue == "word" && MatchNext("ptr"))
        {
            SkipWhitespace();
            if (Peek() == 'p' && Peek(1) == 't' && Peek(2) == 'r')
            {
                Advance(); Advance(); Advance();
                return new Token(TokenType.WordPtr, "word ptr", _line, startColumn);
            }
        }
        else if (lowerValue == "dword" && MatchNext("ptr"))
        {
            SkipWhitespace();
            if (Peek() == 'p' && Peek(1) == 't' && Peek(2) == 'r')
            {
                Advance(); Advance(); Advance();
                return new Token(TokenType.DwordPtr, "dword ptr", _line, startColumn);
            }
        }
        else if (lowerValue == "qword" && MatchNext("ptr"))
        {
            SkipWhitespace();
            if (Peek() == 'p' && Peek(1) == 't' && Peek(2) == 'r')
            {
                Advance(); Advance(); Advance();
                return new Token(TokenType.QwordPtr, "qword ptr", _line, startColumn);
            }
        }
        
        // Check if it's a register
        if (X86Registers.Contains(value))
        {
            return new Token(TokenType.Register, value, _line, startColumn);
        }
        
        return new Token(TokenType.Identifier, value, _line, startColumn);
    }

    private Token ReadOperator()
    {
        var startColumn = _column;
        var currentChar = Advance();
        
        return currentChar switch
        {
            ',' => new Token(TokenType.Comma, ",", _line, startColumn),
            ':' => new Token(TokenType.Colon, ":", _line, startColumn),
            '[' => new Token(TokenType.LeftBracket, "[", _line, startColumn),
            ']' => new Token(TokenType.RightBracket, "]", _line, startColumn),
            '+' => new Token(TokenType.Plus, "+", _line, startColumn),
            '-' => new Token(TokenType.Minus, "-", _line, startColumn),
            '*' => new Token(TokenType.Asterisk, "*", _line, startColumn),
            _ => throw new LexerException($"Unexpected character '{currentChar}'", _line, startColumn)
        };
    }

    private bool MatchNext(string text)
    {
        var savedPos = _position;
        var savedLine = _line;
        var savedColumn = _column;
        
        SkipWhitespace();
        
        foreach (var ch in text)
        {
            if (IsAtEnd() || char.ToLower(Peek()) != char.ToLower(ch))
            {
                _position = savedPos;
                _line = savedLine;
                _column = savedColumn;
                return false;
            }
            Advance();
        }
        
        _position = savedPos;
        _line = savedLine;
        _column = savedColumn;
        return true;
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && (Peek() == ' ' || Peek() == '\t' || Peek() == '\r'))
        {
            Advance();
        }
    }

    private char Peek(int offset = 0)
    {
        var pos = _position + offset;
        return pos >= _source.Length ? '\0' : _source[pos];
    }

    private char Advance()
    {
        var ch = _source[_position++];
        _column++;
        return ch;
    }

    private bool IsAtEnd()
    {
        return _position >= _source.Length;
    }
}
