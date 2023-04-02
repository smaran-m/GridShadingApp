using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace GridShadingApp;

public partial class MainWindow : Window
{
    private const int numRows = 18;
    private const int numCols = 32;
    private double cellSize;
    //private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.White);
    private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.Transparent);
    private SolidColorBrush shadedBrush = new SolidColorBrush(Colors.Black);

    //private ImageBrush imageBrush = new ImageBrush(new Bitmap("resources/images/test.png"));
    private HashSet<Border> modifiedCells = new HashSet<Border>();



    public MainWindow()
    {
        InitializeComponent();
        //this.CanResize = false;
        this.Opened += MainWindow_Opened;
        mainGrid.PointerMoved += MainGrid_PointerMoved;
        saveButton.Click += SaveButton_Click;
        loadButton.Click += LoadButton_Click;

        // Event Handlers
        void MainWindow_Opened(object sender, EventArgs e)
        {
            var windowSize = this.ClientSize;
            cellSize = Math.Min(windowSize.Width / numCols, windowSize.Height / numRows);
            CreateGrid();
        }
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
                Border cell = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    Background = defaultBrush,
                    Width = cellSize,
                    Height = cellSize,
                };

                cell.PointerPressed += Cell_PointerPressed;
                cell.PointerReleased += Cell_PointerReleased;

                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                mainGrid.Children.Add(cell);
            }
        }
    }

    private bool isDragging = false;

    private void Cell_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border cell)
        {
            isDragging = true;
            SetCellBackground(cell, cell.Background == defaultBrush);
            modifiedCells.Add(cell);
        }
    }

    private void MainGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (isDragging)
        {
            var point = e.GetPosition(mainGrid);
            int row = (int)(point.Y / cellSize);
            int col = (int)(point.X / cellSize);

            if (row >= 0 && row < numRows && col >= 0 && col < numCols)
            {
                Border cell = mainGrid.Children[row * numCols + col] as Border;
                if (cell != null && !modifiedCells.Contains(cell))
                {
                    // Toggle the cell background between shadedBrush and defaultBrush
                    SetCellBackground(cell, cell.Background == defaultBrush);
                    modifiedCells.Add(cell);
                }
            }
        }
    }

    private void Cell_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        isDragging = false;
        modifiedCells.Clear(); // Clear the modified cells set
    }
    private void SetCellBackground(Border cell, bool shaded)
    {
        if (shaded)
        {
            cell.Background = shadedBrush;
            CellState.SetState(cell, "shaded");
        }
        else
        {
            cell.Background = defaultBrush;
            CellState.SetState(cell, "default");
        }
    }

    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveGridState();
    }

    private void SaveGridState()
    {
        List<CellData> gridState = new List<CellData>();

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                Border cell = mainGrid.Children[row * numCols + col] as Border;
                if (cell != null)
                {
                    string state = CellState.GetState(cell);
                    gridState.Add(new CellData(row, col, state));
                }
            }
        }

        if (!Directory.Exists("saves"))
        {
        Directory.CreateDirectory("saves");
        }


        int fileNumber = 1;
        string fileName;
        do
        {
            fileName = Path.Combine("saves", $"grid_{fileNumber}.json");
            fileNumber++;
        }
        while (File.Exists(fileName));

        // Save the gridState to the new file.
        string json = JsonSerializer.Serialize(gridState);
        File.WriteAllText(fileName, json);
    }

    private void LoadButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadGridState();
    }

   private void LoadGridState()
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
        string[] fileNames = dialog.ShowAsync(this).GetAwaiter().GetResult();

        if (fileNames.Length == 0)
        {
            return; // User cancelled the file selection.
        }

        string fileName = fileNames[0];

        // Load the grid state from the selected file.
        string json = File.ReadAllText(fileName);
        Console.WriteLine("Loaded JSON data:");
        Console.WriteLine(json);
        List<CellData> gridState = JsonSerializer.Deserialize<List<CellData>>(json);
        Console.WriteLine("Deserialized grid state:");
        foreach (var cellData in gridState)
        {
            Console.WriteLine($"({cellData.Row}, {cellData.Col}): {cellData.State}");
        }

        // Update the grid with the loaded state.
        foreach (CellData cellData in gridState)
        {
            Border cell = mainGrid.Children[cellData.Row * numCols + cellData.Col] as Border;
            if (cell != null)
            {
                CellState.SetState(cell, cellData.State);
            }
            
            // THIS DOESN'T WORK YET, MAYBE NEED TO REFRESH MainWindow AFTER SETTING STATES
        }
    }
}
