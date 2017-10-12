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

#region pathfinding
    public KeyCode findpath;
    private Vector3[] path;
    int targetIndex;
    public float speed = 5f;

    public void OnPathFound(Vector3[] newpath, bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = newpath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
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
    public void SetLeader()
    {
        switch (flank)
        {
            case Flank.left:
                if (flankIndex == 0)
                    Leader = this;
                else
                    Leader = agentSquad.LeftFlank[flankIndex - 1];
                break;
            case Flank.centre:
                if (flankIndex == 0)
                    Leader = this;
                else
                    Leader = agentSquad.Centre[flankIndex - 1];
                break;
            case Flank.right:
                if (flankIndex == 0)
                    Leader = this;
                else
                    Leader = agentSquad.RightFlank[flankIndex - 1];
                break;
            default:
                break;
        }
    }

    void Update ()
    {
		if(Input.GetKeyDown(findpath))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit = new RaycastHit();
            if (Physics.Raycast(ray, out rayhit))
            {
                PathRequestManager.RequestPath(transform.position, rayhit.point, OnPathFound);
            }
        }
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
