using System.IO;
using System.Text.Json;

public class RecipeExtractor{
    private static readonly string jsonPath = Path.Combine("Data", "Recipes.json");

    public List<Recipe> GetAllRecipes()
    {
        string jsonRecipeList = File.ReadAllText(jsonPath);
        var recipes = JsonSerializer.Deserialize<List<Recipe>>(jsonRecipeList);
        if(recipes is not null)
            return recipes;
        else
            return [];
    }
}
