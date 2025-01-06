using EQTool.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services
{
    public enum OutputType
    {
        Map,
        Spells
    }

    public enum MessageType
    {
        Informational,
        Success,
        Warning,
        Error,
        RemoteMessageReceived,
        RemoteMessageSent
    }

    public class DebugOutput
    {
        private readonly ConsoleViewModel consoleViewModel;
        public DebugOutput(ConsoleViewModel consoleViewModel)
        {
            this.consoleViewModel = consoleViewModel;
        }
        public bool LogMapping { get; set; }
        public bool LogSpells { get; set; }
        public void WriteLine(string text, OutputType outputType, MessageType messageType = MessageType.Informational,
            [System.Runtime.CompilerServices.CallerMemberName] string membName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            text = "Line: " + lineNumber + " | " + filePath.Split('\\').Last() + " | " + membName + ":\t" + text;
            Debug.WriteLine(text);
            var color = Brushes.White;
            if (messageType == MessageType.Warning)
            {
                color = Brushes.Orange;
            }
            else if (messageType == MessageType.Success)
            {
                color = Brushes.Green;
            }
            else if (messageType == MessageType.Error)
            {
                color = Brushes.Red;
            }
            else if (messageType == MessageType.RemoteMessageReceived)
            {
                color = Brushes.DarkSalmon;
            }
            else if (messageType == MessageType.RemoteMessageSent)
            {
                color = Brushes.Cyan;
            }

            if (outputType == OutputType.Map && LogMapping)
            {
                consoleViewModel.WriteLine(text, color);
            }

            if (outputType == OutputType.Spells && LogSpells)
            {
                consoleViewModel.WriteLine(text, color);
            }
        }
    }
}
