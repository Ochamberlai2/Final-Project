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

public class PotentialFieldSquad : MonoBehaviour {

    

    public List<PotentialFieldAgent> squadAgents;

    public PotentialFieldAgent squadLeader;
   // public Vector2 leaderPositionInFormation;

    public float formationFieldUpdateInterval = 0.25f;



    /*
     * formation info
     */
    public Formation currentFormation;

    [SerializeField]
    private MovementDirection moveDir;


    public float[,] formationPotentialField;

    private Dictionary<MovementDirection, List<Vector2>> formationDirections;


    [SerializeField]
    private float formationFieldStrength;
    private Grid grid;



    public void Start()
    {

        grid = FindObjectOfType<Grid>();
        formationPotentialField = new float[grid.gridSizeX,grid.gridSizeY];
        formationDirections = new Dictionary<MovementDirection, List<Vector2>>();

        try
        {
            foreach (PotentialFieldAgent agent in squadAgents)
            {
                //call the initialisation function for all of the agents(replaces start)
                agent.Initialise();

                //if the agent is the leader
                if (agent.leader)
                {
                    //set the squad leader
                    squadLeader = agent;
             
                 
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

    //calls the movement function for each agent in the squad, every fixed update cycle
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
                    squadAgents[i].Movement(CostFieldGenerator.Instance.staticObstacleCostField, CostFieldGenerator.Instance.goalCostField);
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    // follower code
                    squadAgents[i].Movement(CostFieldGenerator.Instance.staticObstacleCostField, formationPotentialField);
                    yield return new WaitForFixedUpdate();
                }
            }
            
        }
    }
    //updates the formation field every x seconds
    private IEnumerator UpdateSquadField(float interval)
    {
        while(true)
        {
           CostFieldGenerator.Instance.GenerateFormationField(grid.NodeFromWorldPoint(squadLeader.transform.position), formationDirections[moveDir], formationFieldStrength, SetSquadField);
            yield return new WaitForSeconds(interval);
        }
        
    }

    //callback function from CostFieldGenerator.GenerateFormationField
    private void SetSquadField(float[,] potentialField)
    {
        formationPotentialField = potentialField;
    }

    #region formation matrix functions

    public Vector2 FindLeaderPositionInFormation(MovementDirection movementDirection, bool[,] formationMatrix)
    {
        //the formation width or depth can only be as high as the number of agents in the formation
        int formationSize = currentFormation.NumOfAgents;
        Vector2 leaderPositionInFormation = Vector2.zero;
      






        switch (movementDirection)
        {
            case MovementDirection.Down:

                for (int i = 0; i < formationSize; i++)
                {
                    //if the centremost, or left centre column is true and contains an agent, set this as the leader's position
                    if (formationMatrix[(formationSize -1) / 2, i] == true)
                    {
                        leaderPositionInFormation = new Vector2((formationSize -1)/ 2, i);

                    }
                    //if the formation size is greater than one,
                    //then check to see if the centre rightmost column contains an agent, set this as the leaders position
                    else if (formationSize > 1 && formationMatrix[formationSize / 2, i] == true)
                    {
                        leaderPositionInFormation = new Vector2(formationSize / 2, i);
                    }
                }
                break;

            case MovementDirection.left:
                for (int i = 0; i < formationSize; i++)
                {
                    //if the centremost, or left centre column is true and contains an agent, set this as the leader's position
                    if (formationMatrix[ i, (formationSize -1)/ 2] == true)
                    {
                        leaderPositionInFormation = new Vector2(i,(formationSize -1)/ 2);

                    }
                    //if the formation size is greater than one,
                    //then check to see if the centre rightmost column contains an agent, set this as the leaders position
                    else if (formationSize > 1 && formationMatrix[i,formationSize / 2] == true)
                    {
                        leaderPositionInFormation = new Vector2(i,formationSize / 2);
                    }
                }
                break;
            case MovementDirection.Up:

                for (int i = formationSize-1; i >= 0; i--)
                {
                    //if the centremost, or left centre column is true and contains an agent, set this as the leader's position
                    if (formationMatrix[(formationSize -1)/ 2 , i] == true)
                    {
                        leaderPositionInFormation = new Vector2((formationSize - 1)/ 2, i);

                    }
                    //if the formation size is greater than one,
                    //then check to see if the centre rightmost column contains an agent, set this as the leaders position
                    else if (formationSize > 1 && formationMatrix[formationSize / 2, i] == true)
                    {
                        leaderPositionInFormation = new Vector2(formationSize / 2, i);
                    }
                }
                break;
            case MovementDirection.Right:
                for (int i = formationSize - 1; i >= 0; i--)
                {
                    //if the centremost, or left centre column is true and contains an agent, set this as the leader's position
                    if (formationMatrix[i, (formationSize - 1) / 2 ] == true)
                    {
                        leaderPositionInFormation = new Vector2(i, (formationSize - 1)/ 2);

                    }
                    //if the formation size is greater than one,
                    //then check to see if the centre rightmost column contains an agent, set this as the leaders position
                    else if (formationSize > 1 && formationMatrix[i, formationSize / 2] == true)
                    {
                        leaderPositionInFormation = new Vector2(i, formationSize / 2);
                    }
                }
                break;
            default:
                break;
        }

        
        return leaderPositionInFormation;
    }

    //initialises formation 
    private void GetFormationRotations()
    {
        //find the current formation's matrix
        bool[,] formationMatrix = GetFormationMatrix(currentFormation);
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


    public List<Vector2> FindFormationPointsInRelationToLeader(bool[,] formationMatrix, MovementDirection movementDirection)
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
                if (formationMatrix[x, y] == false || x == leaderPosition.x && y == leaderPosition.y)
                    continue;

                //otherwise, find the offset of the current row position from the leader's position
                Vector2 offsetFromLeader = new Vector2(x, y) - leaderPosition;

                formationPointsInRelationToLeader.Add(offsetFromLeader);
            }
        }
        return formationPointsInRelationToLeader;
    }


    //rotates the formation matrix 90 degrees
    public void RotateMatrix90(ref bool[,] matrix, int matrixSize)
    {
        bool[,] newMat = new bool[matrixSize, matrixSize];

        for(int i = 0; i < matrixSize; i++)
        {
            for(int j = 0; j < matrixSize; j++)
            {
                newMat[i, j] = matrix[matrixSize - j - 1, i];
            }
        }
        matrix = newMat;
    }

    //gets the formation matrix from the argument
    public bool[,] GetFormationMatrix(Formation formation)
    {
        bool[,] currentFormationMatrix = new bool[formation.NumOfAgents, formation.NumOfAgents];
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

    #region UI interfacing 
    public void ShowSquadField(bool show)
    {
        if(show == true)
        {
            CostFieldGenerator.Instance.squadToShow = this;
        }
        else
        {
            CostFieldGenerator.Instance.squadToShow = null;
        }

    }

    #endregion
}
