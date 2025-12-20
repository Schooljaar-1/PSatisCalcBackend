namespace FlowchartServices;

public class FlowChartService{
    
    public FlowchartResult calculateFlowChart(CalculateRecipeRequest recipesAndAmounts)
    {
        return new FlowchartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}