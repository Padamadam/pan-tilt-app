using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace PanTiltApp.WiFi
{
    public class WiFiConnectionUI
    {
        public TextBox IpAddressField { get; private set; }
        public TextBox PortNumberField { get; private set; }
        public Button ConnectButton { get; private set; }
        public Button DisconnectButton { get; private set; }
        public Button SSHConnectButton { get; private set; }
        public Button SSHDisconnectButton { get; private set; }
        private const string ConfigFilePath = "config.ini";

        public Control Panel { get; private set; }

        public WiFiConnectionUI()
        {
            Panel = BuildUI(); // zamiast CreateUI()
        }

        private TableLayoutPanel BuildUI()
        {
            var wifiPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            wifiPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            var ipPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            ipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            ipPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var ipLabel = new Label { Text = "IP:", ForeColor = Color.White, Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            // IpAddressField = new TextBox { Text = "192.168.0.178", Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            IpAddressField = new TextBox { Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            ipPanel.Controls.Add(ipLabel, 0, 0);
            ipPanel.Controls.Add(IpAddressField, 1, 0);

            var portPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            portPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            portPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var portLabel = new Label { Text = "Port:", ForeColor = Color.White, Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            // PortNumberField = new TextBox { Text = "5000", Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            PortNumberField = new TextBox { Font = new Font("Courier New", 10), Dock = DockStyle.Fill };
            portPanel.Controls.Add(portLabel, 0, 0);
            portPanel.Controls.Add(PortNumberField, 1, 0);

            var buttonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            ConnectButton = new Button
            {
                Text = "Connect",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Courier New", 8, FontStyle.Bold),
                Dock = DockStyle.Fill,
            };

            DisconnectButton = new Button
            {
                Text = "Disconnect",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Courier New", 8, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Enabled = false,
            };

            buttonPanel.Controls.Add(ConnectButton, 0, 0);
            buttonPanel.Controls.Add(DisconnectButton, 1, 0);

            var sshButtonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            sshButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            sshButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            SSHConnectButton = new Button
            {
                Text = "SSH Connect",
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Font = new Font("Courier New", 8, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            SSHDisconnectButton = new Button
            {
                Text = "SSH Disconnect",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Courier New", 8, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Enabled = false
            };

            sshButtonPanel.Controls.Add(SSHConnectButton, 0, 0);
            sshButtonPanel.Controls.Add(SSHDisconnectButton, 1, 0);

            wifiPanel.Controls.Add(ipPanel);
            wifiPanel.Controls.Add(portPanel);
            wifiPanel.Controls.Add(buttonPanel);
            wifiPanel.Controls.Add(sshButtonPanel);

            return wifiPanel;
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var lines = File.ReadAllLines(ConfigFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("ip_address"))
                        IpAddressField.Text = line.Split('=')[1].Trim();
                    else if (line.StartsWith("port"))
                        PortNumberField.Text = line.Split('=')[1].Trim();
                }
            }
        }

        public void SetConnectedState()
        {
            ConnectButton.BackColor = Color.DarkOliveGreen; // wyszarzony zielony
            DisconnectButton.BackColor = Color.OrangeRed;   // jaskrawszy czerwony
        }

        public void SetDisconnectedState()
        {
            ConnectButton.BackColor = Color.Green;
            DisconnectButton.BackColor = Color.Red;
        }

        public bool IsAnyInputFieldFocused()
        {
            return IpAddressField.Focused || PortNumberField.Focused;
        }
    }
}
