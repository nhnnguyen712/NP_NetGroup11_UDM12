# Thay đổi Port Server - Hướng dẫn Chi tiết

## 📋 Tóm tắt
Server hỗ trợ 3 cách để thay đổi port:
1. **Command line argument** (Nhanh & Dễ)
2. **Environment variable** (Cho production)
3. **Default port** (8888)

---

## 🚀 **Cách 1: Command Line Argument (Đơn giản nhất)**

### Windows
```powershell
cd Code\MultiFileDownloader.Server

# Chạy với port khác (ví dụ 9999)
dotnet run 9999

# Hoặc nếu đã publish:
cd bin\Release\net10.0\publish
.\MultiFileDownloader.Server.exe 9999
```

### Linux/Mac
```bash
cd Code/MultiFileDownloader.Server

# Chạy với port khác
dotnet run 9999

# Hoặc:
./bin/Release/net10.0/publish/MultiFileDownloader.Server 9999
```

**Kết quả:**
```
╔═══════════════════════════════════════════════════════════╗
║        🔗 Multi File Downloader Server                    ║
╠═══════════════════════════════════════════════════════════╣
║  📍 Server IP (Localhost): 127.0.0.1:9999               ║
║  📍 Server IP (Network):   192.168.1.100:9999           ║
║  ✅ Waiting for client connections...                     ║
╚═══════════════════════════════════════════════════════════╝
```

---

## 🔧 **Cách 2: Environment Variable (Cho Production)**

### Windows (PowerShell)
```powershell
# Set environment variable
$env:FILE_DOWNLOADER_PORT = "9999"

# Chạy server (sẽ dùng port từ env variable)
dotnet run
# hoặc
.\MultiFileDownloader.Server.exe

# Xác nhận
Write-Host $env:FILE_DOWNLOADER_PORT
```

### Windows (Command Prompt)
```cmd
REM Set environment variable
set FILE_DOWNLOADER_PORT=9999

REM Chạy server
dotnet run
REM hoặc
MultiFileDownloader.Server.exe

REM Xác nhận
echo %FILE_DOWNLOADER_PORT%
```

### Linux/Mac (Bash)
```bash
# Set environment variable
export FILE_DOWNLOADER_PORT=9999

# Chạy server
dotnet run
# hoặc
./MultiFileDownloader.Server

# Xác nhận
echo $FILE_DOWNLOADER_PORT
```

### Permanent (Persistent) - Windows
```powershell
# Set user environment variable (permanent)
[System.Environment]::SetEnvironmentVariable('FILE_DOWNLOADER_PORT', '9999', 'User')

# Hoặc system variable
[System.Environment]::SetEnvironmentVariable('FILE_DOWNLOADER_PORT', '9999', 'Machine')

# Sau khi set, cần restart PowerShell hoặc app để apply
```

### Permanent - Linux/Mac
```bash
# Thêm vào ~/.bashrc hoặc ~/.zshrc
echo 'export FILE_DOWNLOADER_PORT=9999' >> ~/.bashrc
source ~/.bashrc

# Hoặc thêm vào ~/.bash_profile
echo 'export FILE_DOWNLOADER_PORT=9999' >> ~/.bash_profile
source ~/.bash_profile
```

---

## 📝 **Cách 3: Windows Service (Tự động start với port tùy chỉnh)**

### Sử dụng NSSM

```powershell
# Tạo service với port 9999
cd C:\Tools\nssm-2.24\win64

# Cách A: Chỉ định port trong command
.\nssm install FileDownloaderService `
    "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 9999"

# Cách B: Sử dụng environment variable
.\nssm install FileDownloaderService `
    "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

# Sau đó set environment variable cho service:
.\nssm set FileDownloaderService AppEnvironmentExtra FILE_DOWNLOADER_PORT=9999
```

### Kiểm tra service
```powershell
# Xem config của service
.\nssm get FileDownloaderService AppEnvironmentExtra

# Start service
Start-Service -Name FileDownloaderService

# Check status
Get-Service FileDownloaderService
```

---

## ⚡ **Ví dụ thực tế**

### Scenario 1: Port 8888 bị chiếm, muốn dùng port 9999

**Cách nhanh nhất:**
```powershell
# Windows
dotnet run 9999

# Linux
dotnet run 9999

# ✓ Server sẽ chạy trên port 9999 ngay lập tức
```

### Scenario 2: Production server, set permanent port

```bash
# Linux Server
export FILE_DOWNLOADER_PORT=8080
echo 'export FILE_DOWNLOADER_PORT=8080' >> ~/.bashrc
source ~/.bashrc

# Chạy server
nohup dotnet bin/Release/net10.0/publish/MultiFileDownloader.Server > server.log 2>&1 &

# Server sẽ luôn dùng port 8080
```

