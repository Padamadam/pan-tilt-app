using System;
using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;
using PanTiltApp;

namespace PanTiltApp
{
    public class MainApp : Form
    {
        private MainPanel mainPanel;

        public MainApp()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Pan-Tilt Connection Manager";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#06402B"); 
            this.Icon = new Icon("Assets/ikona.ico");

            var consoleUI = new AppConsoleUI();
            var consoleLogic = new AppConsoleLogic(consoleUI);

            mainPanel = new MainPanel(consoleLogic)
            {
                Dock = DockStyle.Fill
            };

            // 🔁 Layout główny: 2 wiersze
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = ColorTranslator.FromHtml("#06402B")
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Górna część – main panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Dolna część – konsola

            // Konsola
            var consolePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            consoleUI.Dock = DockStyle.Fill;
            consolePanel.Controls.Add(consoleUI);

            // Dodajemy do layoutu
            mainLayout.Controls.Add(mainPanel, 0, 0);
            mainLayout.Controls.Add(consolePanel, 0, 1);

            this.Controls.Add(mainLayout);

            this.KeyPreview = true;
            this.KeyPress += (sender, e) =>
            {
                var wifiUI = mainPanel?.WiFiUI;
                var inputBox = consoleUI.InputBox;

                if (!char.IsControl(e.KeyChar) &&
                    !(wifiUI?.IpAddressField.Focused ?? false) &&
                    !(wifiUI?.PortNumberField.Focused ?? false))
                {
                    bool inputFocused = inputBox?.Focused ?? false;
                    consoleLogic.HandleGlobalKeyPress(e.KeyChar);
                    if (!inputFocused)
                        e.Handled = true;
                }
            };
        }
    }

}
