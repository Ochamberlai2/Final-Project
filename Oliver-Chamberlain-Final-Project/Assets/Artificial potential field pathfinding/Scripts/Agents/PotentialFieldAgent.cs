using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PotentialFieldAgent : MonoBehaviour {



    public bool leader; //whether or not the agent is the leader of the formation
    private Rigidbody rb;//the agent's rigidbody 

    [Header("Agent movement")]
    
    public Vector3 desiredVelocity;
    public MovementDirection agentMovementDirection = MovementDirection.Down;
    [HideInInspector]
    public float velocityMultiplier = 2f;//the agent's movement speed multiplier
    [Header("Agent collision avoidance")]
    public float AgentMass = 1.5f; //the mass of the agent
    public float agentAvoidanceFieldRadius = 2;//the agent's personal field strength is -infinity

    [HideInInspector]
    public Node agentNode;

    private Grid grid;

    public void Initialise(Grid _grid)
    {
        rb = GetComponent<Rigidbody>();//assign the rigidbody
        grid = _grid;
        agentNode =grid.NodeFromWorldPoint(transform.position);
    }

    /// <summary>
    /// Finds the desired velocity and sets the agent's velocity
    /// There is no need to include agent's personal fields as this is calculated and summed seperately
    /// </summary>
    /// <param name="potentialFields">All potential fields to be summed</param>
    public void Movement(bool ignoreSquad, params float [][,] potentialFields)
    {
        desiredVelocity = FindNextNode(ignoreSquad,potentialFields);//get the desired directional vector
        rb.velocity = new Vector3(desiredVelocity.x, 0, desiredVelocity.z) * velocityMultiplier;//then apply the direction with the velocity multiplier
    }

    /// <summary>
    /// Finds the node with the highest potential and returns a directional unit vector pointing towards the highest potential node.
    /// </summary>
    /// <param name="potentialFields">The potential fields to be considered</param>
    /// <returns>A directional unit vector pointing towards the highest potential node.</returns>
    private Vector3 FindNextNode(bool ignoreSquad, params float[][,] potentialFields)
    {

        agentNode = grid.NodeFromWorldPoint(transform.position); //the node that the agent is currently standing on
        List<Node> neighbourList = grid.GetNeighbours(agentNode); //list of neighbours of the agent's current node


        Node bestNode = null;
        float bestCost = 0;

        //loop through the neighbours of the node the agent is positioned upon
        for (int i = 0; i < neighbourList.Count; i++)
        {
            //if the node is non walkable or null, ignore it
            if (!neighbourList[i].walkable || neighbourList[i] == null || !grid.ValidatePointOnGrid(neighbourList[i].gridX,neighbourList[i].gridY))
                continue;
            

            //get the value attributed to the node based upon the relevant fields
            float neighbourValue = FindNodePotential(neighbourList[i],ignoreSquad,potentialFields);
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
        float currentNodeValue = FindNodePotential(agentNode, ignoreSquad, potentialFields);
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
    /// <param name="ignoreSquad">Whether or not to use agent-agent avoidance</param>
    /// <param name="potentialFields">The potential fields to consider</param>
    /// <returns>The potential value of the desired node</returns>
    private float FindNodePotential(Node node, bool ignoreSquad,params float[][,] potentialFields)
    {
        float potential = 0;

        potential += CostFieldGenerator.GetAgentCollisionPotential(ignoreSquad, node, this, SquadManager.Instance.squads);
        //loop through the supplied potential fields and add their values
        for (int i = 0; i < potentialFields.Length; i++)
        {
            potential += potentialFields[i][node.gridX, node.gridY];
        }

        return potential;
    }
    /// <summary>
    /// Finds which direction the agent is moving in based on which neighbouring node has the highest potential
    /// </summary>
    /// <param name="bestNode">the highest potential neighbouring node</param>
    /// <returns></returns>
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

}
