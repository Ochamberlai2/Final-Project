using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 WorldPosition;
    public Vector2 GridPosition;
    public bool walkable;
    public int gCost;
    public int hCost;

    public Node parentNode;

    public int HeapIndex
    {
        get
        {
            return HeapIndex;
        }
        set
        {
            HeapIndex = value;
        }
    }
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare; 
    }
    public Node( Vector3 _worldPosition,Vector2 _GridPosition, bool _walkable)
    {
        WorldPosition = _worldPosition;
        GridPosition = _GridPosition;
        walkable = _walkable;
    }

	
}
