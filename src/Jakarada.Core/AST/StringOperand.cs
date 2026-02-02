namespace Jakarada.Core.AST;

/// <summary>
/// Represents a string/byte sequence operand (e.g., db "hello")
/// </summary>
public class StringOperand : OperandNode
{
    public string Value { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        // The visitor may not yet have this overload; projects updated accordingly.
        return visitor.Visit(this);
    }
}
