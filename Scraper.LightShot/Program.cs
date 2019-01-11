using System;
using System.IO;

namespace Scraper.LightShot
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataManager = new DataManager(Path.Combine(Helper.DataFolder, "Data.db"));
            using (var indexer = new Indexer("m00000"))
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
