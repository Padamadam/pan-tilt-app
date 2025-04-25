using System;
using System.Drawing;
using PanTiltApp.Network;
using PanTiltApp.Communication;

namespace PanTiltApp.AppConsole
{
    public class AppConsoleLogic
    {
        private readonly AppConsoleUI ui;
        private IPConnectionHandler? connectionHandler;
        private readonly List<string> commandHistory = new();
        private int historyIndex = -1;
        public AppConsoleUI UI => ui;
        public CommandDispatcher? dispatcher;
        private int lastPitchVel = 0;
        private int lastPitchDir = 1;
        private int lastYawVel = 0;
        private int lastYawDir = 1;

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
                "purple" => Color.Purple,
                "cyan" => Color.Cyan,
                "gray" => Color.Gray,
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
            if (command.ToLower().StartsWith("move "))
            {
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
               if (parts.Length == 5 &&
                    ushort.TryParse(parts[1], out ushort pitchPos) &&
                    ushort.TryParse(parts[2], out ushort yawPos) &&
                    short.TryParse(parts[3], out short pitchSpeed) &&
                    short.TryParse(parts[4], out short yawSpeed))
                {
                    byte cmd = 0x00;
                    if (pitchSpeed < 0) cmd |= 0b1100;
                    if (yawSpeed   < 0) cmd |= 0b0011;

                    dispatcher?.SendDualServoFullFrame(
                        pitchPos,
                        yawPos,
                        (ushort)Math.Clamp((int)Math.Abs(pitchSpeed), 0, 4095),
                        (ushort)Math.Clamp((int)Math.Abs(yawSpeed), 0, 4095),
                        cmd
                    );

                    return $"[CMD] move → pitch: pos={pitchPos}, speed={pitchSpeed} | yaw: pos={yawPos}, speed={yawSpeed}";
                }
                else
                {
                    return "Usage: pitch|yaw [POSITION] [SPEED]";
                }
            }
            else if (command.ToLower() == "laser on")
            {
                dispatcher?.SendLaserFrame(true);
                return "Laser turned ON (binary frame)";
            }
            else if (command.ToLower() == "laser off")
            {
                dispatcher?.SendLaserFrame(false);
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
            connectionHandler = handler;
            dispatcher = new CommandDispatcher(handler, (msg, color) => PrintMessage(msg, color));
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
    }
}
