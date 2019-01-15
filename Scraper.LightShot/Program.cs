using System;
using System.IO;

namespace Scraper.LightShot
{
    class Program
    {
        private const int MAX_LENGTH = 6;

        static void Main(string[] args)
        {
            Console.Write("Indexer start (6 symbols max): ");
            var inp = Console.ReadLine();
            var indexerStart = inp.Length > MAX_LENGTH ? inp.Substring(0, MAX_LENGTH) : inp;
            while (indexerStart.Length < MAX_LENGTH)
                indexerStart += "0";

            var dataManager = new DataManager(Path.Combine(Helper.DataFolder, "Data.db"));
            using (var indexer = new Indexer(indexerStart))
            using (var scraper = new Scraper(indexer, dataManager))
            {
                //int count = 0;
                //bool correctInput = false;
                //do
                //{
                //    if (!correctInput)
                //        Console.Clear();

                //    Console.Write("Count (0 = infinity): ");
                //    var inp = Console.ReadLine();
                //    correctInput = int.TryParse(inp, out count) && count >= 0;
                //} while (!correctInput);

                Console.WriteLine("Press any key to exit...");
                Console.WriteLine();

                var count = 46656;
                var thread = scraper.Scrap(count);
                Console.ReadKey(true);
                thread.Abort();
            }
        }
    }
}
