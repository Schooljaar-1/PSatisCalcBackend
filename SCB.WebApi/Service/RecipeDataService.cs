using System.IO;
using System.Text.Json;

public class RecipeDataService{
    private static readonly string jsonPath = Path.Combine("Data", "Recipes.json");


    public List<Recipe> GetAllRecipes()
    {
        string jsonRecipeList = File.ReadAllText(jsonPath);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var recipes = JsonSerializer.Deserialize<List<Recipe>>(jsonRecipeList, jsonOptions);

        if(recipes is not null)
            return recipes;
        else
            return [];
    }
}
