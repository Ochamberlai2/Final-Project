using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldAgent : MonoBehaviour {

    [SerializeField]
    private float velocityMultiplier;

    private Grid grid;
    private Rigidbody rb;

    private Vector3 desiredVelocity;

    private Node recentlyVisitedNode;


   // public double bestcost;
    public double currentNodeCost;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<Grid>();
        StartCoroutine(Movement());
    }

    private IEnumerator Movement()
    {
        while (true)
        {
            desiredVelocity = FindNextNode() * velocityMultiplier;

            rb.velocity = new Vector3(desiredVelocity.x, 0, desiredVelocity.z);


            yield return new WaitForFixedUpdate();
        }
    }
    //returns a normalised vector pointing to the cheapest node
    public Vector3 FindNextNode()
    {
        Node agentNode = grid.NodeFromWorldPoint(transform.position);
        Node[] neighbourList = grid.GetNeighbours(agentNode).ToArray();
        Node bestNode = null;
        float bestCost = 0;
        //loop through the neighbours of the node the agent is positioned upon
        for (int i = 0; i < neighbourList.Length; i++)
        {
            //if the node is non walkable or null, ignore it
            if (!neighbourList[i].walkable || neighbourList[i] == null)
                continue;

            //get the value attributed to the node based upon the relevant fields
            float neighbourValue = CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY];
            //if the sum of the relevant fields is more attractive than the currently most attractive node's value
            if (neighbourValue> bestCost)
            {
                //then set the new closest node
                bestNode = neighbourList[i];
                //and set the current best cost
                bestCost = neighbourValue;
            }
            //if the neighbour's value is the same as the best cost then take the one with the lowest PHYSICAL distance
            else if(neighbourValue == bestCost && bestNode != null)
            {
                int dst1 = CostFieldGenerator.Instance.manhattanDistanceFromGoal[neighbourList[i].gridX, neighbourList[i].gridY];
                int dst2 = CostFieldGenerator.Instance.manhattanDistanceFromGoal[bestNode.gridX, bestNode.gridY];
                if(dst1 < dst2)
                {
                    bestNode = neighbourList[i];
                }
            }
        }       
        
        if (bestNode != null)
        {
            
            recentlyVisitedNode = bestNode;
            currentNodeCost = CostFieldGenerator.Instance.goalCostField[agentNode.gridX, agentNode.gridY] + CostFieldGenerator.Instance.staticObstacleCostField[agentNode.gridX, agentNode.gridY];
            //return the normalized directional vector between the best node's position and the agents current node
            return (bestNode.WorldPosition - agentNode.WorldPosition).normalized;
        }
        //if a best node cannot be returned, return no movement
        return Vector3.zero;
    }
}
