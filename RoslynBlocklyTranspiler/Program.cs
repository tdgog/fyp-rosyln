using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynBlocklyTranspiler.Blocks;

namespace RoslynBlocklyTranspiler;

class Program {
    
    public const string RED = "\e[31m";
    public const string BOLD = "\e[1m";
    public const string RESET = "\e[0m";
    
    public static void Main() {
         // const string code = """
         //                     class Example {
         //                         void Main() {
         //                             int x = 10;
         //                             if (x > 5) {
         //                                 System.Console.WriteLine("ok");
         //                             } else {
         //                                 System.Console.WriteLine("bad");
         //                             }
         //                         }
         //                     }
         //                     """;

        const string code = """
                            class Program
                            {
                                static void Main()
                                {
                                  System.Console.WriteLine("Hello World!");    
                                }
                            }
                            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();
        foreach (Diagnostic diagnostic in tree.GetDiagnostics())
            Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()} @ {diagnostic.Location.GetLineSpan()}");

        printTree(root);
        
        Console.WriteLine("\n\n\n");

        var compilation = CSharpCompilation.Create("Compilation")
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            )
            .AddSyntaxTrees(tree);
        var semanticModel = compilation.GetSemanticModel(tree);

        var codeGenerator = new BlocklyCodeGenerator();
        object result = codeGenerator.Generate(root, semanticModel);
        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    private static void printTree(SyntaxNode node, int indent = 0) {
        string pad = new string(' ', indent);
        Console.WriteLine($"{pad}{node.Kind()}");

        foreach (SyntaxNodeOrToken child in node.ChildNodesAndTokens()) {
            if (child.IsNode)
                printTree(child.AsNode()!, indent + 4);
            else {
                SyntaxToken token = child.AsToken();
                IEnumerable<Diagnostic> diagnostics = token.GetDiagnostics();
                if (diagnostics.Any()) {
                    string marker =
                        " <-- ERROR" +
                        string.Join("",
                            diagnostics.Select(diagnostic => $" \u001b[1m{diagnostic.Id} {diagnostic.GetMessage()}\u001b[0m\u001b[31m"));
                    
                    Console.WriteLine($"\u001b[31m{pad}    {token.Kind()}{marker}\u001b[0m");
                } else
                    Console.WriteLine($"{pad}    {token.Kind()}");
            }
        }
    }
    
}
