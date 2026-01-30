namespace Jakarada.Core.AST;

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class AstNode
{
    /// <summary>
    /// Gets the line number where this node appears in source
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets the column number where this node appears in source
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// Accept a visitor for the visitor pattern
    /// </summary>
    public abstract T Accept<T>(IAstVisitor<T> visitor);
}
