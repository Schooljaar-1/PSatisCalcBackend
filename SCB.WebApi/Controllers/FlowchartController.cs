using FlowchartServices;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace SCB.WebApi.FlowchartControllers;

[Route("api/[Controller]")]
[ApiController]
public class FlowchartController : ControllerBase
{
    FlowChartService FlowChartService = new();

    [HttpPost(Name = "Calculate resources and amounts needed for given recipe(s)")]
    public ActionResult<FlowchartResult> CalculateRecipe([FromBody] CalculateRecipeRequest request)
    {
        if (request?.Items == null || request.Items.Count == 0) 
            return NotFound("No recipes and or amounts are given");
        
        foreach(var item in request.Items)
        {
            if(item.Amount.Integer < 0 || item.Amount.Fraction.Teller < 0 || item.Amount.Fraction.Noemer < 0)
                return BadRequest("No negative recipe amounts are possible");
            
            if(item.Amount.Fraction.Noemer == 0 )
                return BadRequest("Impossible to divide by zero");
        }

        return FlowChartService.calculateFlowChart(request);

    }
}
