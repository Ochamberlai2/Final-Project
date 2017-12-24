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


    public int bestcost;
    public int currentNodeCost;


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
        int bestCost = 0;
         for(int i = 0; i < neighbourList.Length; i++)
        {
            if (!neighbourList[i].walkable || neighbourList[i] == null)
                continue;


            //if the sum of the relevant fields + the recent node cost (-1 to add a weak negative field to push the agent along in case of local optima) is greater than the current best cost
            if(CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY] > bestCost)
            {
                //then set the new closest node
                bestNode = neighbourList[i];
                //and set the current best cost
                bestCost = CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY];
            }

            /*
             * Need to change the below code, this was an attempt  to fix the local minima problem
             * however the tie breaker doesnt always work so something else needs to be done
             */

            //in the case that there is a situation where two nodes are equal, as a tiebreaker we will try to choose the node with the best cost to the goal
            else if(CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY] == bestCost)
            {
                //if there isnt a best node currently assigned, or the best node is already better than the node we are comparing against, check the next node.
                if(bestNode == null || CostFieldGenerator.Instance.goalCostField[bestNode.gridX,bestNode.gridY] >= CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY])
                {
                    continue;
                }
                //otherwise the recently checked node is better than the current best, so assign it as the new best
                else
                {
                    //set the new closest node
                    bestNode = neighbourList[i];
                    //set the current best cost
                    bestCost = CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] + CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY];
                }
            }
        }
        if (bestNode != null)
        {
            //
            bestcost = bestCost;
            recentlyVisitedNode = bestNode;
            currentNodeCost = CostFieldGenerator.Instance.goalCostField[agentNode.gridX, agentNode.gridY] + CostFieldGenerator.Instance.staticObstacleCostField[agentNode.gridX, agentNode.gridY];
            //
            //return the normalized directional vector between the best node's position and the agents current node
            return (bestNode.WorldPosition - agentNode.WorldPosition).normalized;
        }
        //if a best node cannot be returned, return no movement
        return Vector3.zero;
    }
}
