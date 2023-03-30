using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace GridShadingApp;

public partial class MainWindow : Window
{
    private const int numRows = 10;
    private const int numCols = 10;
    private double cellSize;
    private SolidColorBrush defaultBrush = new SolidColorBrush(Colors.LightGray);
    //private SolidColorBrush shadedBrush = new SolidColorBrush(Colors.Black);
    private ImageBrush imageBrush = new ImageBrush(new Bitmap("Images/test.png"));


    public MainWindow()
    {
        InitializeComponent();
        this.Opened += MainWindow_Opened;
        this.LayoutUpdated += MainWindow_LayoutUpdated;

        // Event Handlers
        void MainWindow_Opened(object sender, EventArgs e)
        {
            var windowSize = this.ClientSize;
            cellSize = Math.Min(windowSize.Width / numCols, windowSize.Height / numRows);
            CreateGrid();
        }

        void MainWindow_LayoutUpdated(object? sender, EventArgs e)
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