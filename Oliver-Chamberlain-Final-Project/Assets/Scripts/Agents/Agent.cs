using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    
    [Header("Squad information")]
    //as we are only testing on one squad, for now we will just find a squad with FindObjectOfType
    private Squad agentSquad;
    public Agent Leader;
    public Flank flank;
    public int flankIndex;
    //offset from the unit before a unit in the flank structure
    //if a unit has a flank index of 0 then they are the flank's leader and they must keep an offset from the leader of the squad
    public Vector3 offsetFromLeader;

    public int formationXPos;
    public int formationYPos;
#region pathfinding
    public KeyCode findpath;
    [HideInInspector]
    public Vector3[] path;
  
    int targetIndex;
    public float speed = 5f;

    public void OnPathFound(Vector3[] newpath, bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = newpath;
            agentSquad.CalculateSquadPath();
            foreach(Agent agent in agentSquad.SquadMembers)
            {
                agent.StopCoroutine("FollowPath");
                agent.StartCoroutine("FollowPath");
            }
        }
    }

    private IEnumerator FollowPath()
    {
        targetIndex = 0;
        Vector3 currentWaypoint = path[0];
        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion
    public void Awake()
    {
        agentSquad = FindObjectOfType<Squad>();
    }
   

    void Update ()
    {
        if(agentSquad.SquadLeader == this)
        {
            if (Input.GetKeyDown(findpath))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayhit = new RaycastHit();
                if (Physics.Raycast(ray, out rayhit))
                {
                    PathRequestManager.RequestPath(transform.position, rayhit.point, OnPathFound);
                }
            }
        }
	}

    public Vector3 GetOffsetPosition()
    {
        int offsetX = formationXPos - Leader.formationXPos;
        int offsetY = formationYPos - Leader.formationYPos;

        float positionX = offsetX * agentSquad.currentFormation.DistBetweenColumns;
        float positionZ = offsetY * agentSquad.currentFormation.DistBetweenRows;
       return new Vector3(positionX, Leader.transform.position.y, positionZ);
    }


    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one / 5);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
