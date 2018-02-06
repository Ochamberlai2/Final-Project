using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour {

    public static List<PotentialFieldAgent> agents;


    public void Awake()
    {
        agents = new List<PotentialFieldAgent>();
    }


}
