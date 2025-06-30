namespace QuanLy_PhongTro.Models.Order
{
    public class PendingNapTien
    {
        public int Id { get; set; }
        public string NapTienInfoJson { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
