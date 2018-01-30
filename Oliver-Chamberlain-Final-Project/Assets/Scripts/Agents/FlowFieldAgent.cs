using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldAgent : MonoBehaviour {



    [SerializeField]
    private float velocityMultiplier;

    private Grid grid;
    private Rigidbody rb;

    private Vector3 desiredVelocity;

    [SerializeField]
    [Range(-10,0)]
    private int recentlyVisitedValueReduction = -1;

    [SerializeField]
    private int recentlyVisitedListSize = 4;
    private List<Node> recentlyVisitedNodes = new List<Node>();

    private List<Node> otherAgentNodes = new List<Node>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();//assign the rigidbody
        grid = FindObjectOfType<Grid>();//assign the reference to the grid
        AgentManager.agents.Add(this);//add the agent to the list of agents in order to track position etc
        StartCoroutine(Movement());//then start the movement coroutine.
    }

    private IEnumerator Movement()
    {
        while (true)
        {
            desiredVelocity = FindNextNode();//get the desired directional vector
            rb.velocity = new Vector3(desiredVelocity.x, 0, desiredVelocity.z) * velocityMultiplier;//then apply the direction with the velocity multiplier
            yield return new WaitForFixedUpdate();
        }
    }
    //returns a normalised vector pointing to the cheapest node
    public Vector3 FindNextNode()
    {
        Node agentNode = grid.NodeFromWorldPoint(transform.position);
        Node[] neighbourList = grid.GetNeighbours(agentNode).ToArray();
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
        for (int i = 0; i < neighbourList.Length; i++)
        {
            //if the node is non walkable or null, ignore it
            if (!neighbourList[i].walkable || neighbourList[i] == null)
                continue;

            //get the value attributed to the node based upon the relevant fields
            float neighbourValue = CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + 
                CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY] + 
                agentFieldsSummed[neighbourList[i].gridX, neighbourList[i].gridY];

            //if the node already houses another agent, under no circumstances does the agent want to move onto it as this would cause a collision
            if(otherAgentNodes.Contains(grid.grid[neighbourList[i].gridX,neighbourList[i].gridY]))
            {
                neighbourValue += float.MinValue;
            }
   
            //if the node being evaluated is the one most recently visited, reduce it's value
            if(recentlyVisitedNodes.Contains(neighbourList[i]))
            {
                neighbourValue += recentlyVisitedValueReduction;
            }
            //if the sum of the relevant fields is more attractive than the currently most attractive node's value
            if (neighbourValue> bestCost)
            {
                //then set the new closest node
                bestNode = neighbourList[i];
                //and set the current best cost
                bestCost = neighbourValue;
            }
          
            else if(neighbourValue == bestCost && bestNode != null)
            {
               //attempt to escape local minima
            }
            
        }

        if (bestNode != null)
        {

            Vector3 newVelocity = (bestNode.WorldPosition - transform.position).normalized;

            //TODO add the current node onto this also
            /*
            if (!recentlyVisitedNodes.Contains(bestNode))
            {
                //recently visited nodes is a rolling list, so if it is currently smaller than the desired size
                //then the current next node can be added to it
                if (recentlyVisitedNodes.Count < recentlyVisitedListSize)
                {
                    recentlyVisitedNodes.Add(bestNode);
                }
                // if the list is equal to the desired size, then the oldest node must be taken off of the list, in this case the front node.
                else
                {
                    recentlyVisitedNodes.RemoveAt(0);
                    recentlyVisitedNodes.Add(bestNode);
                }
            }
            */
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
        Gizmos.color = Color.black;
        Gizmos.DrawCube(grid.NodeFromWorldPoint(transform.position).WorldPosition, Vector3.one / 0.75f);
    }
}
