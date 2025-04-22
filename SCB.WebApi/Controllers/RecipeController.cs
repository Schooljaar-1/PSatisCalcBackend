//Handle recipe manipulation
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    [HttpGet(Name = "Testje321")]
    public List<Recipe> GetRecipes()
    {
        return "321";
    }
}
