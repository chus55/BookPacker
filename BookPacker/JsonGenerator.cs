using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace BookPacker
{
    public class JsonGenerator
    {
        public static void GenerateJsonFromTheseBoxes(List<Box> boxList, string destinationPath)
        {
            List<ImportantBox> importantData = GetImportantBoxData(boxList);
            File.WriteAllText(destinationPath, JsonConvert.SerializeObject(importantData, Formatting.Indented));
        }

        private static List<ImportantBox> GetImportantBoxData(List<Box> boxList)
        {
            List<ImportantBox> returnBoxList = new List<ImportantBox>();

            foreach (var box in boxList)
            {
                List<ImportantBookData> importantBookData = GetImportantBookData(box);
                returnBoxList.Add(new ImportantBox()
                {
                    Id = box.Id,
                    TotalWeight = box.TotalWeight.ToString(CultureInfo.InvariantCulture) + " pounds",
                    Contents = importantBookData.ToArray()
                });
            }
            return returnBoxList;
        }

        private static List<ImportantBookData> GetImportantBookData(Box box)
        {
            List<ImportantBookData> returnBookList = new List<ImportantBookData>();

            foreach (var book in box.Contents)
            {
                string authors = GetAuthorsFromBook(book);
                returnBookList.Add(new ImportantBookData()
                {
                    Title = book.Title,
                    Authors = authors,
                    Price = "$" + book.SelectedPrice.ToString(CultureInfo.InvariantCulture) + " USD",
                    ShippingWeight = book.ShippingWeight.ToString(CultureInfo.InvariantCulture) + " pounds",
                    Isbn10 = book.Isbn10,
                    Isbn13 = book.Isbn13
                });
            }
            return returnBookList;
        }

        private static string GetAuthorsFromBook(BookData book)
        {
            string authors = "";
            foreach (var author in book.Authors)
            {
                authors += author + ", ";
            }
            authors = authors.Remove(authors.Length - 2);
            return authors;
        }
    }

    class ImportantBox
    {
        public int Id { get; set; }
        public string TotalWeight { get; set; }
        public ImportantBookData[] Contents { get; set; }
    }

    class ImportantBookData
    {
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Price { get; set; }
        public string ShippingWeight { get; set; }
        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
    }
}
