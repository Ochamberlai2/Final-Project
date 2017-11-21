using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldAgent : MonoBehaviour {

    [SerializeField]
    private float velocityMultiplier;

    private Grid grid;
    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update ()
    {
        Vector2 desiredVelocity = grid.NodeFromWorldPoint(transform.position).NodeVector * velocityMultiplier;
            rb.velocity = new Vector3(desiredVelocity.x,0,desiredVelocity.y);

	}
}
