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
        private WiFiConnection? wifiLogic;
        private RaspberryPiSSHClient? sshClient;
        private IPConnectionHandler? connectionHandler;
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
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); // WiFi Label / Turret Control Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // WiFi Functionality / Turret Control Functionality
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10)); // Bluetooth Label / Turret Orientation Label
            sectionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Bluetooth Functionality / Turret Orientation Functionality

            // First Column (WiFi & Bluetooth Stacked)
            // wifiUI = new WiFiConnectionUI();
            wifiLogic = new WiFiConnection(console);

            cameraLogic = new CameraLogic(console);
            cameraUI = new CameraUI();

            controlLogic = new ControlLogic(console);
            controlUI = new ControlUI();

            feedbackLogic = new FeedbackLogic(console);
            feedbackUI = new FeedbackUI();


            sectionsPanel.Controls.Add(CreateSectionLabel("WiFi Connection"), 0, 0);
            sectionsPanel.Controls.Add(wifiLogic.UI, 0, 1);


            sectionsPanel.Controls.Add(CreateSectionLabel("Camera Feed"), 1, 0);
            sectionsPanel.SetRowSpan(cameraUI, 4); // span przez 4 wiersze
            sectionsPanel.Controls.Add(cameraUI, 1, 1);


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
