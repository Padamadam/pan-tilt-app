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
        private RaspberryPiSSHClient? sshClient;
        private CameraLogic? cameraLogic;
        private Button startSlowStreamButton;


        public event Action<string, string>? ConsolePrint;

        public CameraUI(string ip)
        {
            InitializeUI();
            ShowNoFeedMessage();
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
            videoBox.Paint += VideoBox_Paint;


            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            startStreamButton = new Button
            {
                Text = "Start Fast Stream",
                Dock = DockStyle.Fill,
                BackColor = Color.DarkViolet,
                ForeColor = Color.White,
                Font = new Font("Consolas", 8, FontStyle.Bold)
            };
            startStreamButton.Click += StartStreamButton_Click;

            startSlowStreamButton = new Button
            {
                Text = "Start Slow Stream",
                Dock = DockStyle.Fill,
                BackColor = Color.DarkTurquoise,
                ForeColor = Color.White,
                Font = new Font("Consolas", 8, FontStyle.Bold)
            };
            startSlowStreamButton.Click += StartSlowStreamButton_Click;

            stopStreamButton = new Button
            {
                Text = "Stop Stream",
                Dock = DockStyle.Fill,
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Consolas", 8, FontStyle.Bold)
            };
            stopStreamButton.Click += StopStreamButton_Click;

            buttonPanel.Controls.Add(startStreamButton, 0, 0);
            buttonPanel.Controls.Add(startSlowStreamButton, 1, 0);
            buttonPanel.Controls.Add(stopStreamButton, 2, 0);

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
            var result = MessageBox.Show(
                "Starting Fast Stream may slow down other application functions.\nDo you want to continue?",
                "Performance Warning",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.OK)
            {
                cameraLogic?.StartStream();
            }
            else
            {
                ConsolePrint?.Invoke("Starting Fast Stream was canceled by the user.", "yellow");
            }
        }

        private void StopStreamButton_Click(object? sender, EventArgs e)
        {
            cameraLogic?.StopStream();
            ShowNoFeedMessage();
        }

        private void StartSlowStreamButton_Click(object? sender, EventArgs e)
        {
            cameraLogic?.StartSlowStream();
        }

        private void ShowNoFeedMessage()
        {
            videoBox.Image?.Dispose();
            videoBox.Image = null;
            videoBox.Invalidate(); // wymuś ponowne przerysowanie
        }

        private void VideoBox_Paint(object? sender, PaintEventArgs e)
        {
            if (videoBox.Image == null)
            {
                string message = "No camera feed";
                using Font font = new("Consolas", 16, FontStyle.Bold);
                SizeF textSize = e.Graphics.MeasureString(message, font);
                float x = (videoBox.Width - textSize.Width) / 2;
                float y = (videoBox.Height - textSize.Height) / 2;

                e.Graphics.DrawString(message, font, Brushes.White, x, y);
            }
        }
    }
}
