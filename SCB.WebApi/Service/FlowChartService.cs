namespace FlowchartServices;

using FlowchartServices.Constants;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

public class FlowChartService
{
    private const float VerticalSpacing = 200f; 
    private const float HorizontalSpacing = 250f;
    
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

        // Queue for production requirements
        List<PartsAndTargets> partsList = new();

        // --- 1. SETUP FINAL SINK NODES ---
        // These represent your end goals (e.g., "12 Iron Rods" and "333 Screws")
        int productIndex = 0;
        foreach (var recipeAndAmount in recipesAndAmounts.Items)
        {
            string finalSinkId = $"sink_{recipeAndAmount.Recipe.Name}_{recipeAndAmount.Recipe.Version}_{productIndex}";
            
            calculatedNodes.Add(new Node()
            {
                Id = finalSinkId,
                Position = new Position { X = 0f, Y = GetNextY(0) },
                NodeData = new NodeData {
                    Label = recipeAndAmount.Recipe.Name,
                    SubLabel = FormatFraction(recipeAndAmount.Amount),
                    Image = recipeAndAmount.Recipe.Image
                }
            });

            // Add this as a requirement to be produced by a machine
            var requestedFraction = fractionService.CompressFraction(recipeAndAmount.Amount);
            partsList.Add(new PartsAndTargets {
                Recipe = recipeAndAmount.Recipe,
                Target = finalSinkId,
                TargetAmount = requestedFraction
            });

            productIndex++;
        }

        // --- 2. RECURSIVE PRODUCTION (Breadth-First) ---
        int currentDepth = 1;
        while (partsList.Any())
        {
            List<PartsAndTargets> nextDepthParts = new();
            
            foreach (var part in partsList)
            {
                // SHARED NODE ID: By using Name and Version (without an index), 
                // all requests for "Iron Rod" point to the same machine node.
                string machineNodeId = $"prod_{part.Recipe.Name}_{part.Recipe.Version}";
                
                // Calculate machines needed for this specific path
                var machinesNeededForThisPath = fractionService.Division(part.TargetAmount, part.Recipe.Amount);

                var existingNode = calculatedNodes.FirstOrDefault(n => n.Id == machineNodeId);
                if (existingNode != null)
                {
                    // If node exists, add to its total machine count
                    nodeAmounts[machineNodeId] = fractionService.Addition(nodeAmounts[machineNodeId], machinesNeededForThisPath);
                    
                    // Update labels for the shared node
                    if (part.Recipe.Machine == "Mining drill") {
                         var totalOre = fractionService.Multiplication(nodeAmounts[machineNodeId], part.Recipe.Amount);
                         existingNode.NodeData.SubLabel = $"{FractionToDouble(totalOre):0.###} ore p/m";
                    }
                    else {
                        existingNode.NodeData.SubLabel = FormatFraction(fractionService.DecompressFraction(nodeAmounts[machineNodeId]));
                    }
                }
                else
                {
                    // Create the shared production node for the first time
                    nodeAmounts[machineNodeId] = machinesNeededForThisPath;
                    calculatedNodes.Add(new Node()
                    {
                        Id = machineNodeId,
                        Position = new Position { X = -(currentDepth * HorizontalSpacing), Y = GetNextY(currentDepth) },
                        NodeData = new NodeData {
                            Label = part.Recipe.Machine,
                            SubLabel = (part.Recipe.Machine == "Mining drill")
                                ? $"{FractionToDouble(part.TargetAmount):0.###} ore p/m"
                                : FormatFraction(fractionService.DecompressFraction(machinesNeededForThisPath)),
                            Image = part.Recipe.Machine
                        }
                    });
                }

                // Create or update the edge from the Shared Producer to the Consumer
                UpdateOrAddEdge(calculatedEdges, edgeAmounts, machineNodeId, part.Target, part.TargetAmount, part.Recipe.Name, fractionService);

                // Queue sub-ingredients for the next depth
                if (part.Recipe.Parts != null)
                {
                    foreach (var subPart in part.Recipe.Parts)
                    {
                        var foundSub = GetRecipeOrMockOre(allRecipes, subPart.PartName);
                        if (foundSub != null)
                        {
                            nextDepthParts.Add(new PartsAndTargets {
                                Recipe = foundSub,
                                Target = machineNodeId,
                                TargetAmount = fractionService.Multiplication(machinesNeededForThisPath, subPart.Amount)
                            });
                        }
                    }
                }
            }
            partsList = nextDepthParts;
            currentDepth++;
        }

        float wattage = calculateWattage(calculatedNodes);
        return new FlowchartResult { Nodes = calculatedNodes, Edges = calculatedEdges, Wattage = wattage };
    }

    private void UpdateOrAddEdge(List<Edge> edges, Dictionary<string, dynamic> edgeAmounts, string source, string target, dynamic amountFraction, string itemName, FractionService fs)
    {
        string edgeKey = $"{source}->{target}";
        
        if (edgeAmounts.ContainsKey(edgeKey))
        {
            edgeAmounts[edgeKey] = fs.Addition(edgeAmounts[edgeKey], amountFraction);
            var existingEdge = edges.First(e => e.Source == source && e.Target == target);
            existingEdge.Label = $"{FractionToDouble(edgeAmounts[edgeKey]):0.###} {itemName}s/m";
        }
        else
        {
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

    private Recipe? GetRecipeOrMockOre(List<Recipe> allRecipes, string partName)
    {
        var found = allRecipes.FirstOrDefault(r => r.Name == partName && r.Version == "default");
        
        if (found == null && partName.Contains("ore", StringComparison.OrdinalIgnoreCase))
        {
            return new Recipe
            {
                Name = partName,
                Machine = "Mining drill",
                Version = "default",
                Type = "Ore",
                Image = "Mining drill",
                Amount = new Fraction { Teller = 1, Noemer = 1 }, 
                Parts = null 
            };
        }
        return found;
    }

    private float calculateWattage(List<Node> calculatedNodes)
    {
        float wattageSum = 0f; 

        foreach(Node node in calculatedNodes)
        {
            if (MachineWattages.Values.ContainsKey(node.NodeData.Label))
            {
                float machineWattage, machineAmount;

                // Mining drills and Sinks use custom labels, handle them carefully
                if (node.Id.Contains("sink") || node.NodeData.SubLabel.Contains("ore"))
                {
                    // Sinks (final products) don't use power, or are calculated differently
                    continue; 
                }

                FractionDecompressed nodeFractionDecompressed = Parse(node.NodeData.SubLabel);
                
                // If there's any fractional machine needed, we must round up to the next whole machine
                if(nodeFractionDecompressed.Fraction.Teller != 0)
                    machineAmount = nodeFractionDecompressed.Integer + 1;
                else
                    machineAmount = nodeFractionDecompressed.Integer;

                machineWattage = MachineWattages.Values[node.NodeData.Label];
                wattageSum += machineWattage * machineAmount;
            }
        }
        return wattageSum;
    }

    private static FractionDecompressed Parse(string input)
    {
        var parts = input.Split(' ');
        
        // Handle cases where there is only an integer (e.g. "4")
        if (parts.Length == 1)
        {
            return new FractionDecompressed {
                Integer = int.Parse(parts[0]),
                Fraction = new Fraction { Teller = 0, Noemer = 1 }
            };
        }

        int integerPart = int.Parse(parts[0]);
        var fractionParts = parts[1].Split('/');
        
        return new FractionDecompressed
        {
            Integer = integerPart,
            Fraction = new Fraction 
            { 
                Teller = int.Parse(fractionParts[0]), 
                Noemer = int.Parse(fractionParts[1]) 
            }
        };
    }
}