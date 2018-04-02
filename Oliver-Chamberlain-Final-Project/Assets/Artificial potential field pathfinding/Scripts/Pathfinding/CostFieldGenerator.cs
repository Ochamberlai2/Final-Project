using System.Collections.Generic;
using UnityEngine;



public static class CostFieldGenerator
{


    #region goal field
    /// <summary>
    /// Generate the potential values for every node on the grid for a given goal node
    /// </summary>
    /// <param name="grid">the grid to check against</param>
    /// <param name="target">the target point in the world</param>
    /// <param name="goalCostField">the multidimensional float array to store the potential values in.</param>
    /// <param name="goalFieldStrength">the strength of the field's gravitational pull</param>
    /// <param name="goalNode">a cached reference to the goal node. Used for gizmos for debugging</param>
    public static void GenerateGoalField(Grid grid,Vector3 target, ref float[,] goalCostField, float goalFieldStrength, ref Node goalNode)
    {

        Queue<Node> openSet = new Queue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        //find the target node and initialise its cost
        Node targetNode = grid.NodeFromWorldPoint(target);
        goalCostField[targetNode.gridX, targetNode.gridY] = goalFieldStrength;
        goalNode = targetNode;

        openSet.Enqueue(goalNode);

        while (openSet.Count > 0)
        {
            //get the node at the front of the queue
            Node currentNode = openSet.Dequeue();

            //get the neighbours of the current node
            List<Node> neighbours = grid.GetNeighbours(currentNode);

            //and loop through them
            for (int i = 0; i < neighbours.Count; i++)
            {
                //if the neighbour is already in the closed set, then it doesnt need to be checked
                if (closedSet.Contains(neighbours[i]))
                {
                    continue;
                }
                if (!neighbours[i].walkable)
                {
                    closedSet.Add(neighbours[i]);
                    continue;
                }

                //calculate the distance from the neighbour to the goal node
                float distToGoal = Vector3.Distance(neighbours[i].WorldPosition, targetNode.WorldPosition);

                //then assign a potential to it.
                //potential = field strength / distance
                float potential = goalFieldStrength / distToGoal;

                //assign the value to the potential field
                goalCostField[neighbours[i].gridX, neighbours[i].gridY] = potential;

                //if the open set doesnt already contain the node, add it
                if (!openSet.Contains(neighbours[i]))
                {
                    openSet.Enqueue(neighbours[i]);
                }
            }
            //then add the current node to the closed set.
            closedSet.Add(currentNode);
        }
    }
    #endregion
    #region obstacle avoidance
    /// <summary>
    /// Generates the artificial potential field for static obstacles in the world
    /// </summary>
    /// <param name="grid">the grid to check against</param>
    /// <param name="staticObstacleCostField">the multidimensional array to store the potential field values in</param>
    /// <param name="staticFieldInfluenceRadius">the radius from a given obstacle point before the potential is 0</param>
    /// <param name="staticFieldStrength"> the strength of the potential field</param>
    public static void GenerateStaticObstacleField(Grid grid, ref float[,] staticObstacleCostField, float staticFieldInfluenceRadius, float staticFieldStrength)
    {

        Queue<Node> openSet = new Queue<Node>();
        List<Node> closedSet = new List<Node>();

        Node[] obstacleNodes = GetObstacleNodes(grid);

        //add all of the obstacle nodes to the open set.
        foreach(Node obstacle in obstacleNodes)
        {
            openSet.Enqueue(obstacle);
        }

        while(openSet.Count > 0)
        {

            //remove the front node from the open set.
            Node currentNode = openSet.Dequeue();
            //then get all of that node's neighbours.
            List<Node> neighbours = grid.GetNeighbours(currentNode);

            //loop through all of the current node's neighbours
            for(int i = 0; i < neighbours.Count; i++)
            {
                //if the neighbour is already in the closed set, then ignore it
                if(closedSet.Contains(neighbours[i]))
                {
                    continue;
                }
                //find the distance to the closest obstacle
                float distToClosestObstacle = FindDistanceToClosestArrayNode(neighbours[i].WorldPosition, obstacleNodes);

                //if the current node is outside of the static obstacle's area of influence, ignore it and add it to the closed set.
                if(distToClosestObstacle > staticFieldInfluenceRadius)
                {
                    closedSet.Add(neighbours[i]);
                    continue;
                }

                //if the algorithm reaches this point, then the current node requires a valid potential.
                //potential = field strength / distance to closest obstacle
                float potential = staticFieldStrength / distToClosestObstacle;
                staticObstacleCostField[neighbours[i].gridX, neighbours[i].gridY] = potential;

                //if the neighbour isnt on the open set, add it.
                if(!openSet.Contains(neighbours[i]))
                {
                    openSet.Enqueue(neighbours[i]);
                }
            }
            closedSet.Add(currentNode);
        }
    }


