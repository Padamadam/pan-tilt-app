using System;
using System.Drawing;
using PanTiltApp.Network;

namespace PanTiltApp.AppConsole
{
    public class AppConsoleLogic
    {
        private readonly AppConsoleUI ui;
        private IPConnectionHandler? connectionHandler;
        private readonly List<string> commandHistory = new();
        private int historyIndex = -1;
        public AppConsoleUI UI => ui;


        public AppConsoleLogic(AppConsoleUI ui)
        {
            this.ui = ui;
            ui.CommandSubmitted += OnCommandSubmitted;
            ui.InputBox.KeyDown += HandleKeyDown;
        }


        public void PrintMessage(string message, string color = "white")
        {
            Color messageColor = color.ToLower() switch
            {
                "green" => Color.Green,
                "yellow" => Color.Yellow,
                "red" => Color.Red,
                _ => Color.White
            };

            ui.AppendColoredText(message, messageColor);
        }

        private void OnCommandSubmitted(object? sender, string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                commandHistory.Add(command);
                historyIndex = commandHistory.Count;
            }


            string response = ExecuteCommand(command);
            ui.AppendColoredText(response, Color.Gray);
        }

        private string ExecuteCommand(string command)
        {
            ui.ScrollToBottom();

            // Obsługa poleceń pitch/yaw → ID1/ID2
            if (command.ToLower().StartsWith("pitch ") || command.ToLower().StartsWith("yaw "))
            {
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 &&
                    int.TryParse(parts[1], out int position) &&
                    int.TryParse(parts[2], out int speed))
                {
                    int id = command.ToLower().StartsWith("pitch") ? 1 : 2;
                    SendServoFrame(id, position, speed);
                    return $"Command sent: {(id == 1 ? "Pitch" : "Yaw")} → Pos {position}, Speed {speed}";
                }
                else
                {
                    return "Usage: pitch|yaw [POSITION] [SPEED]";
                }
            }
            else if (command.ToLower() == "laser on")
            {
                SendLaserFrame(true);
                return "Laser turned ON (binary frame)";
            }
            else if (command.ToLower() == "laser off")
            {
                SendLaserFrame(false);
                return "Laser turned OFF (binary frame)";
            }


            // // Gdy połączono – prześlij jako tekst
            // if (connectionHandler != null && connectionHandler.IsConnected)
            // {
            //     return connectionHandler.Send(command);
            // }

            // Pozostałe lokalne komendy
            switch (command.ToLower())
            {
                case "help":
                    return "Available commands: help, status, clear, pitch [pos] [speed], yaw [pos] [speed]";
                case "status":
                    return "System status: All systems operational.";
                case "clear":
                    ui.MessageDisplay.Clear();
                    return "";
                default:
                    return $"'{command}' is not recognized as a valid command.";
            }
        }


        public void SetConnectionHandler(IPConnectionHandler handler)
        {
            this.connectionHandler = handler;
        }

        public void HandleKeyDown(object? sender, KeyEventArgs e)
        {
            if (commandHistory.Count == 0)
                return;

            if (e.KeyCode == Keys.Up)
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    ui.SetInputFieldText(commandHistory[historyIndex]);
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    ui.SetInputFieldText(commandHistory[historyIndex]);
                }
                else
                {
                    historyIndex = commandHistory.Count;
                    ui.SetInputFieldText("");
                }
                e.Handled = true;
            }
        }

        public void HandleGlobalKeyPress(char keyChar)
        {
            // Jeśli InputBox już ma fokus, nie rób nic – system sam doda znak
            if (ui.InputBox.Focused)
                return;

            ui.InputBox.Focus();

            // Przenieś kursor za prompt jeśli trzeba
            if (ui.InputBox.SelectionStart < 2)
                ui.InputBox.SelectionStart = 2;

            // Ręcznie dodaj znak (bo InputBox wcześniej nie miał fokusu)
            int pos = ui.InputBox.SelectionStart;
            ui.InputBox.Text = ui.InputBox.Text.Insert(pos, keyChar.ToString());
            ui.InputBox.SelectionStart = pos + 1;
        }

        public void SendServoFrame(int id, int position, int speed)
        {
            if (connectionHandler == null || !connectionHandler.IsConnected)
            {
                PrintMessage("Connection not established. Cannot send command.", "red");
                return;
            }

            byte[] frame = new byte[8];
            frame[0] = 0xAA;
            frame[1] = (byte)id;

            // Ustalamy typ komendy
            byte command = (byte)(position >= 0 ? 0x01 : 0x02);  // 0x01 - obrót w jedną stronę, 0x02 - w drugą
            frame[2] = command;

            short absPos = (short)Math.Abs(position); // zawsze dodatnie

            byte[] posBytes = BitConverter.GetBytes(absPos);
            frame[3] = posBytes[0];
            frame[4] = posBytes[1];

            byte[] spdBytes = BitConverter.GetBytes((short)speed);
            frame[5] = spdBytes[0];
            frame[6] = spdBytes[1];

            frame[7] = 0x55;

            connectionHandler.Send(frame);
            PrintMessage($"Sent Binary Frame: ID={id}, CMD={command} POS={absPos}, SPD={speed}", "green");
        }


        public void SendLaserFrame(bool on)
        {
            if (connectionHandler == null || !connectionHandler.IsConnected)
            {
                PrintMessage("Connection not established. Cannot send laser command.", "red");
                return;
            }

            byte[] frame = new byte[8];
            frame[0] = 0xAA;
            frame[1] = 0x32;        // laser device ID
            frame[2] = 0x01;        // command type
            frame[3] = on ? (byte)0x01 : (byte)0x00; // position = 1 → ON, 0 → OFF
            frame[4] = 0x00;
            frame[5] = 0x00;
            frame[6] = 0x00;
            frame[7] = 0x55;

            connectionHandler.Send(frame);
            PrintMessage($"Sent Laser Frame: {(on ? "ON" : "OFF")}", "green");
        }
    }
}
