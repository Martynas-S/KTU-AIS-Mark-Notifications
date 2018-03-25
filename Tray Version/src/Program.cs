using System;
using System.Windows.Forms;

namespace Tray_Version
{
    // NOTE: https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast-desktop
    //       https://github.com/WindowsNotifications/desktop-toasts

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ToastNotificator.PrepareNotificationManager();
            ToastNotificator.SendToast(WebScraper.CheckForNewMarks());

            // Show the system tray icon.
            using (ProcessIcon pi = new ProcessIcon())
            {
                pi.Display();

                // Make sure the application runs!
                Application.Run();
            }            
        }        
    }    
}
