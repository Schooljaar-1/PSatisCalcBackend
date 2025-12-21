namespace FlowchartServices.Constants;

using System.Collections.Generic;

public static class MachineWattages
{
    public static readonly IReadOnlyDictionary<string, double> Values = new Dictionary<string, double>
    {
        {"Smelter", 4.0},
        {"Constructor", 4.0},
        {"Assembler", 15.0},
        {"Foundry", 16.0},
        {"Manufacturer", 55.0},
        {"Refinery", 30.0},
        {"Blender", 75.0},
        {"Packager", 10.0}
    };
}