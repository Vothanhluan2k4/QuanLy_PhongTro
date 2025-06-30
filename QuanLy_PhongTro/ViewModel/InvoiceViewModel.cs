namespace QuanLy_PhongTro.ViewModel
{
    public class InvoiceViewModel
    {
        public int MaHoaDon { get; set; }         // Mã hóa đơn
        public int MaHopDong { get; set; }        // Mã hợp đồng
        public int Thang { get; set; }            // Tháng
        public int Nam { get; set; }              // Năm
        public double TienDien { get; set; }      // Tiền điện
        public double TienNuoc { get; set; }      // Tiền nước
        public double TienRac { get; set; }       // Tiền rác
        public double TongTien { get; set; }      // Tổng tiền
        public string NgayPhatHanh { get; set; }  // Ngày phát hành (định dạng chuỗi dd/MM/yyyy)
        public string TrangThai { get; set; }     
    }

}
