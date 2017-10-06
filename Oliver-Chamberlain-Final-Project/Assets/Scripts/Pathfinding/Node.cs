using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 WorldPosition;
    public int gridX, gridY;
    public bool walkable;
    public int gCost;
    public int hCost;

    public Node parentNode;
    private int heapIndex;

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
    }
    public Node( Vector3 _worldPosition,Vector2 _GridPosition, bool _walkable)
    {
        WorldPosition = _worldPosition;
        gridX = (int)_GridPosition.x;
        gridY = (int)_GridPosition.y;
        walkable = _walkable;
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
	
}
