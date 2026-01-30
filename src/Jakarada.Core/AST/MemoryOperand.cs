namespace Jakarada.Core.AST;

/// <summary>
/// Represents a memory operand (e.g., [RAX], [RBP+8], [RIP+0x1000])
/// </summary>
public class MemoryOperand : OperandNode
{
    /// <summary>
    /// Gets or sets the base register
    /// </summary>
    public string? BaseRegister { get; set; }

    /// <summary>
    /// Gets or sets the index register
    /// </summary>
    public string? IndexRegister { get; set; }

    /// <summary>
    /// Gets or sets the scale factor (1, 2, 4, or 8)
    /// </summary>
    public int Scale { get; set; } = 1;

    /// <summary>
    /// Gets or sets the displacement value
    /// </summary>
    public long? Displacement { get; set; }

    /// <summary>
    /// Gets or sets the segment register (if any)
    /// </summary>
    public string? SegmentRegister { get; set; }

    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
