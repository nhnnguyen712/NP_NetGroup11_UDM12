# Setup Server trên Windows Server 2012

## 📋 Tóm tắt
- **Server**: Chạy trên Windows Server 2012, listen port 8888
- **Client**: Chạy trên các máy khác (Windows/Linux), kết nối đến Server
- **Ports**: 8888 (TCP)

---

## 🖥️ **PHẦN 1: Chuẩn bị Windows Server 2012**

### 1.1 Kiểm tra phiên bản Windows
```powershell
# Mở PowerShell as Administrator
winver
# Sẽ thấy: Windows Server 2012
```

### 1.2 Cài đặt .NET 10

**Bước 1: Download .NET 10 SDK/Runtime**

- Truy cập: https://dotnet.microsoft.com/download
- Chọn: `.NET 10` 
- Chọn OS: `Windows x64` (hoặc x86 nếu server là 32-bit)
- Download file `.exe` (Installer)

**Bước 2: Cài đặt**
```powershell
# Chạy file cài đặt vừa download
.\dotnet-sdk-10.0.0-win-x64.exe
# Hoặc runtime:
.\dotnet-runtime-10.0.0-win-x64.exe

# Chọn "Install"
# Chờ hoàn tất
```

**Bước 3: Kiểm tra cài đặt**
```powershell
dotnet --version
# Kết quả: 10.0.0 hoặc cao hơn
```

---

## 📂 **PHẦN 2: Setup Thư mục Server**

### 2.1 Copy Server từ máy local lên Windows Server

**Cách 1: Dùng Remote Desktop (RDP)**
- Mở Remote Desktop từ máy local
- Kết nối đến Windows Server
- Copy folder `MultiFileDownloader.Server` qua clipboard hoặc USB
- Dán vào thư mục (ví dụ: `C:\Apps\MultiFileDownloader.Server`)

**Cách 2: Dùng File Sharing**
```powershell
# Trên Windows Server 2012:
# Bật File Sharing
# Settings > Network and Sharing > Advanced sharing options
# Bật: Network discovery + File and printer sharing

# Từ máy local (Windows):
# \\SERVER_IP\C$ (hoặc share folder)
# Paste folder vào
```

**Cách 3: Dùng 7-Zip / WinRAR**
- Compress folder `MultiFileDownloader.Server` thành `.zip`
- Upload lên server bằng RDP
- Extract trên server

### 2.2 Tạo thư mục "files"

```powershell
# Mở PowerShell as Administrator
cd C:\Apps\MultiFileDownloader.Server

# Tạo folder
mkdir files

# Xác nhận
ls
# Sẽ thấy: files folder
```

### 2.3 Copy file cần download vào folder "files"

```powershell
# Ví dụ: Copy file từ Desktop
copy C:\Users\Admin\Desktop\document.pdf .\files\
copy C:\Users\Admin\Desktop\video.mp4 .\files\

# Kiểm tra
ls .\files\
```

---

## 🚀 **PHẦN 3: Chạy Server**

### 3.1 Chạy trực tiếp (Test)

```powershell
cd C:\Apps\MultiFileDownloader.Server

# Chạy
dotnet run

# Hoặc nếu đã publish:
cd bin\Release\net10.0\publish
.\MultiFileDownloader.Server.exe
```

Khi chạy, bạn sẽ thấy:
```
╔═══════════════════════════════════════════════════════════╗
║        🔗 Multi File Downloader Server                    ║
╠═══════════════════════════════════════════════════════════╣
║  📍 Server IP (Localhost): 127.0.0.1:8888               ║
║  📍 Server IP (Network):   192.168.x.x:8888             ║
║  ✅ Waiting for client connections...                     ║
╚═══════════════════════════════════════════════════════════╝
```

---

### 3.2 Chạy ở background (Publish Release)

**Bước 1: Publish Release Build**

```powershell
cd C:\Apps\MultiFileDownloader.Server

# Publish
dotnet publish -c Release -o publish

# Hoặc nếu dùng self-contained:
dotnet publish -c Release -o publish --self-contained
```

**Bước 2: Tạo Batch File để chạy**

```powershell
# Tạo file: run-server.bat
@echo off
cd /d "C:\Apps\MultiFileDownloader.Server\publish"
MultiFileDownloader.Server.exe
pause
```

Hoặc tạo file `run-server-background.vbs`:

```vbs
' File: run-server-background.vbs
Set objShell = CreateObject("WScript.Shell")
objShell.Run "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe", 0
```

Chạy file VBS sẽ start server ở background.

---

## 🔧 **PHẦN 4: Setup Windows Service (Tự động start)**

### 4.1 Tạo Windows Service (Cách tốt nhất)

**Bước 1: Cài đặt NSSM (Non-Sucking Service Manager)**

```powershell
# Download NSSM từ: https://nssm.cc/download
# Hoặc dùng cmd:
cd C:\Tools
# Giải nén file NSSM

# Hoặc cài qua Chocolatey (nếu có):
choco install nssm
```

**Bước 2: Tạo Service**

```powershell
# Mở PowerShell as Administrator
cd C:\Tools\nssm-2.24\win64  # (hoặc phiên bản khác)

# Tạo service
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

# Hoặc nếu cần config thêm:
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe" "" "" "C:\Apps\MultiFileDownloader.Server\publish"
```

**Bước 3: Kiểm tra Service**

```powershell
# Xem services
Get-Service | findstr "FileDownloader"

# Hoặc mở Services.msc:
services.msc
# Tìm: FileDownloaderService
```

**Bước 4: Start Service**

```powershell
# Start
net start FileDownloaderService

# Hoặc:
Start-Service -Name FileDownloaderService

# Stop:
Stop-Service -Name FileDownloaderService
```

