using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using QuanLySinhVienAPI.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Hỗ trợ cấu hình cổng linh hoạt cho Cloud (Render, Railway, Heroku)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Thêm dịch vụ Controllers
builder.Services.AddControllers();

// Cấu hình CSDL SQLite qua Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Cấu hình OpenAPI chuẩn của .NET 9
builder.Services.AddOpenApi();

var app = builder.Build();

// Khởi tạo Database và Seed dữ liệu mẫu nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu ban đầu.");
    }
}

// Kích hoạt OpenAPI endpoint và giao diện Scalar API Documentation
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("EduManager - RESTful Web API Documentation")
           .WithTheme(ScalarTheme.Moon);
});

// Xử lý proxy headers khi chạy sau reverse proxy trên Render / Cloud
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Chuyển tiếp route /swagger sang /scalar/v1 để tương thích
app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"));

app.UseCors("AllowAll");

// Cho phép phục vụ Single Page Web App từ thư mục wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
