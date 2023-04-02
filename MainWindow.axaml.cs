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
    private ImageBrush imageBrush = new ImageBrush(new Bitmap("Images/test.png"));
    private HashSet<Border> modifiedCells = new HashSet<Border>();



    public MainWindow()
    {
        InitializeComponent();
        //this.CanResize = false;
        this.Opened += MainWindow_Opened;
        mainGrid.PointerMoved += MainGrid_PointerMoved;
        saveButton.Click += SaveButton_Click;

        // Event Handlers
        void MainWindow_Opened(object sender, EventArgs e)
        {
            var windowSize = this.ClientSize;
            cellSize = Math.Min(windowSize.Width / numCols, windowSize.Height / numRows);
            CreateGrid();
        }

        /*void MainWindow_LayoutUpdated(object? sender, EventArgs e)
        {
            var windowSize = this.ClientSize;
            double aspectRatio = (double)numCols / numRows;
            double newWidth = windowSize.Width;
            double newHeight = windowSize.Height;

            if (newWidth / newHeight > aspectRatio)
            {
                newWidth = newHeight * aspectRatio;
            }
            else
            {
                newHeight = newWidth / aspectRatio;
            }

            this.ClientSize = new Size(newWidth, newHeight);

            cellSize = Math.Min(newWidth / numCols, newHeight / numRows);

            foreach (var child in mainGrid.Children)
            {
                if (child is Border cell)
                {
                    cell.Width = cellSize;
                    cell.Height = cellSize;
                }
            }
        }*/
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
            cell.Background = cell.Background == shadedBrush ? defaultBrush : shadedBrush;
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
                    cell.Background = cell.Background == shadedBrush ? defaultBrush : shadedBrush;
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

    private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveGridState();
    }

    private void SaveGridState()
    {
        // You can use the modifiedCells set to save the state of the grid.
        // For example, you can store the row and column information and the brush color.
        List<(int row, int col, string color)> gridState = new List<(int row, int col, string color)>();

        foreach (Border cell in modifiedCells)
        {
            int row = Grid.GetRow(cell);
            int col = Grid.GetColumn(cell);
            string color = cell.Background == shadedBrush ? "shaded" : "default";

            gridState.Add((row, col, color));
        }

        // Save the gridState to a file or any other storage you prefer.
        // For example, you can serialize the gridState list and save it to a file.
        string json = JsonSerializer.Serialize(gridState);
        Console.WriteLine(gridState);
        File.WriteAllText("gridState.json", json);
    }

}