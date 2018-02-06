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
    public Agent LeftLeader;
    public Agent RightLeader;

    public List<Agent> SquadMembers;
    public List<Agent> Centre;
    public List<Agent> LeftFlank;
    public List<Agent> RightFlank;

    public Formation currentFormation;



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
        }
        AssignFormationPosition();
        ReformFormation();
    }

    public void AddSquadMember(Agent agent)
    {
        Flank flank = AssignSquad();
        if(flank == Flank.centre)
        {
            if (SquadLeader == null)
                SquadLeader = agent;

            agent.Leader = SquadLeader;
            agent.flank = flank;
            agent.flankIndex = Centre.Count;
            Centre.Add(agent);
        }
        else if(flank == Flank.right)
        {
            if (RightLeader == null)
                RightLeader = agent;

            if (agent != RightLeader)
                agent.Leader = RightLeader;
            else
                agent.Leader = SquadLeader;

            agent.flank = flank;
            agent.flankIndex = RightFlank.Count;
            RightFlank.Add(agent);
        }
        else
        {
            if (LeftLeader == null)
                LeftLeader = agent;
            if (agent != LeftLeader)
                agent.Leader = LeftLeader;
            else
                agent.Leader = SquadLeader;

            agent.flank = flank;
            agent.flankIndex = LeftFlank.Count;
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
    /// <summary>
    /// Loops through the formation layout multidimentional array and where the value is true, assigns an agent to that position
    /// </summary>
    private void AssignFormationPosition()
    {
        //counter variables for each flank
        int L = 0;
        int C = 0;
        int R = 0;
        //loop through the array
        for (int x = 0; x < currentFormation.NumOfAgents; x++)
        {
            for (int y = currentFormation.NumOfAgents-1; y >= 0; y--)
            {
                if(currentFormation.FormationLayout.rows[y].Column[x] == true)
                {
                    //check which flank needs the assigned agent should be in (The grid is read from the top left downwards to the bottom right
                    if(L < LeftFlank.Count)
                    {
                        LeftFlank[L].formationXPos = x;
                        LeftFlank[L].formationYPos = y;
                        L++;
                    }
                    else if(C < Centre.Count)
                    {
                        Centre[C].formationXPos = x;
                        Centre[C].formationYPos = y;
                        C++;
                    }
                    else if(R < RightFlank.Count)
                    {
                        RightFlank[R].formationXPos = x;
                        RightFlank[R].formationYPos = y;
                        R++;
                    }
                }
            }
        }
    }
    
    public void ReformFormation()
    {
        foreach(Agent agent in SquadMembers)
        {
            if (agent == SquadLeader)
                continue;
            agent.offsetFromLeader = agent.GetOffsetPosition();
            agent.path = new Vector3[1];
            agent.path[0] = agent.Leader.transform.position + agent.offsetFromLeader;

            agent.StopCoroutine("FollowPath");
            agent.StartCoroutine("FollowPath");
        }
    }

    public void CalculateSquadPath()
    {
        LeftLeader.path = GetOffsetPath(SquadLeader, LeftLeader);
        RightLeader.path = GetOffsetPath(SquadLeader, RightLeader);
        foreach(Agent agent in SquadMembers)
        {
            if (agent == SquadLeader || agent == LeftLeader || agent == RightLeader)
                continue;
            agent.path = GetOffsetPath(agent.Leader, agent);
        }
       

    }
    public Vector3[] GetOffsetPath(Agent Agent1, Agent OffsetAgent)
    {
        List<Vector3> offsetPath = new List<Vector3>();
        for(int i = 0; i < Agent1.path.Length; i++)
        {
            offsetPath.Add(Agent1.path[i] + OffsetAgent.GetOffsetPosition());
        }
        return offsetPath.ToArray();

    }



}
