// using System.Drawing;
// using System.Windows.Forms;
// using PanTiltApp.AppConsole;

// namespace PanTiltApp
// {
//     public class AdvancedControlsPanel : UserControl
//     {
//         private BasicControlsTab basicControls;
//         private AppConsoleLogic console;

//         public AdvancedControlsPanel(AppConsoleLogic console)
//         {
//             this.basicControls = basicControls;
//             this.BackColor = ColorTranslator.FromHtml("#003300");
//             this.console = console;

//             var mainLayout = new TableLayoutPanel
//             {
//                 Dock = DockStyle.Fill,
//                 RowCount = 5,
//                 ColumnCount = 1,
//                 Padding = new Padding(20)
//             };

//             mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Label
//             mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));  // Grid
//             mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Label
//             mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Square
//             mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Circle

//             var movementLabel = CreateLabel("Detailed movement control");
//             var directionGrid = CreateDirectionGrid();
//             var sequenceLabel = CreateLabel("Motion sequences");
//             var btnSquare = CreateButton("Move in SQUARE");
//             var btnCircle = CreateButton("Move in CIRCLE");

//             mainLayout.Controls.Add(movementLabel, 0, 0);
//             mainLayout.Controls.Add(directionGrid, 0, 1);
//             mainLayout.Controls.Add(sequenceLabel, 0, 2);
//             mainLayout.Controls.Add(btnSquare, 0, 3);
//             mainLayout.Controls.Add(btnCircle, 0, 4);

//             this.Controls.Add(mainLayout);
//         }

//         private Button CreateButton(string text)
//         {
//             return new Button
//             {
//                 Text = text,
//                 Dock = DockStyle.Fill,
//                 Font = new Font("Courier New", 12, FontStyle.Bold),
//                 BackColor = Color.DarkOliveGreen,
//                 ForeColor = Color.White,
//                 FlatStyle = FlatStyle.Flat,
//                 Margin = new Padding(10),
//                 Height = 50
//             };
//         }

//         private Label CreateLabel(string text)
//         {
//             return new Label
//             {
//                 Text = text,
//                 ForeColor = Color.White,
//                 Font = new Font("Courier New", 12, FontStyle.Bold),
//                 TextAlign = ContentAlignment.MiddleCenter,
//                 Dock = DockStyle.Fill
//             };
//         }

//         private TableLayoutPanel CreateDirectionGrid()
//         {
//             var grid = new TableLayoutPanel
//             {
//                 RowCount = 3,
//                 ColumnCount = 3,
//                 Dock = DockStyle.Fill
//             };

//             for (int i = 0; i < 3; i++)
//             {
//                 grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
//                 grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
//             }

//             var btnUp = CreateButton("↑");
//             var btnDown = CreateButton("↓");
//             var btnLeft = CreateButton("←");
//             var btnRight = CreateButton("→");
//             var btnHome = CreateButton("HOME");

//             grid.Controls.Add(btnUp, 1, 0);
//             grid.Controls.Add(btnLeft, 0, 1);
//             grid.Controls.Add(btnHome, 1, 1);
//             grid.Controls.Add(btnRight, 2, 1);
//             grid.Controls.Add(btnDown, 1, 2);

//             return grid;
//         }
//     }
// }
