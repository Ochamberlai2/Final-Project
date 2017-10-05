using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    public LayerMask UnwalkableLayer;

    [Header("Grid Attributes")]
    public Vector2 gridWorldSize;
    [Range(0, 2)]
    public float nodeRadius;
    [HideInInspector]
    public Node[,] grid;

    [Space]
    [SerializeField]
    private Color GizmoColor;

    private float nodeDiameter;
    [HideInInspector]
    public int gridSizeX;
    [HideInInspector]
    public int gridSizeY;
    [HideInInspector]
    public int maxSize;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        maxSize = gridSizeX* gridSizeY;

        CreateGrid();
    }

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
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // we convert the world position into the percentage of how far along the grid it is
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        //then we need to clamp them to between 0 and 1
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //because arrays are zero indexed, we subtract 1 from the size of the grid
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        //then we return the grid coordinate that we need 
        return grid[x, y];
    }
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
                    int checkX = (int)node.GridPosition.x + x;
                    int checkY = (int)node.GridPosition.y + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        Neighbours.Add(grid[checkX, checkY]);
                    }

                }
            }     
        return Neighbours;
    }

    private void OnDrawGizmos()
    {
        //wire cube for showing the outside bounds of the grid
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,0, gridWorldSize.y));

        if (grid != null)
        {
            Gizmos.color = GizmoColor;
            //draw vertical lines
            for (int x = 0; x < gridSizeX; x++)
            {
                Gizmos.DrawLine(new Vector3(grid[x, 0].WorldPosition.x - nodeRadius, grid[x, 0].WorldPosition.y + 0.1f, grid[x, 0].WorldPosition.z -nodeRadius), new Vector3(grid[x, gridSizeY - 1].WorldPosition.x - nodeRadius, grid[x, gridSizeY - 1].WorldPosition.y + 0.1f, grid[x, gridSizeY - 1].WorldPosition.z + nodeRadius));
            }
            Gizmos.DrawLine(new Vector3(grid[gridSizeX-1, 0].WorldPosition.x + nodeRadius, grid[gridSizeX - 1, 0].WorldPosition.y + 0.1f, grid[gridSizeX-1, 0].WorldPosition.z - nodeRadius), new Vector3(grid[gridSizeX - 1, gridSizeY - 1].WorldPosition.x + nodeRadius, grid[gridSizeX - 1, gridSizeY - 1].WorldPosition.y + 0.1f, grid[gridSizeX - 1, gridSizeY - 1].WorldPosition.z + nodeRadius));
            //draw horizontal lines
            for (int y = 0; y < gridSizeY; y++)
            {
                Gizmos.DrawLine(new Vector3(grid[0, y].WorldPosition.x - nodeRadius, grid[0, y].WorldPosition.y + 0.1f, grid[0, y].WorldPosition.z - nodeRadius), new Vector3(grid[gridSizeX - 1, y].WorldPosition.x + nodeRadius, grid[gridSizeX - 1, y].WorldPosition.y + 0.1f, grid[gridSizeX - 1, y].WorldPosition.z - nodeRadius));
            }
            Gizmos.DrawLine(new Vector3(grid[0, gridSizeY -1].WorldPosition.x - nodeRadius, grid[0, gridSizeY - 1].WorldPosition.y + 0.1f, grid[0, gridSizeY - 1].WorldPosition.z + nodeRadius), new Vector3(grid[gridSizeX - 1, gridSizeY -1].WorldPosition.x + nodeRadius, grid[gridSizeX - 1, gridSizeY - 1].WorldPosition.y + 0.1f, grid[gridSizeX - 1, gridSizeY - 1].WorldPosition.z + nodeRadius));
        }
    }

}

