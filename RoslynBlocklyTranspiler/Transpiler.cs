using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynBlocklyTranspiler;

public class Transpiler {

    public static string textToBlocks(string code) {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();
        var compilation = CSharpCompilation.Create("Compilation")
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            )
            .AddSyntaxTrees(tree);
        var semanticModel = compilation.GetSemanticModel(tree);
        var codeGenerator = new BlocklyCodeGenerator();
        object result = codeGenerator.Generate(root, semanticModel);
        return JsonSerializer.Serialize(result);
    }

}