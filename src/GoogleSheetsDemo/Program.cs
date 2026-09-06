using System;
using System.Net;
using System.Windows.Forms;

namespace GoogleSheetsDemo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Google API endpoints require TLS 1.2; make sure it is enabled
            // even when the app runs on older machines.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
