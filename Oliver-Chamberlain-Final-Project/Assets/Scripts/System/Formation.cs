using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Formation")]
public class Formation : ScriptableObject
{
    public int NumOfAgents;
    public RowData FormationLayout;
    public float DistBetweenColumns;
    public float DistBetweenRows;
	
}
