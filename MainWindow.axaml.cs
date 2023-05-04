using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace GridShadingApp;

public partial class MainWindow : Window
{
    private const int numRows = 18;
    private const int numCols = 32;
    private double cellSize;
    //private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.White);
    private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.Transparent);
    private SolidColorBrush shadedBrush = new SolidColorBrush(Colors.Black);
    private SolidColorBrush variantBrush = new SolidColorBrush(Colors.DarkRed);
    private SolidColorBrush currentBrush;

    private HashSet<Border> modifiedCells = new HashSet<Border>();
    private Random r = new Random();

    private string tileset = "default"; // default tileset

    public MainWindow()
    {
        InitializeComponent();
        PopulateTilesetComboBox();
        //this.CanResize = false;
        currentBrush = shadedBrush;
        //colorButton.Background = shadedBrush;
        
        this.Opened += MainWindow_Opened;
        mainGrid.PointerMoved += MainGrid_PointerMoved;
        saveButton.Click += SaveButton_Click;
        loadButton.Click += LoadButton_Click;
        resetButton.Click += ResetButton_Click;
        generateButton.Click += GenerateButton_Click;
        //colorButton.Click += ColorButton_Click;

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
        mainGrid.Children.Clear();
        mainGrid.RowDefinitions.Clear();
        mainGrid.ColumnDefinitions.Clear();

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
            cell.Background = currentBrush;
            Cell.SetState(cell, "shaded");
        }
        else
        {
            cell.Background = defaultBrush;
            Cell.SetState(cell, "default");
        }
    }

    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveGridState();
    }

    private void SaveGridState()
    {
        List<Cell> gridState = new List<Cell>();

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                Border cell = mainGrid.Children[row * numCols + col] as Border;
                if (cell != null)
                {
                    string state = Cell.GetState(cell);
                    gridState.Add(new Cell(row, col, state));
                }
            }
        }

        if (!Directory.Exists("saves")) { Directory.CreateDirectory("saves"); }


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

    /*private void ColorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SolidColorBrush[] brushes = { shadedBrush, variantBrush, defaultBrush };
        int currentIndex = Array.IndexOf(brushes, colorButton.Background);

        int nextIndex = (currentIndex + 1) % brushes.Length;
        colorButton.Background = brushes[nextIndex];
        currentBrush = brushes[nextIndex];
    }*/


   private void LoadGridState()
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
        string[] fileNames = dialog.ShowAsync(this).GetAwaiter().GetResult();

        if (fileNames == null || fileNames.Length == 0)
        {
            return; // User cancelled the file selection.
        }

        string fileName = fileNames[0];

        // Load the grid state from the selected file.
        string json = File.ReadAllText(fileName);
        List<Cell> gridState = JsonSerializer.Deserialize<List<Cell>>(json);

        // Clear the grid's children
        CreateGrid();

        // Iterate through the deserialized grid state
        foreach (Cell Cell in gridState)
        {
            // Find the corresponding cell in the grid
            Border cell = mainGrid.Children[Cell.Row * numCols + Cell.Col] as Border;

            // Set the cell's state and background
            Cell.SetState(cell, Cell.State);
            SetCellBackground(cell, Cell.State == "shaded"); // add variants
        }
    }

    private void ResetButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CreateGrid();
    }

    private void GenerateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Create a new GeneratedWindow
        GeneratedWindow generatedWindow = new GeneratedWindow();

        // Clone the grid with images
        Grid clonedGrid = CloneGridWithImages(mainGrid);

        // Set the cloned grid as the content of the new window
        generatedWindow.FindControl<Grid>("generatedGrid").Children.Add(clonedGrid);

        // Show the new window
        generatedWindow.Show();
    }

    private Grid CloneGridWithImages(Grid sourceGrid)
    {
        Grid clonedGrid = new Grid();

        // Clone row and column definitions
        foreach (var row in sourceGrid.RowDefinitions)
        {
            clonedGrid.RowDefinitions.Add(new RowDefinition(row.Height));
        }
        foreach (var col in sourceGrid.ColumnDefinitions)
        {
            clonedGrid.ColumnDefinitions.Add(new ColumnDefinition(col.Width));
        }

        // Clone grid cells with images
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                Border sourceCell = sourceGrid.Children[row * numCols + col] as Border;
                Border clonedCell = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.LightGray)
                };

                string state = Cell.GetState(sourceCell);

                double f = r.NextDouble();
                double variation = 0.1;

                string path = "./resources/tilesets/" + tileset + "/";
                string df = f > variation ? "default.png" : "random.png";
                string imagePath = path + (state == "shaded" ? "shaded.png" : df);

                if (!File.Exists(imagePath)) {
                    imagePath = ("./resources/data/default/" + (state == "shaded" ? "shaded.png" : df));
                }

                clonedCell.Background = new ImageBrush(new Bitmap(imagePath));

                Grid.SetRow(clonedCell, row);
                Grid.SetColumn(clonedCell, col);
                clonedGrid.Children.Add(clonedCell);
            }
        }

        return clonedGrid;
    }

    private void PopulateTilesetComboBox()
    {
        string tilesetsPath = "./resources/tilesets";
        List<string> tilesetNames = new List<string>();

        if (Directory.Exists(tilesetsPath))
        {
            string[] directories = Directory.GetDirectories(tilesetsPath);
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);
                tilesetNames.Add(directoryName);
            }
        }

        tilesetSelect.Items = tilesetNames;
        if (tilesetSelect.Items.Equals == null)
        {
            return;
        } else {
            tilesetSelect.SelectedIndex = 0;
        }
    }

    private void OnTilesetSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (tilesetSelect.SelectedItem != null)
        {
            tileset = tilesetSelect.SelectedItem.ToString();
        }
    }
}