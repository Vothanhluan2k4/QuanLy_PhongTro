using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;

namespace QuanLy_PhongTro.Repository
{
    public class DataContext : DbContext    
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<PhanQuyenModel> PhanQuyens { get; set; }
        public DbSet<NguoiDungModel> NguoiDungs { get; set; }
        public DbSet<LuuPhongModel> LuuPhongs { get; set; }
        public DbSet<LoaiPhongModel> LoaiPhongs { get; set; }
        public DbSet<PhongTroModel> PhongTros { get; set; }
        public DbSet<ThietBiModel> ThietBis { get; set; }
        public DbSet<PhongTro_ThietBiModel> PhongTro_ThietBiModels { get; set; }
        public DbSet<AnhPhongTroModel> AnhPhongTros { get; set; }
        public DbSet<HopDongModel> HopDongs { get; set; }

        public DbSet<VnpayModel> VnPayInfor { get; set; }
            
        public DbSet<MomoModel> MomoInfor { get; set; }
        public DbSet<HoaDonModel> HoaDon { get; set; }
        public DbSet<ChiSoDienNuocModel> ChiSoDienNuoc { get; set; }
        public DbSet<AccountPaidModel> AccountPaidInfor { get; set; }
        public DbSet<SuCoModel> SuCos { get; set; }

        public DbSet<TinTucModel> TinTucs { get; set; }

        public DbSet<LoaiTinTucModel> LoaiTinTucs { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình bảng PhanQuyen
            modelBuilder.Entity<PhanQuyenModel>(entity =>
            {
                entity.HasKey(e => e.MaQuyen);
                entity.Property(e => e.TenQuyen)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.MoTa)
                      .HasMaxLength(255);
            });

