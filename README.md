# Hệ Thống Quản Lý Sinh Viên - ASP.NET Core RESTful Web API

Ứng dụng quản lý sinh viên được xây dựng hoàn chỉnh theo chuẩn kiến trúc dịch vụ web **RESTful Web API** trên nền tảng **ASP.NET Core (.NET 9)**, sử dụng **Entity Framework Core**, cơ sở dữ liệu **SQLite**, tài liệu tương tác **Scalar / OpenAPI**, cùng giao diện **Single Page Application (SPA)** trực quan, hiện đại.

---

## 🚀 Tính Năng Chính

1. **Chuẩn RESTful Web API**:
   - Tuân thủ đầy đủ các quy chuẩn REST: sử dụng đúng các HTTP Verbs (`GET`, `POST`, `PUT`, `DELETE`).
   - Mã trạng thái chuẩn HTTP: `200 OK`, `201 Created`, `400 Bad Request`, `404 Not Found`, `500 Internal Server Error`.
   - Định dạng dữ liệu trao đổi: JSON với wrapper chuẩn `ApiResponse<T>`.

2. **Quản Lý Sinh Viên (`/api/sinhvien`)**:
   - `GET /api/sinhvien`: Tìm kiếm từ khóa (Mã SV, họ tên, email, SĐT), lọc theo lớp, giới tính, trạng thái, xếp loại học lực; hỗ trợ sắp xếp động và phân trang mượt mà (`PagedResult<T>`).
   - `GET /api/sinhvien/{id}`: Xem chi tiết 1 sinh viên kèm thông tin lớp và khoa đào tạo.
   - `GET /api/sinhvien/check-masv?masv=...`: Kiểm tra nhanh sự tồn tại của Mã sinh viên.
   - `POST /api/sinhvien`: Thêm mới sinh viên kèm xác thực tính duy nhất của Mã SV, Email, kiểm tra lớp học hợp lệ, phổ điểm từ 0.0 đến 10.0.
   - `PUT /api/sinhvien/{id}`: Cập nhật thông tin sinh viên, kiểm tra trùng lặp thông tin với sinh viên khác.
   - `DELETE /api/sinhvien/{id}`: Xóa sinh viên khỏi hệ thống.

3. **Quản Lý Lớp Học (`/api/lophoc`)**:
   - `GET /api/lophoc`: Lấy danh sách lớp học kèm sĩ số sinh viên của từng lớp.
   - `GET /api/lophoc/{id}`: Chi tiết lớp học.
   - `POST /api/lophoc`: Thêm lớp học mới (kiểm tra trùng mã lớp).
   - `PUT /api/lophoc/{id}`: Cập nhật thông tin lớp.
   - `DELETE /api/lophoc/{id}`: Xóa lớp học (ràng buộc khóa ngoại an toàn: từ chối xóa nếu lớp đang có sinh viên).

4. **Báo Cáo & Thống Kê (`/api/thongke/tong-quan`)**:
   - Tổng hợp số lượng sinh viên, số lớp học, điểm trung bình toàn trường.
   - Phân loại học lực: Xuất sắc, Giỏi, Khá, Trung bình, Yếu.
   - Thống kê tình trạng học vụ: Đang học, Bảo lưu, Đã tốt nghiệp, Đình chỉ.
   - Thống kê quy mô sĩ số và điểm trung bình theo từng lớp học.

5. **Giao Diện Web UI Hiện Đại (Single Page Application)**:
   - Tích hợp sẵn trong thư mục `wwwroot` phục vụ trực tiếp qua Web API.
   - Phong cách thiết kế Glassmorphism, hỗ trợ cả 2 chế độ **Giao diện Tối (Dark)** và **Giao diện Sáng (Light)**.
   - Thẻ số liệu tổng quan trực quan, bảng dữ liệu sinh viên có phân trang, bộ lọc nhanh, tìm kiếm debounce.
   - Modal thêm / sửa sinh viên và lớp học, hộp thoại xác nhận xóa, thông báo Toast góc màn hình.
   - Tích hợp bảng điều khiển thử nghiệm API (RESTful API Explorer) gửi request trực tiếp và xem JSON phản hồi.

6. **Tài Liệu API Tương Tác (OpenAPI & Scalar / Swagger)**:
   - Hỗ trợ xem trực quan và thử nghiệm từng API endpoint tại `/scalar/v1` (tự động chuyển hướng từ `/swagger`).

