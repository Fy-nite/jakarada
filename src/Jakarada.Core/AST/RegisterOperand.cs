namespace Jakarada.Core.AST;

/// <summary>
/// Represents a register operand (e.g., RAX, RBX, ECX)
/// </summary>
public class RegisterOperand : OperandNode
{
    /// <summary>
    /// Gets or sets the register name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
