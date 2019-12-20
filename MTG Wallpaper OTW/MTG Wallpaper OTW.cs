using System;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using System.IO;

namespace MTG_Wallpaper_OTW
{
    public partial class MTG_Wallpaper_OTW : ServiceBase
    {
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

        const string IMAGE_SUFFIX = "_1920x1080_wallpaper.jpg";
        const string IMAGE_PREFIX = "https://media.magic.wizards.com/images/wallpaper/";
        static Timer timer;
        
        private int eventId = 0;

        public MTG_Wallpaper_OTW()
        {
            InitializeComponent();

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MtgWotwSource"))
            {
                EventLog.CreateEventSource("MtgWotwSource", "MtgWotwLog");
            }
            eventLog1.Source = "MtgWotwSource";
            eventLog1.Log = "MtgWotwLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("OnStart", EventLogEntryType.Information, eventId++);
            eventLog1.WriteEntry(Path.GetTempPath(), EventLogEntryType.Information, eventId++);

            SetupTimer();
            ChangeWallpaper();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("OnStop", EventLogEntryType.Information, eventId++);
        }

        protected void SetupTimer()
        {
            DateTime now = DateTime.Now;
            DateTime midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
            if (now > midnight) midnight = midnight.AddDays(1);
            double remainingInterval = (midnight - now).TotalMilliseconds;
            timer = new Timer(remainingInterval);
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            
        }

        protected void OnTimer(object sender, ElapsedEventArgs args)
        {
            timer.Stop();
            ChangeWallpaper();
            SetupTimer();
        }

        protected void ChangeWallpaper()
        {
            string htmlSource;
            int endIndex;
            int startIndex;
            string imageLink;
            string imagePath = Path.Combine(Path.GetTempPath(), "MtgWotwWallpaper.jpg");

            using (WebClient client = new WebClient())
            {
                htmlSource = client.DownloadString("https://magic.wizards.com/en/articles/media/wallpapers");
                endIndex = htmlSource.IndexOf(IMAGE_SUFFIX) + IMAGE_SUFFIX.Length;
                startIndex = htmlSource.LastIndexOf(IMAGE_PREFIX, endIndex);
                imageLink = htmlSource.Substring(startIndex, endIndex - startIndex);
                client.DownloadFile(imageLink, imagePath);
            }

            eventLog1.WriteEntry(SystemParametersInfo(0x0014, 0, "C:\\Users\\Alden\\Downloads\\MtgWotwWallpaper.jpg", 0x2).ToString(), EventLogEntryType.Information, eventId++);
            eventLog1.WriteEntry(imageLink, EventLogEntryType.Information, eventId++);
            eventLog1.WriteEntry(imagePath, EventLogEntryType.Information, eventId++);
        }
    }
}
