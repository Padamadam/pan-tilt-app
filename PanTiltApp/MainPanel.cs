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
        public WiFiConnectionUI? WiFiUI => wifiLogic?.UIInternal;
        private CameraUI? cameraUI;
        private CameraLogic? cameraLogic;

        private ControlUI? controlUI;
        private ControlLogic? controlLogic;

        private Feedback.FeedbackUI? feedbackUI;
        private Feedback.FeedbackLogic? feedbackLogic;
        // private readonly BasicControlsTab basicControls;



        // public MainPanel(AppConsoleLogic appConsole, BasicControlsTab basicControls)
        // {
        //     console = appConsole;
        //     consoleUI = appConsole.UI;
        //     this.basicControls = basicControls;

        //     InitializeUI();
        // }
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
            var ip = wifiLogic?.UIInternal?.IpAddressField.Text ?? "192.168.128.88";
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
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Height = 50,
            };

            var laserOffButton = new Button
            {
                Text = "Laser OFF",
                BackColor = Color.Chocolate,
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

            var turretTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 10, FontStyle.Regular),
                Appearance = TabAppearance.Normal
            };

            // Zakładka Basic
            var basicTab = new TabPage("Basic") { BackColor = ColorTranslator.FromHtml("#003300") };
            basicTab.Controls.Add(controlUI);

            // Zakładka Detailed (przyciski kierunkowe)
            var detailedTab = new TabPage("Detailed") { BackColor = ColorTranslator.FromHtml("#003300") };
            detailedTab.Controls.Add(CreateDirectionGrid());

            // Zakładka Sequences (ruch po kwadracie / kole)
            var sequencesTab = new TabPage("Sequences") { BackColor = ColorTranslator.FromHtml("#003300") };
            sequencesTab.Controls.Add(CreateSequencesPanel());

            // Dodaj zakładki
            turretTabs.TabPages.Add(basicTab);
            turretTabs.TabPages.Add(detailedTab);
            turretTabs.TabPages.Add(sequencesTab);

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
            sectionsPanel.Controls.Add(turretTabs, 2, 1);


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

        private TableLayoutPanel CreateDirectionGrid()
        {
            var grid = new TableLayoutPanel
            {
                RowCount = 3,
                ColumnCount = 3,
                Dock = DockStyle.Fill
            };

            for (int i = 0; i < 3; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            }

            var upButton = CreateButton("up");
            var leftButton = CreateButton("left");
            var rightButton = CreateButton("right");
            var downButton = CreateButton("down");
            var homeButton = CreateButton("home");

            upButton.Click += (s, e) => controlLogic?.SendManualCommand(0x0C);
            downButton.Click += (s, e) => controlLogic?.SendManualCommand(0x0A);
            leftButton.Click += (s, e) => controlLogic?.SendManualCommand(0x03);
            rightButton.Click += (s, e) => controlLogic?.SendManualCommand(0x01);
            homeButton.Click += (s, e) => controlLogic?.StopMotion();

            grid.Controls.Add(upButton, 1, 0);
            grid.Controls.Add(leftButton, 0, 1);
            grid.Controls.Add(homeButton, 1, 1);
            grid.Controls.Add(rightButton, 2, 1);
            grid.Controls.Add(downButton, 1, 2);

            return grid;
        }


        private TableLayoutPanel CreateSequencesPanel()
        {
            var layout = new TableLayoutPanel
            {
                RowCount = 3,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            layout.Controls.Add(CreateButton("Move in quadtratic pattern"), 0, 0);
            layout.Controls.Add(CreateButton("Move in circular pattern"), 0, 1);
            layout.Controls.Add(new Panel(), 0, 2); // puste pole

            return layout;
        }

        private Button CreateButton(string text)
        {
            var button = new Button
            {
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                BackColor = Color.DarkOliveGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10),
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = text
            };

            string imagePath = $"Assets/{text.Replace(" ", "").ToLower()}.png";
            if (File.Exists(imagePath))
            {
                Image img = Image.FromFile(imagePath);
                button.Image = new Bitmap(img, new Size(48, 48)); // Ikona 48x48
                button.Text = ""; // Ukryj tekst
            }

            return button;
        }


    }
}
