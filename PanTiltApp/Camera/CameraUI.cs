using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MjpegProcessor;
using PanTiltApp.Network;

namespace PanTiltApp.Camera
{
    public class CameraUI : UserControl
    {
        private PictureBox videoBox;
        private Button startStreamButton;
        private Button stopStreamButton;
        private MjpegDecoder decoder;
        private RaspberryPiSSHClient? sshClient;
        private Bitmap? latestFrame;
        private readonly object frameLock = new();
        private System.Windows.Forms.Timer refreshTimer;
        private DateTime lastFrameTime = DateTime.MinValue;
        private CameraLogic? cameraLogic;

        public event Action<string, string>? ConsolePrint;

        public CameraUI(string ip)
        {
            InitializeUI();
        }

        public void SetSSHClient(RaspberryPiSSHClient client)
        {
            sshClient = client;
        }

        private void InitializeUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Obraz
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Pasek przycisków

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            videoBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black
            };

            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            startStreamButton = new Button
            {
                Text = "Start Stream",
                Dock = DockStyle.Fill,
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10, FontStyle.Bold)
            };
            startStreamButton.Click += StartStreamButton_Click;

            stopStreamButton = new Button
            {
                Text = "Stop Stream",
                Dock = DockStyle.Fill,
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10, FontStyle.Bold)
            };
            stopStreamButton.Click += StopStreamButton_Click;

            buttonPanel.Controls.Add(startStreamButton, 0, 0);
            buttonPanel.Controls.Add(stopStreamButton, 1, 0);

            layout.Controls.Add(videoBox, 0, 0);
            layout.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(layout);
            this.Dock = DockStyle.Fill;
        }

        public void InitializeLogic(CameraLogic logic, TextBox ipTextBox)
        {
            cameraLogic = logic;
            cameraLogic.SetDependencies(videoBox, ipTextBox);
        }

        private void StartStreamButton_Click(object? sender, EventArgs e)
        {
            cameraLogic?.StartStream();
        }

        private void StopStreamButton_Click(object? sender, EventArgs e)
        {
            cameraLogic?.StopStream();
        }
    }
}