    private static Node[] GetObstacleNodes(Grid grid)
    {
        List<Node> obstacleNodes = new List<Node>();

        //loop through every grid node
        foreach(Node node in grid.grid)
        {
            //if it is walkable, then it is not an obstacle node
            if (node.walkable)
                continue;

            //loop through all of the nodes neighbours
            foreach(Node neighbour in grid.GetNeighbours(node))
            {
                //if this neighbour is walkable, then the node is not surrounded by only other obstacle nodes
                //so it can be counted as an obstacle node to worry about searching. Then break out of the
                //loop so more processing power is not wasted.
                if(neighbour.walkable)
                {
                    obstacleNodes.Add(node);
                    break;
                }
            }
        }
        //then once the entire grid has been searched for obstacles, convert the list to an array and return it.
        return obstacleNodes.ToArray();
    }

    

#endregion
    #region agent collision

    /// <summary>
    /// Takes a node and list of agents in the world and calculates the potential for the node
    /// </summary>
    /// <param name="ignoreSquad">Whether or not the agent's squad should be ignored</param>
    /// <param name="potentialNode">the node to calculate a value for</param>
    /// <param name="searchingAgent">the agent searching for the potential</param>
    /// <param name="agentsToConsider"> all agents to consider for collision avoidance</param>
    /// <returns>The calculated potential value for the node</returns>
    public static float GetAgentCollisionPotential(bool ignoreSquad, Node potentialNode, PotentialFieldAgent searchingAgent, List<PotentialFieldSquad> agentsToConsider)
    {
        float nodePotential = 0;
        
        //search through all squads.
        for(int i = 0; i < agentsToConsider.Count; i++)
        {
            //and all agents in that squad 
            for (int j = 0; j < agentsToConsider[i].squadAgents.Count; j++)
            {
                //if the agent's squad should be ignored and the searching agent is in the squad (making it the agent's squad) then break out of this loop.
                if (ignoreSquad && agentsToConsider[i].squadAgents.Contains(searchingAgent))
                {
                    break;
                }
                //if the current agent is the one searching for a potential value, it need to be ignored.
                if(agentsToConsider[i].squadAgents[j] == searchingAgent)
                {
                    continue;
                }
                //if the node is within the agent's avoidance field radius, then work out a value for it.
                float distanceValue = Vector3.Distance(potentialNode.WorldPosition, agentsToConsider[i].squadAgents[j].agentNode.WorldPosition);
                if(distanceValue <= agentsToConsider[i].squadAgents[j].agentAvoidanceFieldRadius)
                {
                    //potential = agent mass / distance between them
                    float potential = agentsToConsider[i].squadAgents[j].AgentMass / distanceValue;
             
                    nodePotential += potential;

                }
            }
        }
        return nodePotential;
    }
    #endregion
    #region formation
    /// <summary>
    /// Generates the artificial potential field used for moving units into a formation
    /// </summary>
    /// <param name="grid">the grid to check against</param>
    /// <param name="leaderNode">the node that the formation leader is currently situated upon</param>
    /// <param name="pointsInRelationToLeaderNode">all points in the formation stored as offsets from the leader's position</param>
    /// <param name="fieldStrength">the strength of the potential field</param>
    /// <param name="fieldInfluence">the range from the closest formation node the field will extend</param>
    /// <param name="result">a callback function to handle saving</param>
    public static void GenerateFormationField(Grid grid, Node leaderNode ,List<Vector2> pointsInRelationToLeaderNode,float fieldStrength,float fieldInfluence, ref float[,] formationField)
    {

       formationField = new float[grid.gridSizeX,grid.gridSizeY];
        //get the nodes to generate a field around
        Node[] formationNodes = GetFormationNodes(grid, leaderNode, pointsInRelationToLeaderNode).ToArray();

        Queue<Node> openSet = new Queue<Node>();
        List<Node> closedSet = new List<Node>();

        //add all formation nodes to the open set.
        for(int i = 0; i < formationNodes.Length; i++)
        {
            formationField[formationNodes[i].gridX, formationNodes[i].gridY] = fieldStrength;
            openSet.Enqueue(formationNodes[i]);
        }

        while(openSet.Count > 0)
        {
            //dequeue the first node in the open set
            Node currentNode = openSet.Dequeue();

            //and find all of its neighbours
            List<Node> neighbours = grid.GetNeighbours(currentNode);

            //loop through all neighbours
            for(int i = 0; i < neighbours.Count; i++)
            {
                //if the neighbour has already been checked, ignore it
                if(closedSet.Contains(neighbours[i]))
                {
                    continue;
                }
                //get the neighbour's distance from the closest point in the formation
                float distanceFromFormation = FindDistanceToClosestArrayNode(neighbours[i].WorldPosition, formationNodes);
                //if the node is outside of the potential fields area of influence, ignore it and add it to the closed set
                if(distanceFromFormation > fieldInfluence)
                {
                    closedSet.Add(neighbours[i]);
                    continue;
                }

                //potential = field strength/distance from formation
                float potential = fieldStrength / distanceFromFormation;
                //then assign the potential to the correct place in the field
                formationField[neighbours[i].gridX, neighbours[i].gridY] = potential;
                if (!openSet.Contains(neighbours[i]))
                {
                    //add the neighbour to the open set
                    openSet.Enqueue(neighbours[i]);
                }

            }
            //and add the current node to the closed set
            closedSet.Add(currentNode);
        }
    } 

