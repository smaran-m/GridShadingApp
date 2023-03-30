using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;


namespace GridShadingApp;

public partial class MainWindow : Window
{
    private const int numRows = 10;
    private const int numCols = 10;
    private const int cellSize = 40;
    private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.LightGray);
    //private SolidColorBrush shadedBrush = new SolidColorBrush(Colors.Black);
    private ImageBrush imageBrush = new ImageBrush(new Bitmap("Images/test.png"));


    public MainWindow()
    {
        InitializeComponent();
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int i = 0; i < numRows; i++)
        {
            mainGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (int j = 0; j < numCols; j++)
        {
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                var cell = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    Background = defaultBrush,
                    Width = cellSize,
                    Height = cellSize,
                };

                cell.PointerPressed += Cell_PointerPressed;

                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                mainGrid.Children.Add(cell);
            }
        }
    }

    private void Cell_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border cell)
        {
            cell.Background = cell.Background == defaultBrush ? imageBrush : defaultBrush;
        }
    }

}