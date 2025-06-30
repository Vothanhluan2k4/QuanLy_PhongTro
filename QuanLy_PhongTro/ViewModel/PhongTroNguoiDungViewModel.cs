using QuanLy_PhongTro.Models;

namespace QuanLy_PhongTro.ViewModel
{
    public class PhongTroNguoiDungViewModel
    {
        public IEnumerable<PhongTroModel> PhongTros { get; set; }
        public IEnumerable<NguoiDungModel> NguoiDungs { get; set; }
        public IEnumerable<PhanQuyenModel> PhanQuyens { get; set; }
        public IEnumerable<HopDongModel> HopDongs { get; set; }
        public RevenueViewModel Revenue { get; set; }

        public IEnumerable<SuCoModel> SuCos { get; set; }

        public IEnumerable<TinTucModel> TinTucs { get; set;  }
    }
    
}
