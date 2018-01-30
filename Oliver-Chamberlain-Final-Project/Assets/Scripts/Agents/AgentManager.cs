using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour {

    public static List<FlowFieldAgent> agents;


    public void Awake()
    {
        agents = new List<FlowFieldAgent>();
    }


}
