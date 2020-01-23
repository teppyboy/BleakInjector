using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Bleak;
using BleakInjector.Etc;
using BleakInjector.AdditionalInjector;
using Lunar;

namespace BleakInjector
{
    static class Program
    {
        private static readonly Status Status = new Status();
        public static string rootAppExe;

        public static bool isBypasserMode;
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    rootAppExe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                }
                else if (args[0].Contains("-rootAppName."))
                {
                    rootAppExe = args[0].Replace("-rootAppName.", "");
                    isBypasserMode = true;
                }
                else if (args[0].Contains("-clrtemp."))
                {
                    var tempfile = args[0].Replace("-clrtemp.", "");
                    Console.WriteLine("[DEBUG]: Temporary file: " + tempfile);
                    foreach (Process proc in Process.GetProcessesByName(tempfile))
                    {
                        proc.Kill();
                    }
                    Thread.Sleep(250);
                    File.Delete(tempfile);
                    Environment.Exit(0);
                }
                else if (args[0].Contains("-clrt3mp&lc."))
                {
                    var tempfile = args[0].Replace("-clrt3mp&lc.", "");
                    Console.WriteLine("[DEBUG]: Temporary file: " + tempfile);
                    foreach (Process proc in Process.GetProcessesByName(tempfile))
                    {
                        proc.Kill();
                    }
                    Thread.Sleep(250);
                    File.Delete(tempfile);
                    rootAppExe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                }
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                BleakMain f1 = new BleakMain();
                var currappname = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                if (currappname != rootAppExe)
                    f1.Text = GetUniqueKey(13);
                f1.Show();
                Console.WriteLine("[DEBUG]: Main file: " + rootAppExe);
                Application.Run(f1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured, sorry :(\nBelow is error i logged:\n" + ex,"BleakInjector");
                Environment.Exit(0);
            }
        }
        internal static readonly char[] chars =
"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
        public static Status Inject(Config config)
        {
            if (config.EraseHeaders)
            {
                switch (config.InjectionMethod)
                {
                    case "[BLEAK] CreateThread":
                        try
                        {
                            var Injekt = new Injector(config.ProcessName, config.DllPath, InjectionMethod.CreateThread);
                            Injekt.EjectDll();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Eject failed using Bleak-CreateThread.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.EraseHeadersOutcome = true;
                        break;

                    case "[BLEAK] HijackThread":
                        try
                        {
                            var Injekt2 = new Injector(config.ProcessName, config.DllPath, InjectionMethod.HijackThread);
                            Injekt2.EjectDll();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Bleak-HijackThread.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.EraseHeadersOutcome = true;
                        break;

                    case "[BLEAK] ManualMap":
                        try
                        {
                            var Injekt3 = new Injector(config.ProcessName, config.DllPath, InjectionMethod.ManualMap);
                            Injekt3.EjectDll();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Bleak-ManualMap.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.EraseHeadersOutcome = true;
                        break;
                    case "BasicInjector":
                        MessageBox.Show("This inject type dosen't support Eject.", "BleakInjector");
                        break;
                    case "[RI] Injector":
                        MessageBox.Show("This inject type dosen't support Eject.", "BleakInjector");
                        break;
                    case "[LUNAR] Injector":
                        try
                        {
                            foreach (Process proc in Process.GetProcessesByName(config.ProcessName))
                            {
                                var LibMapper = new LibraryMapper(proc, config.DllPath);
                                LibMapper.UnmapLibrary();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Lunar.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.EraseHeadersOutcome = true;
                        break;
                }
            }
            else
            {
                switch (config.InjectionMethod)
                {
                    case "[BLEAK] CreateThread":
                        try
                        {
                            var Injekt = new Injector(config.ProcessName, config.DllPath, InjectionMethod.CreateThread);
                            if (Injekt.InjectDll() != IntPtr.Zero)
                            {
                                Injekt.Dispose();
                                Status.InjectionOutcome = true;
                            }
                            else
                            {
                                MessageBox.Show("Inject failed using Bleak-CreateThread.\nError: Unknown.", "BleakInjector");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Bleak-CreateThread.\nError:\n{ex}", "BleakInjector");
                        }
                        break;

                    case "[BLEAK] HijackThread":
                        try
                        {
                            var Injekt2 = new Injector(config.ProcessName, config.DllPath, InjectionMethod.HijackThread);
                            if (Injekt2.InjectDll() != IntPtr.Zero)
                            {
                                Injekt2.Dispose();
                                Status.InjectionOutcome = true;
                            }
                            else
                            {
                                MessageBox.Show("Inject failed using Bleak-HijackThread.\nError: Unknown.", "BleakInjector");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Bleak-HijackThread.\nError:\n{ex}", "BleakInjector");
                        }
                        break;

                    case "[BLEAK] ManualMap":
                        try
                        {
                            var Injekt3 = new Injector(config.ProcessName, config.DllPath, InjectionMethod.ManualMap);
                            if (Injekt3.InjectDll() != IntPtr.Zero)
                            {
                                Injekt3.Dispose();
                                Status.InjectionOutcome = true;
                            }
                            else
                            {
                                MessageBox.Show("Inject failed using Bleak-ManualMap.\nError: Unknown.", "BleakInjector");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Bleak-ManualMap.\nError:\n{ex}", "BleakInjector");
                        }
                        break;
                    case "BasicInjector":
                        try
                        {
                            if (DllInjector.BasicInject(config.ProcessName, config.DllPath) == DllInjectionResult.Success)
                            {
                                Status.InjectionOutcome = true;
                            }
                            else
                            {
                                if (DllInjector.BasicInject(config.ProcessName, config.DllPath) == DllInjectionResult.DllNotFound)
                                {
                                    MessageBox.Show("Inject failed using BasicInjector.\nError: Dll not found.", "BleakInjector");
                                }
                                else if (DllInjector.BasicInject(config.ProcessName, config.DllPath) == DllInjectionResult.GameProcessNotFound)
                                {
                                    MessageBox.Show("Inject failed using BasicInjector.\nError: Target process isn't running.", "BleakInjector");
                                }
                                else if (DllInjector.BasicInject(config.ProcessName, config.DllPath) == DllInjectionResult.InjectionFailed)
                                {
                                    MessageBox.Show("Inject failed using BasicInjector.\nError: Unknown.", "BleakInjector");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using BasicInjector.\nError:\n{ex}", "BleakInjector");
                        }
                        break;
                    case "[RI] Injector":
                        try
                        {
                            foreach (Process proc in Process.GetProcessesByName(config.ProcessName))
                            {
                                var Injecc = new Reloaded.Injector.Injector(proc);
                                if (Injecc.Inject(config.DllPath) != 0)
                                    break;
                                else
                                    MessageBox.Show("Inject failed using Reloaded Injector.\nError: Unknown.", "BleakInjector");
                                Injecc.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Reloaded Injector.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.InjectionOutcome = true;
                        break;
                    case "[LUNAR] Injector":
                        try
                        {
                            foreach (Process proc in Process.GetProcessesByName(config.ProcessName))
                            {
                                var LibMapper = new LibraryMapper(proc, config.DllPath);
                                LibMapper.MapLibrary();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Inject failed using Lunar.\nError:\n{ex}", "BleakInjector");
                            break;
                        }
                        Status.InjectionOutcome = true;
                        break;
                }
                if (config.CloseAfterInject)
                {
                   Environment.Exit(0);
                }
            }
            return Status;
        }
    }
}
