using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Grid : MonoBehaviour {

    public LayerMask UnwalkableLayer;
    #region DebugGizmos
    [SerializeField]
    private bool DrawGcost;
    [SerializeField]
    private bool DrawFlowField;
#endregion
    [Header("Grid Attributes")]
    public Vector2 gridWorldSize;
    [Range(0, 2)]
    public float nodeRadius;
    [HideInInspector]
    public Node[,] grid;

    [Space]
    [SerializeField]
    private Color WalkableGridColor;
    [SerializeField]
    private Color UnwalkableGridColor;

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
    public Node NodeFromWorldPoint(Vector3 WorldPoint)
    {
        // we convert the world position into the percentage of how far along the grid it is
        float percentX = (WorldPoint.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (WorldPoint.z + gridWorldSize.y / 2) / gridWorldSize.y;
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
    public Node[] GetOrthogonalNeighbours(Node node)
    {
       Node[] neighbours = new Node[4];
        //add top
        if (node.gridY < gridSizeY-1 && grid[node.gridX, node.gridY + 1].walkable)
            neighbours[0] = grid[node.gridX, node.gridY + 1];
        //add bottom
        if (node.gridY > 0 && grid[node.gridX, node.gridY - 1].walkable) 
            neighbours[1] = grid[node.gridX, node.gridY - 1];
        //add left
        if (node.gridX > 0 && grid[node.gridX - 1, node.gridY].walkable)
            neighbours[2] = grid[node.gridX - 1, node.gridY];
        //add right
        if (node.gridX < gridSizeX-1 && grid[node.gridX + 1, node.gridY].walkable)
            neighbours[3] = grid[node.gridX + 1, node.gridY];

       
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        //wire cube for showing the outside bounds of the grid
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x,0, gridWorldSize.y));

        if (grid != null)
        {
        
            foreach(Node node in grid)
            {
                if (DrawGcost)
                {
                    Gizmos.color = (node.walkable)? Color.Lerp(Color.cyan,Color.black, (float)node.gCost/100) : Color.black;
                    Gizmos.DrawCube(node.WorldPosition, new Vector3(1,0.1f,1) * (nodeDiameter - .1f));
                    Handles.Label(node.WorldPosition + new Vector3(-nodeRadius, 5, nodeRadius),node.gCost.ToString());
                }
                if(node.startNode)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(node.WorldPosition, nodeRadius);
                }

                Gizmos.color = (node.walkable) ? WalkableGridColor : UnwalkableGridColor;
                Gizmos.DrawWireCube(node.WorldPosition, new Vector3(0.95f,0,0.95f)* nodeDiameter);


                if (DrawFlowField)
                {
                    Ray ray = new Ray(node.WorldPosition, node.NodeVector);
                    Gizmos.DrawRay(ray);    
                }
                if (node.NodeVector == Vector2.zero && node.searched && !node.startNode)
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (nodeDiameter - .1f));
            }
           
        }
    }

}

