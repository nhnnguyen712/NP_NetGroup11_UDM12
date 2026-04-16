# Thay đổi Port Sau Khi Deploy - Hướng dẫn Chi tiết

## 📋 Tóm tắt
Sau khi deploy, bạn có thể thay đổi port dễ dàng mà **không cần rebuild/redeploy**, tùy vào cách chạy:

| Cách chạy | Thay đổi port | Độ khó |
|-----------|---------------|--------|
| **Chạy trực tiếp** (Console) | Dừng & chạy lại với port mới | ⭐ Dễ |
| **Background (nohup/screen)** | Dừng process & chạy lại | ⭐ Dễ |
| **Windows Service** | Sửa NSSM config & restart | ⭐⭐ Trung bình |
| **Systemd Service** (Linux) | Sửa file service & restart | ⭐⭐ Trung bình |

---

## 🖥️ **PHẦN 1: Server Chạy Trực tiếp (Console)**

### Bước 1: Dừng server hiện tại
```powershell
# Server đang chạy ở console
# Ấn Ctrl+C để dừng
```

### Bước 2: Chạy lại với port mới
```powershell
# Windows
cd C:\Apps\MultiFileDownloader.Server\publish
.\MultiFileDownloader.Server.exe 9999

# Linux
cd /home/ubuntu/MultiFileDownloader.Server/publish
./MultiFileDownloader.Server 9999
```

### Bước 3: Verify
```
✅ Sẽ thấy:
📍 Server IP (Network): 192.168.1.100:9999
```

**Ưu/Nhược:**
- ✅ Đơn giản, không cần tool thêm
- ❌ Server offline trong lúc restart
- ❌ Phải chạy thủ công

---

## 🔄 **PHẦN 2: Server Chạy ở Background (nohup/screen)**

### Scenario: Linux Server chạy bằng nohup

#### **Cách 1: Kill process cũ & Start mới**

```bash
# 1. Tìm process ID
ps aux | grep MultiFileDownloader.Server
# Kết quả: ubuntu 1234 0.1 0.2 100000 50000 ? S 14:30 0:10 ./MultiFileDownloader.Server 8888

# 2. Kill process
kill 1234

# 3. Chạy với port mới
nohup ./MultiFileDownloader.Server 9999 > server.log 2>&1 &

# 4. Verify
ps aux | grep MultiFileDownloader.Server
tail -f server.log  # Xem log
```

#### **Cách 2: Dùng screen (quay lại console khi cần)**

```bash
# 1. List tất cả screen sessions
screen -ls

# Kết quả:
# There are screens on:
#   1234.downloader      (Detached)

# 2. Quay lại session
screen -r downloader

# 3. Bên trong screen, ấn Ctrl+C để dừng server

# 4. Chạy server với port mới
./MultiFileDownloader.Server 9999

# 5. Detach (Ấn Ctrl+A rồi D)

# 6. Verify từ ngoài
screen -ls
```

---

## 🪟 **PHẦN 3: Windows Service (NSSM)**

### Scenario: Windows Server 2012, server chạy dưới service "FileDownloaderService"

#### **Cách 1: Sửa command của service**

```powershell
# 1. Mở PowerShell as Administrator

# 2. Dừng service
Stop-Service -Name FileDownloaderService

# 3. Sửa port (cập nhật command)
# Nếu dùng NSSM:
cd C:\Tools\nssm-2.24\win64
.\nssm set FileDownloaderService AppDirectory "C:\Apps\MultiFileDownloader.Server\publish"
.\nssm set FileDownloaderService Application "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 9999"

# Hoặc sử dụng GUI NSSM:
.\nssm edit FileDownloaderService
# Thay đổi "Path arguments": 9999

# 4. Start service
Start-Service -Name FileDownloaderService

# 5. Verify
Get-Service FileDownloaderService
Get-NetTCPConnection -LocalPort 9999
```

#### **Cách 2: Dùng Environment Variable**

```powershell
# 1. Stop service
Stop-Service -Name FileDownloaderService

# 2. Set environment variable cho service
cd C:\Tools\nssm-2.24\win64
.\nssm set FileDownloaderService AppEnvironmentExtra FILE_DOWNLOADER_PORT=9999

# 3. Start service
Start-Service -Name FileDownloaderService

# 4. Verify
Get-Service FileDownloaderService
tail -f C:\Apps\MultiFileDownloader.Server\publish\server.log
```

#### **Cách 3: Dùng NSSM GUI**

```powershell
# 1. Mở NSSM GUI
cd C:\Tools\nssm-2.24\win64
.\nssm edit FileDownloaderService

# 2. Tab "Details":
#    - Details tab → Arguments: 9999
#    OR
#    - Environment tab → FILE_DOWNLOADER_PORT=9999

# 3. Chọn Apply/OK

# 4. Restart service
Stop-Service -Name FileDownloaderService
Start-Service -Name FileDownloaderService
```

