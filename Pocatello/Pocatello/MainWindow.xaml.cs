using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System;

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

        private Stopwatch sw = new Stopwatch();
        public int AlgoIndex { get; set; }
        public int HeuristicIndex { get; set; }

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

        private System.Drawing.Bitmap CanvasToBitmap()
        {
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

            return new System.Drawing.Bitmap(stream);
        }

        private void myCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (state)
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
            foreach (var i in lines)
            {
                myCanvas.Children.Add(i);
            }
            if (start.X > -1)
                DrawEllipse(Colors.Green, this.start);
            if (end.X > -1)
                DrawEllipse(Colors.Red, this.end);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            lines = new List<Polyline>();
            start = new Point(-1, -1);
            end = new Point(-1, -1);
            redraw();
        }

        private void Button_Click_Bitmap(object sender, RoutedEventArgs e)
        {
            System.Drawing.Bitmap bitmap = CanvasToBitmap();
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Bitmap image (*.bmp)|*.bmp";
            dialog.DefaultExt = "bmp";
            if ((bool)dialog.ShowDialog())
            {
                bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void Button_Click_Find_Path(object sender, RoutedEventArgs e)
        {
            redraw();

            System.Drawing.Bitmap bitmap = CanvasToBitmap();

            int w = bitmap.Width;
            int h = bitmap.Height;

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
            BackgroundWorker bgw = new BackgroundWorker();

            bgw.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            bgw.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            bgw.WorkerReportsProgress = true;

            Label.Visibility = Visibility.Hidden;
            sw.Start();

            bgw.RunWorkerAsync(obstacles);
        }

        private void ComboBoxAlgo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlgoIndex = (sender as ComboBox).SelectedIndex;
        }
        private void ComboBoxHeuristic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HeuristicIndex = (sender as ComboBox).SelectedIndex;
        }

        private void ComboBoxAlgo_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("A*");
            data.Add("JPS");
            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            AlgoIndex = 0;
            comboBox.SelectedIndex = AlgoIndex;
        }
        private void ComboBoxHeuristic_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("Manhattan");
            data.Add("Dijkstra");
            data.Add("Euclid");
            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            HeuristicIndex = 0;
            comboBox.SelectedIndex = HeuristicIndex;
        }

        /*************
         *** ASYNC ***
         *************/

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PathFinder pf = new PathFinder();
            PathFinder.AlgoDelegate alg = null;
            PathFinder.HeuristicDelegate h = null;
            switch (AlgoIndex)
            {
                case 0:
                    alg = pf.lookForNeighbours;
                    break;
                case 1:
                    alg = pf.lookForNeighboursJPS;
                    break;
            }
            Console.WriteLine(HeuristicIndex);
            switch (HeuristicIndex)
            {
                case 0:
                    h = pf.HManhattan;
                    break;
                case 1:
                    h = pf.HDijkstra;
                    break;
                case 2:
                    h = pf.HEuclid;
                    break;
            }
            e.Result = pf.run((byte[,])e.Argument, new int[2] { (int)start.X, (int)start.Y }, new int[2] { (int)end.X, (int)end.Y },  alg, h, (BackgroundWorker)sender);
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            sw.Stop();
            var path = (Stack<int[]>) e.Result;
            Label.Visibility = Visibility.Visible;
            if (path.Count == 1)
            {
                Label.Content = "No path found in " + sw.ElapsedMilliseconds + " milliseconds";
            }
            else
            {
                Polyline pl = new Polyline();
                while (path.Count > 0)
                {
                    int[] i = path.Pop();
                    pl.Points.Add(new Point(i[0], i[1]));
                }

                pl.Stroke = new SolidColorBrush(Colors.Cyan);
                pl.StrokeThickness = obstacleThickness;

                myCanvas.Children.Add(pl);
                DrawEllipse(Colors.Green, start);
                DrawEllipse(Colors.Red, end);
                Label.Content = "Found path in " + sw.ElapsedMilliseconds + " milliseconds";
            }
            sw.Reset();
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var c = (int[]) e.UserState;
            var rekt = new Rectangle();

            rekt.Width = 3;
            rekt.Height = 3;
            rekt.Stroke = new SolidColorBrush(Colors.BlueViolet);
            rekt.Fill = new SolidColorBrush(Colors.BlueViolet);
            Canvas.SetLeft(rekt, c[0]);
            Canvas.SetTop(rekt, c[1]);

            myCanvas.Children.Add(rekt);
        }
    }
}
