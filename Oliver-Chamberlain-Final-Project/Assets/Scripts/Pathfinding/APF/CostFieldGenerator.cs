using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.EventSystems;
using System.Collections;


public class CostFieldGenerator : MonoBehaviour
{

    [HideInInspector]
    public Vector3 targetPos;


    public float[,] zeroField;

    #region field variables
    public float[,] goalCostField;//represents the gravitational force of each grid tile in relation to the goal node
    [Header("Potential field strength variables")]
    public float goalFieldMass;

    public float[,] staticObstacleCostField;//represents the gravitational force of each grid tile in relation to the closest obstacle node
    public float staticFieldMass;
    [SerializeField]
    private float staticFieldInfluence;

    [SerializeField]
    public float AgentMass = 1.5f;
    

    public const float agentPersonalFieldInfluence = 2;//the agent's personal field strength is -infinity
    #endregion

    Grid grid;
    #region UI vars
    public PotentialFieldSquad squadToShow;
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
        goalCostField = new float[grid.gridSizeX, grid.gridSizeY];
        staticObstacleCostField = new float[grid.gridSizeX, grid.gridSizeY];
        zeroField = new float[grid.gridSizeX, grid.gridSizeY];
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
                flowFieldCostText[x, y].transform.position = new Vector3(grid.grid[x, y].WorldPosition.x - (grid.nodeRadius), grid.grid[x, y].WorldPosition.y , grid.grid[x, y].WorldPosition.z + (grid.nodeRadius/2));
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
                }
            }
        }
        setTextMeshText();
    }

    //sums all of the requested values and displays them 
    public void setTextMeshText()
    {
        if(!showGoalField && !showStaticField && squadToShow == null)
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
                if (!grid.grid[x, y].walkable)
                {
                    flowFieldCostText[x, y].text = "-inf";
                    continue;
                }
                float valueToShow = 0f;

                if(showGoalField)
                {
                    valueToShow += goalCostField[x, y];
                }
                if(showStaticField)
                {
                    valueToShow += staticObstacleCostField[x, y];
                }
                if(squadToShow != null)
                {
                    valueToShow += squadToShow.formationPotentialField[x, y];
                }

                flowFieldCostText[x, y].text = valueToShow.ToString("F2");    
            }
        }
    }



    public void GenerateGoalField(Vector3 target)
    {
        List<Node> activeSet = new List<Node>();
        foreach (Node node in grid.grid)
        {
            goalCostField[node.gridX, node.gridY] = 0;
            node.goalNode = false;
        }
        //get the target node
        Node targetNode = grid.NodeFromWorldPoint(target);
        ////////////////////////////////
        targetPos = targetNode.WorldPosition;
        ///////////////////////////////
        targetNode.goalNode = true;
        //set the value of the target node.
        goalCostField[targetNode.gridX, targetNode.gridY] = goalFieldMass;
 
        //and add it to the active set
        activeSet.Add(targetNode);

        //while the active set isnt empty
        while (activeSet.Count > 0)
        {
            Node toCheck = activeSet[0];
            //loop through the neighbour list of the front node of the active set
            //get only the orthogonal neighbours
            Node[] neighbourArr = grid.GetNeighbours(toCheck).ToArray();//grid.GetOrthogonalNeighbours(toCheck);


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

                //generate r^2 for the below equation
                float distBetweenObjects = Vector3.Distance(targetNode.WorldPosition, neighbourArr[i].WorldPosition);
                //then square it
                //distBetweenObjects = distBetweenObjects * distBetweenObjects;

                //here I'm using the equation for gravitational force: G * ((m1 * m2)/r^2)
                float force = /*GravitationalConstant **/ ((goalFieldMass) /distBetweenObjects);
                goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] = force;
                activeSet.Add(neighbourArr[i]);
            }
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
                obstacleNodes.Add(grid.grid[x, y]);
                //set the unwalkable node to -infinity (because it is a negatively charged field)
                staticObstacleCostField[x, y] = float.MinValue;

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
                   //check that the entry is not null and is walkable 
                    if (neighbours[j] == null  || !neighbours[j].walkable)
                    {
                        continue;
                    }
                    //generate r^2 for the below equation
                    float distBetweenObjects = Vector3.Distance(obstacleNodes[i].WorldPosition,neighbours[j].WorldPosition);
                    float force = 0;

                    if (distBetweenObjects >= staticFieldInfluence)//this should stop the field from acting past a 2 node radius 
                        continue;

                    //then square the distance
                    //distBetweenObjects = distBetweenObjects * distBetweenObjects;
                    //the mass of the field divided by the squared distance between them
                    force =staticFieldMass / distBetweenObjects;

                    //if the negative force is greater than the already stored negative force, then update it
                    if(-force < staticObstacleCostField[neighbours[j].gridX,neighbours[j].gridY])
                    {
                            staticObstacleCostField[neighbours[j].gridX, neighbours[j].gridY] = -force;
                            openSet.Add(neighbours[j]);
                    }
                }
                openSet.Remove(nodeToCheck);



            }//end of while
        }//end of for
    }//EOF

    public void GenerateFormationField(Node leaderNode ,List<Vector2> pointsInRelationToLeaderNode,float fieldStrength, System.Action<float[,]> result)
    {
        float [,]formationField = new float[grid.gridSizeX,grid.gridSizeY];
        //get the nodes to generate a field around
        Node[] formationNodes = GetFormationNodes(leaderNode, pointsInRelationToLeaderNode).ToArray();
        Queue<Node> openSet = new Queue<Node>();
        List<Node> closedSet = new List<Node>();
        //add all neighbours of these nodes to the open set to begin with
        for (int i = 0; i < formationNodes.Length; i++)
        {
            formationField[formationNodes[i].gridX, formationNodes[i].gridY] = fieldStrength;
            openSet.Enqueue(formationNodes[i]);
        }


        foreach(Node node in grid.grid)
        {
            int gridX = node.gridX;
            int gridY = node.gridY;
            //if the node isnt walkable, ignore it
            if (!node.walkable)
                continue;

            //generate r^2 for the below equation
            float distBetweenObjects = FindDistanceToClosestFormationPosition(node.WorldPosition, formationNodes);
            //distBetweenObjects = distBetweenObjects * distBetweenObjects;

            //f = mass / r^2
            float force = fieldStrength / distBetweenObjects;
            //apply the value to the field
            formationField[gridX, gridY] = force;

        }
        result(formationField);
    } 

    //this is an expensive function, can be optimised in the future by only generating potentials for points which are candidates
    public float[,] GetAgentFieldsSummed(List<Node> agentPositions)
    {   
        float[,] agentFields = new float[grid.gridSizeX, grid.gridSizeY];

        List<Node> openSet = new List<Node>();

        for(int i = 0; i  < agentPositions.Count; i++)
        {
            openSet.AddRange(grid.GetNeighbours(agentPositions[i]));
            while(openSet.Count > 0)
            {
                Node nodeToCheck = openSet[0];//the node being evaluated is always the first in the open set
                List<Node> neighbours = grid.GetNeighbours(nodeToCheck);

                //loop through the neighbours of the checking node
                for(int j = 0; j < neighbours.Count; j++)
                {
                    //if there is no neighbour or the neighbour is unwalkable, dont check it
                    if(neighbours[j] == null||!neighbours[j].walkable)
                    {
                        continue;
                    }
                    //otherwise get the distance between the current agent and the node we are checking currently
                    float distBetweenObjects = Vector3.Distance(agentPositions[i].WorldPosition, neighbours[j].WorldPosition);
                    //if this node is outside of the desired field of influence, then ignore it
                    if(distBetweenObjects > agentPersonalFieldInfluence)
                    {
                        continue;
                    }
                    //otherwise square the distance
                    //distBetweenObjects = distBetweenObjects * distBetweenObjects;
                    //then figure out the mass of the field
                    float force = AgentMass / distBetweenObjects;

                    //if the force of the tile being assigned to is already greater than the one which would be applied, do not overwrite it.
                    if(-force < agentFields[neighbours[j].gridX,neighbours[j].gridY])
                    {
                        agentFields[neighbours[j].gridX, neighbours[j].gridY] = -force;
                        openSet.Add(neighbours[j]);
                    }
                }
                openSet.Remove(nodeToCheck);
            }
        }
        return agentFields;
    }

    private List<Node> GetFormationNodes(Node leaderNode, List<Vector2> pointsInRelationToLeaderNode)
    {
        List<Node> returnList = new List<Node>();

        foreach(Vector2 offset in pointsInRelationToLeaderNode)
        {
            if(grid.ValidatePointOnGrid((int)offset.x + leaderNode.gridX,(int)offset.y + leaderNode.gridY))
            {
                Node referencedNode = grid.grid[leaderNode.gridX + (int)offset.x, leaderNode.gridY + (int)offset.y];
                returnList.Add(referencedNode);
            }
        }
        return returnList;

    }

    private float FindDistanceToClosestFormationPosition(Vector3 checkingNodePos,Node[] formationNodes)
    {

        float[] nodeDistances = new float[formationNodes.Length];
        //loop through and get the distance from the checking node to each of the formation nodes
        for(int i = 0; i < nodeDistances.Length; i++)
        {
            nodeDistances[i] = Vector3.Distance(checkingNodePos, formationNodes[i].WorldPosition);
        }
        float lowestDist = float.MaxValue;
        //then search for the lowest distance
        for(int j = 0; j < nodeDistances.Length; j++)
        {
            if(nodeDistances[j] < lowestDist)
            {
                lowestDist = nodeDistances[j];
            }
        }
        //error check, if there are no formation nodes that means there is no formation
        if (lowestDist == float.MaxValue)
            UnityEngine.Debug.LogError("closest formation node is not found");

        //if the lowest dist is 0 then return 1, because dividing by 0 yields a value of infinity
        if (lowestDist == 0)
            return 1;

        return lowestDist;
    }
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