---

## 🐧 **PHẦN 4: Systemd Service (Linux)**

### Scenario: Linux Server (Ubuntu/Debian), server chạy dưới systemd

#### **Cách 1: Sửa service file**

```bash
# 1. Mở file service
sudo nano /etc/systemd/system/file-downloader.service

# 2. Sửa ExecStart (thêm port):
# Từ:
ExecStart=/usr/bin/dotnet /home/ubuntu/MultiFileDownloader.Server/bin/Release/net10.0/publish/MultiFileDownloader.Server.dll

# Thành:
ExecStart=/usr/bin/dotnet /home/ubuntu/MultiFileDownloader.Server/bin/Release/net10.0/publish/MultiFileDownloader.Server.dll 9999

# 3. Save & exit (Ctrl+O, Enter, Ctrl+X)

# 4. Reload systemd
sudo systemctl daemon-reload

# 5. Restart service
sudo systemctl restart file-downloader.service

# 6. Verify
sudo systemctl status file-downloader.service
sudo systemctl is-active file-downloader.service
```

#### **Cách 2: Dùng Environment Variable trong systemd**

```bash
# 1. Mở file service
sudo nano /etc/systemd/system/file-downloader.service

# 2. Thêm Environment variable:
[Service]
Type=simple
User=ubuntu
WorkingDirectory=/home/ubuntu/MultiFileDownloader.Server/publish
Environment="FILE_DOWNLOADER_PORT=9999"
ExecStart=/usr/bin/dotnet MultiFileDownloader.Server.dll
Restart=always
RestartSec=10

# 3. Save & exit

# 4. Reload & restart
sudo systemctl daemon-reload
sudo systemctl restart file-downloader.service

# 5. Verify
sudo systemctl status file-downloader.service
journalctl -u file-downloader.service -f
```

#### **Cách 3: Systemctl set-environment**

```bash
# 1. Set environment variable cho service
sudo systemctl set-environment FILE_DOWNLOADER_PORT=9999

# 2. Restart service
sudo systemctl restart file-downloader.service

# 3. Verify
systemctl show-environment | grep FILE_DOWNLOADER_PORT
sudo systemctl status file-downloader.service
```

---

## 📝 **PHẦN 5: Ví dụ Thực tế - Các Scenario**

### Scenario A: VPS Linux, server chạy bằng nohup, port 8888 bị chiếm

```bash
# 1. SSH vào server
ssh ubuntu@192.168.1.50

# 2. Tìm process server hiện tại
ps aux | grep MultiFileDownloader.Server
# ubuntu   1234  0.1  0.2  100000  50000 ?  S  14:30  0:10 ./MultiFileDownloader.Server 8888

# 3. Kill process
kill 1234

# 4. Chạy server với port 9999
cd /home/ubuntu/MultiFileDownloader.Server/publish
nohup ./MultiFileDownloader.Server 9999 > server.log 2>&1 &

# 5. Verify
tail -f server.log
# Sẽ thấy: 📍 Server IP (Network): 192.168.1.50:9999

# 6. Mở firewall (nếu cần)
sudo ufw allow 9999/tcp

# 7. Client kết nối: 192.168.1.50:9999
```

### Scenario B: Windows Server 2012, chạy dưới NSSM Service, port 8888 bị chiếm

```powershell
# 1. RDP vào server
# Mở PowerShell as Administrator

# 2. Dừng service
Stop-Service -Name FileDownloaderService

# 3. Sửa command
cd C:\Tools\nssm-2.24\win64
.\nssm set FileDownloaderService Application "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 7777"

# 4. Start service
Start-Service -Name FileDownloaderService

# 5. Verify
Get-Service FileDownloaderService
Get-NetTCPConnection -LocalPort 7777

# 6. Mở firewall (nếu cần)
New-NetFirewallRule -DisplayName "File Downloader Port 7777" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7777

# 7. Client kết nối: 192.168.1.50:7777
```

### Scenario C: Linux Systemd Service, cần đổi port 8888 → 3000

```bash
# 1. SSH vào server
ssh ubuntu@example.com

# 2. Stop service
sudo systemctl stop file-downloader.service

# 3. Sửa service file
sudo nano /etc/systemd/system/file-downloader.service
# Thay: ExecStart=.../Server.dll
# Thành: ExecStart=.../Server.dll 3000

# 4. Reload & restart
sudo systemctl daemon-reload
sudo systemctl start file-downloader.service

# 5. Verify
sudo systemctl status file-downloader.service
journalctl -u file-downloader.service -n 20

# 6. Mở firewall
sudo ufw allow 3000/tcp

# 7. Client kết nối: example.com:3000
```

---

## 🧪 **PHẦN 6: Verify Port Đã Thay Đổi**

### Windows
```powershell
# Cách 1: PowerShell
Get-NetTCPConnection -LocalPort 9999

# Cách 2: netstat
netstat -ano | findstr :9999

# Cách 3: Telnet
telnet localhost 9999
```

