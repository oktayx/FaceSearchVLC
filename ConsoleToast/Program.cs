using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Diagnostics;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

/// <summary>
/// Modified version of ConsoleToast in Github
/// Used to Windows 10 display notification from console app
/// </summary>
namespace ConsoleToast
{
    public class Program
    {
        public string ImagePath { get; set; }
        public string NotificationText { get; set; }
        private string NotificationTemplate { get; set; }

        public Program()
        {
            //Required to show notification
            ShortCutCreator.TryCreateShortcut("FaceSearch.App", "FaceSearch");

            //Load notification template from file
            NotificationTemplate = File.ReadAllText("toast1.xml");
        }

        static void Main(string[] args)
        {

        }

        public void ShowImageToastM(string appId, string title, string message, string image)
        {
            string modXml = NotificationTemplate.Replace("%EXEPATH%", Environment.CurrentDirectory);
            modXml = modXml.Replace("%TEXT%", message);
            modXml = modXml.Replace("%HEADER%", title);
            modXml = modXml.Replace("%IMAGE%", image);


            XmlDocument toastXml = new XmlDocument();

            toastXml.LoadXml(modXml);


            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            ToastEvents events = new ToastEvents() { Title = title };

            toast.Activated += events.ToastActivated;
            toast.Dismissed += events.ToastDismissed;
            toast.Failed += events.ToastFailed;

            // Show the toast. Be sure to specify the AppUserModelId
            // on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(appId).Show(toast);
        }

        class ToastEvents
        {
            public string Title { get; set; }

            internal void ToastActivated(ToastNotification sender, object e)
            {
                //Replace space for url
                Title = Title.Replace(" ", "%20");
                //Show title in chrome. Customize for other browser or url
                Process.Start("chrome.exe", $"https://www.imdb.com/find?ref_=nv_sr_fn&q={Title}&s=all");

                Console.WriteLine("User activated the toast");
            }

            internal void ToastDismissed(ToastNotification sender, ToastDismissedEventArgs e)
            {
                String outputText = "";
                switch (e.Reason)
                {
                    case ToastDismissalReason.ApplicationHidden:
                        outputText = "The app hid the toast using ToastNotifier.Hide";
                        break;
                    case ToastDismissalReason.UserCanceled:
                        outputText = "The user dismissed the toast";
                        break;
                    case ToastDismissalReason.TimedOut:
                        outputText = "The toast has timed out";
                        break;
                }

                Console.WriteLine(outputText);
            }

            internal void ToastFailed(ToastNotification sender, ToastFailedEventArgs e)
            {
                Console.WriteLine("The toast encountered an error.");
            }
        }

        static class ShortCutCreator
        {
            // In order to display toasts, a desktop application must have
            // a shortcut on the Start menu.
            // Also, an AppUserModelID must be set on that shortcut.
            // The shortcut should be created as part of the installer.
            // The following code shows how to create
            // a shortcut and assign an AppUserModelID using Windows APIs.
            // You must download and include the Windows API Code Pack
            // for Microsoft .NET Framework for this code to function

            internal static bool TryCreateShortcut(string appId, string appName)
            {
                String shortcutPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData) +
                    "\\Microsoft\\Windows\\Start Menu\\Programs\\" + appName + ".lnk";
                if (!File.Exists(shortcutPath))
                {
                    InstallShortcut(appId, shortcutPath);
                    return true;
                }
                return false;
            }

            static void InstallShortcut(string appId, string shortcutPath)
            {
                // Find the path to the current executable
                String exePath = Process.GetCurrentProcess().MainModule.FileName;
                IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

                // Create a shortcut to the exe
                VerifySucceeded(newShortcut.SetPath(exePath));
                VerifySucceeded(newShortcut.SetArguments(""));

                // Open the shortcut property store, set the AppUserModelId property
                IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

                using (PropVariant applicationId = new PropVariant(appId))
                {
                    VerifySucceeded(newShortcutProperties.SetValue(
                        SystemProperties.System.AppUserModel.ID, applicationId));
                    VerifySucceeded(newShortcutProperties.Commit());
                }

                // Commit the shortcut to disk
                IPersistFile newShortcutSave = (IPersistFile)newShortcut;

                VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
            }

            static void VerifySucceeded(UInt32 hresult)
            {
                if (hresult <= 1)
                    return;

                throw new Exception("Failed with HRESULT: " + hresult.ToString("X"));
            }
        }
    }
}