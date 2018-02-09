using System;
using System.Collections.Generic;
using System.IO;

namespace BookPacker
{
    public class Engine
    {
        public static void GenerateFromThisPath(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    GenerateFromSingleFile(sourcePath, destinationPath);
                }
                else if (Directory.Exists(sourcePath))
                {
                    GenerateFromDirectory(sourcePath, destinationPath);
                }
                else
                {
                    Console.WriteLine("Error: Path doesn't exist.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void GenerateFromDirectory(string sourcePath, string destinationPath)
        {
            var extractedBookData = DataExtractor.ParseFolderWithWebsites(sourcePath);
            var boxes = BoxPacker.ShipTheseBooks(extractedBookData, 10);
            JsonGenerator.GenerateJsonFromTheseBoxes(boxes, destinationPath);
        }

        private static void GenerateFromSingleFile(string sourcePath, string destinationPath)
        {
            BookData bookData = DataExtractor.ParseThisWebsite(sourcePath);
            if (bookData != null)
            {
                var boxes = BoxPacker.ShipTheseBooks(new List<BookData> { bookData }, 10);
                JsonGenerator.GenerateJsonFromTheseBoxes(boxes, destinationPath);
            }
        }
    }
}
