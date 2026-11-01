using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLySinhVienAPI.Models
{
    public class SinhVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã sinh viên tối đa 20 ký tự")]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [StringLength(10, ErrorMessage = "Giới tính tối đa 10 ký tự")]
        public string GioiTinh { get; set; } = "Nam"; // Nam, Nữ, Khác

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string SoDienThoai { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        public string? DiaChi { get; set; }

        [Range(0.0, 10.0, ErrorMessage = "Điểm trung bình phải nằm trong khoảng từ 0.0 đến 10.0")]
        public double DiemTrungBinh { get; set; } = 0.0;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Đang học"; // Đang học, Bảo lưu, Đã tốt nghiệp, Đình chỉ

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required(ErrorMessage = "Lớp học là bắt buộc")]
        public int LopHocId { get; set; }

        [ForeignKey("LopHocId")]
        public LopHoc? LopHoc { get; set; }
    }
}
