namespace FlowchartServices;

public class FlowChartService{
    
    public FlowChartResult calculateFlowChart(List<FractionDecompressed> amounts, List<Recipe> recipes)
    {
        return new FlowChartResult()
        {
            Nodes = new(),
            Edges = new()
        };
    }
}