### Linux
```bash
# Cách 1: netstat
netstat -tlnp | grep :9999

# Cách 2: ss (modern)
ss -tlnp | grep :9999

# Cách 3: lsof
lsof -i :9999

# Cách 4: nc (netcat)
nc -zv localhost 9999

# Cách 5: curl (từ client)
curl http://192.168.1.50:9999 2>&1 | head -1
```

---

## ⚠️ **Lưu ý Quan trọng**

### 1️⃣ **Mở Firewall sau khi đổi port**

**Windows Server:**
```powershell
New-NetFirewallRule -DisplayName "File Downloader Port 9999" `
    -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999
```

**Linux:**
```bash
sudo ufw allow 9999/tcp
```

### 2️⃣ **Server bị offline lúc restart**
- Dùng **Load Balancer** nếu cần zero-downtime
- Hoặc chuẩn bị 2 instance server

### 3️⃣ **Port không thay đổi**
```bash
# Kiểm tra log
tail -f server.log
# Nếu thấy: "Port từ environment variable: 9999" ✓
# Nếu thấy: "Sử dụng port mặc định: 8888" ✗

# Vấn đề có thể là:
# - Environment variable không set đúng
# - Service chưa restart
# - Command argument không được pass
```

### 4️⃣ **Kill process bị stuck**

**Windows:**
```powershell
# Force kill
taskkill /PID <PID> /F

# Hoặc kill by name
taskkill /IM MultiFileDownloader.Server.exe /F
```

**Linux:**
```bash
# Force kill
kill -9 <PID>

# Hoặc kill by name
pkill -f MultiFileDownloader.Server
pkill -9 -f MultiFileDownloader.Server  # Force kill
```

---

## 📚 **Cheatsheet - Sau Khi Deploy**

| Nhu cầu | Lệnh | OS |
|--------|------|-----|
| Kill process | `taskkill /IM MultiFileDownloader.Server.exe /F` | Windows |
| Kill process | `pkill -9 -f MultiFileDownloader.Server` | Linux |
| Chạy port mới | `.\Server.exe 9999` | Windows |
| Chạy port mới | `./Server 9999` | Linux |
| Stop service (NSSM) | `Stop-Service -Name FileDownloaderService` | Windows |
| Set port qua NSSM | `.\nssm set FileDownloaderService AppEnvironmentExtra FILE_DOWNLOADER_PORT=9999` | Windows |
| Stop systemd service | `sudo systemctl stop file-downloader` | Linux |
| Edit systemd service | `sudo nano /etc/systemd/system/file-downloader.service` | Linux |
| Reload systemd | `sudo systemctl daemon-reload` | Linux |
| Check port listen | `netstat -ano \| findstr :9999` | Windows |
| Check port listen | `netstat -tlnp \| grep :9999` | Linux |
| Mở firewall Windows | `New-NetFirewallRule -DisplayName "..." -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999` | Windows |
| Mở firewall Linux | `sudo ufw allow 9999/tcp` | Linux |

---

## 🎯 **Quick Checklist - Sau Khi Deploy**

- [ ] **Stop server** (console, nohup, hoặc service)
- [ ] **Thay đổi port** (command line, env variable, hoặc service config)
- [ ] **Start server** (console, nohup, hoặc service)
- [ ] **Verify server chạy** (`Get-NetTCPConnection`, `netstat`, `ss`)
- [ ] **Mở firewall** cho port mới (Windows/Linux)
- [ ] **Test từ client** (connect thành công?)
- [ ] **Check log** (error gì không?)

---

## 💡 **Tips & Tricks**

### Tip 1: Đổi port không cần kill service

```bash
# Chỉ với environment variable (nếu restart được nhanh)
export FILE_DOWNLOADER_PORT=9999
sudo systemctl restart file-downloader.service
```

### Tip 2: Kiểm tra port before change

```bash
# Đảm bảo port mới còn trống
netstat -ano | findstr :9999
# Nếu không có output → port còn trống ✓
```

### Tip 3: Backup config trước khi đổi

```bash
# Linux systemd
sudo cp /etc/systemd/system/file-downloader.service \
        /etc/systemd/system/file-downloader.service.backup
```

### Tip 4: Tự động restart nếu crash

**Systemd:** Đã set `Restart=always` trong service file
**NSSM:** Cài đặt khi tạo service: `.\nssm set FileDownloaderService AppRestart Automatic`

---

## 🚀 **Conclusion**

**Sau khi deploy, dùng cách nào?**

1. **Console run** → Dừng & chạy lại
2. **Nohup/screen** → Kill process & chạy lại
3. **Windows Service (NSSM)** → Sửa config & restart service
4. **Systemd Service (Linux)** → Sửa file service & restart

**Đều dễ, không cần rebuild/redeploy!** ✨
