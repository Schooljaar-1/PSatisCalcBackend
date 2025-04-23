//Handle recipe manipulation
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    RecipeExtractor recipeExtractor = new();


    [HttpGet(Name = "Testje321")]
    public string GetRecipes()
    {
        //Write a service function for this shit
        return "";
    }
}
