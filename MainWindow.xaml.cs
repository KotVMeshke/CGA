using ObjVisualizer.GraphicsComponents;
using ObjVisualizer.Parser;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Numerics;

namespace ObjVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int WindowWidth;
        private int WindowHeight;
        private Image Image;
        private DispatcherTimer timer;
        private TextBlock textBlock;
        private int FrameCount;
        private long PointsCount;
        private ObjReader ObjReader;
        public MainWindow()
        {
            ObjReader = ObjReader.GetObjReader("C:\\Users\\dimon\\OneDrive\\Рабочий стол\\Study\\Универ\\Курс 3\\Семестр 6\\АКГ\\Shrek.obj");

            InitializeComponent();
            InitializeWindowComponents();
            var thread = new Thread(Frame);
            thread.Start();

        }

        private void InitializeWindowComponents()
        {
            Application.Current.MainWindow.SizeChanged += Resize;
            WindowHeight = (int)this.Height;
            WindowWidth = (int)this.Width;
            Image = new Image();
            Image.Width = this.Width;
            Image.Height = this.Height;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            timer.Start();
            var grid = new Grid();
            Image.Stretch = Stretch.Fill;

            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Bottom;
            textBlock.FontSize = 12;
            textBlock.Foreground = Brushes.White;

            grid.Children.Add(Image);
            grid.Children.Add(textBlock);

            Grid.SetRow(Image, 0);
            Grid.SetColumn(Image, 0);
            Grid.SetZIndex(Image, 0);

            Grid.SetRow(textBlock, 0);
            Grid.SetColumn(textBlock, 0);
            Grid.SetZIndex(textBlock, 1);
            

            var matr = MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight);

            this.Content = grid;
           
        }

        private void Resize(object sender, SizeChangedEventArgs e)
        {

            Image.Width = (int)e.NewSize.Width;
            Image.Height = (int)e.NewSize.Height;

            WriteableBitmap writableBitmap = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            WindowHeight = (int)e.NewSize.Height;
            WindowWidth = (int)e.NewSize.Width;

        }


        private void Frame()
        {
            var Vertexes = ObjReader.Vertices.ToList();
            Camera camera = new Camera(new Vector3(0, 0, 5), new Vector3(0, 1, 0), new Vector3(0, 0, 0), 1.5f, 60, 1, 100);
            Matrix4x4 Result = new Matrix4x4();
            Vector4 Result2 = new Vector4();
            while (true)
            {
                PointsCount = 0;
                //await Task.Delay(1);
                WriteableBitmap writableBitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr24, null);
                Int32Rect rect = new Int32Rect(0, 0, WindowWidth, WindowHeight);
                writableBitmap.Lock();
                IntPtr buffer = writableBitmap.BackBuffer;
                int stride = writableBitmap.BackBufferStride;
                unsafe
                {
                    byte* pixels = (byte*)buffer.ToPointer();
                    for (int i = 0; i < 10000; i++)
                    {
                        Result = Matrix4x4.Multiply(MatrixOperator.GetProjectionMatrix(camera), MatrixOperator.GetViewPortMatrix(WindowWidth, WindowHeight));
                        Result = Matrix4x4.Multiply(MatrixOperator.GetViewMatrix(camera), Result);
                        Result2 = Vector4.Transform(new Vector4(1, 10, 100, 1), Result);
                        DrawLine(20, 20, 40 * 3, 40 * 3, pixels, stride);
                        DrawLine(40 * 3, 40 * 3, 10, 100, pixels, stride);
                        DrawLine(10, 100, 20, 20, pixels, stride);
                    }

                }
                writableBitmap.AddDirtyRect(rect);
                writableBitmap.Unlock();
                //Image.Source = writableBitmap;
                Application.Current.Dispatcher.Invoke(() =>
                Image.Source = writableBitmap);
                FrameCount++;
            }

        }

        public unsafe void DrawLine(int x0, int y0, int x1, int y1, byte* data, int stride)
        {

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                (x0, y0) = (y0, x0);
                (x1, y1) = (y1, x1);
            }

            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
                (y0, y1) = (y1, y0);
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            int val1;
            int val2;
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    val1 = x;
                    val2 = y;
                }
                else
                {
                    val1 = y;
                    val2 = x;
                }
                byte* pixelPtr = data + val1 * stride + val2 * 3;
                *(pixelPtr++) = 255;
                *(pixelPtr++) = 255;
                *(pixelPtr++) = 255;

                PointsCount++;
                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }

        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            textBlock.Text = FrameCount.ToString() + " fps";
            FrameCount = 0;
        }
    }
}