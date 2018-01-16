using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CostFieldGenerator : MonoBehaviour
{

    Grid grid;


    #region field variables
    public double[,] goalCostField;//represents the gravitational force of each grid tile in relation to the goal node
    public double goalFieldStrength;

    public double[,] staticObstacleCostField;//represents the gravitational force of each grid tile in relation to the closest obstacle node
    public int staticObstacleFieldStrength;

    private const float AgentMass = 1.5f;
    

    //in order to generate any real level of gravitational force, I have multiplied the gravitational constant by many magnitudes
    private const double GravitationalConstant = 6.67408;
    #endregion

    #region UI bools
    bool showStaticField;
    bool showGoalField;

    #endregion

    public TextMesh[,] flowFieldCostText;

    GameObject textMeshParent;

    private static CostFieldGenerator instance;

    public static CostFieldGenerator Instance
    {
        get
        {
            //if instance is null try to find something that can be the instance
            if (instance == null)
            {
                instance = FindObjectOfType<CostFieldGenerator>();
                //if not, then make a new object and add a component to it which becomes the instance
                if (instance == null)
                {
                    GameObject newGameObject = new GameObject();
                    newGameObject.name = "FlowFieldGenerator";
                    instance = newGameObject.AddComponent<CostFieldGenerator>();
                }
            }
            //then return it
            return instance;
        }
    }


    public void Awake()
    {

        grid = FindObjectOfType<Grid>();
        goalCostField = new double[grid.gridSizeX, grid.gridSizeY];
        staticObstacleCostField = new double[grid.gridSizeX, grid.gridSizeY];
        flowFieldCostText = new TextMesh[grid.gridSizeX, grid.gridSizeY];
        textMeshParent = GameObject.Find("TextMeshes");

        //generate line renderers to represent each vector
        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {


                //text meshes
                flowFieldCostText[x, y] = new GameObject().AddComponent<TextMesh>();
                flowFieldCostText[x, y].transform.SetParent(textMeshParent.transform);
                flowFieldCostText[x, y].transform.localScale = Vector3.one / 2;
                flowFieldCostText[x, y].gameObject.name = "(" + x + "," + y + ")";
                flowFieldCostText[x, y].transform.position = new Vector3(grid.grid[x, y].WorldPosition.x - (grid.nodeRadius/2), grid.grid[x, y].WorldPosition.y , grid.grid[x, y].WorldPosition.z + (grid.nodeRadius/2));
                flowFieldCostText[x, y].transform.Rotate(Vector3.right, 90);

            }
        }
        GenerateStaticObstacleField();
        setTextMeshText();
    }
    public void Update()
    {

        //when left mouse is pressed, make a ray, then generate the vector field
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit = new RaycastHit();
            if (Physics.Raycast(ray, out rayhit))
            {
                //get the clicked node
                Node clickedNode = grid.NodeFromWorldPoint(rayhit.point);
                //then check that the node is walkable, if not we dont need to call the movement functions
                if(grid.grid[clickedNode.gridX,clickedNode.gridY].walkable)
                {
                    GenerateGoalField(rayhit.point);
                    setTextMeshText();
                }
            }
        }
    }

    public void setTextMeshText()
    {
        if(!showGoalField && !showStaticField)
        {
            textMeshParent.SetActive(false);
        }
        else
        {
            textMeshParent.SetActive(true);
        }

        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {

                if (showGoalField && showStaticField)
                {
                    flowFieldCostText[x, y].text = (goalCostField[x, y] + staticObstacleCostField[x, y]).ToString("F2");
                }
                else if (showGoalField && !showStaticField)
                {
                    flowFieldCostText[x, y].text = goalCostField[x, y].ToString("F2");
                }
                else if(!showGoalField && showStaticField)
                {
                    flowFieldCostText[x, y].text =  staticObstacleCostField[x, y].ToString("F2");
                }
            }
        }
    }


    //for the flow field algorithm, we are looking for path distance, so when we are flood filling, we are just looking to fill 
    //in the number of path steps it takes from every node in the grid to reach the goal.
    public void GenerateGoalField(Vector3 target)
    {
        List<Node> activeSet = new List<Node>();
        foreach (Node node in grid.grid)
        {
            goalCostField[node.gridX, node.gridY] = 0;
            node.startNode = false;
        }
        //get the target node
        Node targetNode = grid.NodeFromWorldPoint(target);
        targetNode.startNode = true;
        goalCostField[targetNode.gridX, targetNode.gridY] = goalFieldStrength;
        //and add it to the active set
        activeSet.Add(targetNode);

        //while the active set isnt empty
        while (activeSet.Count > 0)
        {
            Node toCheck = activeSet[0];
            //loop through the neighbour list of the front node of the active set
            //get only the orthogonal neighbours
            Node[] neighbourArr = grid.GetOrthogonalNeighbours(toCheck);


            for (int i = 0; i < neighbourArr.Length; i++)
            {

                //if  the current cost -1 is 0, then we have already faded to zero and we dont need to check any other nodes            
                if (goalCostField[toCheck.gridX, toCheck.gridY] <= 0)
                {
                    return;
                }
                //if it already has a cost higher than 0, leave it at that because we just want the lowest number of steps to the target
                if (neighbourArr[i] == null || goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] != 0 || neighbourArr[i] == targetNode || !neighbourArr[i].walkable)
                    continue;

                //goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] = goalCostField[toCheck.gridX,toCheck.gridY] - 1;
                //activeSet.Add(neighbourArr[i]);


                //generate r^2 for the below equation
                float distBetweenObjects = (targetNode.WorldPosition - neighbourArr[i].WorldPosition).magnitude;
                distBetweenObjects = Mathf.Pow(distBetweenObjects, 2);

                //here I'm using the equation for gravitational force: G * ((m1 * m2)/r^2)
                double force = GravitationalConstant * ((goalFieldStrength * AgentMass) /distBetweenObjects);
                goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] = force;
                activeSet.Add(neighbourArr[i]);
            }
            toCheck.searched = true;
            activeSet.Remove(toCheck);
        }
    }

    public void GenerateStaticObstacleField()
    {
         List<Node> openSet = new List<Node>();
        List<Node> obstacleNodes = new List<Node>();

        //we need to loop through the grid and add all of the unwalkable nodes to the open set
        for(int x = 0; x < grid.gridSizeX; x++)
        {
            for(int y = 0; y  < grid.gridSizeY; y++)
            {
                //look for unwalkable nodes because we only need the unwalkable nodes to generate the field
                if (grid.grid[x, y].walkable)
                    continue;

                int numUnwalkable = 0;//tracks the number of unwalkable nodes 
                List<Node> neighbours = grid.GetNeighbours(grid.grid[x, y]);//the octagonal neighbours of the current node
                //then loop through every neighbour node of the current node
                foreach (Node neighbour in neighbours)
                {
                    //if it's unwalkable, then increase the counter
                    if(!neighbour.walkable)
                    {
                        numUnwalkable++;
                    }
                }//end of foreach

                //if the number of unwalkable neighbours is equal to the number of neighbours, then the node is inconsequential, so we can skip this node
                if(numUnwalkable == neighbours.Count)
                {
                    continue;
                }

               // staticObstacleCostField[x, y] = -staticObstacleFieldStrength;
                obstacleNodes.Add(grid.grid[x, y]);

            }//end of y for
        }//end of x for

        //now we need to generate the field using a brushfire algorithm
      
        for (int i = 0; i < obstacleNodes.Count; i++)
        {
            openSet.AddRange(grid.GetNeighbours(obstacleNodes[i]));
            while (openSet.Count > 0)
            {

                Node nodeToCheck = openSet[0];//set the checking node to the first node in the list



                //get all orthogonal neighbours of this node
                List<Node> neighbours = grid.GetNeighbours(nodeToCheck);

                for (int j = 0; j < neighbours.Count; j++)
                {
                    //check to see if the entry is null, as this is a possibility at the edge of the map or in the case of an unwalkable node
                    if (neighbours[j] == null  || !neighbours[j].walkable || staticObstacleCostField[neighbours[i].gridX, neighbours[i].gridY] != 0)
                    {
                        continue;
                    }
                    //generate r^2 for the below equation
                    float distBetweenObjects = Vector3.Distance(obstacleNodes[i].WorldPosition,neighbours[j].WorldPosition);

                    if(distBetweenObjects <= 4)//this should stop the field from acting past a 2 node radius 
                        openSet.Add(neighbours[j]);

                    distBetweenObjects = Mathf.Pow(distBetweenObjects, 2);
                    //here I'm using the equation for gravitational force: G * ((m1 * m2)/r^2)
                    double force = GravitationalConstant * ((staticObstacleFieldStrength * AgentMass) / distBetweenObjects);

                  
                     staticObstacleCostField[neighbours[j].gridX, neighbours[j].gridY] = -force;
                }
                openSet.Remove(nodeToCheck);
                nodeToCheck.searched = true;

                //List<Node> neighbours = new List<Node>();
                ////convert the array into a list (we have to loop through as there may be null members of the array)
                //for (int i = 0; i < 4; i++)
                //{
                //    if (neighbourArr[i] == null)
                //        continue;
                //    neighbours.Add(neighbourArr[i]);
                //}
                //foreach (Node neighbour in neighbours)
                //{
                //    //if  the current cost -2 is 0, then we have already faded to zero and we dont need to check any other nodes            
                //    if (staticObstacleCostField[nodeToCheck.gridX,nodeToCheck.gridY] +2  >= 0)
                //    {
                //        continue;
                //    }
                //    //if the neighbour isnt walkable, just ignore it
                //    if ( !neighbour.walkable || staticObstacleCostField[neighbour.gridX,neighbour.gridY] != 0)
                //        continue;

                //    staticObstacleCostField[neighbour.gridX, neighbour.gridY] = staticObstacleCostField[nodeToCheck.gridX,nodeToCheck.gridY] + 2;
                //    openSet.Add(neighbour);
                //}
                //nodeToCheck.searched = true;
                //openSet.Remove(nodeToCheck);


            }//end of while
        }//end of for
    }//EOF


    #region UI interfacing functions
    public void ShowStaticCostField(bool show)
    {
        showStaticField = show;
    }
    public void ShowGoalCostField(bool show)
    {
        showGoalField = show;
    }

#endregion

}