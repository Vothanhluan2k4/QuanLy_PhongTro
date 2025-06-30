using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    // Model cho bộ lọc
    public class FilterModel
    {
        public string RoomType { get; set; }
        public string Price { get; set; }
        public string Area { get; set; }
        public string Amenities { get; set; }
        public string Sort { get; set; }
        public Dictionary<int, int> Page { get; set; } = new Dictionary<int, int>(); // Số trang cho từng loại phòng
    }
}
