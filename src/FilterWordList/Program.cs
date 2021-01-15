using System;
using System.Collections.Generic;
using System.IO;

namespace FilterWordList
{
    class Program
    {
        static void Main(string[] args)
        {
            // Got words_alpha.txt from here: https://github.com/dwyl/english-words
            var newLines = new List<string>();
            var lines = File.ReadAllLines("words_alpha.txt");
            foreach (var line in lines)
            {
                if (line.Length >= 2 && line.Length <= 10)
                {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines("words.txt", newLines);
        }
    }
}
