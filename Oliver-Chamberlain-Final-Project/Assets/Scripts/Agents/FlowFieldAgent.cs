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
        StartCoroutine(Movement());
    }


    private IEnumerator Movement()
    {
        while (true)
        {
            if(name == "FlowFieldAgent (1)")
            {
                Debug.Log("here");
            }
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
        Node closestNode = null;
        float bestCost = 0;
         for(int i = 0; i < neighbourList.Length; i++)
        {
            if (!neighbourList[i].walkable || neighbourList[i] == null)
                continue;
            if(CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] - CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY] > bestCost)
            {
                closestNode = neighbourList[i];
                bestCost = CostFieldGenerator.Instance.goalCostField[neighbourList[i].gridX, neighbourList[i].gridY] - CostFieldGenerator.Instance.staticObstacleCostField[neighbourList[i].gridX, neighbourList[i].gridY];
            }
        }
        if (closestNode != null)
            return (closestNode.WorldPosition - agentNode.WorldPosition).normalized;
        return Vector3.zero;
    }
}
