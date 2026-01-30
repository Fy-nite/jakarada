namespace Jakarada.Core.AST;

/// <summary>
/// Represents a reference to a label (e.g., in JMP instructions)
/// </summary>
public class LabelReferenceOperand : OperandNode
{
    /// <summary>
    /// Gets or sets the label name being referenced
    /// </summary>
    public string LabelName { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
