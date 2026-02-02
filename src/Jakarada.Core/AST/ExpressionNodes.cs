using Jakarada.Core.Lexer;

namespace Jakarada.Core.AST;

public abstract class ExpressionNode : AstNode
{
    public override T Accept<T>(IAstVisitor<T> visitor) => default;
}

public class BinaryExpression : ExpressionNode
{
    public ExpressionNode Left { get; }
    public TokenType Operator { get; }
    public ExpressionNode Right { get; }

    public BinaryExpression(ExpressionNode left, TokenType op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override string ToString()
    {
        string op = Operator switch
        {
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            TokenType.Asterisk => "*",
            TokenType.Slash => "/",
            TokenType.Percent => "%",
            _ => Operator.ToString()
        };
        return $"({Left} {op} {Right})";
    }
}

public class UnaryExpression : ExpressionNode
{
    public TokenType Operator { get; }
    public ExpressionNode Operand { get; }

    public UnaryExpression(TokenType op, ExpressionNode operand)
    {
        Operator = op;
        Operand = operand;
    }

    public override string ToString()
    {
        string op = Operator switch
        {
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            _ => Operator.ToString()
        };
        return $"({op}{Operand})";
    }
}

public class LiteralExpression : ExpressionNode
{
    public long Value { get; }

    public LiteralExpression(long value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();
}

public class SymbolExpression : ExpressionNode
{
    public string Name { get; }

    public SymbolExpression(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}

public class DollarExpression : ExpressionNode
{
    // Represents '$' (current location counter)
    public override string ToString() => "$";
}
