using RecipeServices;
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    RecipeDataService recipeDataService = new();

    [HttpGet(Name = "Get all recipes")]
    public ActionResult<List<Recipe>> GetRecipes()
    {
        var recipes = recipeDataService.GetAllRecipes();

        if(recipes is null || !recipes.Any())
            return NotFound("There are no recipes present.");
        else
            return Ok(recipes);
    }

    [HttpGet("name/{name}", Name = "Get recipe by name")]
    public ActionResult<Recipe?> GetRecipeByName(string name)
    {
        var recipes = recipeDataService.GetAllRecipes();
        var recipe = recipeDataService.FindRecipeByName(name);

        if (recipes == null || !recipes.Any())
            return NotFound("There are no recipes");
        
        else if (recipe == null || !recipe.Any())
            return NotFound($"No recipes found with name \"{name}\".");
        
        else
            return Ok(recipe);
    }

    [HttpGet("name/{name}/version/{version}", Name = "Get recipe by name and version")]
    public ActionResult<Recipe?> GetRecipeByNameAndVersion(string name, string version)
    {
        var recipes = recipeDataService.GetAllRecipes();
        var recipe = recipeDataService.FindRecipeByNameAndVersion(name, version);

        if (recipes == null || !recipes.Any())
            return NotFound("There are no recipes");
        else if (recipe == null)
            return NotFound($"No recipes found with name \"{name}\" and version \"{version}\".");
        else
            return Ok(recipe);
    }

    [HttpPost(Name = "Add new recipe")]
    public ActionResult AddNewRecipe(Recipe newRecipe)
    {
        if (newRecipe is null || newRecipe.Parts == null || !newRecipe.Parts.Any())
        {
            return BadRequest("Recipe data is required and must contain at least one part.");
        }
        else if (newRecipe.Parts.Any(part => part.Amount == null || part.Amount.Teller == 0 || part.Amount.Noemer == 0))
        {
            return BadRequest("Each part must have a non-zero amount (numerator and denominator).");
        }
        else if (recipeDataService.FindRecipeByNameAndVersion(newRecipe.Name, newRecipe.Version) is not null)
        {
            return BadRequest($"A recipe with name \"{newRecipe.Name}\" and version \"{newRecipe.Version}\" already exists.");
        }
        else
        {
            recipeDataService.AddNewRecipe(newRecipe);
            return Ok(newRecipe);
        }
    }

    [HttpDelete(Name = "Delete all recipes")]
    public ActionResult DeleteAllRecipes()
    {
        var recipes = recipeDataService.GetAllRecipes();

        if (recipes == null || !recipes.Any())
            return BadRequest("There are no recipes to be deleted.");
        else
        {
            int recipesRemoved = recipes.Count();
            recipeDataService.DeleteAllRecipes();
            return Ok($"Removed all {recipesRemoved} recipes successfully.");
        }
    }

    [HttpDelete("name/{name}", Name = "Delete recipes by name")]
    public ActionResult DeleteRecipesByName(string name)
    {
        var recipes = recipeDataService.GetAllRecipes();
        var recipe = recipeDataService.FindRecipeByName(name);

        if (recipes == null || !recipes.Any())
            return BadRequest("There are no recipes to be deleted.");
        else if (recipe == null || !recipe.Any())
            return NotFound($"No recipes found with name \"{name}\".");
        else
        {
            int deletedRecipes = recipeDataService.DeleteRecipeByName(name);
            if(deletedRecipes == 0)
                return BadRequest("Recipe deletion went wrong...");
            else
                return Ok($"{deletedRecipes} recipes deleted.");
        }       
    }

    [HttpDelete("name/{name}/version/{version}", Name = "Delete recipes by name and version")]
    public ActionResult DeleteRecipeByNameAndVersion(string name, string version)
    {
        var recipes = recipeDataService.GetAllRecipes();
        var recipe = recipeDataService.FindRecipeByNameAndVersion(name, version);

        if (recipes == null || !recipes.Any())
            return NotFound("There are no recipes");
        else if (recipe == null)
            return NotFound($"No recipes found with name \"{name}\" and version \"{version}\".");
        else
        {
            int deletedRecipe = recipeDataService.DeleteRecipeByNameAndVersion(name, version);
            if (deletedRecipe == 0)
                return BadRequest("Recipe deletion went wrong...");
            else
                return Ok($"{deletedRecipe} recipes deleted.");
        }
    }
}
