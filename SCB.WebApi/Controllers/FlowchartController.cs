using FlowchartServices;
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.FlowchartControllers;

[Route("api/[Controller]")]
[ApiController]
public class FlowchartController : ControllerBase
{
    // Just create service manually
    FlowChartService FlowChartService = new();

    [HttpPost(Name = "Calculate resources and amounts needed for given recipe(s)")]
    public ActionResult<FlowchartResult> CalculateRecipe([FromBody] CalculateRecipeRequest request)
    {
        // Map amounts and recipes
        var amounts = request.Items
            .Select(x => x.Amount)
            .ToList();

        var recipes = request.Items
            .Select(x => x.Recipe)
            .ToList();

        // foreach (var amount in amounts)
        // {
        //     Console.WriteLine($"WANTED AMOUNT: Integer: {amount.Integer}, Fraction: {amount.Fraction.Teller}/{amount.Fraction.Noemer}");
        // }

        // foreach (var recipe in recipes)
        // {
        //     Console.WriteLine($"RECIPE INFORMATION: Recipe: {recipe.Name}, Machine: {recipe.Machine}, Amount: {recipe.Amount.Teller}/{recipe.Amount.Noemer}");
        // }

        return new FlowchartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}
