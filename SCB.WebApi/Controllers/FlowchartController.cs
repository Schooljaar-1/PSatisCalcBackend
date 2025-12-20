using FlowchartServices;
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.FlowchartControllers;

[Route("api/[Controller]")]
[ApiController]

public class FlowchartController : ControllerBase
{
    FlowChartService FlowChartService = new();

    [HttpPost(Name = "Calculate resources and amounts needed for given recipe(s)")]
    public ActionResult<FlowChartResult> CalculateRecipe([FromBody] CalculateRecipeRequest request)
    {
        // TODO: Start making basic checks like if any of the amounts are 0 or negative.
        var amounts = request.Amounts;
        var recipes = request.Recipes;

        return new FlowChartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}