using System;
using System.Threading.Tasks;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;

namespace PanTiltApp.WiFi
{
    public class WiFiConnection
    {
        private readonly WiFiConnectionUI ui;
        private readonly AppConsoleLogic consoleLogic;

        private IPConnectionHandler? connectionHandler;
        private RaspberryPiSSHClient? sshClient;
        
        public Control UI => ui.Panel; // dostęp do kontrolki
        private const string ConfigFilePath = "config.ini";
        public WiFiConnectionUI UIInternal => ui;
        public event Action<string, string>? consolePrint;


        public WiFiConnection(AppConsoleLogic console)
        {
            this.ui = new WiFiConnectionUI();
            this.ui.LoadConfig();
            this.consolePrint += console.PrintMessage;
            this.consoleLogic = console;
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
            consolePrint?.Invoke("Łączenie przez IP...", "blue");

            string ip = ui.IpAddressField.Text;
            int port = int.Parse(ui.PortNumberField.Text);

            connectionHandler = new IPConnectionHandler(ip, port);
            connectionHandler.ConsolePrint += (msg, color) => consolePrint?.Invoke(msg, color);
            
            ui.DisconnectButton.Enabled = true;
            ui.SetConnectedState();

            if (await connectionHandler.ConnectAsync())
            {
                ui.ConnectButton.Enabled = false;
                consolePrint?.Invoke("Połączono przez IP.", "green");
                consoleLogic.SetConnectionHandler(connectionHandler);
            }
            else
            {
                consolePrint?.Invoke("Nie udało się połączyć.", "red");
            }
        }

        private void Disconnect()
        {
            connectionHandler?.Close();
            ui.ConnectButton.Enabled = true;
            ui.DisconnectButton.Enabled = false;
            // ui.SetDisconnectedState();
            consolePrint?.Invoke("Rozłączono.", "yellow");
        }

        private async Task ConnectSSH()
        {
            string host = ui.IpAddressField.Text;
            string username = "pan-tilt";
            string password = "pan-tilt";

            sshClient = new RaspberryPiSSHClient(host, username, password);
            sshClient.ConsolePrint += (msg, color) => consolePrint?.Invoke(msg, color);


            consolePrint?.Invoke("Nawiązywanie połączenia SSH...", "blue");

            bool connected = await Task.Run(() => sshClient.Connect());

            if (connected)
            {
                // consolePrint?.Invoke("Połączono przez SSH.");
                ui.SSHConnectButton.Enabled = false;
                ui.SSHDisconnectButton.Enabled = true;

                await Task.Run(() =>
                {
                    sshClient.StartServer();
                });

                consolePrint?.Invoke("Uruchomiono usługę serwera SSH.", "green");
            }
            else
            {
                consolePrint?.Invoke("Nie udało się połączyć przez SSH.", "red");
            }
        }

        private void DisconnectSSH()
        {
            consolePrint?.Invoke("Rozpoczynanie rozłączania z Raspberry Pi...", "blue");

            if (connectionHandler != null && connectionHandler.IsConnected)
            {
                connectionHandler.Close();
                ui.ConnectButton.Enabled = true;
                ui.DisconnectButton.Enabled = false;
                consolePrint?.Invoke("Połączenie IP rozłączone.", "blue");
            }

            if (sshClient != null && sshClient.IsConnected)
            {
                consolePrint?.Invoke("Rozłączanie SSH...", "blue");
                sshClient.Disconnect();
                consolePrint?.Invoke("Rozłączono połączenie SSH.", "blue");
            }

            ui.SSHConnectButton.Enabled = true;
            ui.SSHDisconnectButton.Enabled = false;

            consolePrint?.Invoke("Rozłączono z Raspberry Pi.", "blue");
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
                consolePrint?.Invoke($"IP = {ui.IpAddressField.Text}", "green");
            }
            catch (Exception ex)
            {
                consolePrint?.Invoke($"Błąd zapisu do config.ini: {ex.Message}", "red");
            }
        }
    }
}
