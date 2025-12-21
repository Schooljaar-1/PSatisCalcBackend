namespace FlowchartServices;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

public class FlowChartService
{
    private const float VerticalSpacing = 200f; 
    private const float HorizontalSpacing = 400f;
    
    private Dictionary<int, float> _columnYOffsets = new();

    public FlowchartResult calculateFlowChart(CalculateRecipeRequest recipesAndAmounts)
    {
        _columnYOffsets.Clear();
        List<Node> calculatedNodes = new();
        List<Edge> calculatedEdges = new();
        var fractionService = new FractionService();
        var allRecipes = GetAllRecipes();
        // Stores the cumulative sum of machines (or ore per minute) for each node ID
        Dictionary<string, dynamic> nodeAmounts = new();
        // Stores the cumulative sum of items per minute for each Source-Target pair
        Dictionary<string, dynamic> edgeAmounts = new();

        int productIndex = 0;
        foreach (var recipeAndAmount in recipesAndAmounts.Items)
        {
            // --- 1. FINAL PRODUCT NODE (Depth 0) ---
            string productNodeId = $"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_prod_{productIndex}";
            
            calculatedNodes.Add(new Node()
            {
                Id = productNodeId,
                Position = new Position { X = 0f, Y = GetNextY(0) },
                NodeData = new NodeData {
                    Label = recipeAndAmount.Recipe.Name,
                    SubLabel = FormatFraction(recipeAndAmount.Amount),
                    Image = recipeAndAmount.Recipe.Image
                }
            });

            // --- 2. FINAL MACHINE NODE (Depth 1) ---
            string machineNodeId = $"{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_final_mach_{productIndex}";
            var requestedFraction = fractionService.CompressFraction(recipeAndAmount.Amount);
            var machineAmountResult = fractionService.Division(requestedFraction, recipeAndAmount.Recipe.Amount);
            nodeAmounts[machineNodeId] = machineAmountResult;

            calculatedNodes.Add(new Node()
            {
                Id = machineNodeId,
                Position = new Position { X = -HorizontalSpacing, Y = GetNextY(1) },
                NodeData = new NodeData {
                    Label = recipeAndAmount.Recipe.Machine,
                    SubLabel = FormatFraction(fractionService.DecompressFraction(machineAmountResult)),
                    Image = recipeAndAmount.Recipe.Machine
                }
            });

            UpdateOrAddEdge(calculatedEdges, edgeAmounts, machineNodeId, productNodeId, requestedFraction, recipeAndAmount.Recipe.Name, fractionService);

            // --- 3. RECURSIVE PARTS (Depth 2+) ---
            List<PartsAndTargets> partsList = new();
            if (recipeAndAmount.Recipe.Parts != null)
            {
                foreach (var part in recipeAndAmount.Recipe.Parts)
                {
                    var found = allRecipes.FirstOrDefault(r => r.Name == part.PartName && r.Version == "default");
                    if (found == null) continue;
                    partsList.Add(new PartsAndTargets {
                        Recipe = found,
                        Target = machineNodeId,
                        TargetAmount = fractionService.Multiplication(machineAmountResult, part.Amount)
                    });
                }
            }

            int currentDepth = 2;
            while (partsList.Any())
            {
                List<PartsAndTargets> temporaryPartsList = new();
                
                foreach (var part in partsList)
                {
                    string partNodeId = $"{part.Recipe.Name}_{part.Recipe.Version}";
                    var partCalculatedMachine = fractionService.Division(part.TargetAmount, part.Recipe.Amount);

                    var existingNode = calculatedNodes.FirstOrDefault(n => n.Id == partNodeId);
                    if (existingNode != null)
                    {
                        // Update existing node's math (Summing machine counts)
                        nodeAmounts[partNodeId] = fractionService.Addition(nodeAmounts[partNodeId], partCalculatedMachine);
                        
                        // If it's a miner, we want to show total ORE per minute (which equals the total machine output here)
                        if (part.Recipe.Machine == "Mining drill") {
                             // Re-calculating total ore: (Total Machines * Recipe Output)
                             var totalOre = fractionService.Multiplication(nodeAmounts[partNodeId], part.Recipe.Amount);
                             existingNode.NodeData.SubLabel = $"{FractionToDouble(totalOre):0.###} ore p/m";
                        }
                        else {
                            existingNode.NodeData.SubLabel = FormatFraction(fractionService.DecompressFraction(nodeAmounts[partNodeId]));
                        }
                    }
                    else
                    {
                        nodeAmounts[partNodeId] = partCalculatedMachine;
                        calculatedNodes.Add(new Node()
                        {
                            Id = partNodeId,
                            Position = new Position { X = -(currentDepth * HorizontalSpacing), Y = GetNextY(currentDepth) },
                            NodeData = new NodeData {
                                Label = part.Recipe.Machine,
                                SubLabel = (part.Recipe.Machine == "Mining drill")
                                    ? $"{FractionToDouble(part.TargetAmount):0.###} ore p/m"
                                    : FormatFraction(fractionService.DecompressFraction(partCalculatedMachine)),
                                Image = part.Recipe.Machine
                            }
                        });
                    }

                    // UPDATE OR ADD EDGE (Summing the labels if source/target match)
                    UpdateOrAddEdge(calculatedEdges, edgeAmounts, partNodeId, part.Target, part.TargetAmount, part.Recipe.Name, fractionService);

                    if (part.Recipe.Parts != null)
                    {
                        foreach (var subPart in part.Recipe.Parts)
                        {
                            var foundSub = allRecipes.FirstOrDefault(r => r.Name == subPart.PartName && r.Version == "default");
                            if (foundSub != null)
                            {
                                temporaryPartsList.Add(new PartsAndTargets {
                                    Recipe = foundSub,
                                    Target = partNodeId,
                                    TargetAmount = fractionService.Multiplication(partCalculatedMachine, subPart.Amount)
                                });
                            }
                        }
                    }
                }
                partsList = temporaryPartsList;
                currentDepth++;
            }
            productIndex++;
        }
        return new FlowchartResult { Nodes = calculatedNodes, Edges = calculatedEdges };
    }

