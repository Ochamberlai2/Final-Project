using UnityEngine;

public class Node
{
    public Vector3 WorldPosition;
    public Vector2 GridPosition;
    public bool walkable;
    public int gCost;
    public int hCost;


    //when heap is implemented
    //public int heapIndex;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public Node( Vector3 _worldPosition,Vector2 _GridPosition, bool _walkable)
    {
        WorldPosition = _worldPosition;
        GridPosition = _GridPosition;
        walkable = _walkable;
    }

	
}
