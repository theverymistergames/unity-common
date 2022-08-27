using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Modules {

    public class TextConsoleModule : IConsoleModule {

        [ConsoleCommand("console/clear")]
        [ConsoleCommandHelp("clear console text")]
        public void ClearConsole() {
            ConsoleRunner.Instance.ClearConsole();
        }

        [ConsoleCommand("console/text/size")]
        [ConsoleCommandHelp("set console text field font size")]
        public void SetConsoleTextFontSize(float size) {
            if (size <= 0f) {
                ConsoleRunner.Instance.AppendLine("Font size must be bigger than 0");
                return;
            }

            ConsoleRunner.Instance.SetTextFieldFontSize(size);
        }

        [ConsoleCommand("console/input/size")]
        [ConsoleCommandHelp("set console input field font size")]
        public void SetConsoleInputFontSize(float size) {
            if (size <= 0f) {
                ConsoleRunner.Instance.AppendLine("Font size must be bigger than 0");
                return;
            }

            ConsoleRunner.Instance.SetTextInputFieldFontSize(size);
        }
    }
}
