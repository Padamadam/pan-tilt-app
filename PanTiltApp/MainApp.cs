using System;
using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.Network;

namespace PanTiltApp
{
    public class MainApp : Form
    {
        private AppConsole console;

        public MainApp()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Pan-Tilt Connection Manager";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#06402B"); // Green PCB-like background

            // Create console
            console = new AppConsole();

            // Create connection panel
            ConnectionPanel connectionPanel = new ConnectionPanel(console)
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20) // White padding
            };

            // Add elements to layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };

            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Wrap console inside a Panel to ensure it fully expands
            var consolePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            console.Dock = DockStyle.Fill;
            consolePanel.Controls.Add(console);

            mainLayout.Controls.Add(connectionPanel, 0, 0);
            mainLayout.Controls.Add(consolePanel, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}
