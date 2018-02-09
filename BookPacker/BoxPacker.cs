using System.Collections.Generic;
using System.Linq;

namespace BookPacker
{
    public class BoxPacker
    {
        public static List<Box> ShipTheseBooks(List<BookData> bookList, int maxWeightPerBox)
        {
            List<Box> returnBoxList = new List<Box>();
            List<BookData> sortedBookList = bookList.OrderByDescending(o => o.ShippingWeight).ToList();
            while (sortedBookList.Count > 0)
                returnBoxList.Add(ExtractOneBoxFromList(ref sortedBookList, maxWeightPerBox, returnBoxList.Count + 1));
            return returnBoxList;
        }

        private static Box ExtractOneBoxFromList(ref List<BookData> sortedBookList, int maxWeightPerBox, int boxId)
        {
            List<BookData> listMirror = new List<BookData>(sortedBookList);
            Box newBox = new Box() { Id = boxId };
            List<BookData> contents = new List<BookData>();
            float weight = maxWeightPerBox;
            foreach (var book in sortedBookList)
                ExtractIfBookFits(book, ref weight, ref contents, ref listMirror);
            newBox.Contents = contents.ToArray();
            newBox.TotalWeight = maxWeightPerBox - weight;
            sortedBookList = listMirror;
            return newBox;
        }

        private static void ExtractIfBookFits(BookData book, ref float weight, ref List<BookData> contents, ref List<BookData> listMirror)
        {
            if (book.ShippingWeight <= weight)
            {
                contents.Add(book);
                weight -= book.ShippingWeight;
                listMirror.Remove(book);
            }
        }
    }
}
