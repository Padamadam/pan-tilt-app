using PanTiltApp.AppConsole;
using PanTiltApp.Network;
using MjpegProcessor;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PanTiltApp.Camera
{
    public class CameraLogic
    {
        private readonly AppConsoleLogic console;
        private MjpegDecoder? decoder;
        private RaspberryPiSSHClient? sshClient;
        private readonly object frameLock = new();
        private Bitmap? latestFrame;
        private System.Windows.Forms.Timer? refreshTimer;
        private DateTime lastFrameTime = DateTime.MinValue;
        private PictureBox? videoBox;
        private string? cameraIp;
        private TextBox? ipSourceTextBox;


        public CameraLogic(AppConsoleLogic console)
        {
            this.console = console;
        }

        public void SetDependencies(PictureBox videoBox, TextBox ipSourceTextBox)
        {
            this.videoBox = videoBox;
            this.ipSourceTextBox = ipSourceTextBox;
        }


        public void SetSSHClient(RaspberryPiSSHClient client)
        {
            sshClient = client;
        }

        public void StartStream()
        {
            string? cameraIp = ipSourceTextBox?.Text;
            if (string.IsNullOrWhiteSpace(cameraIp))
            {
                console.PrintMessage("The camera IP address is empty - skipping stream startup.", "yellow");
                return;
            }

            if (sshClient == null || !sshClient.IsConnected)
            {
                console.PrintMessage("No SSH connection to Raspberry Pi. Cannot start the stream.", "red");
                return;
            }

            console.PrintMessage("Starting MJPEG server on Raspberry Pi...", "yellow");

            sshClient.ExecuteCommand("sudo pkill -f mjpeg_server.py");

            bool success = sshClient.ExecuteCommand("nohup python3 /home/pan-tilt/Documents/pan-tilt-rpi/mjpeg_server.py > /dev/null 2>&1 &");

            if (success)
            {
                console.PrintMessage("MJPEG server started. Initializing stream...", "green");
                InitializeStream();
            }
            else
            {
                console.PrintMessage("Failed to start MJPEG server.", "red");
            }
        }

        public void StartSlowStream()
        {
            string? cameraIp = ipSourceTextBox?.Text;
            if (string.IsNullOrWhiteSpace(cameraIp))
            {
                console.PrintMessage("The camera IP address is empty - skipping stream startup.", "yellow");
                return;
            }

            if (sshClient == null || !sshClient.IsConnected)
            {
                console.PrintMessage("No SSH connection to Raspberry Pi. Cannot start the stream.", "red");
                return;
            }

            console.PrintMessage("Starting MJPEG server (slow mode) on Raspberry Pi...", "yellow");

            sshClient.ExecuteCommand("sudo pkill -f mjpeg_server.py");
            sshClient.ExecuteCommand("sudo pkill -f mjpeg_server_slow.py");

            bool success = sshClient.ExecuteCommand("nohup python3 /home/pan-tilt/Documents/pan-tilt-rpi/mjpeg_server_slow.py > /dev/null 2>&1 &");

            if (success)
            {
                console.PrintMessage("MJPEG server (slow) started. Initializing stream...", "green");
                InitializeStream();
            }
            else
            {
                console.PrintMessage("Failed to start MJPEG server (slow).", "red");
            }
        }


        public void StopStream()
        {
            if (sshClient == null || !sshClient.IsConnected)
            {
                console.PrintMessage("No SSH connection to Raspberry Pi. Cannot stop the stream.", "red");
                return;
            }

            console.PrintMessage("Stopping MJPEG server...", "yellow");

            bool success = sshClient.ExecuteCommand("sudo pkill -f mjpeg_server.py");

            if (success)
            {
                console.PrintMessage("MJPEG server on Raspberry Pi stopped.", "green");
            }
            else
            {
                console.PrintMessage("Failed to stop MJPEG server.", "red");
            }
        }

        private void InitializeStream()
        {
            if (videoBox == null || string.IsNullOrWhiteSpace(cameraIp))
            {
                return;
            }

            decoder = new MjpegDecoder();

            decoder.FrameReady += (s, e) =>
            {
                var now = DateTime.Now;
                if ((now - lastFrameTime).TotalMilliseconds < 100)
                    return;

                lastFrameTime = now;

                try
                {
                    using var ms = new MemoryStream(e.FrameBuffer);
                    var bmp = new Bitmap(ms);

                    lock (frameLock)
                    {
                        latestFrame?.Dispose();
                        latestFrame = (Bitmap)bmp.Clone();
                    }
                }
                catch (Exception ex)
                {
                    console.PrintMessage($"MJPEG decoding error: {ex.Message}", "red");
                }
            };

            decoder.Error += (s, e) =>
            {
                console.PrintMessage($"MJPEG stream connection error: {e.Message}", "red");
            };

            try
            {
                string url = $"http://{cameraIp}:8080/stream.mjpeg";
                console.PrintMessage($"Connecting to the camera at: {url}", "blue");
                decoder.ParseStream(new Uri(url));
            }
            catch (Exception ex)
            {
                console.PrintMessage($"Camera connection error: {ex.Message}", "red");
            }

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 100;
            refreshTimer.Tick += (s, e) =>
            {
                lock (frameLock)
                {
                    if (latestFrame != null)
                    {
                        videoBox.Image?.Dispose();
                        videoBox.Image = (Bitmap)latestFrame.Clone();
                    }
                }
            };
            refreshTimer.Start();
        }
    }
}
