using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    public KeyCode findpath;
    private Vector3[] path;
    int targetIndex;
    float speed = 5f;

	// Update is called once per frame
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
    public void OnPathFound(Vector3[] newpath, bool pathSuccess)
    {
        if(pathSuccess)
        {
            path = newpath;
            StopCoroutine("FollowPath");
            StartCoroutine(FollowPath(path));
        }
    }

    private IEnumerator FollowPath(Vector3[] path)
    {
        Vector3 currentWaypoint = path[0];

        if(transform.position == currentWaypoint)
        {
            targetIndex++;
            if(targetIndex >= path.Length)
            {
                yield break;
            }
            currentWaypoint = path[targetIndex];
        }
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed);
        yield return null;
    }
}
