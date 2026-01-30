using Jakarada.Core.AST;
using Jakarada.Core.Lexer;

namespace Jakarada.Core.Parser;

/// <summary>
/// Parser for x86_64 assembly language that builds an AST
/// </summary>
public class AssemblyParser
{
    private readonly List<Token> _tokens;
    private int _position;

    public AssemblyParser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    /// <summary>
    /// Parses the tokens and builds an AST
    /// </summary>
    public ProgramNode Parse()
    {
        var program = new ProgramNode();
        
        while (!IsAtEnd())
        {
            SkipNewLines();
            
            if (IsAtEnd())
                break;
            
            // Check for label (identifier followed by colon)
            if (Current().Type == TokenType.Identifier && Peek(1).Type == TokenType.Colon)
            {
                var label = ParseLabel();
                program.Labels.Add(label);
                
                // Check if there's an instruction on the same line
                if (!IsAtEnd() && Current().Type != TokenType.NewLine && Current().Type != TokenType.Comment)
                {
                    var instruction = ParseInstruction();
                    instruction.Label = label.Name;
                    program.Instructions.Add(instruction);
                }
            }
            else if (Current().Type == TokenType.Identifier)
            {
                var instruction = ParseInstruction();
                program.Instructions.Add(instruction);
            }
            else if (Current().Type == TokenType.Comment)
            {
                Advance(); // Skip comment
            }
            
            SkipNewLines();
        }
        
        return program;
    }

    private LabelNode ParseLabel()
    {
        var labelToken = Current();
        var label = new LabelNode
        {
            Name = labelToken.Value,
            LineNumber = labelToken.Line,
            ColumnNumber = labelToken.Column
        };
        
        Advance(); // identifier
        Advance(); // colon
        
        return label;
    }

    private InstructionNode ParseInstruction()
    {
        var mnemonicToken = Current();
        var instruction = new InstructionNode
        {
            Mnemonic = mnemonicToken.Value.ToUpper(),
            LineNumber = mnemonicToken.Line,
            ColumnNumber = mnemonicToken.Column
        };
        
        Advance(); // mnemonic
        
        // Parse operands
        while (!IsAtEnd() && Current().Type != TokenType.NewLine && Current().Type != TokenType.Comment)
        {
            var operand = ParseOperand();
            instruction.Operands.Add(operand);
            
            // Check for comma (more operands)
            if (Current().Type == TokenType.Comma)
            {
                Advance();
            }
            else
            {
                break;
            }
        }
        
        // Parse optional comment
        if (Current().Type == TokenType.Comment)
        {
            instruction.Comment = Current().Value;
            Advance();
        }
        
        return instruction;
    }

    private OperandNode ParseOperand()
    {
        var token = Current();
        
        // Size specifier (byte ptr, word ptr, etc.)
        int? size = null;
        if (token.Type == TokenType.BytePtr)
        {
            size = 8;
            Advance();
        }
        else if (token.Type == TokenType.WordPtr)
        {
            size = 16;
            Advance();
        }
        else if (token.Type == TokenType.DwordPtr)
        {
            size = 32;
            Advance();
        }
        else if (token.Type == TokenType.QwordPtr)
        {
            size = 64;
            Advance();
        }
        
        token = Current();
        
        // Memory operand [...]
        if (token.Type == TokenType.LeftBracket)
        {
            var memOp = ParseMemoryOperand();
            if (size.HasValue)
                memOp.Size = size.Value;
            return memOp;
        }
        
        // Register operand
        if (token.Type == TokenType.Register)
        {
            var regOp = new RegisterOperand
            {
                Name = token.Value,
                LineNumber = token.Line,
                ColumnNumber = token.Column,
                Size = size
            };
            Advance();
            return regOp;
        }
        
        // Immediate value (number or hex)
        if (token.Type == TokenType.Number)
        {
            var immOp = new ImmediateOperand
            {
                Value = long.Parse(token.Value),
                IsHex = false,
                LineNumber = token.Line,
                ColumnNumber = token.Column,
                Size = size
            };
            Advance();
            return immOp;
        }
        
        if (token.Type == TokenType.HexNumber)
        {
            var hexValue = token.Value.StartsWith("0x") || token.Value.StartsWith("0X")
                ? token.Value.Substring(2)
                : token.Value;
            
            var immOp = new ImmediateOperand
            {
                Value = Convert.ToInt64(hexValue, 16),
                IsHex = true,
                LineNumber = token.Line,
                ColumnNumber = token.Column,
                Size = size
            };
            Advance();
            return immOp;
        }
        
        // Label reference (for jumps, calls, etc.)
        if (token.Type == TokenType.Identifier)
        {
            var labelRef = new LabelReferenceOperand
            {
                LabelName = token.Value,
                LineNumber = token.Line,
                ColumnNumber = token.Column,
                Size = size
            };
            Advance();
            return labelRef;
        }
        
        throw new Exception($"Unexpected token {token.Type} at {token.Line}:{token.Column}");
    }