    /// <summary>
    /// Find which nodes should belong in the formation given a list of points in relation to the formation leader
    /// </summary>
    /// <param name="grid">grid to check against</param>
    /// <param name="leaderNode">the node that the leader of the formation is currently standing on</param>
    /// <param name="pointsInRelationToLeaderNode">a list of points in the formation in relation to the leader's position in the formation</param>
    /// <returns>A list of the nodes that should be a part of the formation</returns>
    private static List<Node> GetFormationNodes(Grid grid, Node leaderNode, List<Vector2> pointsInRelationToLeaderNode)
    {
        List<Node> returnList = new List<Node>();
        //loop through all of the points in relation to the leader and find which node they should be attributed to given the leader's positional node
        foreach(Vector2 offset in pointsInRelationToLeaderNode)
        {
            if(grid.ValidatePointOnGrid((int)offset.x + leaderNode.gridX,(int)offset.y + leaderNode.gridY))
            {
                Node referencedNode = grid.grid[leaderNode.gridX + (int)offset.x, leaderNode.gridY + (int)offset.y];
                returnList.Add(referencedNode);
            }
        }
        return returnList;

    }

    #endregion

    /// <summary>
    /// Find the closest node in the array to the specified point
    /// </summary>
    /// <param name="checkingNodePos">the position of the node currently being checked</param>
    /// <param name="nodeArr">array of nodes to check against</param>
    /// <returns></returns>
    private static float FindDistanceToClosestArrayNode(Vector3 checkingNodePos,Node[] nodeArr)
    {
        float lowestValue = float.MaxValue;

        for(int i = 0; i < nodeArr.Length; i++)
        {
            float distToNode = Vector3.Distance(checkingNodePos, nodeArr[i].WorldPosition);

            if(distToNode < lowestValue)
            {
                lowestValue = distToNode;
            }
        }

        //set the lowest value to 1 to avoid dividing by 0
        if (lowestValue == 0)
            lowestValue = 1;

        return lowestValue;

    }


}