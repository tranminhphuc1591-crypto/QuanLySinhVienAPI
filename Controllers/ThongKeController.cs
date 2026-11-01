using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVienAPI.Data;
using QuanLySinhVienAPI.DTOs;

namespace QuanLySinhVienAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ThongKeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ThongKeController> _logger;

        public ThongKeController(AppDbContext context, ILogger<ThongKeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy số liệu thống kê tổng quan toàn hệ thống
        /// </summary>
        [HttpGet("tong-quan")]
        public async Task<ActionResult<ApiResponse<ThongKeTongQuanDto>>> GetTongQuan()
        {
            try
            {
                var sinhViens = await _context.SinhViens.ToListAsync();
                var lopHocs = await _context.LopHocs.Include(l => l.SinhViens).ToListAsync();

                var tongSV = sinhViens.Count;
                var diemTrungBinh = tongSV > 0 ? Math.Round(sinhViens.Average(s => s.DiemTrungBinh), 2) : 0.0;

                var thongKeLop = lopHocs.Select(l => new LopHocThongKeItemDto
                {
                    LopHocId = l.Id,
                    MaLop = l.MaLop,
                    TenLop = l.TenLop,
                    Khoa = l.Khoa,
                    SoLuongSinhVien = l.SinhViens.Count,
                    DiemTrungBinhLop = l.SinhViens.Any() ? Math.Round(l.SinhViens.Average(s => s.DiemTrungBinh), 2) : 0.0
                }).OrderByDescending(l => l.SoLuongSinhVien).ToList();

                var result = new ThongKeTongQuanDto
                {
                    TongSinhVien = tongSV,
                    TongLopHoc = lopHocs.Count,
                    DiemTrungBinhChung = diemTrungBinh,
                    SoLuongDangHoc = sinhViens.Count(s => s.TrangThai == "Đang học"),
                    SoLuongBaoLuu = sinhViens.Count(s => s.TrangThai == "Bảo lưu"),
                    SoLuongDaTotNghiep = sinhViens.Count(s => s.TrangThai == "Đã tốt nghiệp"),
                    SoLuongDinhChi = sinhViens.Count(s => s.TrangThai == "Đình chỉ"),
                    SoLuongXuatSac = sinhViens.Count(s => s.DiemTrungBinh >= 9.0),
                    SoLuongGioi = sinhViens.Count(s => s.DiemTrungBinh >= 8.0 && s.DiemTrungBinh < 9.0),
                    SoLuongKha = sinhViens.Count(s => s.DiemTrungBinh >= 6.5 && s.DiemTrungBinh < 8.0),
                    SoLuongTrungBinh = sinhViens.Count(s => s.DiemTrungBinh >= 5.0 && s.DiemTrungBinh < 6.5),
                    SoLuongYeu = sinhViens.Count(s => s.DiemTrungBinh < 5.0),
                    ThongKeTheoLop = thongKeLop
                };

                return Ok(ApiResponse<ThongKeTongQuanDto>.Ok(result, "Thống kê tổng quan thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo thống kê");
                return StatusCode(500, ApiResponse<ThongKeTongQuanDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }
    }
}
