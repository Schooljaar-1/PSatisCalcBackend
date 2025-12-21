namespace FlowchartServices;

using System;

public class FlowChartService{
    
    /// <summary>
    /// Takes in wanted recipes and their amounts needed and calculates the whole resource chain. Ouput complies with react-flow input.
    /// </summary>
    /// <param name="recipesAndAmounts"></param>
    /// <returns></returns>
    public FlowchartResult calculateFlowChart(CalculateRecipeRequest recipesAndAmounts)
    {
        List<Node> calculatedNodes = new();
        List<Edge> calculatedEdges = new();

        int index = 0;
        foreach(var recipeAndAmount in recipesAndAmounts.Items)
        {
            // Starting node (endproduct)
            calculatedNodes.Add(new Node()
            {
                Id=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}",
                Position = new Position { X = 0f, Y = index * 100f },
                NodeData = new NodeData
                {
                    Label = recipeAndAmount.Recipe.Name,
                    SubLabel = $"{recipeAndAmount.Amount.Integer + (double)recipeAndAmount.Amount.Fraction.Teller / recipeAndAmount.Amount.Fraction.Noemer:0.###}",
                    Image = recipeAndAmount.Recipe.Image
                }
            });

            // Filling a list of recipes referred as parts. 

            index++;
        }
        return new FlowchartResult()
        {
            Nodes = calculatedNodes,
            Edges = calculatedEdges
        };
    }
}