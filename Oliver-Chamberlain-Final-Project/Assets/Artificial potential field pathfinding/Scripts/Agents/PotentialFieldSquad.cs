using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MovementDirection
{
    Down,
    left,
    Up,
    Right,
}
/*
 * Mathf.RoundToInt(root3(vector3.distance for the two positions) * sin(angle between leader and formation position + 45))
 * Mathf.RoundToInt(root3(vector3.distance for the two positions) * cos(angle between agent and formation position + 45))
 * 
 * then use the initial positions and the 45 degree rotation to work out the others
 */

public class PotentialFieldSquad : MonoBehaviour {

    //debug gizmo colours
    [Header("Squad Gizmo Colours")]
    public Color goalColour = Color.green;
    public Color leaderColour = Color.blue;
    public Color standardAgentColour = Color.black;

    [HideInInspector]
    public Node goalNode;//saved node to show for debugging

    public float[,] goalCostField;//represents the gravitational force of each grid tile in relation to the goal node
    public float[,] formationPotentialField;
    [Header("Potential field values")]
    [SerializeField]
    private float agentMovementSpeed = 2f;

    public float goalFieldMass;

    [SerializeField]
    [Range(0,300)]
    private float formationFieldStrength;//strength of the formation field
    [SerializeField]
    [Range(0,300)]
    private float formationFieldInfluenceRange = 30f;
    public float formationFieldUpdateInterval = 0.25f; //the number of times a second that the formation field updates


    [Header("Formation information")]
    public List<PotentialFieldAgent> squadAgents;
    public PotentialFieldAgent squadLeader;
    public Formation currentFormation;



    private Dictionary<MovementDirection, List<Vector2>> formationDirections;//points in the formation in relation to the leader's position, for all four orthogonal  directions


    private Grid grid;



    public void Initialise(Grid _grid)
    {

        grid = _grid;
        goalCostField = new float[grid.gridSizeX, grid.gridSizeY];
        formationPotentialField = new float[grid.gridSizeX,grid.gridSizeY];
        formationDirections = new Dictionary<MovementDirection, List<Vector2>>();

        SquadManager.Instance.RegisterNewSquad(this);

        try
        {
            if (squadAgents.Count == 0)
                Debug.LogError("Squad is empty and invalid");
            foreach (PotentialFieldAgent agent in squadAgents)
            {
                
                //call the initialisation function for all of the agents(replaces start)
                agent.Initialise(grid);

                //if the agent is the leader
                if (agent.leader)
                {
                    //set the squad leader
                    squadLeader = agent;
                    agent.velocityMultiplier = agentMovementSpeed;
                }
                else
                {
                    //make a follower's movement speed double that of the leader to keep them in formation more effectively 
                    agent.velocityMultiplier = agentMovementSpeed * 1.5f;
                }
            }
            //then find all other positions in the formation in relation to the leader
            GetFormationRotations();
        }
        catch
        {
            Debug.LogError("No formation selected");
        }
        //start movement coroutines
        StartCoroutine(UpdateSquadField(formationFieldUpdateInterval));
        StartCoroutine(SquadMovement());
    }



