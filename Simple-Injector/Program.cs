using System;
using System.Windows.Forms;
using Simple_Injection;
using BleakInjector.Etc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Diagnostics;
using BleakInjector.AdditionalInjector;

namespace BleakInjector
{
    public static class Program
    {
        private static readonly Injector Injector = new Injector();

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
                    foreach (Process proc in Process.GetProcessesByName(tempfile))
                    {
                        proc.Kill();
                    }
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
                    File.Delete(tempfile);
                    rootAppExe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                }
                //Application.SetHighDpiMode(HighDpiMode.SystemAware);
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
                MessageBox.Show("An error has occured, sorry :(\nIf you can launch this app with -CSole paramenter, reporduce this error then copy that and make a issue,Thank you.\nBelow is error i logged for who can't use -CSole paramenter.\n" + ex);
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
            // Inject using specified method

            switch (config.InjectionMethod)
            {
                case "[BLEAK] CreateThread":

                    if(Injector.CreateRemoteThread(config.DllPath, config.ProcessName))
                    {
                        Status.InjectionOutcome = true;
                    }

                    break;

                case "[BLEAK] HijackThread":

                    if (Injector.RtlCreateUserThread(config.DllPath, config.ProcessName))
                    {
                        Status.InjectionOutcome = true;
                    }

                    break;

                case "[BLEAK] ManualMap":

                    if (Injector.SetThreadContext(config.DllPath, config.ProcessName))
                    {
                        Status.InjectionOutcome = true;
                    }

                    break;
                case "BasicInjector":
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
                    break;
                case "[RI] Injector":
                    Process[] proc = Process.GetProcessesByName(config.ProcessName);
                    var Injecc = new Reloaded.Injector.Injector(proc[0]);
                    Injecc.Inject(config.DllPath);
                    Status.InjectionOutcome = true;
                    break;
            }

            // Erase headers if EraseHeaders is checked

            if (config.EraseHeaders)
            {
                if(Injector.EraseHeaders(config.DllPath, config.ProcessName))
                {
                    Status.EraseHeadersOutcome = true;
                }
            }

            // Close the program if CloseAfterInject is checked

            if (config.CloseAfterInject)
            {
                Application.Exit();
            }

            return Status;
        }
    }
}
