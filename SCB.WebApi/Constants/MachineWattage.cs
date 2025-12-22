namespace FlowchartServices.Constants;

using System.Collections.Generic;

public static class MachineWattages
{
    public static readonly IReadOnlyDictionary<string, int> Values = new Dictionary<string, int>
    {
        {"Smelter", 4},
        {"Constructor", 4},
        {"Assembler", 15},
        {"Foundry", 16},
        {"Manufacturer", 55},
        {"Refinery", 30},
        {"Blender", 75},
        {"Packager", 10}
    };
}