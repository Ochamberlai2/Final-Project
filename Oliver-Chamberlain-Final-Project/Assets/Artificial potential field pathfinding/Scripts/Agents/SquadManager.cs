using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SquadManager : MonoBehaviour {

    #region singleton

    private static SquadManager instance;

    public static SquadManager Instance
    {
        get
        {
            //if instance is null try to find something that can be the instance
            if (instance == null)
            {
                instance = FindObjectOfType<SquadManager>();
                //if not, then make a new object and add a component to it which becomes the instance
                if (instance == null)
                {
                    GameObject newGameObject = new GameObject();
                    newGameObject.name = "FlowFieldGenerator";
                    instance = newGameObject.AddComponent<SquadManager>();
                }
            }
            //then return it
            return instance;
        }
    }
    #endregion

    public List<PotentialFieldAgent> agents;//all agents in the world

    public Dictionary<int,PotentialFieldSquad> squads;//all squads in the world

    private PotentialFieldSquad squadToOrder;//the squad that will be ordered to move / to have their squad values shown in the debug menu

    #region UI elements
    [Header("UI element setup")]
    public GameObject squadSelectTickboxPrefab;
    private GameObject squadOrderTickboxParent;//parent object of all toggles, to know where in the ui to place them
    private List<Toggle> squadTickboxes = new List<Toggle>();//a list of all toggles to be able to turn them off when needed

    public TextMesh[,] flowFieldCostText;//text meshes for debugging
    //Potential field debug show variables
    private PotentialFieldAgent agentToShow;
    private bool showStaticField;
    private bool showGoalField;
    private bool showFormationField;
    private bool showGoalNode;
    private bool showTrueAgentPosition;

    GameObject textMeshParent;

    #endregion
    [Header("Static obstacle field config")]
    //represents the gravitational force of each grid tile in relation to the closest obstacle node.
    public float[,] staticObstacleCostField;

    public float staticFieldMass;
    [SerializeField]
    private float staticFieldInfluence;

    public Grid grid;

    public void Awake()
    {
        agents = new List<PotentialFieldAgent>();
      

        squadOrderTickboxParent = GameObject.Find("Squads to order");

        grid = FindObjectOfType<Grid>();

        staticObstacleCostField = new float[grid.gridSizeX, grid.gridSizeY];

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
                flowFieldCostText[x, y].transform.position = new Vector3(grid.grid[x, y].WorldPosition.x - (grid.nodeRadius), grid.grid[x, y].WorldPosition.y, grid.grid[x, y].WorldPosition.z + (grid.nodeRadius / 2));
                flowFieldCostText[x, y].transform.Rotate(Vector3.right, 90);
            }
        }
        CostFieldGenerator.GenerateStaticObstacleField(grid,ref staticObstacleCostField,staticFieldInfluence,staticFieldMass);
        setTextMeshText();
    }

    public void Update()
    {
        //when left mouse is pressed, make a ray, then generate the vector field
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            //if left shift modifier is pressed, set the agent to show 
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayhit = new RaycastHit();
                if (Physics.Raycast(ray, out rayhit))
                {
                    if (rayhit.collider.tag == "Agent")
                    {
                        agentToShow = rayhit.collider.GetComponent<PotentialFieldAgent>();
                    }
                }
                else
                {
                    agentToShow = null;
                }
            }
            //send a move order to the currently selected squad
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayhit = new RaycastHit();
                if (Physics.Raycast(ray, out rayhit) && squadToOrder != null)
                {
                    //get the clicked node
                    Node clickedNode = grid.NodeFromWorldPoint(rayhit.point);
                    //then check that the node is walkable, if not we dont need to call the movement functions
                    if (grid.grid[clickedNode.gridX, clickedNode.gridY].walkable)
                    {
                        CostFieldGenerator.GenerateGoalField(grid, rayhit.point,ref instance.squadToOrder.goalCostField,instance.squadToOrder.goalFieldMass,ref instance.squadToOrder.goalNode);
                    }
                }
            }
        }
    //update the debug text
     setTextMeshText();

    }
    /// <summary>
    ///  sums all of the requested values and displays them as textmeshes
    /// </summary>
    public void setTextMeshText()
    {
        //if there is no squad to order or all of the bools are null, turn the object off
        if (squadToOrder == null || !showGoalField && !showStaticField && !showFormationField && agentToShow == null)
        {
            textMeshParent.SetActive(false);
            return;
        }
        //otherwise ensure that the object is turned on
        else
        {
            textMeshParent.SetActive(true);
        }

        //loop through all nodes in the grid and add the requested values to the value to show.
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

                if (showGoalField)
                {
                    valueToShow += squadToOrder.goalCostField[x, y];
                }
                if (showStaticField)
                {
                    valueToShow += staticObstacleCostField[x, y];
                }
                if (squadToOrder != null && showFormationField)
                {
                    valueToShow += squadToOrder.formationPotentialField[x, y];
                }
                if (agentToShow != null)
                {
                    valueToShow += agentToShow.agentFieldsSummed[x, y];
                }

                flowFieldCostText[x, y].text = valueToShow.ToString("F2");
            }
        }
    }

    /// <summary>
    /// Register a new squad to be added to the game world and create a UI element to allow control of the squad
    /// </summary>
    /// <param name="squadToAdd">The squad to be added to the game world</param>
    public void RegisterNewSquad(PotentialFieldSquad squadToAdd)
    {
        //initialise the dictionary if needed
        if (instance.squads == null)
            instance.squads = new Dictionary<int, PotentialFieldSquad>();

        //add the squad to the dictionary
        instance.squads.Add(instance.squads.Keys.Count + 1, squadToAdd);
        //Create the UI element to select this squad and initialise it
        GameObject tickbox = Instantiate(squadSelectTickboxPrefab, squadOrderTickboxParent.transform) as GameObject;
        tickbox.name = instance.squads.Keys.Count.ToString();
        tickbox.GetComponentInChildren<Text>().text = "Squad " + tickbox.name;
        squadTickboxes.Add(tickbox.GetComponent<Toggle>());
        tickbox.GetComponent<Toggle>().group = squadOrderTickboxParent.GetComponent<ToggleGroup>();



    }

    /// <summary>
    /// Draws debug gizmos regarding agent positions and squad goal.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (grid != null)
        {
            if (instance.showTrueAgentPosition)
            {
                foreach (int key in instance.squads.Keys)
                {
                    foreach (PotentialFieldAgent agent in instance.squads[key].squadAgents)
                    {
                        //draw the agent's true grid position onto the grid
                        Gizmos.color = (agent.leader) ? instance.squads[key].leaderColour : instance.squads[key].standardAgentColour;
                        Gizmos.DrawCube(agent.agentNode.WorldPosition, Vector3.one * (grid.nodeRadius * 1.5f));
                    }
                }
            }
            if (instance.showGoalNode && instance.squadToOrder != null && instance.squadToOrder.goalNode != null)
            {
                Gizmos.color = instance.squadToOrder.goalColour;
                Gizmos.DrawSphere(instance.squadToOrder.goalNode.WorldPosition, 1f);
            }

        }
    }

    #region UI interfacing functions
    public void ShowStaticCostField(bool show)
    {
        instance.showStaticField = show;
    }
    public void ShowGoalCostField(bool show)
    {
        instance.showGoalField = show;
    }
    public void ShowFormationField(bool show)
    {
        instance.showFormationField = show;
    }
    public void ShowGoalNode(bool show)
    {
        instance.showGoalNode = show;
    }
    public void ShowTrueAgentPositions(bool show)
    {
        instance.showTrueAgentPosition = show;
    }
    public void ChangeFocusedSquad(bool isOn)
    {
          if(isOn)
            {
                //set the ordering squad to the correct one
                int squadNum = int.Parse(EventSystem.current.currentSelectedGameObject.name);
                instance.squadToOrder = instance.squads[squadNum];
            }
    }
    #endregion
}