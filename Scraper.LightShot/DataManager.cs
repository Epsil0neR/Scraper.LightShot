using LiteDB;
using System;

namespace Scraper.LightShot
{
    public enum ScrapEntryStatus
    {
        None,
        Downloaded,
        Failed,
        Removed
    }

    public class ScrapEntry
    {
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

        public bool Contains(string name)
        {
            return Entries.FindOne(x=>string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase)) != null;
        }
    }
}