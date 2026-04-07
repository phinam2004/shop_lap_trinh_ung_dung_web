namespace SV22T1020673.Models.Sales
{
    /// <summary>
    /// Thông tin mặt hàng trong giỏ hàng
    /// </summary>
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalPrice => Quantity * SalePrice;
    }
}
