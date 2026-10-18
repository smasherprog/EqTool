using EQTool.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace EQTool.Services
{
    public class UpdateRunner : IDisposable
    {
        private System.Timers.Timer timer;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly IAppDispatcher appDispatcher;
        private readonly LogParser logParser;
        private readonly LoggingService loggingService;

        public UpdateRunner(SpellWindowViewModel spellWindowViewModel, IAppDispatcher appDispatcher, LogParser logParser, LoggingService loggingService)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.loggingService = loggingService;
            this.appDispatcher = appDispatcher;
            this.logParser = logParser;
            timer = new System.Timers.Timer(1000 * 60);
            timer.Elapsed += UpdateTimer_Elapsed;
            timer.Enabled = true;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public int dwTime;
        }
        public static TimeSpan GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            return GetLastInputInfo(ref lastInPut)
                ? TimeSpan.FromMilliseconds(Environment.TickCount - lastInPut.dwTime)
                : TimeSpan.FromMinutes(20);
        }


        private bool updatecalled = false;
        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (updatecalled)
            {
                return;
            }
            updatecalled = true;
            try
            {
                var idletime = GetIdleTime();
                if (spellWindowViewModel?.SpellList.Count() < 2 && (DateTime.Now - logParser.LastYouActivity).TotalMinutes > 10 && idletime.TotalMinutes > 10)
                {
                    UpdateService.CheckForUpdates(App.Version, App.VersionType, appDispatcher, loggingService, false);
                }
            }
            finally
            {
                updatecalled = false;
            }
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }
    }
}
