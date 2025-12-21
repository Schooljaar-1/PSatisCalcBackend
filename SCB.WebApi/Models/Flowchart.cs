public class Node
{
    public required string Id {get; set;} //Unique indentifier of node
    public required Position Position {get; set;} //Position of node inside of flowchart
    public required NodeData NodeData {get; set;} //↓↓↓ See NodeData model class 
}

public class Edge
{
    public required string Id {get; set;} //Unique identifier of edge
    public required string Source {get; set;} //Connection comes from which node?
    public required string Target {get; set;} //Connection goes to which node?
    public required string Label {get; set;} //The amount of units per minute that gets transferred between nodes
}

public class Position
{
    public float X {get; set;} //X-coordinate on flowchart
    public float Y {get; set;} //Y-coordinate on flowchart
}

public class NodeData
{
    public required string Label {get; set;} //Machine type
    public required string SubLabel {get; set;} //Amount of machines needed
    public required string Image {get; set;} //Image of said machine
}

public class FlowchartResult
{
    public required List<Node> Nodes {get; set;}
    public required List<Edge> Edges {get; set;}
}

public class PartsAndTargets
{
    public required Recipe Recipe {get; set;}
    public required string Target {get; set;}
    public required Fraction TargetAmont {get; set;}
}