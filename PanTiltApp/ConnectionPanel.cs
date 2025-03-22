using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.WiFi;
using PanTiltApp.Bluetooth;
using PanTiltApp.AppConsole;


namespace PanTiltApp
{
    public class ConnectionPanel : UserControl
    {
        // private AppConsoleUI consoleUI;
        private readonly AppConsoleLogic console;
        private readonly AppConsoleUI consoleUI;
        private BluetoothConnectionUI? bluetoothUI;
        private BluetoothConnection? bluetoothLogic;
        private WiFiConnectionUI? wifiUI;
        private WiFiConnection? wifiLogic;
        private RaspberryPiSSHClient? sshClient;
        private IPConnectionHandler? connectionHandler;
        // private AppConsole? console;

        public ConnectionPanel(AppConsoleLogic appConsole)
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
                RowCount = 3,
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Sekcje
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Konsola (wysokość jak wcześniej)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // MARGINES DOLNY

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
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // WiFi Label / Turret Control Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // WiFi Functionality / Turret Control Functionality
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Bluetooth Label / Turret Orientation Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Bluetooth Functionality / Turret Orientation Functionality

            // First Column (WiFi & Bluetooth Stacked)
            wifiUI = new WiFiConnectionUI();
            wifiLogic = new WiFiConnection(console);

            sectionsPanel.Controls.Add(CreateSectionLabel("WiFi Connection"), 0, 0);
            sectionsPanel.Controls.Add(wifiLogic.UI, 0, 1);

            bluetoothUI = new BluetoothConnectionUI();
            bluetoothLogic = new BluetoothConnection(console);

            sectionsPanel.Controls.Add(CreateSectionLabel("Bluetooth Connection"), 0, 2);
            sectionsPanel.Controls.Add(bluetoothLogic.UI, 0, 3);


            // Second Column (Camera Feed)
            sectionsPanel.Controls.Add(CreateSectionLabel("Camera Feed"), 1, 0);
            sectionsPanel.SetRowSpan(CreateCameraFeedPanel(), 4);
            sectionsPanel.Controls.Add(CreateCameraFeedPanel(), 1, 1);

            // Third Column (Turret Control & Orientation Stacked)
            sectionsPanel.Controls.Add(CreateSectionLabel("Turret Control"), 2, 0);
            sectionsPanel.Controls.Add(CreateTurretControlPanel(), 2, 1);
            sectionsPanel.Controls.Add(CreateSectionLabel("Turret Orientation"), 2, 2);
            sectionsPanel.Controls.Add(CreateTurretOrientationPanel(), 2, 3);

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

        private TableLayoutPanel CreateCameraFeedPanel() => CreatePlaceholderPanel();
        private TableLayoutPanel CreateTurretControlPanel() => CreatePlaceholderPanel();
        private TableLayoutPanel CreateTurretOrientationPanel() => CreatePlaceholderPanel();

        // Placeholder Panel for Unimplemented Sections
        private TableLayoutPanel CreatePlaceholderPanel()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill };
            var label = new Label
            {
                Text = "Coming Soon",
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(label);
            return panel;
        }
    }
}
