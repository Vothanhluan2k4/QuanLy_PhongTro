namespace QuanLy_PhongTro.Models.Order
{
    public class NapTienModel
    {
        public int MaNguoiDung { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string OrderDescription { get; set; }
        public string OrderType { get; set; }
    }
}
