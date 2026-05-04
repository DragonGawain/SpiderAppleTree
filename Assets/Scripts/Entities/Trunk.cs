using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trunk : IWalkable
{
    public readonly Coord coord;
    public readonly int height;
    public List<Branch> leftSide = new();
    public List<Branch> rightSide = new();

    // can walk hori at the BASE of trees! Connecting trunks are floating trunks with a height of 1.
    public bool CanWalkHorizontal(int y) => y == coord.y;

    public bool CanWalkVertical() => true;
}
