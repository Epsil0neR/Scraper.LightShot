using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper.LightShot
{
    public class Scraper : IDisposable
    {
        public Indexer Indexer { get; }
        public DataManager DataManager { get; }

        private readonly object _lockProceedItem = new object();
        private readonly object _lockFindNextName = new object();
        private readonly List<Regex> _regexes;
        private readonly CancellationTokenSource _ctSource;
        private readonly CancellationToken _ct;
        private readonly HttpClient _http;
        private readonly List<string> _checkedDirs = new List<string>();

        public Scraper(Indexer indexer, DataManager dataManager)
        {
            Indexer = indexer;
            DataManager = dataManager;

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

        private HttpClient GetHttpClient()
        {
            var rv = new HttpClient();
            rv.DefaultRequestHeaders.UserAgent.ParseAdd(@" Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

            return rv;
        }

        public Thread Scrap(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count cannot be negative number.", nameof(count));

            //ScrapInner(count);
            var thread = new Thread(() => ScrapInner(count));
            thread.Start();
            return thread;
        }

        private void ScrapInner(int count)
        {
            var tm = new TaskManager(1, _ct, ProceedItem, count);
            tm.Run();
            //for (int i = 0; i < count; i++)
            //{
            //    if (_ct.IsCancellationRequested)
            //        break;

            //    ProceedItem(i).Wait(_ct);
            //}
        }

        private async Task ProceedItem(int i)
        {
            try
            {
                string name;
                string filename;

                lock (_lockProceedItem)
                {
                    name = FindNextName();

                    if (string.IsNullOrWhiteSpace(name))
                        return;

                    Console.Title = $"Scraped {i} - {name}";
                    filename = Helper.GetFilename(name);
                    DataManager.StartProceeding(name, filename);
                }

                var url = Helper.GetHtmlUrl(name);
                var page = await _http.GetAsync(url, _ct);
                var html = await page.Content.ReadAsStringAsync();
                var match = TryFindMatch(html);

                if (string.IsNullOrWhiteSpace(match?.Value))
                {
                    DataManager.SetStatus(name, ScrapEntryStatus.Failed);
                    return;
                }

                DataManager.SetStatus(name, ScrapEntryStatus.Downloading);

                CreateFolderIfMisses(filename);

                Download(new Uri(match.Value), filename); //TODO: Make async
                DataManager.SetStatus(name, ScrapEntryStatus.Success);
            }
            catch (Exception e)
            {
                Debugger.Break();
                Console.WriteLine(e);
                throw;
            }
        }


        private string FindNextName()
        {
            lock (_lockFindNextName)
            {
                string rv;
                do
                {
                    rv = Indexer.Current;

                    //Check if name is not parsed yet.
                    if (DataManager.Contains(rv)) //TODO: Check status if finds
                        rv = null;

                } while (string.IsNullOrEmpty(rv) && Indexer.MoveNext());

                if (!string.IsNullOrEmpty(rv))
                    Indexer.MoveNext();

                return rv;
            }
        }

        private Match TryFindMatch(string html)
        {
            foreach (var regex in _regexes)
            {
                var match = regex.Match(html);
                if (!string.IsNullOrWhiteSpace(match.Value))
                    return match;
            }

            return null;
        }

        private void Download(Uri uri, string path)
        {
            using (var web = new WebClient())
            {
                web.DownloadFile(uri, path);
            }
        }

        private void CreateFolderIfMisses(string filename)
        {
            var dir = Path.GetDirectoryName(filename);

            if (_checkedDirs.Contains(dir) || string.IsNullOrEmpty(dir))
                return;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _checkedDirs.Insert(0, dir);
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