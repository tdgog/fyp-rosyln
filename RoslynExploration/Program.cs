using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynExploration;

class Program {
    
    public const string RED = "\u001b[31m";
    public const string BOLD = "\u001b[1m";
    public const string RESET = "\u001b[0m";
    
    public static void Main() {
        const string code = @"
class Example {
    void Foo( {
        int x = 10
        if (x > 5) {
            Console.WriteLine(""ok"")
        } else {
            Console.WriteLine(""bad"")
        }
    }
}
";

        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();
        foreach (Diagnostic diagnostic in tree.GetDiagnostics())
            Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()} @ {diagnostic.Location.GetLineSpan()}");

        printTree(root);
        
        Console.WriteLine("\n\n\n");

        CodeGenerator codeGenerator = new CodeGenerator();
        string result = codeGenerator.generate(root);
        Console.WriteLine(result);
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
