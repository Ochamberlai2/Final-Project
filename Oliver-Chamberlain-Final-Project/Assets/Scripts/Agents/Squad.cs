using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Flank
{
    left,
    centre,
    right,
}

public class Squad : MonoBehaviour
{
    public Agent SquadLeader;
    public List<Agent> SquadMembers;

    public List<Agent> Centre;
    public List<Agent> LeftFlank;
    public List<Agent> RightFlank;



    public void Awake()
    {
        Centre = new List<Agent>();
        LeftFlank = new List<Agent>();
        RightFlank = new List<Agent>();
    }

    public void Start()
    {
        for(int i = 0; i < SquadMembers.Count; i++)
        {
            AddSquadMember(SquadMembers[i]);
            SquadMembers[i].SetLeader();
        }
    }

    public void AddSquadMember(Agent agent)
    {
        Flank flank = AssignSquad();
        if(flank == Flank.centre)
        {
            if (Centre.Count == 0)
            {
            SquadLeader = agent;
            }
            agent.flank = flank;
            agent.flankIndex = Centre.Count;
            Centre.Add(agent);
        }
        else if(flank == Flank.right)
        {
            agent.flank = flank;
            agent.flankIndex = RightFlank.Count;
            RightFlank.Add(agent);
        }
        else
        {
            agent.flank = flank;
            agent.flankIndex = RightFlank.Count;
            LeftFlank.Add(agent);
        }

    }
    /// <summary>
    /// Returns the flank with the lowest number of members.
    /// </summary>
    private Flank AssignSquad()
    {

        if (Centre.Count <= LeftFlank.Count && Centre.Count <= RightFlank.Count)
        {
            return Flank.centre;
        }
        if(RightFlank.Count <= LeftFlank.Count && RightFlank.Count <= Centre.Count)
        {
            return Flank.right;
        }

        return Flank.left;
         
    }

    



}