            // Cấu hình bảng NguoiDung
            modelBuilder.Entity<NguoiDungModel>(entity =>
            {
                entity.HasKey(e => e.MaNguoiDung);
                entity.Property(e => e.HoTen)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.SoDienThoai)
                      .IsRequired()
                      .HasMaxLength(15);
                entity.Property(e => e.MatKhau)
                      .IsRequired()
                      .HasMaxLength(255);
                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("GETDATE()");

                // Ràng buộc unique cho Email và SoDienThoai
                entity.HasIndex(e => e.Email)
                      .IsUnique();
                entity.HasIndex(e => e.SoDienThoai)
                      .IsUnique();

                // Ràng buộc khóa ngoại với PhanQuyen
                entity.HasOne(e => e.PhanQuyen)
                      .WithMany(pq => pq.NguoiDungs)
                      .HasForeignKey(e => e.MaQuyen)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa PhanQuyen khi còn NguoiDung
            });

            // Cấu hình bảng LuuPhong
            modelBuilder.Entity<LuuPhongModel>(entity =>
            {
                entity.HasKey(e => e.MaLuuPhong);
                entity.Property(e => e.NgayLuu)
                      .HasDefaultValueSql("GETDATE()");

                // Ràng buộc khóa ngoại với PhongTro
                entity.HasOne(e => e.PhongTro)
                      .WithMany(pt => pt.LuuPhongs)
                      .HasForeignKey(e => e.MaPhong)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa LuuPhong khi xóa PhongTro

                // Ràng buộc khóa ngoại với NguoiDung
                entity.HasOne(e => e.NguoiDung)
                      .WithMany(nd => nd.LuuPhongs)
                      .HasForeignKey(e => e.MaNguoiDung)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa LuuPhong khi xóa NguoiDung
            });

            // Cấu hình bảng LoaiPhong
            modelBuilder.Entity<LoaiPhongModel>(entity =>
            {
                entity.HasKey(e => e.MaLoaiPhong);
                entity.Property(e => e.TenLoaiPhong)
                      .IsRequired();
            });

            // Cấu hình bảng PhongTro
            modelBuilder.Entity<PhongTroModel>(entity =>
            {
                entity.HasKey(e => e.MaPhong);
                entity.Property(e => e.TenPhong)
                      .IsRequired();
                entity.Property(e => e.DiaChi)
                      .IsRequired();
                entity.Property(e => e.TrangThai)
                      .IsRequired();
                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("GETDATE()");

                // Ràng buộc khóa ngoại với LoaiPhong
                entity.HasOne(p => p.LoaiPhong)
                      .WithMany(l => l.PhongTros)
                      .HasForeignKey(p => p.MaLoaiPhong)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa LoaiPhong khi còn PhongTro

                // Ràng buộc với HopDongs (Restrict để ngăn xóa khi còn hợp đồng)
                entity.HasMany(p => p.HopDongs)
                      .WithOne(hd => hd.PhongTro)
                      .HasForeignKey(hd => hd.MaPhong)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình bảng ThietBi
            modelBuilder.Entity<ThietBiModel>(entity =>
            {
                entity.HasKey(e => e.MaThietBi);
                entity.Property(e => e.TenThietBi)
                      .IsRequired();
                entity.Property(e => e.MoTa)
                      .HasMaxLength(255);
                entity.Property(e => e.TinhTrang)
                     .HasMaxLength(50);
                entity.Property(e => e.DonViTinh)
                      .HasMaxLength(50);
            });

            // Cấu hình bảng PhongTro_ThietBi
            modelBuilder.Entity<PhongTro_ThietBiModel>(entity =>
            {
                entity.HasKey(pt => pt.MaPhongThietBi);

                // Quan hệ giữa PhongTro_ThietBi và PhongTro
                entity.HasOne(pt => pt.PhongTro)
                      .WithMany(p => p.PhongTroThietBis)
                      .HasForeignKey(pt => pt.MaPhong)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa khi xóa PhongTro

                // Quan hệ giữa PhongTro_ThietBi và ThietBi
                entity.HasOne(pt => pt.ThietBi)
                      .WithMany(t => t.PhongTroThietBis)
                      .HasForeignKey(pt => pt.MaThietBi)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa ThietBi khi còn PhongTro_ThietBi
            });

            // Cấu hình bảng AnhPhongTro
            modelBuilder.Entity<AnhPhongTroModel>(entity =>
            {
                entity.HasKey(a => a.MaAnh);
                entity.Property(a => a.DuongDan)
                      .IsRequired();

                // Quan hệ giữa AnhPhongTro và PhongTro
                entity.HasOne(a => a.PhongTro)
                      .WithMany(p => p.AnhPhongTros)
                      .HasForeignKey(a => a.MaPhong)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa ảnh khi xóa PhongTro
            });

            // Cấu hình bảng HopDong
            modelBuilder.Entity<HopDongModel>(entity =>
            {
                entity.HasKey(hd => hd.MaHopDong);
                entity.Property(hd => hd.NgayBatDau)
                      .IsRequired();
                entity.Property(hd => hd.NgayKetThuc)
                      .IsRequired();
                entity.Property(hd => hd.TienCoc)
                      .IsRequired();
                entity.Property(hd => hd.TrangThai)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(hd => hd.GhiChu)
                      .HasMaxLength(400);

                // Ràng buộc khóa ngoại với PhongTro
                entity.HasOne(hd => hd.PhongTro)
                      .WithMany(pt => pt.HopDongs)
                      .HasForeignKey(hd => hd.MaPhong)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa PhongTro khi còn HopDong

                // Ràng buộc khóa ngoại với NguoiDung
                entity.HasOne(hd => hd.NguoiDung)
                      .WithMany(nd => nd.HopDongs)
                      .HasForeignKey(hd => hd.MaNguoiDung)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa NguoiDung khi còn HopDong
            });

            // Cấu hình bảng ChiSoDienNuoc
            modelBuilder.Entity<ChiSoDienNuocModel>(entity =>
            {
                entity.HasKey(cs => cs.MaChiSo);
                entity.Property(cs => cs.ChiSoDienCu)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(cs => cs.ChiSoDienMoi)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(cs => cs.ChiSoNuocCu)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(cs => cs.ChiSoNuocMoi)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                // Quan hệ với HopDong
                entity.HasOne(cs => cs.HopDong)
                      .WithMany(hd => hd.ChiSoDienNuocs)
                      .HasForeignKey(cs => cs.MaHopDong)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa khi xóa HopDong
            });

            modelBuilder.Entity<HoaDonModel>(entity =>
            {
                entity.HasKey(hd => hd.MaHoaDon);
                entity.Property(hd => hd.TienDien)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(hd => hd.TienNuoc)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(hd => hd.TienRac)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(hd => hd.TrangThai)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("Chưa thanh toán");

                // Quan hệ với HopDong
                entity.HasOne(hd => hd.HopDong)
                      .WithMany(h => h.HoaDons)
                      .HasForeignKey(hd => hd.MaHopDong)
                      .OnDelete(DeleteBehavior.Cascade); // Tự động xóa khi xóa HopDong

                // Quan hệ với MomoModel (nullable)
                entity.HasOne(hd => hd.Momo)
                      .WithOne()
                      .HasForeignKey<HoaDonModel>(hd => hd.MomoId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ với VnpayModel (nullable)
                entity.HasOne(hd => hd.Vnpay)
                      .WithOne()
                      .HasForeignKey<HoaDonModel>(hd => hd.VnpayId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cấu hình bảng MomoModel
            modelBuilder.Entity<MomoModel>(entity =>
            {
                entity.HasKey(m => m.MomoId);
                entity.Property(m => m.Amount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(m => m.OrderInfo)
                      .HasMaxLength(255);
                entity.Property(m => m.TransactionId)
                      .HasMaxLength(50);
                entity.Property(m => m.OrderId)
                      .HasMaxLength(50);
                entity.Property(m => m.PaymentMethod)
                      .HasMaxLength(50);
            });

            // Cấu hình bảng VnpayModel
            modelBuilder.Entity<VnpayModel>(entity =>
            {
                entity.HasKey(v => v.VnpayId);
                entity.Property(v => v.Amount)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.Property(v => v.OrderDescription)
                      .HasMaxLength(255);
                entity.Property(v => v.TransactionId)
                      .HasMaxLength(50);
                entity.Property(v => v.OrderId)
                      .HasMaxLength(50);
                entity.Property(v => v.PaymentMethod)
                      .HasMaxLength(50);
                entity.Property(v => v.PaymentId)
                      .HasMaxLength(50);
            });

            modelBuilder.Entity<AccountPaidModel>(entity =>
            {
                entity.Property(a => a.MaHopDong)
                        .IsRequired(false);
                entity.Property(a => a.MaHoaDon)
                    .IsRequired(false);
            });

            modelBuilder.Entity<SuCoModel>()
            .HasOne(s => s.Phong)
            .WithMany()
            .HasForeignKey(s => s.MaPhong)
            .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa PhongTro khi còn SuCo

            modelBuilder.Entity<SuCoModel>()
                .HasOne(s => s.NguoiBaoCao)
                .WithMany()
                .HasForeignKey(s => s.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa NguoiDung khi còn SuCo


            modelBuilder.Entity<TinTucModel>()
                .HasOne(t => t.LoaiTinTuc)
                .WithMany(l => l.TinTucs)
                .HasForeignKey(t => t.MaLoaiTinTuc)
                .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa LoaiTinTuc khi còn TinTuc

        }

    }

}
    