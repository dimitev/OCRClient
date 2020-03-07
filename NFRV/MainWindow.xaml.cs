using Microsoft.Win32;
using NFRV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Path = System.IO.Path;

namespace KZ
{

    public partial class MainWindow : Window
    {
        private const string tempFileName = "tempFile.png";
        System.Drawing.Bitmap input;
        System.Drawing.Bitmap output;
        private bool IsSelecting = false;
        Rectangle rect;
        Point startPoint;
        Point endPoint; 

        int startX = 0;
        int startY = 0;
        int endX = 0;
        int endY = 0;
        double imgScale = 1;
        OCRInterface ocrInterface = new OCRInterface();

        public MainWindow()
        {
            InitializeComponent();
            AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Canvas_MouseLeftButtonDown), true);
            AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Canvas_MouseLeftButtonUp), true);
            AddHandler(FrameworkElement.MouseMoveEvent, new MouseEventHandler(Canvas_MouseMove), true);
            this.bgRadBtn.IsChecked = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(canvas);
            // Do nothing it we're not selecting an area.
            if (!IsSelecting) return;
            // Save the new point.
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            
            /*if (pos.X < 0 || pos.Y < 0 || pos.X > input.Width * imgScale || pos.Y > input.Height * imgScale)
                return;*/
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;
            if (pos.X > input.Width * imgScale) pos.X = input.Width * imgScale;
            if (pos.Y > input.Height * imgScale) pos.Y = input.Height * imgScale;
                var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            endPoint.X = pos.X;
            endPoint.Y = pos.Y;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint=e.GetPosition(canvas);
            if ( input == null )
                return;
            if (currentPoint.X < 0 || currentPoint.Y < 0 || currentPoint.X > input.Width * imgScale || currentPoint.Y > input.Height * imgScale )
                return;
            startPoint = currentPoint;
            canvas.Children.Clear();
            rect = null;
            startPoint = e.GetPosition(canvas);
            

            IsSelecting = true;
            rect = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            canvas.Children.Add(rect);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSelecting = false;
            Point current = e.GetPosition(canvas);
            if (input == null)
                return;
            if (current.X < 0 || current.Y < 0 || current.X > input.Width * imgScale || current.Y > input.Height * imgScale)
            {
                return;
            }
            if(current == startPoint )
            {
                canvas.Children.Clear();
                rect = null;
            }

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                imagePath.Text = filename;
            }
            if (imagePath.Text == "")
            {
                return;
            }
            input = new System.Drawing.Bitmap(imagePath.Text);
            ReloadImage();
        }
        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            input.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
            this.ReloadImage();
        }
        private void ReloadImage()
        {
            selectedImg.Source = BitmapToImageSource(input);
            selectedImg.Stretch = Stretch.Uniform;

            imgScale = canvas.Width / input.Width;
            if (imgScale > canvas.Height / input.Height)
                imgScale = canvas.Height / input.Height;
            endX = (int)(input.Width * imgScale);
            endY = (int)(input.Height * imgScale);
        }
        private void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            prepareRegion();
            prepareOutput();
            System.Drawing.Rectangle cropArea = new System.Drawing.Rectangle(startX, startY, endX-startX, endY-startY);
            System.Drawing.Bitmap bmpImage = input;
            output = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            recognizeSelected_ClickAsync();
        }

        BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
        void prepareOutput()
        {
            if(output != null)output.Dispose();
            output = new System.Drawing.Bitmap(endX-startX, endY - startY);
        }
        void prepareRegion()
        {
            startX = 0;
            startY = 0;
            endX = input.Width;
            endY = input.Height;
            if (rect == null)
                return;
            startX = (int)(startPoint.X / imgScale);
            startY = (int)(startPoint.Y / imgScale);
            endX = (int)(endPoint.X / imgScale);
            endY = (int)(endPoint.Y / imgScale);
            if (startPoint.Y > endPoint.Y)
            {
                startY = (int)(endPoint.Y / imgScale);
                endY = (int)(startPoint.Y / imgScale);
            }
            if (startPoint.X > endPoint.X)
            {
                startX = (int)(endPoint.X / imgScale);
                endX = (int)(startPoint.X / imgScale);
            }
        }

        private void SaveExternal()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = "recognized.txt";
            save.Filter = "Text File | *.txt";
            if (save.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(save.OpenFile());
                writer.WriteLine(outputTxtBox.Text);
                writer.Dispose();
                writer.Close();
            }

        }

        private async Task recognizeSelected_ClickAsync()
        {
            ocrInterface.image = output;
            outputTxtBox.Text = ocrInterface.Recognize();
            if (externalFileCheckBox.IsChecked == true)
            {
                this.SaveExternal();
            }
        }
        
    }
}
