using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace RetroJumpList
{
    public static class Program
    {
        [STAThread]
        static void Main() 
        {
            // Access command line arguments
            string[] args = Environment.GetCommandLineArgs();
            foreach (var a in args) { Debug.WriteLine($"Detected argument: {a}"); }

            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 1) // 1st arg is always the application path
                Application.Run(new AppContext(args));
            else
                Application.Run(new AppContext());
        }
        static void UnhandledException(object sender, UnhandledExceptionEventArgs e) => Console.WriteLine($"{(Exception)e.ExceptionObject}  IsTerminating:{e.IsTerminating}");
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => Console.WriteLine($"ThreadException:{e.Exception?.Message}");
    }
}
