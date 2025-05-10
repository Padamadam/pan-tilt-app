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
                console.PrintMessage("Adres IP kamery jest pusty - pomijam uruchamianie strumienia.", "yellow");
                return;
            }

            if (sshClient == null || !sshClient.IsConnected)
            {
                console.PrintMessage("Brak połączenia SSH z Raspberry Pi. Nie można uruchomić strumienia.", "red");
                return;
            }

            console.PrintMessage("Uruchamianie serwera MJPEG na Raspberry Pi...", "yellow");

            sshClient.ExecuteCommand("sudo pkill -f mjpeg_server.py");

            bool success = sshClient.ExecuteCommand("nohup python3 /home/pan-tilt/Documents/pan-tilt-rpi/mjpeg_server.py > /dev/null 2>&1 &");

            if (success)
            {
                console.PrintMessage("Serwer MJPEG uruchomiony. Inicjalizuję strumień...", "green");
                InitializeStream();
            }
            else
            {
                console.PrintMessage("Nie udało się uruchomić serwera MJPEG.", "red");
            }
        }

        public void StopStream()
        {
            if (sshClient == null || !sshClient.IsConnected)
            {
                console.PrintMessage("Brak połączenia SSH z Raspberry Pi. Nie można zatrzymać strumienia.", "red");
                return;
            }

            console.PrintMessage("Zatrzymywanie serwera MJPEG...", "yellow");

            bool success = sshClient.ExecuteCommand("sudo pkill -f mjpeg_server.py");

            if (success)
            {
                console.PrintMessage("Zatrzymano serwer MJPEG na Raspberry Pi.", "green");
            }
            else
            {
                console.PrintMessage("Nie udało się zatrzymać serwera MJPEG.", "red");
            }
        }

        private void InitializeStream()
        {
            if (videoBox == null || string.IsNullOrWhiteSpace(cameraIp))
            {
                console.PrintMessage("Brak danych wejściowych do uruchomienia strumienia.", "red");
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
                    console.PrintMessage($"Błąd dekodowania MJPEG: {ex.Message}", "red");
                }
            };

            decoder.Error += (s, e) =>
            {
                console.PrintMessage($"Błąd połączenia ze strumieniem MJPEG: {e.Message}", "red");
            };

            try
            {
                string url = $"http://{cameraIp}:8080/stream.mjpeg";
                console.PrintMessage($"Łączenie z kamerą pod adresem: {url}", "blue");
                decoder.ParseStream(new Uri(url));
            }
            catch (Exception ex)
            {
                console.PrintMessage($"Błąd połączenia z kamerą: {ex.Message}", "red");
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
