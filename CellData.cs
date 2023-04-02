public class CellData
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string State { get; set; }

    public CellData(int row, int col, string state)
    {
        Row = row;
        Col = col;
        State = state;
    }
}