---

## 📂 Cấu Trúc Dự Án

```
QuanLySinhVienAPI/
├── Controllers/
│   ├── SinhVienController.cs     # API điều khiển Quản lý Sinh viên
│   ├── LopHocController.cs       # API điều khiển Quản lý Lớp học
│   └── ThongKeController.cs      # API Báo cáo & Thống kê
├── Data/
│   ├── AppDbContext.cs           # Entity Framework Core DbContext
│   └── DbInitializer.cs          # Tự động khởi tạo dữ liệu mẫu ban đầu
├── DTOs/
│   ├── ApiResponse.cs            # Cấu trúc phản hồi JSON chuẩn hóa
│   ├── PagedResult.cs            # Cấu trúc dữ liệu phân trang
│   ├── SinhVienDtos.cs           # DTO xem, thêm, sửa sinh viên
│   ├── LopHocDtos.cs             # DTO xem, thêm, sửa lớp học
│   └── ThongKeDtos.cs            # DTO thống kê số liệu
├── Models/
│   ├── SinhVien.cs               # Thực thể Sinh viên
│   └── LopHoc.cs                 # Thực thể Lớp học
├── wwwroot/                      # Giao diện Web SPA
│   ├── css/style.css             # Hệ thống phong cách Glassmorphism
│   ├── js/app.js                 # Xử lý logic gọi API, lọc, modals
│   └── index.html                # Trang bảng điều khiển chính
├── Properties/
│   └── launchSettings.json       # Cấu hình cổng chạy ứng dụng
├── Program.cs                    # Thiết lập Pipeline, DI, CSDL, CORS, OpenAPI
├── appsettings.json              # Cấu hình chuỗi kết nối SQLite
└── QuanLySinhVienAPI.csproj      # File cấu hình dự án .NET 9
```

---

## 🛠️ Hướng Dẫn Chạy Ứng Dụng

### 1. Yêu Cầu Môi Trường
- [.NET SDK 8.0, 9.0 hoặc 10.0](https://dotnet.microsoft.com/download) đã được cài đặt trên máy.

### 2. Khởi Chạy Ứng Dụng
Mở Terminal / PowerShell tại thư mục dự án và thực hiện:

```bash
cd d:\webapi\QuanLySinhVienAPI
dotnet run --launch-profile http
```

### 3. Truy Cập Các Liên Kết
- **Giao diện Dashboard Web Quản lý:** [http://localhost:5296/](http://localhost:5296/)
- **Tài liệu API tương tác (Scalar UI / Swagger):** [http://localhost:5296/scalar/v1](http://localhost:5296/scalar/v1) hoặc [http://localhost:5296/swagger](http://localhost:5296/swagger)

---

## 📊 Danh Sách Các Endpoint RESTful API

| Method | Endpoint | Mô Tả |
| :--- | :--- | :--- |
| `GET` | `/api/sinhvien` | Danh sách sinh viên (hỗ trợ `searchTerm`, `lopHocId`, `gioiTinh`, `trangThai`, `xepLoai`, `sortBy`, `isDescending`, `pageIndex`, `pageSize`) |
| `GET` | `/api/sinhvien/{id}` | Chi tiết một sinh viên theo ID |
| `GET` | `/api/sinhvien/check-masv` | Kiểm tra nhanh mã sinh viên đã tồn tại chưa |
| `POST` | `/api/sinhvien` | Thêm sinh viên mới |
| `PUT` | `/api/sinhvien/{id}` | Cập nhật thông tin sinh viên |
| `DELETE` | `/api/sinhvien/{id}` | Xóa sinh viên |
| `GET` | `/api/lophoc` | Danh sách các lớp học kèm số lượng sinh viên |
| `GET` | `/api/lophoc/{id}` | Chi tiết một lớp học theo ID |
| `POST` | `/api/lophoc` | Thêm mới lớp học |
| `PUT` | `/api/lophoc/{id}` | Cập nhật thông tin lớp học |
| `DELETE` | `/api/lophoc/{id}` | Xóa lớp học (chặn xóa nếu lớp có sinh viên) |
| `GET` | `/api/thongke/tong-quan` | Báo cáo thống kê toàn trường |
