using Avalonia;
using Avalonia.Controls;

public class CellState : AvaloniaObject
{
    public static readonly AttachedProperty<string> StateProperty =
        AvaloniaProperty.RegisterAttached<CellState, Control, string>("State");

    public static string GetState(Control element)
    {
        return element.GetValue(StateProperty);
    }

    public static void SetState(Control element, string value)
    {
        element.SetValue(StateProperty, value);
    }
}
