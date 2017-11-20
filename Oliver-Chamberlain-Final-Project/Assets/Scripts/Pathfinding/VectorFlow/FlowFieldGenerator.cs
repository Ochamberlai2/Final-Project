using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldGenerator : MonoBehaviour {

    Grid grid;
    public Vector2[,] flowField;

    public void Start()
    {
        grid = FindObjectOfType<Grid>();
        flowField = new Vector2[grid.gridSizeX,grid.gridSizeY];
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            DijkstraFloodFill(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            GenerateFlowField();
        }
    }
    
    public void GenerateFlowField()
    {
        for(int x = 0; x < grid.gridSizeX; x++)
        {
            for(int y = 0; y < grid.gridSizeY; y++)
            {
                //unwalkable nodes dont have a flow value so ignore it
                if (!grid.grid[x, y].walkable)
                    continue;
                //int minDist = int.MaxValue;
                //Node minNode = null;
                //foreach (Node neighbour in grid.GetNeighbours(grid.grid[x,y]))
                //{
                //    int dist = neighbour.gCost - grid.grid[x, y].gCost;
                //    if(dist < minDist)
                //    {
                //        minDist = dist;
                //        minNode = neighbour;
                //    }

                //}
                //if(minNode != null)
                //{
                //    grid.grid[x, y].NodeVector = (minNode.WorldPosition - grid.grid[x, y].WorldPosition).normalized; 
                //}

                Node[] orthoNeighbours = grid.GetOrthogonalNeighbours(grid.grid[x, y]);
                int[] gCosts = new int[4];

                for (int i = 0; i < 4; i++)
                {
                    if(orthoNeighbours[i] != null)
                    {
                        gCosts[i] = orthoNeighbours[i].gCost;
                    }
                    else
                    {
                        gCosts[i] = grid.grid[x, y].gCost;
                    }
                }
                
                //left tile's distance - right tile's distance
                grid.grid[x, y].NodeVector.x = (gCosts[2] - gCosts[3]);

                //top tile's distance - left tile's distance
                grid.grid[x, y].NodeVector.y = gCosts[0] - gCosts[1];

                grid.grid[x, y].NodeVector.Normalize();
            }
        }
    }





    //for the flow field algorithm, we are looking for path distance, so when we are flood filling, we are just looking to fill 
    //in the number of path steps it takes from every node in the grid to reach the goal.
    public void DijkstraFloodFill(Vector3 target)
    {
        List<Node> activeSet = new List<Node>();
        foreach(Node node in grid.grid)
        {
            node.gCost = 0;
        }
        //get the target node
        Node targetNode = grid.NodeFromWorldPoint(target);
        //and add it to the active set
        activeSet.Add(targetNode);

        //while the active set isnt empty
        while(activeSet.Count > 0)
        {
           
            Node toCheck = activeSet[0];
            //loop through the neighbour list of the front node of the active set
            foreach(Node neighbour in grid.GetNeighbours(toCheck))
            { 
            
                //if it already has a cost higher than 0, leave it at that because we just want the lowest number of steps to the target
                if (neighbour.gCost != 0 || neighbour == targetNode || !neighbour.walkable)
                    continue;          

                neighbour.gCost = toCheck.gCost + 1;
                activeSet.Add(neighbour);
            }
            toCheck.searched = true;
            activeSet.Remove(toCheck);
        }


    }
}
