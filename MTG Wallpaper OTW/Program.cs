using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;

namespace MTG_Wallpaper_OTW
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const string IMAGE_SUFFIX = "_1920x1080_wallpaper.jpg";
        const string IMAGE_PREFIX = "https://media.magic.wizards.com/images/wallpaper/";
        const string WALLPAPER_PAGE = "https://magic.wizards.com/en/articles/media/wallpapers";
        const string TASK_NAME = "MTG Wallpaper OTW";
        const string DESCRIPTION = "Grabs new weekly Magic: The Gathering wallpaper from the official website and sets it as the desktop wallpaper.";
        const string SETUP_ARG = "setup";
        const string REMOVE_ARG = "remove";

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    UpdateWallpaper();
                else if (args[0].Equals(SETUP_ARG, StringComparison.CurrentCultureIgnoreCase)
                        && args.Length == 4
                        && int.Parse(args[1]) <= 12 && int.Parse(args[1]) > 0
                        && int.Parse(args[2]) >= 0 && int.Parse(args[2]) < 60
                        && (args[3].Equals("am", StringComparison.CurrentCultureIgnoreCase)
                        || args[3].Equals("pm", StringComparison.CurrentCultureIgnoreCase)))
                    SetupTask(args[1], int.Parse(args[2]), args[3]);
                else if (args[0].Equals(SETUP_ARG, StringComparison.CurrentCultureIgnoreCase))
                    SetupTask();
                else if (args[0].Equals(REMOVE_ARG, StringComparison.CurrentCultureIgnoreCase))
                    RemoveTask();
            } catch (Exception) { SetupTask(); }
        }

        static void UpdateWallpaper()
        {
            string htmlSource;
            int endIndex;
            int startIndex;
            string imageLink;
            string imagePath = Path.Combine(Path.GetTempPath(), "MtgWotwWallpaper.jpg");

            using (WebClient client = new WebClient())
            {
                htmlSource = client.DownloadString(WALLPAPER_PAGE);
                endIndex = htmlSource.IndexOf(IMAGE_SUFFIX, System.StringComparison.CurrentCultureIgnoreCase) + IMAGE_SUFFIX.Length;
                startIndex = htmlSource.LastIndexOf(IMAGE_PREFIX, endIndex, System.StringComparison.CurrentCultureIgnoreCase);
                imageLink = htmlSource.Substring(startIndex, endIndex - startIndex);
                client.DownloadFile(imageLink, imagePath);
            }
            SystemParametersInfo(0x14, 0, imagePath, 0x1 | 0x2);
        }

        static void SetupTask(string hr = "12", int min = 0, string meridiem = "am")
        {
            EventLog eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MtgWotwSource"))
            {
                EventLog.CreateEventSource("MtgWotwSource", "MtgWotwLog");
            }
            eventLog1.Source = "MtgWotwSource";
            eventLog1.Log = "MtgWotwLog";
            eventLog1.WriteEntry(hr + ":" + min + " " + meridiem);
            
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Description = DESCRIPTION;

            // use daily in case of irregular wallpaper uploads
            taskDefinition.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Parse(hr + " " + meridiem.ToUpper()).AddMinutes(min) });
            taskDefinition.Triggers.Add(new LogonTrigger { UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name });
            taskDefinition.Settings.StartWhenAvailable = true;

            taskDefinition.Actions.Add(System.Reflection.Assembly.GetEntryAssembly().Location, null, null);
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TASK_NAME, taskDefinition);
            TaskService.Instance.FindTask(TASK_NAME).Run();
        }

        static void RemoveTask()
        {
            TaskService.Instance.RootFolder.DeleteTask(TASK_NAME, false);
        }
    }
}
