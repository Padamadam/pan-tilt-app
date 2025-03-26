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


        public AppConsoleLogic(AppConsoleUI ui)
        {
            this.ui = ui;
            ui.CommandSubmitted += OnCommandSubmitted;
            ui.InputBox.KeyDown += HandleKeyDown;
        }

        public AppConsoleUI UI => ui;

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

            if (connectionHandler != null && connectionHandler.IsConnected)
            {
                return connectionHandler.Send(command); // << wysyła do RPi
            }

            switch (command.ToLower())
            {
                case "help":
                    return "Available commands: help, status, clear";
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
    }
}
