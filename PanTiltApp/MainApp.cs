using System;
using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;

namespace PanTiltApp
{
    public class MainApp : Form
    {
        private AppConsoleLogic consoleLogic;
        // private AppConsoleUI consoleUI;


        public MainApp()
        {

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Pan-Tilt Connection Manager";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#06402B"); // Green PCB-like background
            
            this.Icon = new Icon("Assets/ikona.ico");


            var consoleUI = new AppConsoleUI();
            consoleLogic = new AppConsoleLogic(consoleUI);


            // Utwórz panel połączeń
            ConnectionPanel connectionPanel = new ConnectionPanel(consoleLogic)
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
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
            mainLayout.Padding = new Padding(20); // White padding

            var consolePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            // consoleUI.Dock = DockStyle.Fill;
            // consolePanel.Controls.Add(consoleUI);
            consoleLogic.UI.Dock = DockStyle.Fill;
            consolePanel.Controls.Add(consoleLogic.UI); 

            mainLayout.Controls.Add(connectionPanel, 0, 0);
            mainLayout.Controls.Add(consolePanel, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}
