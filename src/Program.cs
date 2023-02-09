using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace EQTool
{
    public class Program
    {
        private const string configFile = "EQTool.exe.config";

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                _ = OnResolveAssembly(null, new ResolveEventArgs("System.Threading.Tasks.Extensions"));
                AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
                //WriteSqlLiteDlls();
                if (!File.Exists(configFile))
                {
                    UpdateConfig(args);
                    Thread.Sleep(1000);
                    return;
                }
                else
                {
                    var fileondisk = File.ReadAllText(configFile);
                    if (fileondisk != Resources.App)
                    {
                        UpdateConfig(args);
                        Thread.Sleep(1000);
                        return;
                    }
                }

                App.Main();
            }

        private static void UpdateConfig(string[] args)
            {
                File.WriteAllText(configFile, Resources.App);
                var path = System.IO.Directory.GetCurrentDirectory() + "/EQTool.exe";
                _ = System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args.FirstOrDefault(),
                    UseShellExecute = true
                });
            }

            private static void WriteSqlLiteDlls()
            {
                var executingAssembly = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                var dlls = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("EQTool.runtimes.win10_x64", "runtimes\\win10-x64\\nativeassets\\uap10.0"),
                new KeyValuePair<string, string>("EQTool.runtimes.win10_x86", "runtimes\\win10-x86\\nativeassets\\uap10.0"),
                new KeyValuePair<string, string>("EQTool.runtimes.win_x64", "runtimes\\win-x64\\native"),
                new KeyValuePair<string, string>("EQTool.runtimes.win_x86", "runtimes\\win-x86\\native"),
            };

                foreach (var item in dlls)
                {
                    var found = executingAssembly.FirstOrDefault(a => a.StartsWith(item.Key));
                    var assemblyName = new AssemblyName("e_sqlite3");
                    var path = assemblyName.Name + ".dll";
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(found))
                    {
                        if (stream == null)
                        {
                            continue;
                        }

                        var assemblyRawBytes = new byte[stream.Length];
                        _ = stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);

                        try
                        {
                            _ = Directory.CreateDirectory(item.Value);
                            File.WriteAllBytes(Directory.GetCurrentDirectory() + "/" + item.Value + "/e_sqlite3.dll", assemblyRawBytes);
                        }
                        catch { }
                    }
                }

            }

            private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                var assemblyName = new AssemblyName(args.Name);
                var path = assemblyName.Name + ".dll";
                Debug.WriteLine($"Try Resolve {args.Name}");
                using (var stream = executingAssembly.GetManifestResourceStream(path))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    var assemblyRawBytes = new byte[stream.Length];
                    _ = stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                    if (args.Name.StartsWith("System.Threading.Tasks.Extensions"))
                    {
                        try
                        {
                            File.WriteAllBytes("System.Threading.Tasks.Extensions.dll", assemblyRawBytes);
                        }
                        catch { }
                    }
                    return Assembly.Load(assemblyRawBytes);
                }
            }
        }
    }
