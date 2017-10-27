
[System.Serializable]
public class RowData
{
    [System.Serializable]
    public struct Row
    {
        public bool[] Column;
    }
    public Row[] rows;
    public int NumAgents;
}