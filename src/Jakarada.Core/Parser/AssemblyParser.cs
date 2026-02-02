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
    private LabelNode? _pendingLabelNode;

    public AssemblyParser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    private void FlushPendingLabel(ProgramNode program)
    {
        if (_pendingLabelNode != null)
        {
            var dummyInstr = new InstructionNode
            {
                Mnemonic = "DB",
                Label = _pendingLabelNode.Name,
                LineNumber = _pendingLabelNode.LineNumber,
                ColumnNumber = _pendingLabelNode.ColumnNumber
            };
            dummyInstr.Comment = "Implicit label anchor";
            program.Instructions.Add(dummyInstr);
            _pendingLabelNode = null;
        }
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
                FlushPendingLabel(program); // Flush any previous pending label

                var label = ParseLabel();
                program.Labels.Add(label);
                
                // Check if there's an instruction on the same line
                if (!IsAtEnd() && Current().Type != TokenType.NewLine && Current().Type != TokenType.Comment)
                {
                    var instr = ParseInstruction();
                    instr.Label = label.Name;
                    program.Instructions.Add(instr);
                }
                else
                {
                    _pendingLabelNode = label;
                }
            }
            else if (Current().Type == TokenType.Identifier)
            {
                // Support label before directive without a colon (e.g., "msg db ..." or "len eqo ...").
                // If the next token is an identifier and matches a known directive, treat the current
                // identifier as a label attached to the following instruction.
                if (Peek(1).Type == TokenType.Identifier)
                {
                    var next = Peek(1).Value.ToLower();
                    if (next == "db" || next == "dw" || next == "dd" || next == "dq" || next == "eq" || next == "eqo" || next == "equ")
                    {
                        FlushPendingLabel(program);

                        // Create label node
                        var labelToken = Current();
                        var label = new LabelNode
                        {
                            Name = labelToken.Value,
                            LineNumber = labelToken.Line,
                            ColumnNumber = labelToken.Column
                        };
                        program.Labels.Add(label);

                        Advance(); // consume label identifier

                        // Now parse the following instruction (directive)
                        if (!IsAtEnd() && Current().Type == TokenType.Identifier)
                        {
                            var instrDir = ParseInstruction();
                            instrDir.Label = label.Name;
                            program.Instructions.Add(instrDir);
                        }
                        continue;
                    }
                }

                var instrGeneric = ParseInstruction();
                
                if (_pendingLabelNode != null)
                {
                    instrGeneric.Label = _pendingLabelNode.Name;
                    _pendingLabelNode = null;
                }
                
                program.Instructions.Add(instrGeneric);
            }
            else if (Current().Type == TokenType.Comment)
            {
                Advance(); // Skip comment
            }
            
            SkipNewLines();
        }

        FlushPendingLabel(program);
        
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

        var mnemonicLower = instruction.Mnemonic.ToLower();

        // Some directives (EQU/EQO) accept a raw expression -- capture it verbatim
        if (mnemonicLower == "eqo" || mnemonicLower == "equ" || mnemonicLower == "eq")
        {
            instruction.DirectiveExprAST = ParseExpression();
            instruction.DirectiveExpression = "EXPR"; 
        }
        else
        {
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
        
        // String literal (for directives like db)
        if (token.Type == TokenType.String)
        {
            var strOp = new Jakarada.Core.AST.StringOperand
            {
                Value = token.Value,
                LineNumber = token.Line,
                ColumnNumber = token.Column,
                Size = size
            };
            Advance();
            return strOp;
        }
        
        // Parse expression
        var expr = ParseExpression();
        
        if (expr is LiteralExpression lit)
        {
            return new ImmediateOperand
            {
                Value = lit.Value,
                IsHex = false,
                LineNumber = lit.LineNumber,
                ColumnNumber = lit.ColumnNumber,
                Size = size
            };
        }
        
        if (expr is SymbolExpression sym)
        {
            return new LabelReferenceOperand
            {
                LabelName = sym.Name,
                LineNumber = sym.LineNumber,
                ColumnNumber = sym.ColumnNumber,
                Size = size
            };
        }
        
        return new ExpressionOperand(expr) { Size = size };
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
                    throw new ParserException("Multiple index registers not supported", Current().Line, Current().Column);
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
            throw new ParserException("Expected ']'", Current().Line, Current().Column);
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
                ? token.Value[2..]
                : token.Value;
            Advance();
            return Convert.ToInt64(hexValue, 16);
        }
        
        throw new ParserException("Expected numeric value", token.Line, token.Column);
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

    private ExpressionNode ParseExpression()
    {
        return ParseBinary(0);
    }

    private ExpressionNode ParseBinary(int precedence)
    {
        var left = ParseUnary();

        while (true)
        {
            var opToken = Current();
            int newPrecedence = GetPrecedence(opToken.Type);
           
            if (newPrecedence <= precedence)
                break;
            
            Advance(); // consume operator
            var right = ParseBinary(newPrecedence);
            left = new BinaryExpression(left, opToken.Type, right)
            {
                LineNumber = opToken.Line,
                ColumnNumber = opToken.Column
            };
        }
        return left;
    }
    
    private ExpressionNode ParseUnary()
    {
        var token = Current();
        if (token.Type == TokenType.Plus || token.Type == TokenType.Minus)
        {
            Advance();
            var operand = ParseUnary();
            return new UnaryExpression(token.Type, operand)
            {
                LineNumber = token.Line,
                ColumnNumber = token.Column
            };
        }
        return ParsePrimary();
    }
    
    private ExpressionNode ParsePrimary()
    {
        var token = Current();
        
        if (token.Type == TokenType.Number)
        {
            Advance();
            return new LiteralExpression(long.Parse(token.Value))
            {
                LineNumber = token.Line,
                ColumnNumber = token.Column
            };
        }
        if (token.Type == TokenType.HexNumber)
        {
            Advance();
            var hexValue = token.Value.StartsWith("0x") || token.Value.StartsWith("0X") ? token.Value[2..] : token.Value;
            return new LiteralExpression(Convert.ToInt64(hexValue, 16))
            {
                LineNumber = token.Line,
                ColumnNumber = token.Column
            };
        }
        if (token.Type == TokenType.Dollar)
        {
            Advance();
            return new DollarExpression()
            {
                LineNumber = token.Line,
                ColumnNumber = token.Column
            };
        }
        if (token.Type == TokenType.Identifier)
        {
            Advance();
            return new SymbolExpression(token.Value)
            {
                LineNumber = token.Line,
                ColumnNumber = token.Column
            };
        }
        if (token.Type == TokenType.LeftParen)
        {
            Advance();
            var expr = ParseExpression();
            if (Current().Type != TokenType.RightParen)
                 throw new ParserException("Expected )", Current().Line, Current().Column);
            Advance();
            return expr;
        }
        throw new ParserException($"Unexpected token in expression: {token.Type}", token.Line, token.Column);
    }
    
    private int GetPrecedence(TokenType type)
    {
        if (type == TokenType.Asterisk || type == TokenType.Slash || type == TokenType.Percent) return 2;
        if (type == TokenType.Plus || type == TokenType.Minus) return 1;
        return 0;
    }
}
