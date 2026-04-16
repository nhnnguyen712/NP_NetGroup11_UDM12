# Setup Server trên VPS và Client ở các máy

## 📋 Tóm tắt
- **Server**: Chạy trên VPS, listen port 8888
- **Client**: Chạy trên các máy khác, kết nối đến VPS
- **Ports**: 8888 (TCP)

---

## 🖥️ **PHẦN 1: Setup Server trên VPS**

### 1.1 Yêu cầu
- .NET 10 SDK hoặc Runtime
- Quyền SSH/Root trên VPS
- Port 8888 không bị block

### 1.2 Các bước

#### **Bước 1: Upload Server lên VPS**
```bash
# Sử dụng SCP (từ máy local)
scp -r Code/MultiFileDownloader.Server user@YOUR_VPS_IP:/home/user/

# Hoặc dùng SFTP/FTP tool
```

#### **Bước 2: Kết nối SSH vào VPS**
```bash
ssh user@YOUR_VPS_IP
```

#### **Bước 3: Kiểm tra .NET installation**
```bash
dotnet --version
# Phải là .NET 10 trở lên
```

#### **Bước 4: Tạo thư mục "files" để lưu file**
```bash
cd /home/user/MultiFileDownloader.Server
mkdir -p files
```

#### **Bước 5: Copy file cần download vào folder "files"**
```bash
# Ví dụ:
cp /path/to/myfile.txt ./files/
```

#### **Bước 6: Chạy Server**
```bash
dotnet run
# Hoặc publish release:
dotnet publish -c Release
cd bin/Release/net10.0/publish
./MultiFileDownloader.Server
```

#### **Bước 7: Kiểm tra Server đang chạy**
```bash
# Đang xem log:
# Sẽ thấy: "Server is running on port 8888"
```

---

### 1.3 Setup Firewall (Mở Port 8888)

**Nếu dùng UFW (Ubuntu/Debian):**
```bash
sudo ufw allow 8888/tcp
sudo ufw status
```

**Nếu dùng iptables:**
```bash
sudo iptables -A INPUT -p tcp --dport 8888 -j ACCEPT
sudo iptables-save
```

**Nếu dùng AWS Security Group:**
- Đi đến EC2 → Security Groups
- Thêm Inbound Rule:
  - Type: Custom TCP
  - Port: 8888
  - Source: 0.0.0.0/0 (hoặc IP cụ thể)

**Nếu dùng Azure Network Security Group:**
- Thêm Allow rule cho port 8888

---

### 1.4 Chạy Server ở background (nên dùng)

**Cách 1: Dùng `nohup`**
```bash
nohup dotnet run > server.log 2>&1 &
# Hoặc
nohup ./MultiFileDownloader.Server > server.log 2>&1 &
```

**Cách 2: Dùng `screen`**
```bash
screen -S downloader
dotnet run
# Ấn Ctrl+A rồi D để thoát session nhưng giữ process chạy

# Sau này quay lại:
screen -r downloader
```

**Cách 3: Dùng `systemd` service (tốt nhất cho production)**
```bash
sudo nano /etc/systemd/system/file-downloader.service
```

Thêm nội dung:
```ini
[Unit]
Description=File Downloader Server
After=network.target

[Service]
Type=simple
User=ubuntu
WorkingDirectory=/home/ubuntu/MultiFileDownloader.Server
ExecStart=/usr/bin/dotnet /home/ubuntu/MultiFileDownloader.Server/bin/Release/net10.0/publish/MultiFileDownloader.Server.dll
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Chạy:
```bash
sudo systemctl daemon-reload
sudo systemctl enable file-downloader.service
sudo systemctl start file-downloader.service
sudo systemctl status file-downloader.service
```

---

## 💻 **PHẦN 2: Setup Client ở máy local**

### 2.1 Chỉnh sửa Config (không cần, đã sửa rồi!)

Client sẽ tự động hỏi Server Address khi khởi động. Nhập:
```
YOUR_VPS_IP:8888
```

**Ví dụ:**
```
45.123.45.67:8888
example.com:8888
```

### 2.2 Chạy Client

**Cách 1: Chạy từ Visual Studio**
- Mở solution
- Build & Run Client project
- Nhập Server Address khi được hỏi

**Cách 2: Chạy từ Command Line**
```bash
cd Code\MultiFileDownloader.Client
dotnet run
```

---

## 🧪 **PHẦN 3: Test Kết nối**

### 3.1 Test từ máy local
```bash
# Kiểm tra có kết nối được không
ping YOUR_VPS_IP

