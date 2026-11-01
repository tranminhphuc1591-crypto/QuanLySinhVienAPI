using Microsoft.EntityFrameworkCore;
using QuanLySinhVienAPI.Models;

namespace QuanLySinhVienAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LopHoc> LopHocs => Set<LopHoc>();
        public DbSet<SinhVien> SinhViens => Set<SinhVien>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index unique cho MaLop
            modelBuilder.Entity<LopHoc>()
                .HasIndex(l => l.MaLop)
                .IsUnique();

            // Index unique cho MaSV
            modelBuilder.Entity<SinhVien>()
                .HasIndex(s => s.MaSV)
                .IsUnique();

            // Cấu hình quan hệ LopHoc - SinhVien (1 - N)
            modelBuilder.Entity<SinhVien>()
                .HasOne(s => s.LopHoc)
                .WithMany(l => l.SinhViens)
                .HasForeignKey(s => s.LopHocId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