    private MemoryOperand ParseMemoryOperand()
    {
        var token = Current();
        var memOp = new MemoryOperand
        {
            LineNumber = token.Line,
            ColumnNumber = token.Column
        };
        
        Advance(); // '['
        
        // Parse segment register (e.g., fs:[...])
        if (Current().Type == TokenType.Register && Peek(1).Type == TokenType.Colon)
        {
            memOp.SegmentRegister = Current().Value;
            Advance(); // segment register
            Advance(); // ':'
        }
        
        // Parse base register or displacement
        if (Current().Type == TokenType.Register)
        {
            memOp.BaseRegister = Current().Value;
            Advance();
        }
        else if (Current().Type == TokenType.Number || Current().Type == TokenType.HexNumber)
        {
            memOp.Displacement = ParseNumericValue();
        }
        
        // Parse '+' or '-' for displacement or index register
        while (Current().Type == TokenType.Plus || Current().Type == TokenType.Minus)
        {
            var isPlus = Current().Type == TokenType.Plus;
            Advance();
            
            if (Current().Type == TokenType.Register)
            {
                if (memOp.IndexRegister == null)
                {
                    memOp.IndexRegister = Current().Value;
                    Advance();
                    
                    // Check for scale (*1, *2, *4, *8)
                    if (Current().Type == TokenType.Asterisk)
                    {
                        Advance();
                        if (Current().Type == TokenType.Number)
                        {
                            memOp.Scale = int.Parse(Current().Value);
                            Advance();
                        }
                    }
                }
                else
                {
                    throw new Exception($"Multiple index registers not supported at {Current().Line}:{Current().Column}");
                }
            }
            else if (Current().Type == TokenType.Number || Current().Type == TokenType.HexNumber)
            {
                var value = ParseNumericValue();
                memOp.Displacement = isPlus ? value : -value;
            }
        }
        
        if (Current().Type != TokenType.RightBracket)
        {
            throw new Exception($"Expected ']' at {Current().Line}:{Current().Column}");
        }
        
        Advance(); // ']'
        
        return memOp;
    }

    private long ParseNumericValue()
    {
        var token = Current();
        
        if (token.Type == TokenType.Number)
        {
            Advance();
            return long.Parse(token.Value);
        }
        
        if (token.Type == TokenType.HexNumber)
        {
            var hexValue = token.Value.StartsWith("0x") || token.Value.StartsWith("0X")
                ? token.Value.Substring(2)
                : token.Value;
            Advance();
            return Convert.ToInt64(hexValue, 16);
        }
        
        throw new Exception($"Expected numeric value at {token.Line}:{token.Column}");
    }

    private void SkipNewLines()
    {
        while (!IsAtEnd() && Current().Type == TokenType.NewLine)
        {
            Advance();
        }
    }

    private Token Current()
    {
        return _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    }

    private Token Peek(int offset)
    {
        var pos = _position + offset;
        return pos < _tokens.Count ? _tokens[pos] : _tokens[^1];
    }

    private void Advance()
    {
        if (_position < _tokens.Count)
            _position++;
    }

    private bool IsAtEnd()
    {
        return _position >= _tokens.Count || Current().Type == TokenType.EndOfFile;
    }
}
