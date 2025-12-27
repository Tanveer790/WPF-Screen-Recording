using System;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;

namespace ScreenRecorderDemo
{
    public partial class MainWindow : Window
    {
        private VideoFileWriter writer;
        private DispatcherTimer timer;
        private int frameRate = 25;
        private string tempFile = "temp_demo.mp4";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            writer = new VideoFileWriter();
            writer.Open(
                tempFile,
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                frameRate,
                VideoCodec.MPEG4,
                5000000);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / frameRate);
            timer.Tick += Timer_Tick;
            timer.Start();

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            lblStatus.Text = "Status: Recording...";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            using (Bitmap bmp = new Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                writer.WriteVideoFrame(bmp);
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void StopRecording()
        {
            timer?.Stop();
            writer?.Close();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MP4 Video (*.mp4)|*.mp4",
                FileName = "ScreenRecording.mp4"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                if (File.Exists(tempFile))
                    File.Move(tempFile, saveFileDialog.FileName);

                lblStatus.Text = "Recording saved successfully";
            }
            else
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
    }
}
