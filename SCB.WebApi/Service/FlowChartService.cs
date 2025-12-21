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
        var allRecipes = GetAllRecipes();

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
                Label=$"{(recipeAndAmount.Amount.Integer + (double)recipeAndAmount.Amount.Fraction.Teller / recipeAndAmount.Amount.Fraction.Noemer):0.###} {recipeAndAmount.Recipe.Name}s per minute"
            });

            #endregion

            // Filling a list of recipes referred as parts.
            List<PartsAndTargets> partsList = new();
            #region getting a list of all parts and filling partslist with the recipes of parts needed from parent, saving parent as target
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

                    // TargetAmount should represent the resource flow (items/min) required by the target machine:
                    // machinesNeededForTarget * inputPerMachine
                    var requiredPartAmount = fractionService.Multiplication(machineAmountresult, part.Amount);

                    partsList.Add(new(){
                        Recipe=found,
                        Target=$"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_machine{index}",
                        TargetAmount=requiredPartAmount
                    });
                }
            }
            #endregion

            int depth = 0;
            while(partsList.Count != 0){
                List<PartsAndTargets> temporaryPartsList = new();
                int partIndex = 0;
                
                foreach(var part in partsList){
                    var partNodeId = $"{part.Recipe.Name}_{part.Recipe.Version}_node_{index}_{depth}_{partIndex}";
                    #region calculating part machine amount
                    var partCalculatedMachine = fractionService.Division(part.TargetAmount, part.Recipe.Amount);
                    var partCalculatedMachineDecompressed = fractionService.DecompressFraction(partCalculatedMachine);
                    var partSublabel = partCalculatedMachineDecompressed.Fraction.Teller == 0
                        ? $"{partCalculatedMachineDecompressed.Integer}"
                        : $"{partCalculatedMachineDecompressed.Integer} {partCalculatedMachineDecompressed.Fraction.Teller}/{partCalculatedMachineDecompressed.Fraction.Noemer}";
                    #endregion

                    # region adding part node and edge
                    if(part.Recipe.Machine != "Mining drill")
                    {
                        calculatedNodes.Add(new Node()
                        {
                            Id = partNodeId,
                            Position = new Position { X = (-600f - (depth * 300f)) - (partIndex * 200f), Y = index * 100f },
                            NodeData = new NodeData 
                            {
                                Label = part.Recipe.Machine,
                                SubLabel = $"{partSublabel}",
                                Image = part.Recipe.Machine
                            }
                        });
                    }
                    else{
                        calculatedNodes.Add(new Node()
                        {
                            Id = partNodeId,
                            Position = new Position { X = (-600f - (depth * 300f)) - (partIndex * 200f), Y = index * 100f },
                            NodeData = new NodeData 
                            {
                                Label = part.Recipe.Machine,
                                SubLabel = $"{(double)part.TargetAmount.Teller / part.TargetAmount.Noemer:0.###} ore p/m",
                                Image = part.Recipe.Machine
                            }
                        });
                    }

                    calculatedEdges.Add(new Edge()
                    {
                        Id = $"{part.Recipe.Name}_{part.Recipe.Version}_edge_{index}_{depth}_{partIndex}",
                        Source = partNodeId,
                        Target=$"{part.Target}",
                        Label=$"{(double)part.TargetAmount.Teller / part.TargetAmount.Noemer:0.###} {part.Recipe.Name}s per minute"
                    });

                    #endregion

                    # region adding new parts to partlist, skipping if no parts inside of next level recipe
                    if (part.Recipe.Parts != null && part.Recipe.Parts.Count != 0)
                    {
                        foreach (var subPart in part.Recipe.Parts)
                        {
                            var subPartName = subPart.PartName;
                            var subPartVersion = "default";

                            var foundSub = allRecipes.FirstOrDefault(r => r.Name == subPartName && r.Version == subPartVersion);
                            if (foundSub == null)
                                throw new Exception($"recipe with name {subPartName} and version {subPartVersion} is unknown");
                                
                            var neededSubPartAmount = fractionService.Multiplication(partCalculatedMachine, subPart.Amount);

                            temporaryPartsList.Add(new PartsAndTargets(){
                                Recipe = foundSub,
                                Target = partNodeId,
                                TargetAmount = neededSubPartAmount
                            });
                        }
                    }
                    #endregion
                    partIndex++;
                }

                partsList.Clear();
                partsList.AddRange(temporaryPartsList);
                temporaryPartsList.Clear();
                depth++;
            }

            index++;
        }
        return new FlowchartResult()
        {
            Nodes = calculatedNodes,
            Edges = calculatedEdges
        };
    }
    
    public List<Recipe> GetAllRecipes(){
        // Load recipes from Data/Recipes.json
        var recipesJsonPath = Path.Combine("Data", "Recipes.json");
        if (!File.Exists(recipesJsonPath))
            throw new Exception($"Recipes data file not found at {recipesJsonPath}");

        var recipesJson = File.ReadAllText(recipesJsonPath);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        List<Recipe>? allRecipes = JsonSerializer.Deserialize<List<Recipe>>(recipesJson, jsonOptions);
        if (allRecipes == null)
            throw new Exception("Failed to parse recipes data");
        
        return allRecipes;
    }
}