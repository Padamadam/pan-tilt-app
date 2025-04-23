using System;
using System.Drawing;
using System.Windows.Forms;

namespace PanTiltApp.AppConsole
{
    public class AppConsoleUI : UserControl
    {
        private const string Prompt = "> ";
        private const int PromptLength = 2; // długość "> "

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
        public RichTextBox MessageDisplay { get; private set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
        public RichTextBox InputBox { get; private set; }
        private bool isResettingText = false;
        private string lastValidInput = Prompt;


        private Panel consolePanel;

        public event EventHandler<string>? CommandSubmitted;

        public AppConsoleUI()
        {
            InitializeConsoleUI();
        }

        private void InitializeConsoleUI()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 150;
            this.BackColor = Color.Black;
            this.Load += (s, e) => InputBox.Focus();

            consolePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            InputBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Courier", 10, FontStyle.Regular),
                Multiline = false,
                ScrollBars = RichTextBoxScrollBars.None,
                WordWrap = false,
                Text = "",
            };

            InputBox.SelectionStart = InputBox.SelectionLength;

            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.Black,
                Padding = new Padding(10)
            };

            inputPanel.Controls.Add(InputBox);

            InputBox.KeyDown += InputBox_KeyDown;

            var messageDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Black,
            };

            MessageDisplay = new RichTextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BackColor = Color.Black,
                ForeColor = ColorTranslator.FromHtml("#F8F9FA"),
                Font = new Font("Courier", 10, FontStyle.Regular),
                WordWrap = true
            };

            messageDisplayPanel.Controls.Add(MessageDisplay);
            // MessageDisplay.MouseDown += (s, e) => InputBox.Focus();
            InputBox.GotFocus += InputBox_GotFocus;
            InputBox.MouseUp += InputBox_MouseUp;
            InputBox.KeyPress += InputBox_KeyPress;
            InputBox.TextChanged += InputBox_TextChanged;


            SetPrompt();
            InputBox.SelectionStart = InputBox.Text.Length;


            consolePanel.Controls.Add(inputPanel);
            consolePanel.Controls.Add(messageDisplayPanel);

            this.Controls.Add(consolePanel);
        }
        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string text = InputBox.Text;
                if (text.StartsWith(Prompt))
                {
                    string command = text.Substring(PromptLength).Trim();
                    if (!string.IsNullOrEmpty(command))
                    {
                        string timestamp = DateTime.Now.ToString("[HH:mm:ss]");
                        AppendColoredText($"{timestamp} {command}", Color.White);
                        CommandSubmitted?.Invoke(this, command);
                    }
                }

                SetPrompt();
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (InputBox.SelectionStart <= PromptLength)
                {
                    e.Handled = true; // Blokuj usuwanie prompta
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (InputBox.SelectionStart <= PromptLength)
                {
                    e.Handled = true; // Blokuj przesuwanie kursora przed prompt
                    e.SuppressKeyPress = true;
                }
            }
        }

        public void AppendColoredText(string message, Color color)
        {
            MessageDisplay.Invoke((MethodInvoker)delegate
            {
                // Tymczasowo pozwalamy na edycję
                MessageDisplay.ReadOnly = false;

                // Jeśli końcówka to pusty wiersz – usuń go bez utraty formatowania
                if (MessageDisplay.Text.EndsWith("\n"))
                {
                    int lastIndex = MessageDisplay.Text.LastIndexOf('\n');
                    if (lastIndex == MessageDisplay.Text.Length - 1 && lastIndex > 0)
                    {
                        MessageDisplay.Select(lastIndex, 1);
                        MessageDisplay.SelectedText = "";
                    }
                }

                MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
                MessageDisplay.SelectionLength = 0;
                MessageDisplay.SelectionColor = color;
                MessageDisplay.AppendText($"{message} \n");
                MessageDisplay.AppendText("\n");
                MessageDisplay.SelectionColor = MessageDisplay.ForeColor;
                MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
                MessageDisplay.ScrollToCaret();

                // Zablokuj edycję z powrotem
                MessageDisplay.ReadOnly = true;
            });
        }

        public void ScrollToBottom()
        {
            MessageDisplay.SelectionStart = MessageDisplay.Text.Length;
            MessageDisplay.ScrollToCaret();
        }

        public void SetInputFieldText(string text)
        {
            isResettingText = true;
            InputBox.Text = Prompt + text;
            InputBox.SelectionStart = InputBox.Text.Length;
            lastValidInput = InputBox.Text;
            isResettingText = false;
        }


        private void InputBox_GotFocus(object? sender, EventArgs e)
        {
            // Jeśli kursor przypadkiem znajduje się przed promptem, przesuń go
            if (InputBox.SelectionStart < Prompt.Length)
            {
                InputBox.SelectionStart = Prompt.Length;
            }
        }

        private void InputBox_MouseUp(object? sender, MouseEventArgs e)
        {
            // To zdarzenie wywołuje się po kliknięciu — daj szansę myszce ustawić pozycję, a potem popraw
            if (InputBox.SelectionStart < Prompt.Length)
            {
                InputBox.SelectionStart = Prompt.Length;
            }
        }

        private void InputBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Jeśli kursor wejdzie przed prompt – blokuj pisanie
            if (InputBox.SelectionStart < PromptLength)
            {
                e.Handled = true;
                InputBox.SelectionStart = PromptLength;
            }
        }

        private int CountPromptOccurrences(string input)
        {
            int count = 0, index = 0;
            while ((index = input.IndexOf(Prompt, index)) != -1)
            {
                count++;
                index += Prompt.Length;
            }
            return count;
        }
        private void InputBox_TextChanged(object? sender, EventArgs e)
        {
            if (isResettingText)
                return;

            string text = InputBox.Text;

            // Jeśli prompt zniknął lub jest powielony
            if (!text.StartsWith(Prompt) || CountPromptOccurrences(text) > 1)
            {
                string cleaned = text.Replace(Prompt, "");
                isResettingText = true;
                InputBox.Text = Prompt + cleaned;
                InputBox.SelectionStart = (Prompt + cleaned).Length;
                isResettingText = false;
                lastValidInput = InputBox.Text; // <-- aktualizuj TYLKO tutaj
                return;
            }

            // Jeśli tekst jest taki sam jak poprzednio – nie reaguj
            if (text == lastValidInput)
                return;

            // Jeśli kursor wchodzi przed prompt – przesuń
            if (InputBox.SelectionStart < PromptLength)
            {
                InputBox.SelectionStart = PromptLength;
            }

            // Na końcu: aktualizuj lastValidInput
            lastValidInput = text;
        }

        private void SetPrompt()
        {
            isResettingText = true;
            InputBox.Text = Prompt;
            InputBox.SelectionStart = PromptLength;
            lastValidInput = InputBox.Text;
            isResettingText = false;
        }
    }
}
