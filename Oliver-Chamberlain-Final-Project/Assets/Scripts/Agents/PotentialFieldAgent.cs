using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PotentialFieldAgent : MonoBehaviour {

    public bool leader; //whether or not the agent is the leader of the formation
   
    public Rigidbody rb;
    public Vector3 desiredVelocity;
    public float velocityMultiplier = 2f;

    private Grid grid;

    private List<Node> otherAgentNodes = new List<Node>();

    public void Initialise()
    {

        rb = GetComponent<Rigidbody>();//assign the rigidbody
        grid = FindObjectOfType<Grid>();//assign the reference to the grid
        AgentManager.agents.Add(this);//add the agent to the list of agents in order to track position etc
      
    }
    /// <summary>
    /// Finds the desired velocity and sets the agent's velocity
    /// There is no need to include agent's personal fields as this is calculated and summed seperately
    /// </summary>
    /// <param name="potentialFields">All potential fields to be summed</param>
    public void Movement(params float [][,] potentialFields)
    {
        desiredVelocity = FindNextNode(potentialFields);//get the desired directional vector
        rb.velocity = new Vector3(desiredVelocity.x, 0, desiredVelocity.z) * velocityMultiplier;//then apply the direction with the velocity multiplier
    }

    //returns a normalised vector pointing to the cheapest node
    private Vector3 FindNextNode(params float[][,] potentialFields)
    {
        Node agentNode = grid.NodeFromWorldPoint(transform.position);
        List<Node> neighbourList = grid.GetNeighbours(agentNode);
        neighbourList.Add(agentNode);
        float[,] agentFieldsSummed = CostFieldGenerator.Instance.GetAgentFieldsSummed(otherAgentNodes);
        Node bestNode = null;
        float bestCost = 0;

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
        //loop through the neighbours of the node the agent is positioned upon
        for (int i = 0; i < neighbourList.Count; i++)
        {
            //if the node is non walkable or null, ignore it
            if (!neighbourList[i].walkable || neighbourList[i] == null)
                continue;

            //get the value attributed to the node based upon the relevant fields
            float neighbourValue =agentFieldsSummed[neighbourList[i].gridX, neighbourList[i].gridY];
            for (int j = 0; j < potentialFields.Length; j++)
            {
                neighbourValue += potentialFields[j][neighbourList[i].gridX, neighbourList[i].gridY];
            }


            //if the node being evaluated is the one most recently visited, reduce it's value
         
            //if the sum of the relevant fields is more attractive than the currently most attractive node's value
            if (neighbourValue> bestCost)
            {
                //then set the new closest node
                bestNode = neighbourList[i];
                //and set the current best cost
                bestCost = neighbourValue;
            }
          
            
        }

        if (bestNode != null)
        {

            Vector3 newVelocity = (bestNode.WorldPosition - transform.position).normalized;

         
            //return the normalized directional vector between the best node's position and the agents current node
            return newVelocity;
        }
        //if a best node cannot be returned, return no movement
        return Vector3.zero;
    }


    private void OnDrawGizmos()
    {
        if(grid == null)
            return;
        //if the agent is the leader, set the cube to blue, otherwise black
        Gizmos.color = (leader) ? Color.blue:Color.black;
        Gizmos.DrawCube(grid.NodeFromWorldPoint(transform.position).WorldPosition, Vector3.one / 0.75f);
    }
}
