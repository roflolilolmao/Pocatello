﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pocatello
{
    enum ClickState { Obstacles, Start, End };

    public partial class MainWindow : Window
    {
        private Point startLine;
        private Polyline line;
        private List<Polyline> lines = new List<Polyline>();
        private ClickState state = ClickState.Obstacles;
        private Point start = new Point(-1, -1);
        private Point end = new Point(-1, -1);
        private double obstacleThickness = 5;
        private const double CIRCLEDIAMETER = 10;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawEllipse(Color color, Point whereat)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(color);
            ellipse.Fill = new SolidColorBrush(color);
            ellipse.Margin = new Thickness(whereat.X - CIRCLEDIAMETER / 2, whereat.Y - CIRCLEDIAMETER / 2, 0, 0);
            ellipse.Width = CIRCLEDIAMETER;
            ellipse.Height = CIRCLEDIAMETER;

            myCanvas.Children.Add(ellipse);
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch(state)
            {
                case ClickState.Obstacles:
                    startLine = e.GetPosition(myCanvas);
                    line = new Polyline();
                    line.Stroke = new SolidColorBrush(Colors.Black);
                    line.StrokeThickness = obstacleThickness;

                    myCanvas.Children.Add(this.line);
                    break;
                case ClickState.Start:
                    start = e.GetPosition(myCanvas);
                    state = ClickState.Obstacles;
                    redraw();
                    break;
                case ClickState.End:
                    end = e.GetPosition(myCanvas);
                    state = ClickState.Obstacles;
                    redraw();
                    break;
            }
        }

        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && line != null)
            {
                Point currentPoint = e.GetPosition(myCanvas);
                if (startLine != currentPoint)
                {
                    line.Points.Add(currentPoint);
                }
            }
        }
        private void myCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (line != null)
            {
                lines.Add(line);
                line = null;
            }
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            state = ClickState.Start;
            line = null;
        }

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            state = ClickState.End;
            line = null;
        }

        private void redraw()
        {
            myCanvas.Children.Clear();
            foreach(var i in lines)
            {
                myCanvas.Children.Add(i);
            }
            if(start.X > -1)
                DrawEllipse(Colors.Green, this.start);
            if (end.X > -1)
                DrawEllipse(Colors.Red, this.end);
        }

        private void Button_Click_Find_Path(object sender, RoutedEventArgs e)
        {
            redraw();
            int w = (int)myCanvas.RenderSize.Width;
            int h = (int)myCanvas.RenderSize.Height;

            var target = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(myCanvas);

            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();

            drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Point(w, h)));

            drawingContext.PushOpacityMask(brush);

            drawingContext.Close();

            target.Render(visual);

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));
            encoder.Save(stream);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);

            w = bitmap.Width;
            h = bitmap.Height;
            Console.WriteLine(w + " " + h);
            byte[,] obstacles = new byte[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    System.Drawing.Color c = bitmap.GetPixel(i, j);
                    if (c.R < 10 && c.G < 10 && c.B < 10)
                    {
                        obstacles[i, j] = 1;
                    }
                    else
                    {
                        obstacles[i, j] = 0;
                    }
                }
            }
            path(obstacles);
        }
        /******************************
         ******* TODO ASYNCHRONE ******
         ******************************/
        private async void path(byte[,] obstacles)
        {
            //List<Node> nodes = new List<Node>();
            Stack<int[]> path = new PathFinder().run(obstacles, new int[2] { (int)start.X, (int)start.Y }, new int[2] { (int)end.X, (int)end.Y }, myCanvas);
            Polyline pl = new Polyline();
            while (path.Count > 0)
            {
                int[] i = path.Pop();
                pl.Points.Add(new Point(i[0], i[1]));
            }

            pl.Stroke = new SolidColorBrush(Colors.Blue);
            pl.StrokeThickness = obstacleThickness;

            myCanvas.Children.Add(pl);
        }
    }
}
