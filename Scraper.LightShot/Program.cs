using System;

namespace Scraper.LightShot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var scraper = new Scraper())
            {
                int count = 0;
                bool correctInput = false;
                do
                {
                    if (!correctInput)
                        Console.Clear();

                    Console.Write("Count (0 = infinity): ");
                    var inp = Console.ReadLine();
                    correctInput = int.TryParse(inp, out count) && count >= 0;
                } while (!correctInput);

                Console.Write("Filter (max 5 symbols): ");
                var filter = Console.ReadLine();


                Console.WriteLine("Press any key to exit...");
                Console.WriteLine();

                var thread = scraper.Scrap(filter, count);
                thread.Start();

                Console.ReadKey(true);
            }
        }
    }
}
