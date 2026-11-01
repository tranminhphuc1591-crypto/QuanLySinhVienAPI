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
    public class LopHocController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LopHocController> _logger;

        public LopHocController(AppDbContext context, ILogger<LopHocController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các lớp học kèm số lượng sinh viên
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<LopHocDto>>>> GetAll()
        {
            try
            {
                var lopHocs = await _context.LopHocs
                    .Include(l => l.SinhViens)
                    .Select(l => new LopHocDto
                    {
                        Id = l.Id,
                        MaLop = l.MaLop,
                        TenLop = l.TenLop,
                        Khoa = l.Khoa,
                        MoTa = l.MoTa,
                        NgayTao = l.NgayTao,
                        SoLuongSinhVien = l.SinhViens.Count
                    })
                    .OrderBy(l => l.TenLop)
                    .ToListAsync();

                return Ok(ApiResponse<List<LopHocDto>>.Ok(lopHocs, "Lấy danh sách lớp học thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lớp học");
                return StatusCode(500, ApiResponse<List<LopHocDto>>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một lớp học theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LopHocDto>>> GetById(int id)
        {
            try
            {
                var lopHoc = await _context.LopHocs
                    .Include(l => l.SinhViens)
                    .Where(l => l.Id == id)
                    .Select(l => new LopHocDto
                    {
                        Id = l.Id,
                        MaLop = l.MaLop,
                        TenLop = l.TenLop,
                        Khoa = l.Khoa,
                        MoTa = l.MoTa,
                        NgayTao = l.NgayTao,
                        SoLuongSinhVien = l.SinhViens.Count
                    })
                    .FirstOrDefaultAsync();

                if (lopHoc == null)
                {
                    return NotFound(ApiResponse<LopHocDto>.Fail($"Không tìm thấy lớp học với mã Id: {id}"));
                }

                return Ok(ApiResponse<LopHocDto>.Ok(lopHoc, "Lấy thông tin lớp học thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết lớp học {Id}", id);
                return StatusCode(500, ApiResponse<LopHocDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Tạo một lớp học mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<LopHocDto>>> Create([FromBody] CreateLopHocDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<LopHocDto>.Fail("Dữ liệu đầu vào không hợp lệ", errors));
            }

            try
            {
                // Kiểm tra trùng mã lớp
                var exists = await _context.LopHocs.AnyAsync(l => l.MaLop.ToLower() == dto.MaLop.Trim().ToLower());
                if (exists)
                {
                    return BadRequest(ApiResponse<LopHocDto>.Fail($"Mã lớp '{dto.MaLop}' đã tồn tại trong hệ thống"));
                }

                var lopHoc = new LopHoc
                {
                    MaLop = dto.MaLop.Trim().ToUpper(),
                    TenLop = dto.TenLop.Trim(),
                    Khoa = dto.Khoa.Trim(),
                    MoTa = dto.MoTa?.Trim(),
                    NgayTao = DateTime.UtcNow
                };

                _context.LopHocs.Add(lopHoc);
                await _context.SaveChangesAsync();

                var result = new LopHocDto
                {
                    Id = lopHoc.Id,
                    MaLop = lopHoc.MaLop,
                    TenLop = lopHoc.TenLop,
                    Khoa = lopHoc.Khoa,
                    MoTa = lopHoc.MoTa,
                    NgayTao = lopHoc.NgayTao,
                    SoLuongSinhVien = 0
                };

                return CreatedAtAction(nameof(GetById), new { id = lopHoc.Id }, ApiResponse<LopHocDto>.Ok(result, "Thêm lớp học thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lớp học mới");
                return StatusCode(500, ApiResponse<LopHocDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật thông tin lớp học theo Id
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<LopHocDto>>> Update(int id, [FromBody] UpdateLopHocDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<LopHocDto>.Fail("Dữ liệu đầu vào không hợp lệ", errors));
            }

            try
            {
                var lopHoc = await _context.LopHocs.FindAsync(id);
                if (lopHoc == null)
                {
                    return NotFound(ApiResponse<LopHocDto>.Fail($"Không tìm thấy lớp học với mã Id: {id}"));
                }

                // Kiểm tra trùng mã lớp với các lớp khác
                var exists = await _context.LopHocs.AnyAsync(l => l.Id != id && l.MaLop.ToLower() == dto.MaLop.Trim().ToLower());
                if (exists)
                {
                    return BadRequest(ApiResponse<LopHocDto>.Fail($"Mã lớp '{dto.MaLop}' đã được sử dụng bởi lớp khác"));
                }

                lopHoc.MaLop = dto.MaLop.Trim().ToUpper();
                lopHoc.TenLop = dto.TenLop.Trim();
                lopHoc.Khoa = dto.Khoa.Trim();
                lopHoc.MoTa = dto.MoTa?.Trim();

                await _context.SaveChangesAsync();

                var count = await _context.SinhViens.CountAsync(s => s.LopHocId == id);

                var result = new LopHocDto
                {
                    Id = lopHoc.Id,
                    MaLop = lopHoc.MaLop,
                    TenLop = lopHoc.TenLop,
                    Khoa = lopHoc.Khoa,
                    MoTa = lopHoc.MoTa,
                    NgayTao = lopHoc.NgayTao,
                    SoLuongSinhVien = count
                };

                return Ok(ApiResponse<LopHocDto>.Ok(result, "Cập nhật thông tin lớp học thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lớp học {Id}", id);
                return StatusCode(500, ApiResponse<LopHocDto>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }

        /// <summary>
        /// Xóa lớp học theo Id (Không cho xóa nếu còn sinh viên trong lớp)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var lopHoc = await _context.LopHocs
                    .Include(l => l.SinhViens)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lopHoc == null)
                {
                    return NotFound(ApiResponse<bool>.Fail($"Không tìm thấy lớp học với mã Id: {id}"));
                }

                if (lopHoc.SinhViens.Any())
                {
                    return BadRequest(ApiResponse<bool>.Fail($"Không thể xóa lớp học '{lopHoc.TenLop}' vì vẫn còn {lopHoc.SinhViens.Count} sinh viên đang theo học."));
                }

                _context.LopHocs.Remove(lopHoc);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, $"Đã xóa lớp học '{lopHoc.TenLop}' thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lớp học {Id}", id);
                return StatusCode(500, ApiResponse<bool>.Fail("Lỗi hệ thống: " + ex.Message));
            }
        }
    }
}
