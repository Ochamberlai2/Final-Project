using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Grid : MonoBehaviour {

    public LayerMask UnwalkableLayer;

    [Header("Grid Attributes")]
    public Vector2 gridWorldSize; //the size of the grid in world space
    [Range(0, 2)]
    public float nodeRadius;
    [HideInInspector]
    public Node[,] grid;

    
    private float nodeDiameter;
    [HideInInspector]
    public int gridSizeX;//size of the grid arrays x dimension
    [HideInInspector]
    public int gridSizeY;//size of the grid arrays y dimension
    [HideInInspector]
    public int maxSize;//the grids x size multiplied by the y


    //colours to represent each grid square
    [Space]
    [SerializeField]
    private Color WalkableGridColor;
    [SerializeField]
    private Color UnwalkableGridColor;
   
    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        maxSize = gridSizeX* gridSizeY;

        CreateGrid();
    }
    /// <summary>
    /// Generate the pathfinding grid using the Node class
    /// </summary>
    private void CreateGrid()
    {
        grid = new Node[gridSizeX,gridSizeY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        //populate the grid
        for(int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, UnwalkableLayer));

                grid[x, y] = new Node(worldPoint, new Vector2(x, y), walkable);

            }
        }
    }

    /// <summary>
    /// Finds a node on the grid using a vector3 world point
    /// </summary>
    /// <param name="WorldPoint">The position in world space to check against</param>
    /// <returns>The node situated at the specified point</returns>
    public Node NodeFromWorldPoint(Vector3 WorldPoint)
    {
        // we convert the world position into the percentage of how far along the grid it is
        float percentX = (WorldPoint.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (WorldPoint.z + gridWorldSize.y / 2) / gridWorldSize.y;
        //then we need to clamp them to between 0 and 1
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //because arrays are zero indexed, we subtract 1 from the size of the grid
        int x = Mathf.RoundToInt((gridSizeX) * percentX);
        int y = Mathf.RoundToInt((gridSizeY) * percentY);

        //then we return the grid coordinate that we need 
        return grid[x, y];
    }

    /// <summary>
    /// Find diagonal and orthogonal neighbours of a given node
    /// </summary>
    /// <param name="node">The node to check against</param>
    /// <returns>The diagonal and orthogonal neighbours of the argument</returns>
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> Neighbours = new List<Node>();
      
            //loop through the 9 surrounding nodes
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //if the x and y are both 0, it is the searching node so skip it
                    if (x == 0 && y == 0)
                        continue;
                    
                    //then check the x and y to give their position in the grid
                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        Neighbours.Add(grid[checkX, checkY]);
                    }

                }
            }     
        return Neighbours;
    }
    /// <summary>
    /// Validate that a grid position is inside of the grid
    /// </summary>
    /// <param name="x">Distance along the x axis of the grid</param>
    /// <param name="y">Distance along the y axis of the grid</param>
    /// <returns>true if the point is on the grid, false if not</returns>
    public bool ValidatePointOnGrid(int x, int y)
    {
        return (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY);
    }

    /// <summary>
    /// Draw gizmos relating to the grid itself.
    /// </summary>
    private void OnDrawGizmos()
    {
        //wire cube for showing the outside bounds of the grid
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,0, gridWorldSize.y));

        if (grid != null)
        {
            foreach(Node node in grid)
            {
                //draw each node in the grid
                Gizmos.color = (node.walkable) ? WalkableGridColor : UnwalkableGridColor;
                Gizmos.DrawWireCube(node.WorldPosition, new Vector3(0.95f,0,0.95f)* nodeDiameter);

            }
           
        }
    }
  

}