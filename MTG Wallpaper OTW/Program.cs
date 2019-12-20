using System.ServiceProcess;

namespace MTG_Wallpaper_OTW
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MTG_Wallpaper_OTW()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
