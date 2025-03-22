using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PanTiltApp.Network;

namespace PanTiltApp
{
    public class ConnectionPanel : UserControl
    {
        private TextBox ipAddressField = new TextBox();
        private TextBox portNumberField = new TextBox();
        private Button connectButton = new Button();
        private Button disconnectButton = new Button();
        private Label statusLabel = new Label();
        private Button sshConnectButton = new Button();
        private Button sshDisconnectButton = new Button();
        private RaspberryPiSSHClient? sshClient;

        private IPConnectionHandler? connectionHandler;
        private AppConsole console;

        public ConnectionPanel(AppConsole appConsole)
        {
            console = appConsole;
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


            // mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Sections

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
            sectionsPanel.Controls.Add(CreateSectionLabel("WiFi Connection"), 0, 0);
            sectionsPanel.Controls.Add(CreateWiFiPanel(), 0, 1);
            sectionsPanel.Controls.Add(CreateSectionLabel("Bluetooth Connection"), 0, 2);
            sectionsPanel.Controls.Add(CreateBluetoothPanel(), 0, 3);

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
            // consolePanel.Controls.Add(console);
            console.Margin = new Padding(0, 0, 0, 20); // ⬅️ dolny margines 20px
            consolePanel.Controls.Add(console);

            mainLayout.Controls.Add(consolePanel, 0, 1);

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

        // WiFi Connection Section (With Connect/Disconnect Buttons)
        private TableLayoutPanel CreateWiFiPanel()
        {
            var wifiPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25)); // Buttons
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25)); // Buttons

            var ipPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            ipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            ipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var ipLabel = new Label { Text = "IP:", ForeColor = Color.White, Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            ipAddressField = new TextBox { Text = "192.168.0.178", Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            ipPanel.Controls.Add(ipLabel, 0, 0);
            ipPanel.Controls.Add(ipAddressField, 1, 0);

            var portPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            portPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            portPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var portLabel = new Label { Text = "Port:", ForeColor = Color.White, Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            portNumberField = new TextBox { Text = "5000", Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            portPanel.Controls.Add(portLabel, 0, 0);
            portPanel.Controls.Add(portNumberField, 1, 0);

            var buttonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            connectButton = new Button
            {
                Text = "Connect",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            connectButton.Click += async (s, e) => await Connect();

            disconnectButton = new Button
            {
                Text = "Disconnect",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Enabled = false
            };
            disconnectButton.Click += (s, e) => Disconnect();

            buttonPanel.Controls.Add(connectButton, 0, 0);
            buttonPanel.Controls.Add(disconnectButton, 1, 0);

            wifiPanel.Controls.Add(ipPanel);
            wifiPanel.Controls.Add(portPanel);
            wifiPanel.Controls.Add(buttonPanel);

            var sshButtonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            sshButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            sshButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            sshConnectButton = new Button
            {
                Text = "SSH Connect",
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill
            };
            sshConnectButton.Click += async (s, e) => await ConnectSSH();

            sshDisconnectButton = new Button
            {
                Text = "SSH Disconnect",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Enabled = false
            };
            sshDisconnectButton.Click += (s, e) => DisconnectSSH();

            sshButtonPanel.Controls.Add(sshConnectButton, 0, 0);
            sshButtonPanel.Controls.Add(sshDisconnectButton, 1, 0);

            // Dodajemy panel SSH poniżej przycisków połączenia IP
            wifiPanel.Controls.Add(sshButtonPanel);



            return wifiPanel;
        }

        // Placeholder Methods for Other Sections
        private TableLayoutPanel CreateBluetoothPanel() => CreatePlaceholderPanel();
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

        private async Task Connect()
        {
            connectionHandler = new IPConnectionHandler(ipAddressField.Text, int.Parse(portNumberField.Text));
            connectionHandler.ConsolePrint += console.PrintMessage;

            if (await connectionHandler.ConnectAsync())
            {
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
            }
        }

        private void Disconnect()
        {
            connectionHandler?.Close();
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
        }

        private async Task ConnectSSH()
        {
            string host = ipAddressField.Text; // Pobiera IP z pola tekstowego
            string username = "pan-tilt"; // Zmień, jeśli używasz innego użytkownika
            string password = "pan-tilt"; // Podaj właściwe hasło do Raspberry Pi

            sshClient = new RaspberryPiSSHClient(host, username, password);
            
            console.PrintMessage("Nawiązywanie połączenia SSH...");

            bool connected = await Task.Run(() => sshClient.Connect());

            if (connected)
            {
                console.PrintMessage("Połączono z Raspberry Pi przez SSH.");
                sshConnectButton.Enabled = false;
                sshDisconnectButton.Enabled = true;
                
                // Przeniesienie uruchomienia serwera do osobnego wątku
                await Task.Run(() =>
                {
                    sshClient.StartServer();
                });

                console.PrintMessage("Uruchomiono usługę serwera SSH na Raspberry Pi.");
            }
            else
            {
                console.PrintMessage("Nie udało się połączyć z Raspberry Pi przez SSH.");
            }
        }

        private void DisconnectSSH()
        {
            if (sshClient != null)
            {
                console.PrintMessage("Rozłączanie SSH...");
                sshClient.Disconnect();
                console.PrintMessage("Rozłączono z Raspberry Pi.");

                sshConnectButton.Enabled = true;
                sshDisconnectButton.Enabled = false;
            }
        }
    }
}
