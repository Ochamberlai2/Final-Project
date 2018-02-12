using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO 06/02/18:
//move the calling of the movement function from each individual agent to this class


public class PotentialFieldSquad : MonoBehaviour {


    public List<PotentialFieldAgent> squadAgents;
    public List<Vector2> formationPointsInRelationToLeader;
    public PotentialFieldAgent squadLeader;
    public Vector2 leaderPositionInFormation;

    public Formation currentFormation;

    public float[,] formationPotentialField;

    private bool showSquadField;
    [SerializeField]
    private float formationFieldStrength;
    private Grid grid;

    public void Start()
    {

        grid = FindObjectOfType<Grid>();
        formationPotentialField = new float[grid.gridSizeX,grid.gridSizeY];
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
                    //populate the leader positional vector
                    FindLeaderPositionInFormation();
                }
            }
                //then find all other positions in the formation in relation to the leader
                FindFormationPointsInRelationToLeader();
        }
        catch
        {
            Debug.LogError("No formation selected");
        }
        StartCoroutine(SquadMovement());
    }


    private IEnumerator SquadMovement()
    {
        while (true)
        {
            CostFieldGenerator.Instance.GenerateFormationField(ref formationPotentialField, grid.NodeFromWorldPoint(squadLeader.transform.position), formationPointsInRelationToLeader, formationFieldStrength);
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

    public void FindLeaderPositionInFormation()
    {
        //the formation width or depth can only be as high as the number of agents in the formation
        int formationSize = currentFormation.NumOfAgents;

        //find the closest point to the centre of the formation on the x axis
        //find the closest point to the front of the formation on the y axis
        for (int i = 0; i < formationSize; i++)
        {
            //if the centremost, or left centre column is true and contains an agent, set this as the leader's position
            if (currentFormation.FormationLayout.rows[i].Column[(formationSize / 2) - 1] == true)
            {
                leaderPositionInFormation = new Vector2((formationSize / 2) - 1, i);
                return;
            }
            //if the formation size is greater than one (ie, size/2 will not be out of bounds),
            //then check to see if the centre rightmost column contains an agent, set this as the leaders position
            else if (formationSize > 1 && currentFormation.FormationLayout.rows[i].Column[formationSize / 2] == true)
            {
                leaderPositionInFormation = new Vector2(formationSize / 2, i);
                return;
            }
        }

        //if there is no agent in the centremost columns log an error displaying that there is no valid formation
        Debug.LogError(currentFormation.name + " is not a valid formation. No formation centre can be found");
    }

    public void FindFormationPointsInRelationToLeader()
    {
        //for the formation width
        for(int x = 0; x < currentFormation.FormationLayout.NumAgents; x++)
        {
            //for the formation depth
            for(int y = 0; y < currentFormation.FormationLayout.NumAgents;y++)
            {
                //if the formation place is false, then an agent should not stand there
                if(currentFormation.FormationLayout.rows[y].Column[x] == false || x == leaderPositionInFormation.x && y == leaderPositionInFormation.y)
                {
                    continue;
                }
                //otherwise, find the offset of the current row position from the leader's position
                Vector2 offsetFromLeader = new Vector2(x, y) - leaderPositionInFormation;

                formationPointsInRelationToLeader.Add(offsetFromLeader);
            }
        }
    }


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
        showSquadField = show;
    }

    #endregion
}
