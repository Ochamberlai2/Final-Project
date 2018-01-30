using UnityEngine;

public class Node
{
    public Vector3 WorldPosition;
    public int gridX, gridY;
    public bool walkable;

    //for debugging only
    public bool searched;
    public bool goalNode;
    //for debugging only




    public int gCost; //total path distance covered, used for both the A* pathfinding algorithm and the Dijksra algorithm used for the vector flow field

    public Node( Vector3 _worldPosition,Vector2 _GridPosition, bool _walkable)
    {
        WorldPosition = _worldPosition;
        gridX = (int)_GridPosition.x;
        gridY = (int)_GridPosition.y;
        walkable = _walkable;
    }


	
}
