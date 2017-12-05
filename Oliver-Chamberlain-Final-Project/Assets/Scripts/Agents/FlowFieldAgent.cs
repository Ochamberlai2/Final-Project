using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldAgent : MonoBehaviour {

    [SerializeField]
    private float velocityMultiplier;

    private Grid grid;
    private Rigidbody rb;

    private Vector3 desiredVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        desiredVelocity = FindNextNode() * velocityMultiplier;
        //Vector2 desiredVelocity = (grid.NodeFromWorldPoint(transform.position) as PFNode).NodeVector * velocityMultiplier;
        //    rb.velocity = new Vector3(desiredVelocity.x,0,desiredVelocity.y);
        rb.velocity = new Vector3(desiredVelocity.x,0,desiredVelocity.z);
       

	}
    //returns a normalised vector pointing to the cheapest node
    public Vector3 FindNextNode()
    {
        Node colonistNode = grid.NodeFromWorldPoint(transform.position);
        Node closestNode = null;
        float bestCost = colonistNode.gCost;
        foreach (Node node in grid.GetNeighbours(colonistNode))
        {
            if (!node.walkable)
                continue;
            if(FlowFieldGenerator.Instance.goalCostField[node.gridX,node.gridY] - 
                FlowFieldGenerator.Instance.staticObstacleCostField[node.gridX,node.gridY]
                > bestCost)
            {
                closestNode = node;
                bestCost = FlowFieldGenerator.Instance.goalCostField[node.gridX, node.gridY] -
                FlowFieldGenerator.Instance.staticObstacleCostField[node.gridX, node.gridY];
            }
        }
        if (closestNode != null)
            return (closestNode.WorldPosition - transform.position).normalized;
        return Vector3.zero;
    }
}
