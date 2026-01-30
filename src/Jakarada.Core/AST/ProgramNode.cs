namespace Jakarada.Core.AST;

/// <summary>
/// Represents a complete assembly program
/// </summary>
public class ProgramNode : AstNode
{
    /// <summary>
    /// Gets or sets the list of instructions in the program
    /// </summary>
    public List<InstructionNode> Instructions { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of labels in the program
    /// </summary>
    public List<LabelNode> Labels { get; set; } = new();

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
