
[System.Serializable]
public class RowData
{
    [System.Serializable]
    public struct Row
    {
        public int[] Column;
    }
    public Row[] rows;
    public int NumAgents;
}