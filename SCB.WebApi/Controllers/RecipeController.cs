using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    RecipeDataService recipeDataService = new();

    [HttpGet(Name = "Get all recipes")]
    public List<Recipe> GetRecipes()
    {
        return recipeDataService.GetAllRecipes();
    }

    [HttpPost(Name = "Add new recipe")]
    public void AddNewRecipe(Recipe newRecipe)
    {
        recipeDataService.AddNewRecipe(newRecipe);
    }
}
