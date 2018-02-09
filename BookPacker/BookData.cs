namespace BookPacker
{
    public class BookData
    {
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public float SelectedPrice { get; set; }
        public BookFormats[] FormatPrices { get; set; }
        public float ShippingWeight { get; set; }
        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
    }

    public class BookFormats
    {
        public string Format { get; set; }
        public float Price { get; set; }
    }
}
