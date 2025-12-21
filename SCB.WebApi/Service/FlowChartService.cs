namespace FlowchartServices;

using System;
using System.Reflection.Emit;
using System.IO;
using System.Text.Json;
using System.Linq;

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
            var machineAmountresult = fractionService.Division(requestedFraction, recipeAmount);
            var decompressed = fractionService.DecompressFraction(machineAmountresult);
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
            List<PartsAndTargets> parstList = new();
            #region getting a list of all parts and filling partslist with the recipes of parts needed from parent, saving parent as target
            // Load recipes from Data/Recipes.json
            var recipesJsonPath = Path.Combine("Data", "Recipes.json");
            if (!File.Exists(recipesJsonPath))
                throw new Exception($"Recipes data file not found at {recipesJsonPath}");

            var recipesJson = File.ReadAllText(recipesJsonPath);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<Recipe>? allRecipes = JsonSerializer.Deserialize<List<Recipe>>(recipesJson, jsonOptions);
            if (allRecipes == null)
                throw new Exception("Failed to parse recipes data");

            // If there are no parts, skip
            if (recipeAndAmount.Recipe.Parts != null)
            {
                foreach (var part in recipeAndAmount.Recipe.Parts)
                {
                    var partName = part.PartName;
                    var partVersion = "default"; // use default when not provided

                    var found = allRecipes.FirstOrDefault(r => r.Name == partName && r.Version == partVersion);
                    if (found == null)
                        throw new Exception($"recipe with name {partName} and version {partVersion} is unknown");

                    parstList.Add(new(){
                        Recipe=found,
                        Target=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_machine{index}",
                        TargetAmont=machineAmountresult
                    });
                }
            }
            #endregion

            while(parstList.Count != 0){
               Console.WriteLine(JsonSerializer.Serialize(parstList, new JsonSerializerOptions { WriteIndented = true }));
               parstList.Clear();
            }

            index++;
        }
        return new FlowchartResult()
        {
            Nodes = calculatedNodes,
            Edges = calculatedEdges
        };
    }
}