# Test port 8888 với telnet (nếu có)
telnet YOUR_VPS_IP 8888
# Nếu kết nối được sẽ thấy prompt hoặc timeout
```

### 3.2 Xem log Server
```bash
# Nếu chạy bằng nohup
tail -f server.log

# Nếu chạy bằng screen
screen -r downloader

# Nếu chạy bằng systemd
sudo journalctl -u file-downloader.service -f
```

---

## 🔧 **Troubleshooting**

### ❌ **Client không kết nối được**

**Nguyên nhân 1: Port 8888 bị block**
```bash
# Test từ VPS
curl http://localhost:8888
# Hoặc
nc -zv YOUR_VPS_IP 8888
```

**Nguyên nhân 2: Server chưa start**
```bash
# Kiểm tra process
ps aux | grep dotnet
```

**Nguyên nhân 3: Sai IP/Port**
```bash
# Xem Server log:
tail -f server.log
```

### ❌ **"Connection refused"**
- Kiểm tra server đang chạy không
- Kiểm tra firewall: `sudo ufw status`
- Kiểm tra port:
  ```bash
  sudo netstat -tlnp | grep 8888
  # hoặc
  sudo ss -tlnp | grep 8888
  ```

### ❌ **Folder "files" không tồn tại**
- Server sẽ tự tạo, nhưng cũng có thể tạo thủ công:
  ```bash
  mkdir -p /path/to/server/files
  ```

---

## 📝 **Ví dụ hoàn chỉnh**

### VPS: 45.123.45.67

**Bước 1: Upload & Setup**
```bash
scp -r Code/MultiFileDownloader.Server ubuntu@45.123.45.67:/home/ubuntu/
ssh ubuntu@45.123.45.67

# Setup
cd /home/ubuntu/MultiFileDownloader.Server
mkdir -p files
# Copy file vào folder files
# Ví dụ:
# cp ~/document.pdf ./files/
# cp ~/video.mp4 ./files/

# Publish
dotnet publish -c Release

# Chạy
nohup dotnet bin/Release/net10.0/publish/MultiFileDownloader.Server.dll > server.log 2>&1 &
```

### Client: Máy local (192.168.1.100)

**Bước 1: Chạy Client**
```bash
# Từ Visual Studio hoặc:
dotnet run
```

**Bước 2: Nhập Server Address**
```
45.123.45.67:8888
```

**Bước 3: Tải file!**
- Chọn file từ danh sách
- Click "Download Selected"
- File sẽ được lưu vào Downloads folder

---

## 🔐 **Best Practices**

1. **Không chạy server với root account** - Sử dụng unprivileged user
2. **Backup folder "files"** - Sử dụng rsync hoặc GitHub
3. **Monitor server** - Dùng systemd hoặc tools khác
4. **Cập nhật .NET** - Giữ OS & .NET luôn cập nhật
5. **Firewall** - Chỉ mở port 8888 khi cần
6. **SSL/TLS** - Nếu bảo mật quan trọng, thêm mã hóa

---

## 📚 **Thêm tài liệu**

- [.NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- [Systemd Services](https://www.freedesktop.org/software/systemd/man/systemd.service.html)
- [UFW Firewall](https://help.ubuntu.com/community/UFW)
