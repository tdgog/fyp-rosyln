namespace RoslynBlocklyTranspiler.Blocks;

public record Block(
    string type,
    string message0,
    Block.Argument[] args0,
    int colour,
    string? previousStatement = null,
    string? nextStatement = null,
    bool inputsInline = true
)
{
    public record Argument(
        string type,
        string name,
        string? text = null
    );
}
