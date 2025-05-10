using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.WiFi;
using PanTiltApp.AppConsole;
using PanTiltApp.Camera;
using PanTiltApp.Operate;
using PanTiltApp.Feedback;


namespace PanTiltApp
{
    public class MainPanel : UserControl
    {
        // private AppConsoleUI consoleUI;
        private readonly AppConsoleLogic console;
        private readonly AppConsoleUI consoleUI;
        public WiFiConnection? wifiLogic;
        // private RaspberryPiSSHClient? sshClient;
        // private IPConnectionHandler? connectionHandler;
        // private AppConsole? console;
        public WiFiConnectionUI? WiFiUI => wifiLogic?.UIInternal;
        private CameraUI? cameraUI;
        private CameraLogic? cameraLogic;

        private ControlUI? controlUI;
        private ControlLogic? controlLogic;

        private Feedback.FeedbackUI? feedbackUI;
        private Feedback.FeedbackLogic? feedbackLogic;



        public MainPanel(AppConsoleLogic appConsole)
        {
            console = appConsole;
            consoleUI = appConsole.UI;

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = ColorTranslator.FromHtml("#06402B"); // Green PCB-like background
            this.Padding = new Padding(10);

            // Main Layout (2 Rows: Sections, Console)
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Sekcje
            // mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Konsola (wysokość jak wcześniej)

            // Sections Panel (Stacked WiFi/Bluetooth + Camera + Stacked Turret)
            var sectionsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
            };

            sectionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // WiFi & Bluetooth
            sectionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Camera Feed
            sectionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Turret Control & Orientation

            // Row Proportions
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); // WiFi Label / Turret Control Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // WiFi Functionality / Turret Control Functionality
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); // Laser Label / Turret Orientation Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Laser Functionality / Turret Orientation Functionality

            // First Column (WiFi & Bluetooth Stacked)
            // wifiUI = new WiFiConnectionUI();
            wifiLogic = new WiFiConnection(console);

            cameraLogic = new CameraLogic(console);
            var ip = wifiLogic?.UIInternal?.IpAddressField.Text ?? "127.0.0.1";
            cameraLogic = new CameraLogic(console);
            cameraUI = new CameraUI(ip);
            cameraUI.InitializeLogic(cameraLogic, wifiLogic.UIInternal.IpAddressField);

            cameraUI.ConsolePrint += console.PrintMessage;
            wifiLogic.OnSSHConnected += ssh => cameraUI.SetSSHClient(ssh);


            // Laser Control Panel with 2 buttons
            var laserControlPanel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 2,
                Dock = DockStyle.Fill,
            };
            laserControlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            laserControlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            laserControlPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            laserControlPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Extra space margin

            var laserOnButton = new Button
            {
                Text = "Laser ON",
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Height = 50,
            };

            var laserOffButton = new Button
            {
                Text = "Laser OFF",
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
            };

            laserOnButton.FlatAppearance.BorderSize = 2;
            laserOffButton.FlatAppearance.BorderSize = 2;
            laserOnButton.FlatAppearance.BorderColor = Color.White;
            laserOffButton.FlatAppearance.BorderColor = Color.White;

            laserControlPanel.Controls.Add(laserOnButton, 0, 0);
            laserControlPanel.Controls.Add(laserOffButton, 1, 0);

            bool isLaserOn = false;
            laserOnButton.Enabled = true;
            laserOffButton.Enabled = false;

            laserOnButton.Click += (s, e) =>
            {
                if (!isLaserOn)
                {
                    isLaserOn = true;
                    laserOnButton.Enabled = false;
                    laserOffButton.Enabled = true;

                    console?.dispatcher?.SendLaserFrame(true);
                    console?.PrintMessage("Laser turned ON", "cyan");
                }
            };

            laserOffButton.Click += (s, e) =>
            {
                if (isLaserOn)
                {
                    isLaserOn = false;
                    laserOnButton.Enabled = true;
                    laserOffButton.Enabled = false;

                    console?.dispatcher?.SendLaserFrame(false);
                    console?.PrintMessage("Laser turned OFF", "cyan");
                }
            };


            controlLogic = new ControlLogic(console);
            controlUI = new ControlUI();
            controlLogic.ConnectJoystick(controlUI);


            feedbackLogic = new FeedbackLogic(console);
            feedbackUI = new FeedbackUI();


            sectionsPanel.Controls.Add(CreateSectionLabel("WiFi Connection"), 0, 0);
            sectionsPanel.Controls.Add(wifiLogic.UI, 0, 1);

            sectionsPanel.Controls.Add(CreateSectionLabel("Laser"), 0, 2);
            sectionsPanel.Controls.Add(laserControlPanel, 0, 3);

            sectionsPanel.Controls.Add(CreateSectionLabel("Camera Feed"), 1, 0);
            sectionsPanel.Controls.Add(cameraUI, 1, 1);
            sectionsPanel.SetRowSpan(cameraUI, 3);

            sectionsPanel.Controls.Add(CreateSectionLabel("Turret Control"), 2, 0);
            sectionsPanel.Controls.Add(controlUI, 2, 1);

            sectionsPanel.Controls.Add(CreateSectionLabel("Turret Orientation"), 2, 2);
            sectionsPanel.Controls.Add(feedbackUI, 2, 3);


            mainLayout.Controls.Add(sectionsPanel, 0, 0);

            // Console Panel
            var consolePanel = new TableLayoutPanel { Dock = DockStyle.Fill };
            consolePanel.Controls.Add(consoleUI);
            this.Controls.Add(mainLayout);
        }

        // Helper Method to Create Section Labels
        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
        }
    }
}
