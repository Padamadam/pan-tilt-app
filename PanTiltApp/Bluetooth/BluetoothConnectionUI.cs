using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp.Bluetooth
{
    public class BluetoothConnectionUI
    {
        public Button ScanButton { get; private set; }
        public Button ConnectButton { get; private set; }

        public Control Panel { get; private set; }

        public BluetoothConnectionUI()
        {
            Panel = BuildUI();
        }

        private TableLayoutPanel BuildUI()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            ScanButton = new Button
            {
                Text = "Scan",
                BackColor = Color.DarkCyan,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            ConnectButton = new Button
            {
                Text = "Connect",
                BackColor = Color.Teal,
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            panel.Controls.Add(ScanButton, 0, 0);
            panel.Controls.Add(ConnectButton, 0, 1);

            return panel;
        }
    }
}
