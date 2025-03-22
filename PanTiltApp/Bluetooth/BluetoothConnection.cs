using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using PanTiltApp.AppConsole;

namespace PanTiltApp.Bluetooth
{
    public class BluetoothConnection
    {
        private readonly BluetoothConnectionUI ui;
        private readonly AppConsoleLogic console;

        public Control UI => ui.Panel;

        public BluetoothConnection(AppConsoleLogic console)
        {
            this.console = console;
            this.ui = new BluetoothConnectionUI();
            WireEvents();
        }

        private void WireEvents()
        {
            ui.ScanButton.Click += async (s, e) => await ScanForDevices();
            ui.ConnectButton.Click += async (s, e) => await ConnectToDevice();
        }

        private async Task ScanForDevices()
        {
            console.PrintMessage("Skanowanie urządzeń Bluetooth...");
            await Task.Delay(1000); // Placeholder
            console.PrintMessage("Znaleziono urządzenia: RaspberryPi_BT");
        }

        private async Task ConnectToDevice()
        {
            console.PrintMessage("Łączenie z urządzeniem Bluetooth...");
            await Task.Delay(1000); // Placeholder
            console.PrintMessage("Połączono przez Bluetooth.");
        }
    }
}
