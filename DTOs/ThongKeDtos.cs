namespace QuanLySinhVienAPI.DTOs
{
    public class ThongKeTongQuanDto
    {
        public int TongSinhVien { get; set; }
        public int TongLopHoc { get; set; }
        public double DiemTrungBinhChung { get; set; }
        public int SoLuongDangHoc { get; set; }
        public int SoLuongBaoLuu { get; set; }
        public int SoLuongDaTotNghiep { get; set; }
        public int SoLuongDinhChi { get; set; }

        public int SoLuongXuatSac { get; set; }
        public int SoLuongGioi { get; set; }
        public int SoLuongKha { get; set; }
        public int SoLuongTrungBinh { get; set; }
        public int SoLuongYeu { get; set; }

        public List<LopHocThongKeItemDto> ThongKeTheoLop { get; set; } = new List<LopHocThongKeItemDto>();
    }

    public class LopHocThongKeItemDto
    {
        public int LopHocId { get; set; }
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string Khoa { get; set; } = string.Empty;
        public int SoLuongSinhVien { get; set; }
        public double DiemTrungBinhLop { get; set; }
    }
}
