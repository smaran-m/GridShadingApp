using Avalonia;
using Avalonia.Controls;

public class Cell : AvaloniaObject
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string State { get; set; }

    public static readonly AttachedProperty<string> StateProperty =
        AvaloniaProperty.RegisterAttached<Cell, Control, string>("State");

    public Cell(int row, int col, string state)
    {
        Row = row;
        Col = col;
        State = state;
    }

    public static string GetState(Control element)
    {
        return element.GetValue(StateProperty);
    }

    public static void SetState(Control element, string value)
    {
        element.SetValue(StateProperty, value);
    }

    enum States
    {
        EMPTY,
        SHADED,
        RANDOM
    }
}
