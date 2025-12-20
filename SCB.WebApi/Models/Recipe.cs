public class Recipe
{
    public required string Name {get; set;}
    public required string Version {get; set;}
    public required string Machine {get; set;}
    public required Fraction Amount { get; set;}
    public required string Type {get; set;}
    public required string Image { get; set;}
    public List<Parts>? Parts { get; set; }
}
public class Parts
{
    public required string PartName {get; set;}
    public required Fraction Amount  {get; set;}
}

public class Fraction
{
    public required int Teller {get; set;}
    public required int Noemer {get; set;}
}

public class FractionDecompressed
{
    public required int Integer {get;set;}
    public required Fraction Fraction {get;set;}
}
