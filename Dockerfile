# Multi-stage Dockerfile cho ASP.NET Core 9 Web API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file csproj và restore dependencies trước để tận dụng cache
COPY ["QuanLySinhVienAPI.csproj", "./"]
RUN dotnet restore "QuanLySinhVienAPI.csproj"

# Copy toàn bộ mã nguồn và build release
COPY . .
RUN dotnet publish "QuanLySinhVienAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image siêu nhẹ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng chạy mặc định cho môi trường Cloud (Render, Railway, Fly.io, Azure)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "QuanLySinhVienAPI.dll"]
