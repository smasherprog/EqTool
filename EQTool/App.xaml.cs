using EQTool.Models;
using EQTool.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EQTool
{
    public partial class App : Application
    {
        public static HttpClient httpclient = new HttpClient();

        private readonly Autofac.IContainer container1;

        private readonly AppRoot appRoot;
        private readonly WindowFactory windowFactory;

        private const string programName = "eqtool";
        private bool WaitForEQToolToStop()
        {
#if DEBUG
            return true;
#endif
            var counter = 0;
            int count;
            do
            {
                count = Process.GetProcessesByName(programName).Count();
                if (counter++ > 6)
                {
                    return false;
                }
                Debug.WriteLine($"Waiting for {programName} {count} on counter {counter}");
                Thread.Sleep(3000);
            }
            while (count != 1);
            return true;
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                appRoot?.LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException", Version);
            };

            DispatcherUnhandledException += (s, e) =>
            {
                appRoot?.LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException", Version);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                appRoot?.LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException", Version);
            };
        }

        private bool ShouldShutDownDueToNoWriteAccess()
        {
            try
            {
                File.Delete("test.json");
            }
            catch { }
            try
            {
                File.WriteAllText("test.json", "test");
            }
            catch (UnauthorizedAccessException)
            {
                _ = MessageBox.Show("Pigparse is running from a directory where it does not have permission to save settings. Please, move it to a folder where it can write!", "Pigparse Permissions!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            try
            {
                File.Delete("test.json");
            }
            catch { }
            var cwd = Directory.GetCurrentDirectory();
            if (cwd.ToLower().Contains("program files"))
            {
                _ = MessageBox.Show("Pigparse is running from a directory where it does not have permission to save settings. Please, move it to a folder where it can write!", "Pigparse Permissions!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            return false;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender1, cert, chain, sslPolicyErrors) => true;
            if (ShouldShutDownDueToNoWriteAccess())
            {
                App.Current.Shutdown();
                return;
            }
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SetupExceptionHandling();
            if (!WaitForEQToolToStop())
            {
                MessageBox.Show("Another Pigparse is currently running. You must shut that one down first!", "Multiple Pigparse running!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }
            try
            {
                var curr = Directory.GetCurrentDirectory();
                var path = Path.Combine(curr, "eqgame.exe");
                if (File.Exists(path))
                {
                    MessageBox.Show("Pigparse does not support running from in the EQ directory. Please move the pigparse and try again", "Pigparse Invalid Folder!", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Current.Shutdown();
                    return;
                }
            }
            catch { }
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            try
            {
                container = DI.Init();
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex, "DI.Init", null);
            }
            var updateservice = new UpdateService();
            var did_update = updateservice.ApplyUpdate(e.Args.FirstOrDefault());
            if (did_update == UpdateService.UpdateStatus.UpdatesApplied)
            {
                return;
            }
            else if (did_update == UpdateService.UpdateStatus.NoUpdateApplied)
            {
#if !DEBUG
                updateservice.CheckForUpdates(Version, VersionType, container, true);
#endif
            }
            try
            {
                InitStuff();
            }
            catch (Exception ex)
            {
                appRoot?.LogUnhandledException(ex, "InitStuff", AppRoot.Version);
                Thread.Sleep(1000 * 20);/// Sleep for 20 seconds here this will hopfully allow the update to occur and fix any problems
            }
        }

        public void UpdateBackgroundOpacity(string name, double opacity)
        {
            var newcolor = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a1919");
            newcolor.Opacity = opacity;
            var style = new System.Windows.Style { TargetType = typeof(Window) };
            style.Setters.Add(new Setter(Window.BackgroundProperty, newcolor));
            style.Setters.Add(new Setter(Window.FontSizeProperty, (double)EQToolSettings.FontSize.Value));
            App.Current.Resources[name] = style;
        }

        public void ApplyAlwaysOnTop()
        {
            appRoot.ApplyAlwaysOnTop();
        }
    }
}
