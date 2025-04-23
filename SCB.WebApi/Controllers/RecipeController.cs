using Microsoft.AspNetCore.Mvc;
using SCB.WebApi.Services; 

namespace SCB.WebApi.Controllers;

[Route("api/[Controller]")]
[ApiController]

public class RecipeController : ControllerBase
{
    [HttpGet(Name = "Testje321")]
    public string GetRecipes()
    {
        return "";
    }
}
