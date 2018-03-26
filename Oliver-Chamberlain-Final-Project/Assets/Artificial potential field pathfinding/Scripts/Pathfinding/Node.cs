using UnityEngine;

public class Node
{
    public Vector3 WorldPosition;
    public int gridX, gridY;
    public bool walkable;





    public Node( Vector3 _worldPosition,Vector2 _GridPosition, bool _walkable)
    {
        WorldPosition = _worldPosition;
        gridX = (int)_GridPosition.x;
        gridY = (int)_GridPosition.y;
        walkable = _walkable;
    }


	
}
