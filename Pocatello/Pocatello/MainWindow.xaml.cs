using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pocatello
{
    enum ClickState { Obstacles, Start, End };

    public partial class MainWindow : Window
    {
        private Point startLine;
        private Polyline line;
        private ClickState state = ClickState.Obstacles;
        private Point start;
        private Point end;
        private double obstacleThickness = 5;
        private const double CIRCLEDIAMETER = 10;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawEllipse(Color color, Point whereat)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
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
                    this.startLine = e.GetPosition(myCanvas);
                    this.line = new Polyline();
                    this.line.Stroke = new SolidColorBrush(Colors.Black);
                    this.line.StrokeThickness = obstacleThickness;

                    myCanvas.Children.Add(this.line);
                    break;
                case ClickState.Start:
                    this.start = e.GetPosition(myCanvas);
                    DrawEllipse(Colors.Green, this.start);
                    this.state = ClickState.Obstacles;
                    break;
                case ClickState.End:
                    this.end = e.GetPosition(myCanvas);
                    DrawEllipse(Colors.Red, this.end);
                    this.state = ClickState.Obstacles;
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

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            this.state = ClickState.Start;
            line = null;
        }

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            this.state = ClickState.End;
            line = null;
        }

        private void Button_Click_Find_Path(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)myCanvas.RenderSize.Width,
                (int)myCanvas.RenderSize.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(myCanvas);
            
            using (System.IO.FileStream outStream = new System.IO.FileStream("./test.png", System.IO.FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(outStream);
            }
        }
    }
}