    private void UpdateOrAddEdge(List<Edge> edges, Dictionary<string, dynamic> edgeAmounts, string source, string target, dynamic amountFraction, string itemName, FractionService fs)
    {
        string edgeKey = $"{source}->{target}";
        
        if (edgeAmounts.ContainsKey(edgeKey))
        {
            // Update the math
            edgeAmounts[edgeKey] = fs.Addition(edgeAmounts[edgeKey], amountFraction);
            
            // Find the existing edge object and update its label
            var existingEdge = edges.First(e => e.Source == source && e.Target == target);
            existingEdge.Label = $"{FractionToDouble(edgeAmounts[edgeKey]):0.###} {itemName}s/m";
        }
        else
        {
            // Create new
            edgeAmounts[edgeKey] = amountFraction;
            edges.Add(new Edge() {
                Id = $"edge_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Source = source,
                Target = target,
                Label = $"{FractionToDouble(amountFraction):0.###} {itemName}s/m"
            });
        }
    }

    private float GetNextY(int depth)
    {
        if (!_columnYOffsets.ContainsKey(depth))
        {
            _columnYOffsets[depth] = 0f;
            return 0f;
        }
        _columnYOffsets[depth] += VerticalSpacing;
        return _columnYOffsets[depth];
    }

    private string FormatFraction(dynamic amount)
    {
        if (amount.Fraction.Teller == 0) return $"{amount.Integer}";
        return $"{amount.Integer} {amount.Fraction.Teller}/{amount.Fraction.Noemer}";
    }

    private double FractionToDouble(dynamic fraction)
    {
        if (fraction.Noemer == 0) return 0;
        return (double)fraction.Teller / fraction.Noemer;
    }

    public List<Recipe> GetAllRecipes()
    {
        var recipesJsonPath = Path.Combine("Data", "Recipes.json");
        var recipesJson = File.ReadAllText(recipesJsonPath);
        return JsonSerializer.Deserialize<List<Recipe>>(recipesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Recipe>();
    }
}