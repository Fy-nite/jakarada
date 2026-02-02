using Jakarada.Core.AST;

namespace Jakarada.Core.AST;

public class ExpressionOperand : OperandNode
{
    public ExpressionNode Expression { get; }

    public ExpressionOperand(ExpressionNode expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
