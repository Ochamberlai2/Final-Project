using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is present to account for the eventuality when there are multiple squads, and agent avoidance requires knowing all other agent's positions
/// </summary>
public class AgentManager : MonoBehaviour {

    public static List<PotentialFieldAgent> agents;


    public void Awake()
    {
        agents = new List<PotentialFieldAgent>();
    }


}
