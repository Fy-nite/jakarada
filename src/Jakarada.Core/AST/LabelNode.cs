namespace Jakarada.Core.AST;

/// <summary>
/// Represents a label in assembly code
/// </summary>
public class LabelNode : AstNode
{
    /// <summary>
    /// Gets or sets the label name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
