using FlowchartServices;
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.FlowchartControllers;

[Route("api/[Controller]")]
[ApiController]

public class FlowchartController : ControllerBase
{
    FlowChartService FlowChartService = new();

    [HttpPost(Name = "Calculate resources and amounts needed for given recipe(s)")]
    public ActionResult<FlowChartResult> CalculateRecipe(List<FractionDecompressed> amounts, List<Recipe> recipes)
    {
        return new FlowChartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}