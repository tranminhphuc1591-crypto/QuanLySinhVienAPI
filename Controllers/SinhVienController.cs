using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVienAPI.Data;
using QuanLySinhVienAPI.DTOs;
using QuanLySinhVienAPI.Models;

namespace QuanLySinhVienAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SinhVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SinhVienController> _logger;

        public SinhVienController(AppDbContext context, ILogger<SinhVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách sinh viên có phân trang, tìm kiếm, lọc và sắp xếp
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<SinhVienDto>>>> GetAll([FromQuery] SinhVienQueryParameters query)
        {
            try
            {
                var q = _context.SinhViens.Include(s => s.LopHoc).AsQueryable();

                // Tìm kiếm theo từ khóa (Mã SV, Họ tên, Email, Số điện thoại)
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim().ToLower();
                    q = q.Where(s =>
                        s.MaSV.ToLower().Contains(term) ||
                        s.HoTen.ToLower().Contains(term) ||
                        s.Email.ToLower().Contains(term) ||
                        s.SoDienThoai.Contains(term));
                }

                // Lọc theo Lớp
                if (query.LopHocId.HasValue && query.LopHocId.Value > 0)
                {
                    q = q.Where(s => s.LopHocId == query.LopHocId.Value);
                }

                // Lọc theo Giới tính
                if (!string.IsNullOrWhiteSpace(query.GioiTinh))
                {
                    q = q.Where(s => s.GioiTinh == query.GioiTinh);
                }

                // Lọc theo Trạng thái
                if (!string.IsNullOrWhiteSpace(query.TrangThai))
                {
                    q = q.Where(s => s.TrangThai == query.TrangThai);
                }

                // Lọc theo Xếp loại học lực
                if (!string.IsNullOrWhiteSpace(query.XepLoai))
                {
                    switch (query.XepLoai)
                    {
                        case "Xuất sắc":
                            q = q.Where(s => s.DiemTrungBinh >= 9.0);
                            break;
                        case "Giỏi":
                            q = q.Where(s => s.DiemTrungBinh >= 8.0 && s.DiemTrungBinh < 9.0);
                            break;
                        case "Khá":
                            q = q.Where(s => s.DiemTrungBinh >= 6.5 && s.DiemTrungBinh < 8.0);
                            break;
                        case "Trung bình":
                            q = q.Where(s => s.DiemTrungBinh >= 5.0 && s.DiemTrungBinh < 6.5);
                            break;
                        case "Yếu":
                            q = q.Where(s => s.DiemTrungBinh < 5.0);
                            break;
                    }
                }

                // Sắp xếp
                q = query.SortBy.ToLower() switch
                {
                    "masv" => query.IsDescending ? q.OrderByDescending(s => s.MaSV) : q.OrderBy(s => s.MaSV),
                    "hoten" => query.IsDescending ? q.OrderByDescending(s => s.HoTen) : q.OrderBy(s => s.HoTen),
                    "diemtrungbinh" => query.IsDescending ? q.OrderByDescending(s => s.DiemTrungBinh) : q.OrderBy(s => s.DiemTrungBinh),
                    "ngaysinh" => query.IsDescending ? q.OrderByDescending(s => s.NgaySinh) : q.OrderBy(s => s.NgaySinh),
                    "ngaytao" => query.IsDescending ? q.OrderByDescending(s => s.NgayTao) : q.OrderBy(s => s.NgayTao),
                    _ => query.IsDescending ? q.OrderByDescending(s => s.Id) : q.OrderBy(s => s.Id)
                };

                // Tổng số bản ghi thỏa mãn điều kiện
                var totalCount = await q.CountAsync();

                // Phân trang
                var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
                var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

                var items = await q
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SinhVienDto
                    {
                        Id = s.Id,
                        MaSV = s.MaSV,
                        HoTen = s.HoTen,
                        NgaySinh = s.NgaySinh,
                        GioiTinh = s.GioiTinh,
                        Email = s.Email,
                        SoDienThoai = s.SoDienThoai,
                        DiaChi = s.DiaChi,
                        DiemTrungBinh = s.DiemTrungBinh,
                        XepLoai = SinhVienDto.TinhXepLoai(s.DiemTrungBinh),
                        TrangThai = s.TrangThai,
                        NgayTao = s.NgayTao,
                        LopHocId = s.LopHocId,
                        MaLop = s.LopHoc != null ? s.LopHoc.MaLop : string.Empty,
                        TenLop = s.LopHoc != null ? s.LopHoc.TenLop : string.Empty,
                        Khoa = s.LopHoc != null ? s.LopHoc.Khoa : string.Empty
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<SinhVienDto>(items, totalCount, pageIndex, pageSize);
                return Ok(ApiResponse<PagedResult<SinhVienDto>>.Ok(pagedResult, "Lấy danh sách sinh viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn danh sách sinh viên");
                return StatusCode(500, ApiResponse<PagedResult<SinhVienDto>>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Lấy chi tiết thông tin một sinh viên theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SinhVienDto>>> GetById(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens
                    .Include(s => s.LopHoc)
                    .Where(s => s.Id == id)
                    .Select(s => new SinhVienDto
                    {
                        Id = s.Id,
                        MaSV = s.MaSV,
                        HoTen = s.HoTen,
                        NgaySinh = s.NgaySinh,
                        GioiTinh = s.GioiTinh,
                        Email = s.Email,
                        SoDienThoai = s.SoDienThoai,
                        DiaChi = s.DiaChi,
                        DiemTrungBinh = s.DiemTrungBinh,
                        XepLoai = SinhVienDto.TinhXepLoai(s.DiemTrungBinh),
                        TrangThai = s.TrangThai,
                        NgayTao = s.NgayTao,
                        LopHocId = s.LopHocId,
                        MaLop = s.LopHoc != null ? s.LopHoc.MaLop : string.Empty,
                        TenLop = s.LopHoc != null ? s.LopHoc.TenLop : string.Empty,
                        Khoa = s.LopHoc != null ? s.LopHoc.Khoa : string.Empty
                    })
                    .FirstOrDefaultAsync();

                if (sinhVien == null)
                {
                    return NotFound(ApiResponse<SinhVienDto>.Fail($"Không tìm thấy sinh viên với Id: {id}"));
                }

                return Ok(ApiResponse<SinhVienDto>.Ok(sinhVien, "Lấy thông tin sinh viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sinh viên {Id}", id);
                return StatusCode(500, ApiResponse<SinhVienDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Kiểm tra nhanh Mã sinh viên đã tồn tại hay chưa
        /// </summary>
        [HttpGet("check-masv")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckMaSV([FromQuery] string masv, [FromQuery] int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(masv))
            {
                return BadRequest(ApiResponse<bool>.Fail("Vui lòng cung cấp mã sinh viên"));
            }

            var query = _context.SinhViens.Where(s => s.MaSV.ToLower() == masv.Trim().ToLower());
            if (excludeId.HasValue && excludeId.Value > 0)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync();
            return Ok(ApiResponse<bool>.Ok(exists, exists ? "Mã sinh viên đã tồn tại" : "Mã sinh viên hợp lệ"));
        }

        /// <summary>
        /// Thêm mới một sinh viên
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SinhVienDto>>> Create([FromBody] CreateSinhVienDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SinhVienDto>.Fail("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                // Kiểm tra trùng Mã SV
                var existsMaSV = await _context.SinhViens.AnyAsync(s => s.MaSV.ToLower() == dto.MaSV.Trim().ToLower());
                if (existsMaSV)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Mã sinh viên '{dto.MaSV}' đã tồn tại trong hệ thống"));
                }

                // Kiểm tra trùng Email
                var existsEmail = await _context.SinhViens.AnyAsync(s => s.Email.ToLower() == dto.Email.Trim().ToLower());
                if (existsEmail)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Email '{dto.Email}' đã được đăng ký bởi sinh viên khác"));
                }

                // Kiểm tra Lớp học có tồn tại
                var lopHoc = await _context.LopHocs.FindAsync(dto.LopHocId);
                if (lopHoc == null)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Lớp học được chọn (Id: {dto.LopHocId}) không tồn tại"));
                }

                var sinhVien = new SinhVien
                {
                    MaSV = dto.MaSV.Trim().ToUpper(),
                    HoTen = dto.HoTen.Trim(),
                    NgaySinh = dto.NgaySinh,
                    GioiTinh = dto.GioiTinh.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    SoDienThoai = dto.SoDienThoai.Trim(),
                    DiaChi = dto.DiaChi?.Trim(),
                    DiemTrungBinh = Math.Round(dto.DiemTrungBinh, 2),
                    TrangThai = dto.TrangThai.Trim(),
                    LopHocId = dto.LopHocId,
                    NgayTao = DateTime.UtcNow
                };

                _context.SinhViens.Add(sinhVien);
                await _context.SaveChangesAsync();

                var result = new SinhVienDto
                {
                    Id = sinhVien.Id,
                    MaSV = sinhVien.MaSV,
                    HoTen = sinhVien.HoTen,
                    NgaySinh = sinhVien.NgaySinh,
                    GioiTinh = sinhVien.GioiTinh,
                    Email = sinhVien.Email,
                    SoDienThoai = sinhVien.SoDienThoai,
                    DiaChi = sinhVien.DiaChi,
                    DiemTrungBinh = sinhVien.DiemTrungBinh,
                    XepLoai = SinhVienDto.TinhXepLoai(sinhVien.DiemTrungBinh),
                    TrangThai = sinhVien.TrangThai,
                    NgayTao = sinhVien.NgayTao,
                    LopHocId = lopHoc.Id,
                    MaLop = lopHoc.MaLop,
                    TenLop = lopHoc.TenLop,
                    Khoa = lopHoc.Khoa
                };

                return CreatedAtAction(nameof(GetById), new { id = sinhVien.Id }, ApiResponse<SinhVienDto>.Ok(result, "Thêm mới sinh viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sinh viên mới");
                return StatusCode(500, ApiResponse<SinhVienDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật thông tin sinh viên theo Id
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SinhVienDto>>> Update(int id, [FromBody] UpdateSinhVienDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SinhVienDto>.Fail("Dữ liệu không hợp lệ", errors));
            }

            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    return NotFound(ApiResponse<SinhVienDto>.Fail($"Không tìm thấy sinh viên với Id: {id}"));
                }

                // Kiểm tra trùng Mã SV với sinh viên khác
                var existsMaSV = await _context.SinhViens.AnyAsync(s => s.Id != id && s.MaSV.ToLower() == dto.MaSV.Trim().ToLower());
                if (existsMaSV)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Mã sinh viên '{dto.MaSV}' đã được dùng cho sinh viên khác"));
                }

                // Kiểm tra trùng Email với sinh viên khác
                var existsEmail = await _context.SinhViens.AnyAsync(s => s.Id != id && s.Email.ToLower() == dto.Email.Trim().ToLower());
                if (existsEmail)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Email '{dto.Email}' đã được đăng ký bởi sinh viên khác"));
                }

                // Kiểm tra Lớp học có tồn tại
                var lopHoc = await _context.LopHocs.FindAsync(dto.LopHocId);
                if (lopHoc == null)
                {
                    return BadRequest(ApiResponse<SinhVienDto>.Fail($"Lớp học được chọn (Id: {dto.LopHocId}) không tồn tại"));
                }

                sinhVien.MaSV = dto.MaSV.Trim().ToUpper();
                sinhVien.HoTen = dto.HoTen.Trim();
                sinhVien.NgaySinh = dto.NgaySinh;
                sinhVien.GioiTinh = dto.GioiTinh.Trim();
                sinhVien.Email = dto.Email.Trim().ToLower();
                sinhVien.SoDienThoai = dto.SoDienThoai.Trim();
                sinhVien.DiaChi = dto.DiaChi?.Trim();
                sinhVien.DiemTrungBinh = Math.Round(dto.DiemTrungBinh, 2);
                sinhVien.TrangThai = dto.TrangThai.Trim();
                sinhVien.LopHocId = dto.LopHocId;

                await _context.SaveChangesAsync();

                var result = new SinhVienDto
                {
                    Id = sinhVien.Id,
                    MaSV = sinhVien.MaSV,
                    HoTen = sinhVien.HoTen,
                    NgaySinh = sinhVien.NgaySinh,
                    GioiTinh = sinhVien.GioiTinh,
                    Email = sinhVien.Email,
                    SoDienThoai = sinhVien.SoDienThoai,
                    DiaChi = sinhVien.DiaChi,
                    DiemTrungBinh = sinhVien.DiemTrungBinh,
                    XepLoai = SinhVienDto.TinhXepLoai(sinhVien.DiemTrungBinh),
                    TrangThai = sinhVien.TrangThai,
                    NgayTao = sinhVien.NgayTao,
                    LopHocId = lopHoc.Id,
                    MaLop = lopHoc.MaLop,
                    TenLop = lopHoc.TenLop,
                    Khoa = lopHoc.Khoa
                };

                return Ok(ApiResponse<SinhVienDto>.Ok(result, "Cập nhật sinh viên thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sinh viên {Id}", id);
                return StatusCode(500, ApiResponse<SinhVienDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Xóa một sinh viên theo Id
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    return NotFound(ApiResponse<bool>.Fail($"Không tìm thấy sinh viên với Id: {id}"));
                }

                _context.SinhViens.Remove(sinhVien);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, $"Đã xóa sinh viên '{sinhVien.HoTen}' ({sinhVien.MaSV}) thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sinh viên {Id}", id);
                return StatusCode(500, ApiResponse<bool>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }
    }
}
