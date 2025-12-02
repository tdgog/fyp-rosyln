using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynBlocklyTranspiler.Blocks;

namespace RoslynBlocklyTranspiler;

public class BlocklyCodeGenerator
{
    private SemanticModel semanticModel;
    public Dictionary<string, Block> constructedFunctionBlocks { get; } = new();

    public object Generate(SyntaxNode root, SemanticModel model)
    {
        semanticModel = model;

        var blocks = new List<object>();

        foreach (var member in root.ChildNodes())
        {
            var block = VisitNode(member);
            if (block != null)
                blocks.Add(block);
        }

        return new {
            blocks = new {
                blocks = blocks
            },
            functions = constructedFunctionBlocks
        };
    }

    private BlockJson? VisitNode(SyntaxNode? node)
    {
        if (node == null) return null;

        switch (node)
        {
            case ClassDeclarationSyntax cls:
                return GenerateClassBlock(cls);

            case MethodDeclarationSyntax method:
                return GenerateMethodBlock(method);

            case IfStatementSyntax ifs:
                return GenerateIfBlock(ifs);

            case LocalDeclarationStatementSyntax localDecl:
                return GenerateVariableBlock(localDecl);

            case ExpressionStatementSyntax expr:
                return VisitNode(expr.Expression);

            case InvocationExpressionSyntax inv:
                return GenerateInvocation(inv);

            case IdentifierNameSyntax id:
                return LiteralBlock("identifier_block", id.Identifier.Text);

            case LiteralExpressionSyntax lit:
                return LiteralBlock("literal_block", lit.Token.Text);

            default:
                // Visit children for constructs we don't explicitly block-ify
                BlockJson? first = null;
                BlockJson? prev = null;

                foreach (var child in node.ChildNodes())
                {
                    var b = VisitNode(child);
                    if (b == null) continue;

                    if (first == null)
                        first = b;
                    else
                        prev.next = b;

                    prev = b;
                }

                return first;
        }
    }

    private BlockJson GenerateClassBlock(ClassDeclarationSyntax cls)
    {
        var body = ChainBlocks(cls.Members);

        return new BlockJson {
            type = "class_block",
            fields = new Dictionary<string, string> {
                ["IDENT"] = cls.Identifier.Text
            },
            inputs = new Dictionary<string, BlockJson?> {
                ["BODY"] = body
            }
        };
    }

    private BlockJson GenerateMethodBlock(MethodDeclarationSyntax m)
    {
        var body = m.Body != null
            ? ChainBlocks(m.Body.Statements)
            : null;

        return new BlockJson {
            type = "method_block",
            fields = new Dictionary<string, string> {
                ["MODIFIERS"] = "",
                ["NAME"] = m.Identifier.Text,
                ["PARAMS"] = ""
            },
            inputs = new Dictionary<string, BlockJson?> {
                ["BODY"] = body
            }
        };
    }

    private BlockJson GenerateIfBlock(IfStatementSyntax ifs)
    {
        var cond = VisitNode(ifs.Condition);
        var body = VisitNode(ifs.Statement);
        var elseBody = ifs.Else != null ? VisitNode(ifs.Else.Statement) : null;

        return new BlockJson {
            type = "if_block",
            inputs = new Dictionary<string, BlockJson?> {
                ["COND"] = cond,
                ["BODY"] = body,
                ["ELSE"] = elseBody
            }
        };
    }

    private BlockJson GenerateVariableBlock(LocalDeclarationStatementSyntax local)
    {
        var decl = local.Declaration;
        var v = decl.Variables.First();

        var init = v.Initializer != null
            ? VisitNode(v.Initializer.Value)
            : null;

        return new BlockJson {
            type = "var_decl_block",
            fields = new Dictionary<string, string> {
                ["NAME"] = v.Identifier.Text,
                ["TYPE"] = decl.Type.ToString()
            },
            inputs = new Dictionary<string, BlockJson?> {
                ["VALUE"] = init
            }
        };
    }

    private BlockJson GenerateInvocation(InvocationExpressionSyntax inv)
    {
        var symbol = semanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;

        // Build reference to the invocation block
        var callBlock = new BlockJson {
            type = symbol != null 
                ? $"func_decl_{symbol.ContainingType.Name}_{symbol.Name}"
                : "call_unknown",
            inputs = new Dictionary<string, BlockJson?>()
        };

        // Add argument inputs
        var args = inv.ArgumentList.Arguments;
        for (int i = 0; i < args.Count; i++)
        {
            callBlock.inputs[$"ARG{i}"] = VisitNode(args[i].Expression);
        }

        // Store generated function definition block
        if (symbol != null)
        {
            var key = $"{symbol.ContainingType}.{symbol.Name}";

            if (!constructedFunctionBlocks.ContainsKey(key))
            {
                // Build (%1, %2, ...)
                string paramPlaceholders = symbol.Parameters.Length == 0
                    ? "()"
                    : "(" + string.Join(", ", symbol.Parameters.Select((p, idx) => $"%{idx + 1}")) + ")";

                // Build argument definitions based on *actual invocation* text
                var argDefs = args
                    .Select((a, idx) => new Block.Argument(
                        type: "field_input",
                        name: $"ARG{idx}",
                        text: a.Expression.ToString() // ← use actual literal
                    ))
                    .ToArray();

                constructedFunctionBlocks.Add(key, new Block(
                    type: $"func_decl_{symbol.ContainingType.Name}_{symbol.Name}",
                    message0: $"{symbol.ContainingType}.{symbol.Name}{paramPlaceholders};",
                    args0: argDefs,
                    colour: 30
                ));
            }
        }

        return callBlock;
    }


    private BlockJson LiteralBlock(string type, string value) =>
        new BlockJson {
            type = type,
            fields = new Dictionary<string, string> { ["VALUE"] = value }
        };

    /// Combine a list of statements into a Blockly chain
    private BlockJson? ChainStatements(IEnumerable<StatementSyntax> statements)
    {
        BlockJson? first = null;
        BlockJson? prev = null;

        foreach (var s in statements)
        {
            var block = VisitNode(s);
            if (block == null) continue;

            if (first == null)
                first = block;
            else
                prev.next = block;

            prev = block;
        }
        return first;
    }
    
    private BlockJson? ChainBlocks(IEnumerable<SyntaxNode> nodes)
    {
        BlockJson? first = null;
        BlockJson? prev = null;

        foreach (var node in nodes)
        {
            var block = VisitNode(node);
            if (block == null) continue;

            if (first == null)
                first = block;
            else
                prev.next = block;

            prev = block;
        }

        return first;
    }
    
}
