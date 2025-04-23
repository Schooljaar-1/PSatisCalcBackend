public class Recipe
{
    public required string Name {get; set;}
    public required string Version {get; set;}
    public required string Machine {get; set;}
    public required decimal Amount { get; set;}
    public required string Type {get; set;}
    public List<Parts>? Parts {get; set;}
}