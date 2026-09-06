namespace GoogleSheetsDemo.Validation
{
    /// <summary>
    /// Result of parsing and validating the append form: the failures to show
    /// (if any) plus the parsed values, which are only meaningful when valid.
    /// </summary>
    public class ProductInputResult
    {
        public ProductInputResult()
        {
            Validation = new ValidationResult();
            Name = "";
        }

        public ValidationResult Validation { get; private set; }

        public bool IsValid
        {
            get { return Validation.IsValid; }
        }

        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
