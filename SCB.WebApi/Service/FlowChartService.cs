namespace FlowchartServices;

using System;
using System.Reflection.Emit;

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
        var fractionService = new FractionService();

        int index = 0;
        foreach(var recipeAndAmount in recipesAndAmounts.Items)
        {
            #region Starting nodes and edge
            // Starting node 1 (endproduct)
            var productSubLabel = recipeAndAmount.Amount.Fraction.Teller == 0
                ? $"{recipeAndAmount.Amount.Integer}"
                : $"{recipeAndAmount.Amount.Integer} {recipeAndAmount.Amount.Fraction.Teller}/{recipeAndAmount.Amount.Fraction.Noemer}";
            calculatedNodes.Add(new Node()
            {
                Id=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_product{index}",
                Position = new Position { X = 0f, Y = index * 100f },
                NodeData = new NodeData
                {
                    Label = recipeAndAmount.Recipe.Name,
                    SubLabel = productSubLabel,
                    Image = recipeAndAmount.Recipe.Image
                }
            });

            // Starting node 2 (machine to endproduct)
            #region using fraction service
            var recipeAmount = recipeAndAmount.Recipe.Amount;
            var requestedFraction = fractionService.CompressFraction(recipeAndAmount.Amount);
            var result = fractionService.Division(requestedFraction, recipeAmount);
            var decompressed = fractionService.DecompressFraction(result);
            var machineSubLabel = decompressed.Fraction.Teller == 0
                ? $"{decompressed.Integer}"
                : $"{decompressed.Integer} {decompressed.Fraction.Teller}/{decompressed.Fraction.Noemer}";
            #endregion
            calculatedNodes.Add(new Node()
            {
                Id=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_machine{index}",
                Position = new Position { X = -300f, Y = index * 100f },
                NodeData = new NodeData
                {
                    Label = recipeAndAmount.Recipe.Machine,
                    SubLabel = machineSubLabel,
                    Image = recipeAndAmount.Recipe.Machine
                }
            });
            
            // Starting edge
            calculatedEdges.Add(new Edge(){
                Id=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_machine_to_endproduct{index}",
                Source=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_machine{index}",
                Target=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_product{index}",
                Label=$"{productSubLabel} {recipeAndAmount.Recipe.Name}s per minute"
            });

            #endregion

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