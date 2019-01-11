using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace Scraper.LightShot
{
    public class Scraper : IDisposable
    {
        private List<Regex> _regexes;
        private CancellationTokenSource _ctSource;
        private CancellationToken _ct;
        private HttpClient _http;

        public Scraper()
        {
            _ctSource = new CancellationTokenSource();
            _ct = _ctSource.Token;

            _regexes = new List<Regex>()
            {
                new Regex(@"https://image.prntscr.com/image/([A-Za-z0-9\-_]+).png"),
                new Regex(@"https://i.imgur.com/([A-Za-z0-9\-_]+).png"),
            };

            Directory.CreateDirectory(Helper.DataFolder);
            _http = GetHttpClient();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _ctSource.Cancel();
        }


        public void Scrap()
        {
            var name = "m5p54s";
            var fn = Path.Combine(Helper.DataFolder, name);
            var url = Helper.GetHtmlUrl(name);
            var httpClient = GetHttpClient();
            var webClient = new WebClient();

            var get = httpClient.GetAsync(url);
            get.ContinueWith(async task =>
            {
                var html = await task.Result.Content.ReadAsStringAsync();
                Match match;
                foreach (var regex in _regexes)
                {
                    match = regex.Match(html);
                    if (!string.IsNullOrWhiteSpace(match?.Value))
                    {
                        webClient.DownloadFile(match.Value, $"{fn}.png");
                        break;
                    }
                }
            }, _ct).Wait(_ct);
        }

        private HttpClient GetHttpClient()
        {
            var rv = new HttpClient();
            rv.DefaultRequestHeaders.UserAgent.ParseAdd(@" Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

            return rv;
        }

        public Thread Scrap(string filter, int count)
        {
            if (count < 0)
                throw new ArgumentException("Count cannot be negative number.", nameof(count));

            filter = new string(filter.ToCharArray().Where(x => char.IsLetterOrDigit(x)).Take(5).ToArray());

            var thread = new Thread(() => ScrapInner(filter, count));
            return thread;
        }

        private void ScrapInner(string filter, int count)
        {
            var list = new List<Tuple<string, bool>>();

            for (int i = 0; i < count; i++)
            {
                if (_ct.IsCancellationRequested)
                    break;

                list.Add(ScrapItem(filter));
            }
        }

        private Tuple<string, bool> ScrapItem(string filter)
        {
            string name;
            string fn;
            string reason = string.Empty;
            var downloaded = false;
            var charsToGenerate = 6 - filter.Length;
            do
            {
                name = filter + NameGenerator.Generate(charsToGenerate);
                fn = Helper.GetFilename(name);
            } while (File.Exists(fn));

            //Console.WriteLine($"{name} - starting");
            var url = Helper.GetHtmlUrl(name);

            var get = _http.GetAsync(url, _ct);
            get.ContinueWith(task =>
            {
                if (!task.IsCompleted)
                {
                    reason = "faulted to download HTML";
                    //Console.WriteLine($"{name} - faulted to get HTML.");
                    return;
                }

                var htmlTask = task.Result.Content.ReadAsStringAsync();
                htmlTask.Wait(_ct);

                if (_ct.IsCancellationRequested)
                    return;

                Match match = null;
                foreach (var regex in _regexes)
                {
                    match = regex.Match(htmlTask.Result);
                    if (!string.IsNullOrWhiteSpace(match?.Value))
                        break;
                }

                if (!string.IsNullOrWhiteSpace(match?.Value))
                {
                    //Console.WriteLine($"{name} - downloading image.");
                    using (var web = new WebClient())
                    {
                        downloaded = true;
                        web.DownloadFile(new Uri(match.Value), fn);
                        //Console.WriteLine($"{name} - OK.");
                    }
                }
                else
                {
                    reason = "image URL not found.";
                    //Console.WriteLine($"{name} - image URL not found.");
                }
            }, _ct).Wait(_ct);


            var status = downloaded ? "OK" : "Faulted.";
            Console.WriteLine($"{name} - {status} {reason}");

            return new Tuple<string, bool>(name, downloaded);
        }
    }

    public class Scheduler
    {
        public string DataFolder { get; }
        private List<string> _scheduled = new List<string>();

        /// <inheritdoc />
        public Scheduler(string dataFolder)
        {
            DataFolder = dataFolder;
        }

        public bool TrySchedule(string name)
        {
            if (_scheduled.Contains(name))
                return false;

            var fn = Path.Combine(DataFolder, $"{name}.png");

            return false;
        }
    }
}