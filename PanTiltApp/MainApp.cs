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
        private MainPanel mainPanel;



        public MainApp()
        {

            InitializeUI();
            // Automatyczne połączenie z RPi po uruchomieniu aplikacji
            // Task.Run(() =>
            // {
            //     var sshClient = new RaspberryPiSSHClient("192.168.1.100", "pi", "twoje_haslo"); // <-- Dostosuj dane
            //     bool connected = sshClient.Connect();

            //     if (connected)
            //     {
            //         sshClient.StartServer();
            //     }
            //     else
            //     {
            //         consoleLogic?.PrintMessage("Nie udało się połączyć z Raspberry Pi przez SSH.");
            //     }
            // });

            this.KeyPreview = true;
            this.KeyPress += (sender, e) =>
            {
                var wifiUI = mainPanel.WiFiUI;
                if (!char.IsControl(e.KeyChar) &&
                    !(wifiUI?.IpAddressField.Focused ?? false) &&
                    !(wifiUI?.PortNumberField.Focused ?? false))
                {
                    consoleLogic?.HandleGlobalKeyPress(e.KeyChar);
                }
            };
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
            mainPanel = new MainPanel(consoleLogic)
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

            mainLayout.Controls.Add(mainPanel, 0, 0);
            mainLayout.Controls.Add(consolePanel, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}
