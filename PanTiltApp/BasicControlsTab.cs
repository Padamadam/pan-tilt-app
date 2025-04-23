using System.Drawing;
using System.Windows.Forms;
using PanTiltApp.Network;
using PanTiltApp.AppConsole;

namespace PanTiltApp
{
    public class BasicControlsTab : TabPage
    {
        public AppConsoleLogic? consoleLogic;
        public AppConsoleUI? consoleUI;
        public MainPanel? mainPanel;
        public BasicControlsTab()
        {
            this.Text = "Basic Controls";
            this.BackColor = ColorTranslator.FromHtml("#003300");

            consoleUI = new AppConsoleUI();
            consoleLogic = new AppConsoleLogic(consoleUI);

            mainPanel = new MainPanel(consoleLogic)
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(20)
            };

            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var consolePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            consoleLogic.UI.Dock = DockStyle.Fill;
            consolePanel.Controls.Add(consoleLogic.UI);

            mainLayout.Controls.Add(mainPanel, 0, 0);
            mainLayout.Controls.Add(consolePanel, 0, 1);

            this.Controls.Add(mainLayout);
        }
    }
}
