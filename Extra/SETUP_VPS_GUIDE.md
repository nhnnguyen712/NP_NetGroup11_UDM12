# Server Setup on VPS and Client Configuration

## Overview

* **Server**: Runs on a VPS and listens on port 8888
* **Client**: Runs on remote machines and connects to the VPS
* **Protocol/Port**: TCP 8888

---

## Part 1: Server Setup on VPS

### 1.1 Requirements

* .NET 10 SDK or Runtime
* SSH/root access to the VPS
* Port 8888 must be accessible (not blocked by firewall)

### 1.2 Setup Steps

#### Step 1: Upload Server to VPS

```bash
# From local machine using SCP
scp -r Code/MultiFileDownloader.Server user@YOUR_VPS_IP:/home/user/

# Alternatively, use an SFTP/FTP tool
```

#### Step 2: Connect to VPS via SSH

```bash
ssh user@YOUR_VPS_IP
```

#### Step 3: Verify .NET Installation

```bash
dotnet --version
# Ensure version is .NET 10 or later
```

#### Step 4: Create "files" Directory

```bash
cd /home/user/MultiFileDownloader.Server
mkdir -p files
```

#### Step 5: Add Files to Download Directory

```bash
# Example:
cp /path/to/myfile.txt ./files/
```

#### Step 6: Run the Server

```bash
dotnet run

# Or publish for production:
dotnet publish -c Release
cd bin/Release/net10.0/publish
./MultiFileDownloader.Server
```

#### Step 7: Verify Server Status

Check logs for confirmation:

```
Server is running on port 8888
```

---

### 1.3 Firewall Configuration (Open Port 8888)

**Using UFW (Ubuntu/Debian):**

```bash
sudo ufw allow 8888/tcp
sudo ufw status
```

**Using iptables:**

```bash
sudo iptables -A INPUT -p tcp --dport 8888 -j ACCEPT
sudo iptables-save
```

**AWS Security Group:**

* Go to EC2 → Security Groups
* Add inbound rule:

  * Type: Custom TCP
  * Port: 8888
  * Source: 0.0.0.0/0 (or restrict to specific IPs)

**Azure Network Security Group:**

* Add an allow rule for TCP port 8888

---

### 1.4 Running the Server in the Background

**Option 1: nohup**

```bash
nohup dotnet run > server.log 2>&1 &
# Or
nohup ./MultiFileDownloader.Server > server.log 2>&1 &
```

**Option 2: screen**

```bash
screen -S downloader
dotnet run

# Detach: Ctrl + A, then D
# Reattach later:
screen -r downloader
```

**Option 3: systemd service (recommended for production)**

```bash
sudo nano /etc/systemd/system/file-downloader.service
```

Service configuration:

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

Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable file-downloader.service
sudo systemctl start file-downloader.service
sudo systemctl status file-downloader.service
```

---

## Part 2: Client Setup

### 2.1 Configuration

The client prompts for the server address at startup. Enter:

```
YOUR_VPS_IP:8888
```

Examples:

```
45.123.45.67:8888
example.com:8888
```

### 2.2 Run the Client

**Option 1: Visual Studio**

* Open the solution
* Build and run the Client project
* Enter the server address when prompted

**Option 2: Command Line**

```bash
cd Code/MultiFileDownloader.Client
dotnet run
```

---

## Part 3: Connection Testing

### 3.1 Test from Local Machine

```bash
# Check connectivity
ping YOUR_VPS_IP

# Test port (if telnet is available)
telnet YOUR_VPS_IP 8888
```

### 3.2 View Server Logs

```bash
# If using nohup
tail -f server.log

# If using screen
screen -r downloader

# If using systemd
sudo journalctl -u file-downloader.service -f
```

---

## Troubleshooting

### Client Cannot Connect

**Possible cause: Port 8888 is blocked**

```bash
curl http://localhost:8888
# Or
nc -zv YOUR_VPS_IP 8888
```

**Possible cause: Server not running**

```bash
ps aux | grep dotnet
```

**Possible cause: Incorrect IP/port**

```bash
tail -f server.log
```

---

### Connection Refused

* Ensure the server is running
* Check firewall rules:

```bash
sudo ufw status
```

* Verify port binding:

```bash
sudo netstat -tlnp | grep 8888
# Or
sudo ss -tlnp | grep 8888
```

---

### Missing "files" Directory

```bash
mkdir -p /path/to/server/files
```

---

## Example Deployment

### VPS: 45.123.45.67

```bash
scp -r Code/MultiFileDownloader.Server ubuntu@45.123.45.67:/home/ubuntu/
ssh ubuntu@45.123.45.67

cd /home/ubuntu/MultiFileDownloader.Server
mkdir -p files

# Add files to ./files directory

dotnet publish -c Release

nohup dotnet bin/Release/net10.0/publish/MultiFileDownloader.Server.dll > server.log 2>&1 &
```

### Client (Local Machine)

```bash
dotnet run
```

Enter:

```
45.123.45.67:8888
```

Then:

* Select files from the list
* Click "Download Selected"
* Files will be saved to the Downloads directory

---

## Best Practices

1. Do not run the server as root; use a non-privileged user
2. Regularly back up the "files" directory (e.g., rsync, GitHub)
3. Monitor the service using systemd or other tools
4. Keep the OS and .NET runtime up to date
5. Restrict firewall rules where possible
6. Use SSL/TLS if security is required

---

## Additional Resources

* [https://learn.microsoft.com/en-us/dotnet/core/install/linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
* [https://www.freedesktop.org/software/systemd/man/systemd.service.html](https://www.freedesktop.org/software/systemd/man/systemd.service.html)
* [https://help.ubuntu.com/community/UFW](https://help.ubuntu.com/community/UFW)
