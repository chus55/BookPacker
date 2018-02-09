namespace BookPacker
{
    public class Box
    {
        public int Id { get; set; }
        public float TotalWeight { get; set; }
        public BookData[] Contents { get; set; }
    }
}
