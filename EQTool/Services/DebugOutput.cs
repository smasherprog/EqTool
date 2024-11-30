using System;
using System.Diagnostics;
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
        public static extern bool AllocConsole();

        [DllImport("Kernel32", SetLastError = true)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);

        private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
        private bool ConsoleCreated = false;
        public bool LogMapping { get; set; }
        public bool LogSpells { get; set; }
        public void OpenConsole()
        {
            _ = FreeConsole();
            ConsoleCreated = AllocConsole();
            _ = AttachConsole(ATTACH_PARENT_PROCESS);
        }

        public void WriteLine(string text, OutputType outputType, MessageType messageType = MessageType.Informational,
            [System.Runtime.CompilerServices.CallerMemberName] string membName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            text = "Line: " + lineNumber + " | " + filePath.Split('\\').Last() + " | " + membName + ":\t" + text;
            if (!ConsoleCreated)
            {
                Debug.WriteLine(text);
                return;
            }

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
                Console.ForegroundColor = ConsoleColor.Cyan;
            }


            if (outputType == OutputType.Map && LogMapping)
            {
                Console.WriteLine(text);
            }

            if (outputType == OutputType.Spells && LogSpells)
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
