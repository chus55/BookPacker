using System;
using BookPacker;

namespace BookPacker_Console
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please enter both a source (directory or single html file) and a destination path, separated by a space.");
                Console.WriteLine("Example:");
                Console.WriteLine(".\\AllMyBooksArePacked.exe \"C:\\Users\" \"C:\\Users\\boxes.json\"");
                return 1;
            }
            Engine.GenerateFromThisPath(args[0], args[1]);
            return 0;
        }
    }
}
