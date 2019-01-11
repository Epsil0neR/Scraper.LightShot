using System.IO;

namespace Scraper.LightShot
{
    public static class Helper
    {
        public static string DataFolder { get; }
        public static string HtmlUrlRoot { get; }
        public static string ImageUrlRoot { get; }

        public static string GetFilename(string name)
        {
            return Path.Combine(DataFolder, $"{name}.png");
        }

        public static string GetHtmlUrl(string name)
        {
            return $@"{HtmlUrlRoot}/{name}";
        }

        static Helper()
        {
            var t = typeof(Helper);
            var root = Directory.GetParent(t.Assembly.Location).FullName;
            DataFolder = Path.Combine(root, "Data");

            HtmlUrlRoot = @"https://prnt.sc";
            ImageUrlRoot = @"https://image.prntscr.com/image";
        }
    }
}