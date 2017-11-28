using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarNode : Node, IHeapItem<AstarNode>
{
    public int hCost;//the heuristic value used in A* 

    public AstarNode parentNode;
    private int heapIndex; //heap index used in the A* algorithm

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }//heuristic value + total path distance covered
    public AstarNode(Vector3 _worldPosition, Vector2 _GridPosition, bool _walkable) : base(_worldPosition, _GridPosition, _walkable)
    {
        
    }
    public int CompareTo(AstarNode nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
