using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    RecipeDataService dataManipulator = new();

    [HttpGet(Name = "Testje321")]
    public List<Recipe> GetRecipes()
    {
        return dataManipulator.GetAllRecipes();
    }
}
