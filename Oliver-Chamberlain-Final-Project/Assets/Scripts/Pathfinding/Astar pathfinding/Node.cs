using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 WorldPosition;
    public int gridX, gridY;
    public bool walkable;

    public bool searched;

    public int gCost; //total path distance covered, used for both the A* pathfinding algorithm and the Dijksra algorithm used for the vector flow field


    public int hCost;//the heuristic value used in A* 

    public Node parentNode;
    private int heapIndex; //heap index used in the A* algorithm

    public Vector2 NodeVector;// vector the node contains which points towards the quickest path to the goal


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
