using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        const string TAG_TEXT = ">1920x1080</a>";
        const string IMAGE_SUFFIX = ".jpg";
        const string IMAGE_PREFIX = "https://media.magic.wizards.com/images/wallpaper/";
        const string WALLPAPER_PAGE = "https://magic.wizards.com/en/articles/media/wallpapers";

        static void Main(string[] args)
        {
            string htmlSource;
            int textIndex;
            int endIndex;
            int startIndex;
            string imageLink;
            string imagePath = Path.Combine(Path.GetTempPath(), "MtgWotwWallpaper.jpg");

            using (WebClient client = new WebClient())
            {
                htmlSource = client.DownloadString(WALLPAPER_PAGE);
                textIndex = htmlSource.IndexOf(TAG_TEXT, System.StringComparison.CurrentCultureIgnoreCase);
                endIndex = htmlSource.LastIndexOf(IMAGE_SUFFIX, textIndex, System.StringComparison.CurrentCultureIgnoreCase) + IMAGE_SUFFIX.Length;
                startIndex = htmlSource.LastIndexOf(IMAGE_PREFIX, endIndex, System.StringComparison.CurrentCultureIgnoreCase);
                imageLink = htmlSource.Substring(startIndex, endIndex - startIndex);
            }
            Console.WriteLine(imageLink);
            Console.ReadLine();
        }
    }
}
