using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVienAPI.DTOs
{
    public class SinhVienDto
    {
        public int Id { get; set; }
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public double DiemTrungBinh { get; set; }
        public string XepLoai { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }

        public int LopHocId { get; set; }
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string Khoa { get; set; } = string.Empty;

        public static string TinhXepLoai(double diem)
        {
            if (diem >= 9.0) return "Xuất sắc";
            if (diem >= 8.0) return "Giỏi";
            if (diem >= 6.5) return "Khá";
            if (diem >= 5.0) return "Trung bình";
            return "Yếu";
        }
    }

    public class CreateSinhVienDto
    {
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
        public string GioiTinh { get; set; } = "Nam";

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

        [Range(0.0, 10.0, ErrorMessage = "Điểm trung bình từ 0.0 đến 10.0")]
        public double DiemTrungBinh { get; set; } = 0.0;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Đang học";

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int LopHocId { get; set; }
    }

    public class UpdateSinhVienDto
    {
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
        public string GioiTinh { get; set; } = "Nam";

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

        [Range(0.0, 10.0, ErrorMessage = "Điểm trung bình từ 0.0 đến 10.0")]
        public double DiemTrungBinh { get; set; } = 0.0;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(50, ErrorMessage = "Trạng thái tối đa 50 ký tự")]
        public string TrangThai { get; set; } = "Đang học";

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int LopHocId { get; set; }
    }

    public class SinhVienQueryParameters
    {
        public string? SearchTerm { get; set; }
        public int? LopHocId { get; set; }
        public string? GioiTinh { get; set; }
        public string? TrangThai { get; set; }
        public string? XepLoai { get; set; }
        public string SortBy { get; set; } = "Id"; // Id, MaSV, HoTen, DiemTrungBinh, NgaySinh, NgayTao
        public bool IsDescending { get; set; } = false;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
