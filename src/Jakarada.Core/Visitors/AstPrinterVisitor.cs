using System.Text;
using Jakarada.Core.AST;

namespace Jakarada.Core.Visitors;

/// <summary>
/// AST visitor that prints the tree structure
/// </summary>
public class AstPrinterVisitor : IAstVisitor<string>
{
    private int _indentLevel;
    private const int IndentSize = 2;

    public string Visit(ProgramNode node)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Program:");
        
        _indentLevel++;
        
        if (node.Labels.Count > 0)
        {
            sb.AppendLine(Indent() + "Labels:");
            _indentLevel++;
            foreach (var label in node.Labels)
            {
                sb.AppendLine(Indent() + label.Accept(this));
            }
            _indentLevel--;
        }
        
        if (node.Instructions.Count > 0)
        {
            sb.AppendLine(Indent() + "Instructions:");
            _indentLevel++;
            foreach (var instruction in node.Instructions)
            {
                sb.AppendLine(Indent() + instruction.Accept(this));
            }
            _indentLevel--;
        }
        
        _indentLevel--;
        
        return sb.ToString();
    }

    public string Visit(InstructionNode node)
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(node.Label))
        {
            sb.Append($"[{node.Label}] ");
        }
        
        sb.Append(node.Mnemonic);
        
        if (node.Operands.Count > 0)
        {
            sb.Append(" ");
            for (int i = 0; i < node.Operands.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(node.Operands[i].Accept(this));
            }
        }
        
        if (!string.IsNullOrEmpty(node.Comment))
        {
            sb.Append($" {node.Comment}");
        }
        
        return sb.ToString();
    }

    public string Visit(RegisterOperand node)
    {
        return node.Name;
    }

    public string Visit(ImmediateOperand node)
    {
        return node.IsHex ? $"0x{node.Value:X}" : node.Value.ToString();
    }

    public string Visit(MemoryOperand node)
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(node.SegmentRegister))
        {
            sb.Append($"{node.SegmentRegister}:");
        }
        
        sb.Append("[");
        
        bool needsPlus = false;
        
        if (!string.IsNullOrEmpty(node.BaseRegister))
        {
            sb.Append(node.BaseRegister);
            needsPlus = true;
        }
        
        if (!string.IsNullOrEmpty(node.IndexRegister))
        {
            if (needsPlus)
                sb.Append("+");
            sb.Append(node.IndexRegister);
            if (node.Scale > 1)
                sb.Append($"*{node.Scale}");
            needsPlus = true;
        }
        
        if (node.Displacement.HasValue && node.Displacement.Value != 0)
        {
            if (needsPlus)
            {
                if (node.Displacement.Value > 0)
                    sb.Append("+");
            }
            sb.Append(node.Displacement.Value);
        }
        
        sb.Append("]");
        
        return sb.ToString();
    }

    public string Visit(LabelNode node)
    {
        return $"{node.Name}:";
    }

    public string Visit(LabelReferenceOperand node)
    {
        return node.LabelName;
    }

    private string Indent()
    {
        return new string(' ', _indentLevel * IndentSize);
    }
}
