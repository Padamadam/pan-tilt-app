using PanTiltApp.AppConsole;

namespace PanTiltApp.Feedback
{
    public class FeedbackLogic
    {
        private readonly AppConsoleLogic console;

        public FeedbackLogic(AppConsoleLogic console)
        {
            this.console = console;
        }
    }
}
