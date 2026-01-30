namespace Jakarada.Core.AST;

/// <summary>
/// Represents an immediate value operand (e.g., 42, 0x10)
/// </summary>
public class ImmediateOperand : OperandNode
{
    /// <summary>
    /// Gets or sets the immediate value
    /// </summary>
    public long Value { get; set; }

    /// <summary>
    /// Gets or sets whether this is a hexadecimal value
    /// </summary>
    public bool IsHex { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
