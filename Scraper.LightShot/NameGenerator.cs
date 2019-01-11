using System;
using System.Linq;

namespace Scraper.LightShot
{
    public static class NameGenerator
    {
        private static Random random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string Generate(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}