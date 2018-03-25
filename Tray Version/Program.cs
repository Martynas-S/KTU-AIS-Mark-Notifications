using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

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

            // Register AUMID and COM server (for Desktop Bridge apps, this no-ops)
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("Manomama7.KTU-AIS_Scraper");
            //Step 7
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();

            ToastIt();

            // Show the system tray icon.
            using (ProcessIcon pi = new ProcessIcon())
            {
                pi.Display();

                // Make sure the application runs!
                Application.Run();
            }
        }

        private static void ToastIt()
        {
            // Construct the visuals of the toast (using Notifications library)
            ToastContent toastContent = new ToastContent()
            {
                // Arguments when the user taps body of toast
                Launch = "action=viewConversation&conversationId=5",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                            new AdaptiveText()
                                {
                                Text = "Hello world!"
                                }
                            }
                    }
                }
            };

            // Create the XML document (BE SURE TO REFERENCE WINDOWS.DATA.XML.DOM)
            var doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            // And create the toast notification
            var toast = new ToastNotification(doc);

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

    }

    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("aaf32174-b6d7-45c4-94ce-8d2469c07916"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            // TODO: Handle activation
        }
    }
}
