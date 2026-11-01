using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVienAPI.Models
{
    public class LopHoc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã lớp là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã lớp tối đa 20 ký tự")]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lớp tối đa 100 ký tự")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa/Viện là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string Khoa { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Mô tả tối đa 250 ký tự")]
        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
    }
}
