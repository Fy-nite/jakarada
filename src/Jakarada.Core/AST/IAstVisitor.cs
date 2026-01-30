namespace Jakarada.Core.AST;

/// <summary>
/// Visitor interface for AST traversal
/// </summary>
public interface IAstVisitor<T>
{
    T Visit(ProgramNode node);
    T Visit(InstructionNode node);
    T Visit(RegisterOperand node);
    T Visit(ImmediateOperand node);
    T Visit(MemoryOperand node);
    T Visit(LabelNode node);
    T Visit(LabelReferenceOperand node);
}
