using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PotentialFieldAgent : MonoBehaviour {

    static private bool showTruePosition = true;

    public bool leader; //whether or not the agent is the leader of the formation
   
    public Rigidbody rb;
    public Vector3 desiredVelocity;
    public MovementDirection agentMovementDirection = MovementDirection.Down;
    public float velocityMultiplier = 2f;
    public float[,] agentFieldsSummed;

    private Grid grid;
    private List<Node> otherAgentNodes = new List<Node>();
    private Node agentNode;



    public void Initialise()
    {

        rb = GetComponent<Rigidbody>();//assign the rigidbody
        grid = FindObjectOfType<Grid>();//assign the reference to the grid
        AgentManager.agents.Add(this);//add the agent to the list of agents in order to track position etc
        agentFieldsSummed = new float[grid.gridSizeX, grid.gridSizeY];
        StartCoroutine(UpdateAgentCollisionField(.05f));
    }

    public IEnumerator UpdateAgentCollisionField(float updateDelay)
    {
        while(true)
        {
            agentFieldsSummed = CostFieldGenerator.Instance.GetAgentFieldsSummed(otherAgentNodes);//the agent-agent APF
            yield return new WaitForSeconds(updateDelay);
        }
    }

    /// <summary>
    /// Finds the desired velocity and sets the agent's velocity
    /// There is no need to include agent's personal fields as this is calculated and summed seperately
    /// </summary>
    /// <param name="potentialFields">All potential fields to be summed</param>
    public void Movement(bool useAgentAvoidance, params float [][,] potentialFields)
    {
        desiredVelocity = FindNextNode(useAgentAvoidance,potentialFields);//get the desired directional vector
        rb.velocity = new Vector3(desiredVelocity.x, 0, desiredVelocity.z) * velocityMultiplier;//then apply the direction with the velocity multiplier
    }

    /// <summary>
    /// Finds the node with the highest potential and returns a directional unit vector pointing towards the highest potential node.
    /// </summary>
    /// <param name="potentialFields">The potential fields to be considered</param>
    /// <returns>A directional unit vector pointing towards the highest potential node.</returns>
    private Vector3 FindNextNode(bool useAgentAvoidance, params float[][,] potentialFields)
    {

        agentNode = grid.NodeFromWorldPoint(transform.position); //the node that the agent is currently standing on
        List<Node> neighbourList = grid.GetNeighbours(agentNode); //list of neighbours of the agent's current node


        Node bestNode = null;
        float bestCost = 0;

#region adding other agent's current nodes for agent-agent avoidance

        otherAgentNodes.Clear();
        //find all nodes of the agents which are not the current one
        for(int j = 0; j < AgentManager.agents.Count; j++)
        {
            if (AgentManager.agents[j] == this)
            {
                continue;
            }

            otherAgentNodes.Add(grid.NodeFromWorldPoint(AgentManager.agents[j].transform.position));
        }
      
#endregion


        //loop through the neighbours of the node the agent is positioned upon
        for (int i = 0; i < neighbourList.Count; i++)
        {
            //if the node is non walkable or null, ignore it
            if (!neighbourList[i].walkable || neighbourList[i] == null || !grid.ValidatePointOnGrid(neighbourList[i].gridX,neighbourList[i].gridY))
                continue;
            

            //get the value attributed to the node based upon the relevant fields
            float neighbourValue = FindNodePotential(neighbourList[i],useAgentAvoidance,potentialFields);
            //if the sum of the relevant fields is more attractive than the currently most attractive node's value
            if (neighbourValue > bestCost)
            {
                //then set the new closest node
                bestNode = neighbourList[i];
                //and set the current best cost
                bestCost = neighbourValue;
            }
          
            
        }
        //find the value of the agent's current node
        float currentNodeValue = FindNodePotential(agentNode, useAgentAvoidance, potentialFields);
        //if the best neighbour has a more desirable potential than the agent's current node -1 (to avoid the agent from being stuck in a local minima)
        if (bestNode == null || bestCost < currentNodeValue - 1)
       {
            bestNode = agentNode;
        }


        agentMovementDirection = FindAgentMovementDirection(bestNode);

        //find the directional vector to the best node
        Vector3 newVelocity = (bestNode.WorldPosition - transform.position).normalized;

       
        //return the normalized directional vector between the best node's position and the agents current node
        return newVelocity;
    }

    /// <summary>
    /// Find the potential value of a specified node.
    /// </summary>
    /// <param name="node">The node to retrieve the potential for</param>
    /// <param name="useAgentAvoidance">Whether or not to use agent-agent avoidance</param>
    /// <param name="potentialFields">The potential fields to consider</param>
    /// <returns>The potential value of the desired node</returns>
    private float FindNodePotential(Node node, bool useAgentAvoidance,params float[][,] potentialFields)
    {
        float returnFloat = 0;
        if(useAgentAvoidance)
            returnFloat += agentFieldsSummed[node.gridX,node.gridY];
        for (int i = 0; i < potentialFields.Length; i++)
        {
            returnFloat += potentialFields[i][node.gridX, node.gridY];
        }

        return returnFloat;
    }

    private MovementDirection FindAgentMovementDirection(Node bestNode)
    {
        MovementDirection returnDirection = MovementDirection.Down;

        if(bestNode.gridX == agentNode.gridX -1)
        {
            //0|0|0
            //0|0|0
            //1|0|0
            if (bestNode.gridY == agentNode.gridY - 1)
            {
                returnDirection = MovementDirection.left;
            }
            //0|0|0
            //1|0|0
            //0|0|0
            else if (bestNode.gridY == agentNode.gridY)
            {
                returnDirection = MovementDirection.left;
            }
            //1|0|0
            //0|0|0
            //0|0|0
            else if (bestNode.gridY == agentNode.gridY + 1)
            {
                returnDirection = MovementDirection.left;
            }
        }
        else if(bestNode.gridX == agentNode.gridX)
        {
            //0|0|0
            //0|0|0
            //0|1|0
            if (bestNode.gridY == agentNode.gridY - 1)
            {
                returnDirection = MovementDirection.Down;
            }
            //0|0|0
            //0|1|0
            //0|0|0
            //if the x and y are equal to the agent node, then the agent should not move, so give it whatever movement direction it last moved.
            else if (bestNode.gridY == agentNode.gridY)
            {
                returnDirection = agentMovementDirection;
            }
            //0|1|0
            //0|0|0
            //0|0|0
            else if (bestNode.gridY == agentNode.gridY + 1)
            {
                returnDirection = MovementDirection.Up;
            }
        }
        else if(bestNode.gridX == agentNode.gridX + 1)
        {
            //0|0|0
            //0|0|0
            //0|0|1
            if (bestNode.gridY == agentNode.gridY - 1)
            {
                returnDirection = MovementDirection.Right;
            }
            //0|0|0
            //0|0|1
            //0|0|0
            else if (bestNode.gridY == agentNode.gridY)
            {
                returnDirection = MovementDirection.Right;
            }
            //0|0|1
            //0|0|0
            //0|0|0
            else if (bestNode.gridY == agentNode.gridY + 1)
            {
                returnDirection = MovementDirection.Right;
            }
        }


        return returnDirection;
    }

    private void OnDrawGizmos()
    {
        if(grid == null || !showTruePosition)
            return;
        //if the agent is the leader, set the cube to blue, otherwise black
        Gizmos.color = (leader) ? Color.blue:Color.black;
        Gizmos.DrawCube(grid.NodeFromWorldPoint(transform.position).WorldPosition, Vector3.one / 0.75f);
    }

    public void ShowAgentTruePosition()
    {
        showTruePosition = !showTruePosition;
    }


}
