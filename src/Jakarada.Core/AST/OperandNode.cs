namespace Jakarada.Core.AST;

/// <summary>
/// Base class for instruction operands
/// </summary>
public abstract class OperandNode : AstNode
{
    /// <summary>
    /// Gets or sets the size of the operand in bits (8, 16, 32, 64)
    /// </summary>
    public int? Size { get; set; }
}
