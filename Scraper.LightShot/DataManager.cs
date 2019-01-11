using LiteDB;
using System;
using System.Diagnostics;

namespace Scraper.LightShot
{
    public enum ScrapEntryStatus
    {
        None,
        Started,
        Downloading,
        Success,
        Failed,
        Removed
    }

    public class ScrapEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ScrapEntryStatus Status { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }

    public class DataManager : IDisposable
    {
        public LiteDatabase DataBase { get; }
        public LiteCollection<ScrapEntry> Entries { get; }

        public DataManager(string path)
        {
            DataBase = new LiteDatabase(path);
            Entries = DataBase.GetCollection<ScrapEntry>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DataBase.Dispose();
        }

        public void Add(ScrapEntry entry)
        {
            Entries.Insert(entry);
            Entries.EnsureIndex(x => x.Name);
        }

        public void StartProceeding(string name, string path)
        {
            var scrapEntry = new ScrapEntry()
            {
                Name = name,
                Path = path,
                Created = DateTime.UtcNow,
                Status = ScrapEntryStatus.Started,
            };
            Add(scrapEntry);
        }

        public ScrapEntry FirstOrDefault(string name)
        {
            if (Entries.Count() == 0)
                return null;

            try
            {

                return
                    Entries.FindOne(x =>
                        x.Name == name); //string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            return null;
        }

        public bool Contains(string name)
        {
            return FirstOrDefault(name) != null;
        }

        public void SetStatus(string name, ScrapEntryStatus status)
        {
            var entry = FirstOrDefault(name);
            entry.Status = status;
            Entries.Update(entry);
            Entries.EnsureIndex(x => x.Name);

        }
    }
}