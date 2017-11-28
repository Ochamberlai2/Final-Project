using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class FlowFieldGenerator : MonoBehaviour {

    Grid grid;
    public LineRenderer[,] flowField;
    public int goalAttractionValueStart;
    public void Start()
    {
        
        grid = FindObjectOfType<Grid>();
        flowField = new LineRenderer[grid.gridSizeX,grid.gridSizeY];
        GameObject lineRendererParent = GameObject.Find("Vector renderers");
        //generate line renderers to represent each vector
        for(int x = 0; x < grid.gridSizeX; x++)
        {
            for(int y = 0; y < grid.gridSizeY; y++)
            {
                flowField[x, y] = new GameObject().AddComponent<LineRenderer>();
                flowField[x, y].gameObject.name = "(" + x + "," + y + ")";
                flowField[x, y].transform.SetParent(lineRendererParent.transform);
                flowField[x, y].material = Resources.Load<Material>("FlowFieldArrow");
                flowField[x, y].startWidth = .2f;
                flowField[x, y].endWidth = .2f;
            }
        }
    }
    public void Update()
    {
    
        //when left mouse is pressed, make a ray, then generate the vector field
         if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit = new RaycastHit();
            if (Physics.Raycast(ray, out rayhit))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                BrushfireAlgorithm(rayhit.point);
                GenerateFlowField();
                sw.Stop();
                UnityEngine.Debug.Log("Elapsed time: " + sw.ElapsedMilliseconds + " ms");
            }
        }
    }
    public void FixedUpdate()
    {
        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {
                flowField[x, y].useWorldSpace = true;
                flowField[x, y].positionCount = 2;
                Vector3[] positions = new Vector3[2];

                positions[0] = grid.grid[x, y].WorldPosition + new Vector3(0,.1f,0);
                positions[1] =grid.grid[x, y].WorldPosition + new Vector3((grid.grid[x, y] as PFNode).NodeVector.x,.1f , ((grid.grid[x, y]as PFNode).NodeVector.y));

                flowField[x, y].SetPositions(positions);
            }
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


                ////get the orthogonal neighbours
                //Node[] orthoNeighbours = grid.GetOrthogonalNeighbours(grid.grid[x, y]);
                //int[] gCosts = new int[4];

                ////loop through four times (for each of the orthogonal neighbours
                //for (int i = 0; i < 4; i++)
                //{
                //    //if the neighbour is null, that means that they are either on the edge of the grid or their neighbour is unwalkable
                //    if (orthoNeighbours[i] != null)
                //    {
                //        gCosts[i] = orthoNeighbours[i].gCost;
                //    }
                //    //if it is null, then we simply use the gCost of the current node 
                //    else
                //    {
                //        gCosts[i] =int.MaxValue /*grid.grid[x, y].gCost*/;
                //    }
                //}

                ////left tile's distance - right tile's distance
                //(grid.grid[x, y] as PFNode).NodeVector.x = (gCosts[2] - gCosts[3]);

                ////top tile's distance - bottom tile's distance
                //(grid.grid[x, y] as PFNode).NodeVector.y = gCosts[1] - gCosts[0];

                //(grid.grid[x, y] as PFNode).NodeVector.Normalize();

                Node closestNode = null;
                int lowestGcost  = int.MaxValue;
                foreach (Node node in grid.GetNeighbours(grid.NodeFromWorldPoint(grid.grid[x, y].WorldPosition)))
                {
                    if (!node.walkable)
                        continue;
                    if(node.gCost < lowestGcost)
                    {
                        lowestGcost = node.gCost;
                        closestNode = node;
                    }
                }
                if(closestNode != null)
                {
                    (grid.grid[x, y] as PFNode).NodeVector.x = closestNode.gridX - grid.grid[x, y].gridX;
                    (grid.grid[x, y] as PFNode).NodeVector.y = closestNode.gridY - grid.grid[x, y].gridY;
                    (grid.grid[x, y] as PFNode).NodeVector.Normalize();
                }
            }
        }
    }





    //for the flow field algorithm, we are looking for path distance, so when we are flood filling, we are just looking to fill 
    //in the number of path steps it takes from every node in the grid to reach the goal.
    public void BrushfireAlgorithm(Vector3 target)
    {
        List<Node> activeSet = new List<Node>();
        foreach(Node node in grid.grid)
        {
            node.gCost = 0;
            node.startNode = false;
        }
        //get the target node
        Node targetNode = grid.NodeFromWorldPoint(target);
        targetNode.startNode = true;
        //and add it to the active set
        activeSet.Add(targetNode);

        //while the active set isnt empty
        while(activeSet.Count > 0)
        {
           
            Node toCheck = activeSet[0];
            //loop through the neighbour list of the front node of the active set
            //get only the orthogonal neighbours
            Node[] neighbourArr = grid.GetOrthogonalNeighbours(toCheck);
            
            List<Node> neighbours = new List<Node>();
            //convert the array into a list (we have to loop through as there may be null members of the array)
            for(int i = 0; i < 4; i++)
            {
                if (neighbourArr[i] == null)
                    continue;
                neighbours.Add(neighbourArr[i]);
            }

            foreach(Node neighbour in neighbours)
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
