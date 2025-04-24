using System.IO;
using System.Text.Json;

namespace RecipeServices;

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
            return new List<Recipe>();
    }

    public void AddNewRecipe(Recipe newRecipe)
    {
        List<Recipe> recipes = GetAllRecipes();
        recipes.Add(newRecipe);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string updatedJson = JsonSerializer.Serialize(recipes, jsonOptions);

        File.WriteAllText(jsonPath, updatedJson);
    }

    public List<Recipe>? FindRecipeByName(string nameRaw)
    {
        List<Recipe> recipes = GetAllRecipes();
        List<Recipe> foundRecipes = new();
        string name = nameRaw.ToLower();

        foreach(var recipe in recipes)
        {
            if(recipe.Name.ToLower() == name)
                foundRecipes.Add(recipe);
        }
        
        if(foundRecipes.Count == 0)
            return null;
        else
            return foundRecipes;
    }

    public Recipe? FindRecipeByNameAndVersion(string nameRaw, string versionRaw)
    {
        List<Recipe> recipes = GetAllRecipes();
        string name = nameRaw.ToLower();
        string version = versionRaw.ToLower();

        foreach(var recipe in recipes)
        {
            if(recipe.Name.ToLower() == name && recipe.Version.ToLower() == version)
                return recipe;
        }
        return null;
    }
    public void DeleteAllRecipes()
    {
        List<Recipe> emptyRecipes = new();

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(emptyRecipes, jsonOptions);
        File.WriteAllText(jsonPath, updatedJson);
    }

    public int DeleteRecipeByName(string nameRaw)
    {
        List<Recipe> recipes = GetAllRecipes();
        string name = nameRaw.ToLower();

        int removed = recipes.RemoveAll(recipe => recipe.Name.ToLower() == name);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(recipes, jsonOptions);
        File.WriteAllText(jsonPath, updatedJson);

        return removed; 
    }

    public int DeleteRecipeByNameAndVersion(string nameRaw, string versionRaw)
    {
        List<Recipe> recipes = GetAllRecipes();
        string name = nameRaw.ToLower();
        string version = versionRaw.ToLower();

        int removed = recipes.RemoveAll(recipe => recipe.Name.ToLower() == name && recipe.Version.ToLower() == version);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(recipes, jsonOptions);
        File.WriteAllText(jsonPath, updatedJson);

        return removed; 
    }
}
