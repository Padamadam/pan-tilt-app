using System;
using System.Drawing;

namespace PanTiltApp.AppConsole
{
    public class AppConsoleLogic
    {
        private readonly AppConsoleUI ui;
        public AppConsoleLogic(AppConsoleUI ui)
        {
            this.ui = ui;
            ui.CommandSubmitted += OnCommandSubmitted;
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
            string response = ExecuteCommand(command);
            ui.AppendColoredText(response, Color.Gray);
        }

        private string ExecuteCommand(string command)
        {
            ui.ScrollToBottom();
            
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
    }
}
