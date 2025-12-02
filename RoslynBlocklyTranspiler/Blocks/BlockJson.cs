namespace RoslynBlocklyTranspiler.Blocks;

public class BlockJson {
    
    public string type { get; set; }
    public Dictionary<string, string> fields { get; set; } = new();
    public Dictionary<string, BlockJson?>? inputs { get; set; }
    public BlockJson? next { get; set; }
    
}
