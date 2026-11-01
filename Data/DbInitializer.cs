using QuanLySinhVienAPI.Models;

namespace QuanLySinhVienAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Kiểm tra nếu đã có dữ liệu thì không seed lại
            if (context.LopHocs.Any())
            {
                return;
            }

            // Tạo các lớp học mẫu
            var lopHocs = new List<LopHoc>
            {
                new LopHoc
                {
                    MaLop = "CNTT01",
                    TenLop = "Kỹ thuật Phần mềm 01",
                    Khoa = "Công nghệ Thông tin",
                    MoTa = "Lớp chuyên ngành Công nghệ thông tin định hướng Phần mềm",
                    NgayTao = DateTime.UtcNow.AddMonths(-6)
                },
                new LopHoc
                {
                    MaLop = "CNTT02",
                    TenLop = "Hệ thống Thông tin 01",
                    Khoa = "Công nghệ Thông tin",
                    MoTa = "Lớp chuyên ngành Phân tích thiết kế hệ thống dữ liệu",
                    NgayTao = DateTime.UtcNow.AddMonths(-6)
                },
                new LopHoc
                {
                    MaLop = "ATTT01",
                    TenLop = "An toàn Thông tin 01",
                    Khoa = "An toàn Thông tin",
                    MoTa = "Lớp chuyên sâu về An ninh mạng và Mật mã học",
                    NgayTao = DateTime.UtcNow.AddMonths(-5)
                },
                new LopHoc
                {
                    MaLop = "QTKD01",
                    TenLop = "Quản trị Doanh nghiệp 01",
                    Khoa = "Quản trị Kinh doanh",
                    MoTa = "Lớp đào tạo cử nhân Quản trị điều hành doanh nghiệp",
                    NgayTao = DateTime.UtcNow.AddMonths(-4)
                },
                new LopHoc
                {
                    MaLop = "DTVT01",
                    TenLop = "Điện tử Viễn thông 01",
                    Khoa = "Điện - Điện tử",
                    MoTa = "Lớp chuyên ngành Kỹ thuật mạng và IoT",
                    NgayTao = DateTime.UtcNow.AddMonths(-3)
                }
            };

            context.LopHocs.AddRange(lopHocs);
            context.SaveChanges();

            // Lấy ID lớp sau khi lưu
            var cntt01Id = lopHocs[0].Id;
            var cntt02Id = lopHocs[1].Id;
            var attt01Id = lopHocs[2].Id;
            var qtkd01Id = lopHocs[3].Id;
            var dtvt01Id = lopHocs[4].Id;

            // Tạo danh sách sinh viên mẫu
            var sinhViens = new List<SinhVien>
            {
                new SinhVien
                {
                    MaSV = "SV2024001",
                    HoTen = "Nguyễn Văn An",
                    NgaySinh = new DateTime(2003, 3, 15),
                    GioiTinh = "Nam",
                    Email = "an.nguyen@univ.edu.vn",
                    SoDienThoai = "0901234567",
                    DiaChi = "123 Cầu Giấy, Hà Nội",
                    DiemTrungBinh = 9.2,
                    TrangThai = "Đang học",
                    LopHocId = cntt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-60)
                },
                new SinhVien
                {
                    MaSV = "SV2024002",
                    HoTen = "Trần Thị Mai Hương",
                    NgaySinh = new DateTime(2003, 7, 22),
                    GioiTinh = "Nữ",
                    Email = "huong.tran@univ.edu.vn",
                    SoDienThoai = "0912345678",
                    DiaChi = "45 Lê Lợi, Quận 1, TP.HCM",
                    DiemTrungBinh = 8.8,
                    TrangThai = "Đang học",
                    LopHocId = cntt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-58)
                },
                new SinhVien
                {
                    MaSV = "SV2024003",
                    HoTen = "Lê Hoàng Long",
                    NgaySinh = new DateTime(2002, 11, 5),
                    GioiTinh = "Nam",
                    Email = "long.le@univ.edu.vn",
                    SoDienThoai = "0987654321",
                    DiaChi = "78 Nguyễn Huệ, Đà Nẵng",
                    DiemTrungBinh = 7.6,
                    TrangThai = "Đang học",
                    LopHocId = cntt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-55)
                },
                new SinhVien
                {
                    MaSV = "SV2024004",
                    HoTen = "Phạm Quỳnh Chi",
                    NgaySinh = new DateTime(2003, 1, 18),
                    GioiTinh = "Nữ",
                    Email = "chi.pham@univ.edu.vn",
                    SoDienThoai = "0934567890",
                    DiaChi = "209 Đội Cấn, Ba Đình, Hà Nội",
                    DiemTrungBinh = 9.5,
                    TrangThai = "Đang học",
                    LopHocId = attt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-50)
                },
                new SinhVien
                {
                    MaSV = "SV2024005",
                    HoTen = "Vũ Đức Thắng",
                    NgaySinh = new DateTime(2003, 9, 30),
                    GioiTinh = "Nam",
                    Email = "thang.vu@univ.edu.vn",
                    SoDienThoai = "0978901234",
                    DiaChi = "56 Hùng Vương, Hải Phòng",
                    DiemTrungBinh = 6.4,
                    TrangThai = "Đang học",
                    LopHocId = attt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-45)
                },
                new SinhVien
                {
                    MaSV = "SV2024006",
                    HoTen = "Đỗ Thu Trang",
                    NgaySinh = new DateTime(2003, 5, 12),
                    GioiTinh = "Nữ",
                    Email = "trang.do@univ.edu.vn",
                    SoDienThoai = "0965432109",
                    DiaChi = "12 Phan Chu Trinh, Huế",
                    DiemTrungBinh = 8.2,
                    TrangThai = "Đang học",
                    LopHocId = cntt02Id,
                    NgayTao = DateTime.UtcNow.AddDays(-40)
                },
                new SinhVien
                {
                    MaSV = "SV2024007",
                    HoTen = "Hoàng Minh Tuấn",
                    NgaySinh = new DateTime(2002, 4, 25),
                    GioiTinh = "Nam",
                    Email = "tuan.hoang@univ.edu.vn",
                    SoDienThoai = "0943210987",
                    DiaChi = "88 Lý Thường Kiệt, Cần Thơ",
                    DiemTrungBinh = 5.8,
                    TrangThai = "Bảo lưu",
                    LopHocId = cntt02Id,
                    NgayTao = DateTime.UtcNow.AddDays(-38)
                },
                new SinhVien
                {
                    MaSV = "SV2024008",
                    HoTen = "Ngô Bảo Ngọc",
                    NgaySinh = new DateTime(2003, 10, 8),
                    GioiTinh = "Nữ",
                    Email = "ngoc.ngo@univ.edu.vn",
                    SoDienThoai = "0921098765",
                    DiaChi = "99 Hai Bà Trưng, Nha Trang",
                    DiemTrungBinh = 8.5,
                    TrangThai = "Đang học",
                    LopHocId = qtkd01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-30)
                },
                new SinhVien
                {
                    MaSV = "SV2024009",
                    HoTen = "Bùi Anh Tuấn",
                    NgaySinh = new DateTime(2003, 6, 14),
                    GioiTinh = "Nam",
                    Email = "tuan.bui@univ.edu.vn",
                    SoDienThoai = "0918765432",
                    DiaChi = "34 Võ Thị Sáu, Biên Hòa",
                    DiemTrungBinh = 7.1,
                    TrangThai = "Đang học",
                    LopHocId = qtkd01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-25)
                },
                new SinhVien
                {
                    MaSV = "SV2024010",
                    HoTen = "Dương Gia Bảo",
                    NgaySinh = new DateTime(2002, 8, 19),
                    GioiTinh = "Nam",
                    Email = "bao.duong@univ.edu.vn",
                    SoDienThoai = "0909876543",
                    DiaChi = "15 Quang Trung, Vũng Tàu",
                    DiemTrungBinh = 4.6,
                    TrangThai = "Đang học",
                    LopHocId = dtvt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-20)
                },
                new SinhVien
                {
                    MaSV = "SV2024011",
                    HoTen = "Lý Mỹ Linh",
                    NgaySinh = new DateTime(2003, 12, 3),
                    GioiTinh = "Nữ",
                    Email = "linh.ly@univ.edu.vn",
                    SoDienThoai = "0932109876",
                    DiaChi = "67 Pasteur, Quận 3, TP.HCM",
                    DiemTrungBinh = 8.9,
                    TrangThai = "Đang học",
                    LopHocId = dtvt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-15)
                },
                new SinhVien
                {
                    MaSV = "SV2024012",
                    HoTen = "Đặng Nhật Huy",
                    NgaySinh = new DateTime(2001, 2, 27),
                    GioiTinh = "Nam",
                    Email = "huy.dang@univ.edu.vn",
                    SoDienThoai = "0981234567",
                    DiaChi = "102 Kim Mã, Ba Đình, Hà Nội",
                    DiemTrungBinh = 9.1,
                    TrangThai = "Đã tốt nghiệp",
                    LopHocId = cntt01Id,
                    NgayTao = DateTime.UtcNow.AddDays(-10)
                }
            };

            context.SinhViens.AddRange(sinhViens);
            context.SaveChanges();
        }
    }
}
