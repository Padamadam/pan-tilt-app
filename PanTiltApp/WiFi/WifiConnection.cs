using System;
using System.Threading.Tasks;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;

namespace PanTiltApp.WiFi
{
    public class WiFiConnection
    {
        private readonly WiFiConnectionUI ui;
        private readonly AppConsoleLogic console;

        private IPConnectionHandler? connectionHandler;
        private RaspberryPiSSHClient? sshClient;
        
        public Control UI => ui.Panel; // dostęp do kontrolki
        private const string ConfigFilePath = "config.ini";
        public WiFiConnectionUI UIInternal => ui;


        public WiFiConnection(AppConsoleLogic console)
        {
            this.console = console;
            this.ui = new WiFiConnectionUI();
            this.ui.LoadConfig();
            WireEvents();
        }


        private void WireEvents()
        {
            ui.ConnectButton.Click += async (s, e) => await Connect();
            ui.DisconnectButton.Click += (s, e) => Disconnect();

            ui.SSHConnectButton.Click += async (s, e) => await ConnectSSH();
            ui.SSHDisconnectButton.Click += (s, e) => DisconnectSSH();
        }

        private async Task Connect()
        {
            SaveConfig();
            console.PrintMessage("Łączenie przez IP...");

            string ip = ui.IpAddressField.Text;
            int port = int.Parse(ui.PortNumberField.Text);

            connectionHandler = new IPConnectionHandler(ip, port);
            connectionHandler.ConsolePrint += console.PrintMessage;
            
            ui.DisconnectButton.Enabled = true;
            // ui.SetConnectedState();

            if (await connectionHandler.ConnectAsync())
            {
                ui.ConnectButton.Enabled = false;
                console.PrintMessage("Połączono przez IP.");
                console.SetConnectionHandler(connectionHandler);

            }
            else
            {
                console.PrintMessage("Nie udało się połączyć.");
            }
        }

        private void Disconnect()
        {
            connectionHandler?.Close();
            ui.ConnectButton.Enabled = true;
            ui.DisconnectButton.Enabled = false;
            // ui.SetDisconnectedState();
            console.PrintMessage("Rozłączono.");
        }

        private async Task ConnectSSH()
        {
            string host = ui.IpAddressField.Text;
            string username = "pan-tilt";
            string password = "pan-tilt";

            sshClient = new RaspberryPiSSHClient(host, username, password);
            console.PrintMessage("Nawiązywanie połączenia SSH...");

            bool connected = await Task.Run(() => sshClient.Connect());

            if (connected)
            {
                console.PrintMessage("Połączono przez SSH.");
                ui.SSHConnectButton.Enabled = false;
                ui.SSHDisconnectButton.Enabled = true;

                await Task.Run(() =>
                {
                    sshClient.StartServer();
                });

                console.PrintMessage("Uruchomiono usługę serwera SSH.");
            }
            else
            {
                console.PrintMessage("Nie udało się połączyć przez SSH.");
            }
        }

        private void DisconnectSSH()
        {
            if (sshClient != null)
            {
                console.PrintMessage("Rozłączanie SSH...");
                sshClient.Disconnect();
                console.PrintMessage("Rozłączono z Raspberry Pi.");

                ui.SSHConnectButton.Enabled = true;
                ui.SSHDisconnectButton.Enabled = false;
            }
        }
        private void SaveConfig()
        {
            try
            {
                File.WriteAllLines(ConfigFilePath, new[]
                {
                    "[Network]",
                    $"ip_address = {ui.IpAddressField.Text}",
                    $"port = {ui.PortNumberField.Text}"
                });
                console.PrintMessage($"IP = {ui.IpAddressField.Text}");
            }
            catch (Exception ex)
            {
                console.PrintMessage($"Błąd zapisu do config.ini: {ex.Message}");
            }
        }
        
    }
}
