using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;



public static class CostFieldGenerator
{


    #region goal field
    /// <summary>
    /// Generate the potential values for every node on the grid for a given goal node
    /// </summary>
    /// <param name="grid">the grid to check against</param>
    /// <param name="target">the target point in the world</param>
    /// <param name="goalCostField">the multidimensional float array to store the potential values in.</param>
    /// <param name="goalFieldMass">the strength of the field's gravitational pull</param>
    /// <param name="goalNode">a cached reference to the goal node. Used for gizmos for debugging</param>
    public static void GenerateGoalField(Grid grid,Vector3 target, ref float[,] goalCostField, float goalFieldMass, ref Node goalNode)
    {
        /*
         * 
         * 
         * THIS NEEDS TO HAVE A CLOSED SET, THIS IS AN INCORRECT ALGORITHM
         * 
         * 
         * 
         */


        List<Node> activeSet = new List<Node>();
        //reset the cost for all nodes in the field
        foreach (Node node in grid.grid)
        {
            goalCostField[node.gridX, node.gridY] = 0;
        }
        //get the target node
        Node targetNode = grid.NodeFromWorldPoint(target);
        goalNode = targetNode;

        //set the value of the target node.
        goalCostField[targetNode.gridX, targetNode.gridY] = goalFieldMass;
 
        //and add it to the active set
        activeSet.Add(targetNode);

        //while the active set isnt empty
        while (activeSet.Count > 0)
        {
            Node toCheck = activeSet[0];
            //loop through the neighbour list of the front node of the active set
            Node[] neighbourArr = grid.GetNeighbours(toCheck).ToArray();


            for (int i = 0; i < neighbourArr.Length; i++)
            {

                //if  the current cost -1 is 0, then we have already faded to zero and we dont need to check any other nodes            
                if (goalCostField[toCheck.gridX, toCheck.gridY] <= 0)
                {
                    return;
                }
                //if it already has a cost higher than 0, leave it at that because we just want the lowest number of steps to the target
                if (neighbourArr[i] == null || goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] != 0 || neighbourArr[i] == targetNode || !neighbourArr[i].walkable)
                    continue;

                //generate the distance value used in the potential equation
                float distBetweenPoints = Vector3.Distance(targetNode.WorldPosition, neighbourArr[i].WorldPosition);
              

                //potential = field strength / distance between the two points
                float potential =goalFieldMass /distBetweenPoints;
                goalCostField[neighbourArr[i].gridX, neighbourArr[i].gridY] = potential;
                activeSet.Add(neighbourArr[i]);
            }
            activeSet.Remove(toCheck);
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
         List<Node> openSet = new List<Node>();
        List<Node> obstacleNodes = new List<Node>();

        //we need to loop through the grid and add all of the unwalkable nodes to the open set
        for(int x = 0; x < grid.gridSizeX; x++)
        {
            for(int y = 0; y  < grid.gridSizeY; y++)
            {
                //look for unwalkable nodes because we only need the unwalkable nodes to generate the field
                if (grid.grid[x, y].walkable)
                    continue;

                int numUnwalkable = 0;//tracks the number of unwalkable nodes 
                List<Node> neighbours = grid.GetNeighbours(grid.grid[x, y]);//the octagonal neighbours of the current node

                //then loop through every neighbour node of the current node
                foreach (Node neighbour in neighbours)
                {
                    //if it's unwalkable, then increase the counter
                    if(!neighbour.walkable)
                    {
                        numUnwalkable++;
                    }
                }//end of foreach

                //if the number of unwalkable neighbours is equal to the number of neighbours, then the node is inconsequential, so we can skip this node
                if(numUnwalkable == neighbours.Count)
                {
                    continue;
                }
                obstacleNodes.Add(grid.grid[x, y]);
                //set the unwalkable node to -infinity (because it is a negatively charged field)
                staticObstacleCostField[x, y] = float.MinValue;

            }//end of y for
        }//end of x for

        //now we need to generate the field using a brushfire algorithm
      
        for (int i = 0; i < obstacleNodes.Count; i++)
        {
            openSet.AddRange(grid.GetNeighbours(obstacleNodes[i]));
            while (openSet.Count > 0)
            {

                Node nodeToCheck = openSet[0];//set the checking node to the first node in the list

                //get all orthogonal neighbours of this node
                List<Node> neighbours = grid.GetNeighbours(nodeToCheck);

                for (int j = 0; j < neighbours.Count; j++)
                {
                   //check that the entry is not null and is walkable 
                    if (neighbours[j] == null  || !neighbours[j].walkable)
                    {
                        continue;
                    }
                    //generate r^2 for the below equation
                    float distBetweenObjects = Vector3.Distance(obstacleNodes[i].WorldPosition,neighbours[j].WorldPosition);
                    float force = 0;

                    if (distBetweenObjects >= staticFieldInfluenceRadius)//this should stop the field from acting past a 2 node radius 
                        continue;

                    //then square the distance
                    //distBetweenObjects = distBetweenObjects * distBetweenObjects;
                    //the mass of the field divided by the squared distance between them
                    force = staticFieldStrength / distBetweenObjects;

                    //if the negative force is greater than the already stored negative force, then update it
                    if(-force < staticObstacleCostField[neighbours[j].gridX,neighbours[j].gridY])
                    {
                            staticObstacleCostField[neighbours[j].gridX, neighbours[j].gridY] = -force;
                            openSet.Add(neighbours[j]);
                    }
                }
                openSet.Remove(nodeToCheck);



            }//end of while
        }//end of for
    }//EOF
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
                    //only take the highest potential value
                    if (potential > nodePotential)
                    {
                        nodePotential = potential;
                    }
                }
            }
        }
        return -nodePotential;
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
                
         foreach (Node node in grid.grid)
         {
             int gridX = node.gridX;
             int gridY = node.gridY;
             //if the node isnt walkable, ignore it
             if (!node.walkable)
                 continue;

             //generate distance for the potential equation
             float distBetweenPoints = FindDistanceToClosestFormationPosition(node.WorldPosition, formationNodes);

            if (distBetweenPoints > fieldInfluence)
                continue;

             //potential = field strength / distance between the two points
             float force = fieldStrength / distBetweenPoints;
             //apply the value to the field
             formationField[gridX, gridY] = force;
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

    /// <summary>
    /// Find a node's distance from the closest node within the formation
    /// </summary>
    /// <param name="checkingNodePos">the position of the node currently being checked</param>
    /// <param name="formationNodes">an array of the nodes contained in the formation</param>
    /// <returns></returns>
    private static float FindDistanceToClosestFormationPosition(Vector3 checkingNodePos,Node[] formationNodes)
    {

        float[] nodeDistances = new float[formationNodes.Length];
        //loop through and get the distance from the checking node to each of the formation nodes
        for(int i = 0; i < nodeDistances.Length; i++)
        {
            nodeDistances[i] = Vector3.Distance(checkingNodePos, formationNodes[i].WorldPosition);
        }
        float lowestDist = float.MaxValue;
        //then search for the lowest distance
        for(int j = 0; j < nodeDistances.Length; j++)
        {
            if(nodeDistances[j] < lowestDist)
            {
                lowestDist = nodeDistances[j];
            }
        }
        //error check, if there are no formation nodes that means there is no formation
        if (lowestDist == float.MaxValue)
            UnityEngine.Debug.LogError("closest formation node is not found");

        //if the lowest dist is 0 then return 1, because dividing by 0 yields a value of infinity
        if (lowestDist == 0)
            return 1;

        return lowestDist;
    }
    #endregion



}