---

### 4.2 Setup Tự động Start khi Server Khởi động

```powershell
# Mở PowerShell as Administrator

# Set service để auto start
Set-Service -Name FileDownloaderService -StartupType Automatic

# Xác nhận
Get-Service FileDownloaderService | Select StartType
```

---

## 🔥 **PHẦN 5: Mở Firewall Port 8888**

### 5.1 Mở Port bằng GUI

1. **Mở Windows Firewall**
   - Control Panel > Windows Firewall > Advanced settings
   - Hoặc: `wf.msc`

2. **Thêm Inbound Rule**
   - Click "Inbound Rules"
   - Click "New Rule..."
   - Chọn: "Port"
   - Next
   - Chọn: "TCP"
   - Specific local ports: `8888`
   - Next
   - Chọn: "Allow the connection"
   - Next
   - Chọn Domain, Private, Public (hoặc tùy)
   - Next
   - Name: "File Downloader"
   - Finish

### 5.2 Mở Port bằng PowerShell

```powershell
# Mở PowerShell as Administrator

# Thêm firewall rule
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888

# Xác nhận
Get-NetFirewallRule -DisplayName "File Downloader"

# Nếu muốn xóa:
Remove-NetFirewallRule -DisplayName "File Downloader"
```

---

## 📊 **PHẦN 6: Kiểm tra Server đang chạy**

### 6.1 Check từ Windows Server

```powershell
# Kiểm tra port 8888 đang listen
netstat -ano | findstr 8888

# Kết quả sẽ hiển thị process ID

# Hoặc dùng:
Get-NetTCPConnection -LocalPort 8888
```

### 6.2 Check từ máy client

```powershell
# Kiểm tra kết nối (Windows Client)
telnet SERVER_IP 8888

# Hoặc:
Test-NetConnection -ComputerName SERVER_IP -Port 8888
```

---

## 💻 **PHẦN 7: Setup Client**

### 7.1 Client từ máy Windows

```powershell
cd Code\MultiFileDownloader.Client
dotnet run
```

Nhập Server Address:
```
192.168.x.x:8888
```

### 7.2 Client từ máy khác

Làm tương tự như trên, chỉ thay Server IP.

---

## 📝 **Ví dụ hoàn chỉnh**

### Windows Server 2012 - IP: 192.168.1.50

**Setup:**
```powershell
# 1. Kiểm tra .NET
dotnet --version

# 2. Copy file
mkdir C:\Apps\MultiFileDownloader.Server
# (Copy files vào)

# 3. Tạo folder files
cd C:\Apps\MultiFileDownloader.Server
mkdir files

# 4. Copy files cần download
copy D:\data\*.pdf .\files\

# 5. Publish
dotnet publish -c Release

# 6. Tạo Service
cd C:\Tools\nssm-2.24\win64
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

# 7. Set auto start
Set-Service -Name FileDownloaderService -StartupType Automatic

# 8. Start service
Start-Service -Name FileDownloaderService

# 9. Mở firewall
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888

# 10. Kiểm tra
Get-Service FileDownloaderService
Get-NetTCPConnection -LocalPort 8888
```

**Client (máy khác):**
```powershell
dotnet run
# Nhập: 192.168.1.50:8888
# Tải file!
```

---

## 🔧 **Troubleshooting**

### ❌ ".NET SDK not found"
```powershell
# Kiểm tra cài đặt
dotnet --version

# Nếu lỗi, cài lại .NET 10
# https://dotnet.microsoft.com/download
```

### ❌ "Port 8888 already in use"
```powershell
# Tìm process dùng port 8888
netstat -ano | findstr 8888

# Kill process (nếu cần)
taskkill /PID <PID> /F
```

### ❌ "Access Denied" khi tạo Service
```powershell
# Chạy PowerShell as Administrator
# Right-click > Run as Administrator
```

### ❌ "Service không start"
```powershell
# Kiểm tra log
Get-EventLog -LogName Application | Where-Object { $_.EventID -eq 7000 } | Select-Object -Last 5

# Hoặc check service status:
Get-Service FileDownloaderService | Select Status, StartType
```

### ❌ "Firewall block port"
```powershell
# Kiểm tra rule
Get-NetFirewallRule -DisplayName "File Downloader"

# Nếu chưa có, tạo:
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888
```

---

## 📚 **Tài liệu tham khảo**

- [.NET on Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
- [NSSM - Windows Service Manager](https://nssm.cc/)
- [Windows Firewall](https://learn.microsoft.com/en-us/windows/security/operating-system-security/network-security/windows-firewall/)
- [PowerShell Networking](https://learn.microsoft.com/en-us/powershell/module/nettcpip/)

---

## ⚡ **Quick Start (Nhanh nhất)**

```powershell
# 1. Cài .NET 10
# (Download & cài từ: https://dotnet.microsoft.com/download)

# 2. Copy & Setup
mkdir C:\Apps\MultiFileDownloader.Server
# Paste files + create ./files folder

# 3. Publish
cd C:\Apps\MultiFileDownloader.Server
dotnet publish -c Release

# 4. Test run (optional)
cd publish
.\MultiFileDownloader.Server.exe

# 5. Tạo Windows Service (NSSM)
# Download NSSM từ https://nssm.cc/
# .\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

# 6. Auto start
Set-Service -Name FileDownloaderService -StartupType Automatic

# 7. Mở firewall
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888

# 8. Start
Start-Service -Name FileDownloaderService

# ✅ Done! Server sẽ chạy tự động khi reboot
```

Server sẽ in ra IP, bạn copy-paste vào client là xong! 🎉