### Scenario 3: Windows Server 2012, chạy dưới dạng service với port khác

```powershell
# Bước 1: Tạo service với port
cd C:\Tools\nssm-2.24\win64
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 7777"

# Bước 2: Set auto start
Set-Service -Name FileDownloaderService -StartupType Automatic

# Bước 3: Mở firewall cho port 7777
New-NetFirewallRule -DisplayName "File Downloader Port 7777" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7777

# Bước 4: Start service
Start-Service -Name FileDownloaderService

# Bước 5: Verify
Get-Service FileDownloaderService
Get-NetTCPConnection -LocalPort 7777
```

---

## 🧪 **Test Port Có Hoạt động Không**

### Windows
```powershell
# Test port 9999
Test-NetConnection -ComputerName localhost -Port 9999

# Hoặc
telnet localhost 9999

# Hoặc check port nào đang listen
netstat -ano | findstr :9999

# Xem chi tiết
Get-NetTCPConnection -LocalPort 9999
```

### Linux/Mac
```bash
# Test port 9999
nc -zv localhost 9999

# Hoặc
telnet localhost 9999

# Hoặc
netstat -tlnp | grep :9999

# Hoặc (modern):
ss -tlnp | grep :9999

# Xem process chiếm port
lsof -i :9999
```

---

## 🔍 **Tìm Port Khác Nếu Port Đang Bị Chiếm**

### Windows
```powershell
# Tìm process chiếm port 8888
netstat -ano | findstr :8888

# Kết quả: TCP 0.0.0.0:8888 LISTENING 5432 (PID: 5432)

# Kill process (nếu cần)
taskkill /PID 5432 /F

# Hoặc tìm port nào còn trống
1..10000 | Where-Object { -not (Test-NetConnection localhost -Port $_ -WarningAction SilentlyContinue).TcpTestSucceeded } | Select-Object -First 5
```

### Linux/Mac
```bash
# Tìm process chiếm port 8888
lsof -i :8888
# hoặc
netstat -tlnp | grep :8888

# Kill process (nếu cần)
kill -9 <PID>

# Tìm port nào còn trống (từ 9000-9010)
for port in {9000..9010}; do
  (echo >/dev/tcp/localhost/$port) 2>/dev/null && echo "Port $port: IN USE" || echo "Port $port: FREE"
done
```

---

## 📊 **Priority Order (Ưu tiên)**

Khi chạy server, port được xác định theo thứ tự:

1. **Command line argument** (cao nhất) - ví dụ: `dotnet run 9999`
2. **Environment variable** - ví dụ: `$env:FILE_DOWNLOADER_PORT = 9999`
3. **Default port** (8888) - nếu không set cái nào

**Ví dụ:**
```powershell
# Set env variable
$env:FILE_DOWNLOADER_PORT = 8080

# Chạy với command argument
dotnet run 9999
# Result: Port 9999 (command argument take priority)

# Chạy không có argument
dotnet run
# Result: Port 8080 (env variable)

# Nếu cả hai không set
dotnet run
# Result: Port 8888 (default)
```

---

## 💡 **Best Practices**

### ✅ Nên làm
- Dùng **command line** cho quick test
- Dùng **environment variable** cho production
- Dùng **NSSM** + environment variable cho Windows Service
- Mở firewall **trước** khi start service

### ❌ Không nên làm
- Hardcode port trong code (đã fix rồi!)
- Chạy nhiều instance cùng port
- Quên mở firewall
- Không test port trước khi start

---

## 📚 **Cheatsheet**

| Nhu cầu | Lệnh |
|--------|------|
| Chạy port 9999 (nhanh) | `dotnet run 9999` |
| Set env variable | `$env:FILE_DOWNLOADER_PORT = 9999` |
| Kiểm tra port đang dùng | `netstat -ano \| findstr :9999` |
| Kill process chiếm port | `taskkill /PID <PID> /F` |
| Test port hoạt động | `Test-NetConnection localhost -Port 9999` |
| Tìm port trống | `1..10000 \| Where-Object { -not (Test-NetConnection localhost -Port $_ -WarningAction SilentlyContinue).TcpTestSucceeded } \| Select-Object -First 5` |
| Mở firewall (Windows) | `New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999` |
| Mở firewall (Linux) | `sudo ufw allow 9999/tcp` |

---

## 🎯 **Quick Start**

**Nếu port 8888 bị chiếm:**

```powershell
# Windows
dotnet run 9999

# Linux
dotnet run 9999

# Client nhập: localhost:9999
```

**Done!** 🎉
