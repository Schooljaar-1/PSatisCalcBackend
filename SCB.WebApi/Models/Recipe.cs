public class Recipe
{
    public required string Name {get; set;}
    public required string Version {get;set;}
    public List<Parts>? Parts {get; set;}
}