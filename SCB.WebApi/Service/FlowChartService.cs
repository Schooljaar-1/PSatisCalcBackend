namespace FlowchartServices;

public class FlowChartService{
    
    public FlowChartResult calculateFlowChart(CalculateRecipeRequest recipesAndAmounts)
    {
        return new FlowChartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}