    /// <summary>
    ///   calls the movement function for each agent in the squad, every fixed update
    /// </summary>
    private IEnumerator SquadMovement()
    {
        while (true)
        {

            //loop through all squad and call their movement functions
            for(int i = 0; i < squadAgents.Count; i++)
            {

                if (squadAgents[i] == squadLeader)
                {
                    //leader code
                    squadAgents[i].Movement(true,SquadManager.Instance.staticObstacleCostField, goalCostField);
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    // follower code
                    squadAgents[i].Movement(false,  SquadManager.Instance.staticObstacleCostField, formationPotentialField);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }
    /// <summary>
    ///    updates the formation field every x seconds
    /// </summary>
    private IEnumerator UpdateSquadField(float interval)
    {
        while(true)
        {
            CostFieldGenerator.GenerateFormationField(grid, grid.NodeFromWorldPoint(squadLeader.transform.position), formationDirections[squadLeader.agentMovementDirection], formationFieldStrength,formationFieldInfluenceRange, ref formationPotentialField);
            yield return new WaitForSeconds(interval);
        }
        
    }



    #region formation matrix functions

    /// <summary>
    /// Finds the leader's position in the current formation.
    /// </summary>
    /// <param name="movementDirection">the direction that the current formation matrix faces</param>
    /// <param name="formationMatrix">the current formation matrix</param>
    /// <returns></returns>
    public Vector2 FindLeaderPositionInFormation(MovementDirection movementDirection, int[,] formationMatrix)
    {

        //loop through the formation matrix
        for(int x = 0; x < formationMatrix.GetLength(0); x++)
        {
            for(int y = 0; y < formationMatrix.GetLength(1);y++)
            {
                //if the entry is 2, then its designated as the leader's spot
                if(formationMatrix[x,y] == 2)
                {
                    return new Vector2(x, y); ;
                }
            }
        }
        return Vector2.zero;
    }

    /// <summary>
    ///Initialises the formation and finds all permutations of the formation rotation
    /// </summary>
    private void GetFormationRotations()
    {
        //find the current formation's matrix
        int[,] formationMatrix = GetFormationMatrix(currentFormation);
        formationDirections.Clear();
        //add the down direction
        formationDirections.Add(MovementDirection.Down, FindFormationPointsInRelationToLeader(formationMatrix, MovementDirection.Down));
        //then add the other 3 directions
        for(int i = 1; i < 4; i++)
        {
            RotateMatrix90(ref formationMatrix, currentFormation.NumOfAgents);
            formationDirections.Add((MovementDirection)i, FindFormationPointsInRelationToLeader(formationMatrix,(MovementDirection)i));
        }
    }

    /// <summary>
    /// Find formation positions in relation to the leader's current position
    /// </summary>
    /// <param name="formationMatrix">the current formation matrix</param>
    /// <param name="movementDirection">the direction that the formation matrix is facing</param>
    /// <returns></returns>
    public List<Vector2> FindFormationPointsInRelationToLeader(int[,] formationMatrix, MovementDirection movementDirection)
    {
        List<Vector2> formationPointsInRelationToLeader = new List<Vector2>();
        //get the leader's position in the formation
        Vector2 leaderPosition = FindLeaderPositionInFormation(movementDirection,formationMatrix);

        //for the formation width
        for(int x = 0; x < currentFormation.FormationLayout.NumAgents; x++)
        {
            //for the formation depth
            for(int y = 0; y < currentFormation.FormationLayout.NumAgents;y++)
            {
                //if the formation place is false, then an agent should not stand there
                if (formationMatrix[x, y] == 0 || x == leaderPosition.x && y == leaderPosition.y)
                    continue;

                //otherwise, find the offset of the current row position from the leader's position
                Vector2 offsetFromLeader = new Vector2(x, y) - leaderPosition;

                formationPointsInRelationToLeader.Add(offsetFromLeader);
            }
        }
        return formationPointsInRelationToLeader;
    }


    /// <summary>
    ///  rotates the formation matrix 90 degrees
    /// </summary>
    public void RotateMatrix90(ref int[,] matrix, int matrixSize)
    {
        int[,] newMat = new int[matrixSize, matrixSize];

        for(int i = 0; i < matrixSize; i++)
        {
            for(int j = 0; j < matrixSize; j++)
            {
                newMat[i, j] = matrix[matrixSize - j - 1, i];
            }
        }
        matrix = newMat;
    }

    /// <summary>
    ///gets the formation matrix from the argument
    /// </summary>
    public int[,] GetFormationMatrix(Formation formation)
    {
        int[,] currentFormationMatrix = new int[formation.NumOfAgents, formation.NumOfAgents];
        for(int x = 0; x < formation.NumOfAgents; x++)
        {
            for(int y = 0; y < formation.NumOfAgents; y++)
            {
                currentFormationMatrix[x, y] = formation.FormationLayout.rows[y].Column[x];
            }
        }
        return currentFormationMatrix;
    }
    #endregion
}
