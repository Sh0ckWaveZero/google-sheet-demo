namespace GoogleSheetsDemo.Models
{
    /// <summary>
    /// One row of the demo sheet: ID | Name | Quantity | Price
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
