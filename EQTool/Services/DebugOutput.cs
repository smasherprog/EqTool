using System;
using System.Linq;
using System.Runtime.InteropServices;

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
        public DebugOutput()
        {

        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32", SetLastError = true)]
        public static extern void FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);

        private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        public bool LogMapping { get; set; }
        public bool LogSpells { get; set; }
        public void OpenConsole()
        {
            FreeConsole();
            AllocConsole();
            _ = AttachConsole(ATTACH_PARENT_PROCESS);
        }

        public void WriteLine(string text, OutputType outputType, MessageType messageType = MessageType.Informational,
            [System.Runtime.CompilerServices.CallerMemberName] string membName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            var colorchanged = false;
            if (messageType == MessageType.Warning)
            {
                colorchanged = true;
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else if (messageType == MessageType.Success)
            {
                colorchanged = true;
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (messageType == MessageType.Error)
            {
                colorchanged = true;
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (messageType == MessageType.RemoteMessageReceived)
            {
                colorchanged = true;
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (messageType == MessageType.RemoteMessageSent)
            {
                colorchanged = true;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            }

            text = "Line: " + lineNumber + " | " + filePath.Split('\\').Last() + " | " + membName + ":\t" + text;
            if (outputType == OutputType.Map)
            {
                Console.WriteLine(text);
            }

            if (outputType == OutputType.Spells)
            {
                Console.WriteLine(text);
            }
            if (colorchanged)
            {
                Console.ResetColor();
            }
        }
    }
}
