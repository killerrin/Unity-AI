using System.Collections.Generic;
using UnityEngine;

public class AStarNode {
    public Vector2 GridPosition =   new Vector2(-1, -1);
    public GameObject debugDraw;

    public int HeuristicValue =     0; // h
    public int MovementCost =       0; // g
    public int TotalCost =          0; // f

    public int HValue
    {
        get { return HeuristicValue; }
        set { HeuristicValue = value; }
    }
    public int GValue
    {
        get { return MovementCost; }
        set { MovementCost = value; }
    }
    public int FValue
    {
        get { return TotalCost; }
        set { TotalCost = value; }
    }

    public AStarNode Parent             = null;
    public List<AStarNode> Connections  = new List<AStarNode>() { };

    //public AStarNode North              = null;
    //public AStarNode South              = null;
    //public AStarNode East               = null;
    //public AStarNode West               = null;

    public AStarNode(Vector2 gridPos) {
        GridPosition = gridPos;
    }
    public AStarNode(Vector2 gridPos, AStarNode parent)
    {
        GridPosition = gridPos;
        Parent = parent;
    }

    public void CalculateFValue()
    {
        TotalCost = GValue + HValue;
    }
}
