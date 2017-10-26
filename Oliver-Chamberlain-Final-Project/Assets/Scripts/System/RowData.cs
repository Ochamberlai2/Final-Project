
[System.Serializable]
public class RowData
{
    [System.Serializable]
    public struct Row
    {
        public bool[] row;
    }
    public Row[] rows;
    public int NumAgents;
}