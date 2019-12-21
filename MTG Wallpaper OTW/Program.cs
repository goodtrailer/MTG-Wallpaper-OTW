using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using Microsoft.Win32.TaskScheduler;

namespace MTG_Wallpaper_OTW
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const string IMAGE_SUFFIX = "_1920x1080_wallpaper.jpg";
        const string IMAGE_PREFIX = "https://media.magic.wizards.com/images/wallpaper/";
        const string WALLPAPER_PAGE = "https://magic.wizards.com/en/articles/media/wallpapers";
        const string EXE_NAME = "MTG Wallpaper OTW.exe";
        const string TASK_NAME = "MTG Wallpaper OTW";
        const string DESCRIPTION = "Grabs new weekly Magic: The Gathering wallpaper from the official website and sets it as the desktop wallpaper.";
        const string SETUP_ARG = "setup";
        const string REMOVE_ARG = "remove";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                UpdateWallpaper();
            }
            else if (args[0].Equals(SETUP_ARG, StringComparison.CurrentCultureIgnoreCase))
            {
                SetupTask();
            }
            else if (args[0].Equals(REMOVE_ARG, StringComparison.CurrentCultureIgnoreCase))
            {
                RemoveTask();
            }
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

        static void SetupTask()
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Description = DESCRIPTION;
            // use daily in case of irregular wallpaper uploads
            taskDefinition.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today });
            taskDefinition.Settings.StartWhenAvailable = true;
            taskDefinition.Actions.Add(AppDomain.CurrentDomain.BaseDirectory + EXE_NAME, null, null);
            TaskService.Instance.RootFolder.RegisterTaskDefinition(TASK_NAME, taskDefinition);
            TaskService.Instance.FindTask(TASK_NAME).Run();
        }

        static void RemoveTask()
        {
            TaskService.Instance.RootFolder.DeleteTask(TASK_NAME, false);
        }
    }
}
