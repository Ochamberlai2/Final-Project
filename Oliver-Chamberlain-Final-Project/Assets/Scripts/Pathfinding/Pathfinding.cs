using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    bool pathSuccess;

    private void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }
    private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] wayPoints = new Vector3[0];

        //get the grid position corresponding to these world points
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(grid.maxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {
                Debug.Log("Path found successfully");
                pathSuccess = true;
                break;
            }

            foreach(Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                //get the movement cost to that neighbour node
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parentNode = currentNode;
                }
                if(!openSet.contains(neighbour))
                {
                    openSet.Add(neighbour);
                }
                else
                {
                    openSet.UpdateItem(neighbour);
                }
            }
        }
        yield return null;
        if(pathSuccess)
        {
            wayPoints = retracePath(startNode, targetNode);
        }
        PathRequestManager.Instance.FinishedProcessingPath(wayPoints, pathSuccess);
    }
    //retrace the bath between node a and node b, starting from the target
    Vector3[] retracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = targetNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;

        }
            Vector3[] wayPoints = simplifyPath(path);
            Array.Reverse(wayPoints);

            return wayPoints;
    }

    //if the vector between two nodes is the same as the last (the agent would move in the same direction) ignore it until we find a change in direction
    Vector3[] simplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i = 0; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].GridPosition.x - path[i].GridPosition.x, path[i - 1].GridPosition.y - path[i].GridPosition.y);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i].WorldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    //using 1 as the movement left, right, up or down, and sqrt(2) for diagonal movement, both multiplied by 10
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = (int)Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x);
        int dstY = (int)Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y);

        return 10 * (dstX + dstY) + (14 - 2 * 10) * Mathf.Min(dstX, dstY);
    }

}
