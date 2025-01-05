namespace JWT.Core.Model
{
    public class Product : GeneralModel
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string? StockCode { get; set; }

    }
}
