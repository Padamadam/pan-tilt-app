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
        public RaspberryPiSSHClient? SSHClient => sshClient;
        public event Action<RaspberryPiSSHClient>? OnSSHConnected;
        private bool sshPreviouslyConnected = false;

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

            consolePrint?.Invoke("Inizjalizacja systemu głowicy...", "blue");

            bool connected = await Task.Run(() => sshClient.Connect());

            if (connected)
            {
                sshPreviouslyConnected = true;
                ui.SSHConnectButton.Text = "Reinitialize turret system";
                ui.SSHConnectButton.BackColor = Color.MediumBlue;

                OnSSHConnected?.Invoke(sshClient);

                await Task.Run(() =>
                {
                    sshClient.StartServer();
                });
                consolePrint?.Invoke("Uruchomiono serwer", "green");
            }

            else
            {
                consolePrint?.Invoke("Nie udało się zainicjalizować systemu.", "red");
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
                consolePrint?.Invoke($"IP = {ui.IpAddressField.Text}", "green");
            }
            catch (Exception ex)
            {
                consolePrint?.Invoke($"Błąd zapisu do config.ini: {ex.Message}", "red");
            }
        }
    }
}
