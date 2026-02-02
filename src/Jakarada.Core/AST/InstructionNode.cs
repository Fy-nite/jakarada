namespace Jakarada.Core.AST;

/// <summary>
/// Represents a single assembly instruction
/// </summary>
public class InstructionNode : AstNode
{
    /// <summary>
    /// Gets or sets the mnemonic (e.g., MOV, ADD, JMP)
    /// </summary>
    public string Mnemonic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of operands
    /// </summary>
    public List<OperandNode> Operands { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional label that precedes this instruction
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets an optional comment
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets an optional directive expression (e.g., for EQU/EQO)
    /// </summary>
    public string? DirectiveExpression { get; set; }

    /// <summary>
    /// Gets or sets the parsed AST for the directive expression
    /// </summary>
    public ExpressionNode? DirectiveExprAST { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
