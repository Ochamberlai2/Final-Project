using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFNode : Node {

    public Vector2 NodeVector;// vector the node contains which points towards the quickest path to the goal


    public PFNode(Vector3 _worldPosition, Vector2 _GridPosition, bool _walkable):base(_worldPosition,_GridPosition,_walkable)
    {

    }
}
