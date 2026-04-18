## Changing Server Port – Detailed Guide

### Overview

The server supports three methods for configuring the port:

1. **Command-line argument** (fast and simple)
2. **Environment variable** (recommended for production)
3. **Default port** (8888)

---

## 1. Command-Line Argument (Simplest Method)

### Windows

```powershell
cd Code\MultiFileDownloader.Server

# Run with a custom port (e.g., 9999)
dotnet run 9999

# If already published:
cd bin\Release\net10.0\publish
.\MultiFileDownloader.Server.exe 9999
```

### Linux / macOS

```bash
cd Code/MultiFileDownloader.Server

# Run with a custom port
dotnet run 9999

# Or:
./bin/Release/net10.0/publish/MultiFileDownloader.Server 9999
```

**Result:**
The server will start and listen on the specified port (e.g., `127.0.0.1:9999`).

---

## 2. Environment Variable (Recommended for Production)

### Windows (PowerShell)

```powershell
# Set environment variable
$env:FILE_DOWNLOADER_PORT = "9999"

# Run server
dotnet run
# or
.\MultiFileDownloader.Server.exe

# Verify
Write-Host $env:FILE_DOWNLOADER_PORT
```

### Windows (Command Prompt)

```cmd
REM Set environment variable
set FILE_DOWNLOADER_PORT=9999

REM Run server
dotnet run
REM or
MultiFileDownloader.Server.exe

REM Verify
echo %FILE_DOWNLOADER_PORT%
```

### Linux / macOS (Bash)

```bash
# Set environment variable
export FILE_DOWNLOADER_PORT=9999

# Run server
dotnet run
# or
./MultiFileDownloader.Server

# Verify
echo $FILE_DOWNLOADER_PORT
```

### Persistent Configuration

#### Windows

```powershell
# User-level
[System.Environment]::SetEnvironmentVariable('FILE_DOWNLOADER_PORT', '9999', 'User')

# System-level
[System.Environment]::SetEnvironmentVariable('FILE_DOWNLOADER_PORT', '9999', 'Machine')
```

Restart the shell or application after setting.

#### Linux / macOS

```bash
echo 'export FILE_DOWNLOADER_PORT=9999' >> ~/.bashrc
source ~/.bashrc
```

---

## 3. Windows Service Configuration (Using NSSM)

```powershell
cd C:\Tools\nssm-2.24\win64

# Option A: Pass port via command argument
.\nssm install FileDownloaderService `
    "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 9999"

# Option B: Use environment variable
.\nssm install FileDownloaderService `
    "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

.\nssm set FileDownloaderService AppEnvironmentExtra FILE_DOWNLOADER_PORT=9999
```

### Service Management

```powershell
Start-Service -Name FileDownloaderService
Get-Service FileDownloaderService
```

---

## Practical Examples

### Scenario 1: Default port (8888) is in use

```powershell
dotnet run 9999
```

### Scenario 2: Production server with persistent port

```bash
export FILE_DOWNLOADER_PORT=8080
echo 'export FILE_DOWNLOADER_PORT=8080' >> ~/.bashrc
source ~/.bashrc

nohup dotnet bin/Release/net10.0/publish/MultiFileDownloader.Server > server.log 2>&1 &
```

### Scenario 3: Windows service with custom port

```powershell
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe 7777"

Set-Service -Name FileDownloaderService -StartupType Automatic

New-NetFirewallRule -DisplayName "File Downloader Port 7777" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7777

Start-Service -Name FileDownloaderService
```

---

## Testing the Port

### Windows

```powershell
Test-NetConnection -ComputerName localhost -Port 9999
netstat -ano | findstr :9999
Get-NetTCPConnection -LocalPort 9999
```

### Linux / macOS

```bash
nc -zv localhost 9999
ss -tlnp | grep :9999
lsof -i :9999
```

---

## Handling Port Conflicts

### Windows

```powershell
netstat -ano | findstr :8888
taskkill /PID <PID> /F
```

### Linux / macOS

```bash
lsof -i :8888
kill -9 <PID>
```

---

## Port Priority Order

The server determines the port in the following order:

1. Command-line argument (highest priority)
2. Environment variable
3. Default port (8888)

**Example:**

```powershell
$env:FILE_DOWNLOADER_PORT = 8080
dotnet run 9999   # Uses 9999

dotnet run        # Uses 8080

# If nothing is set:
dotnet run        # Uses 8888
```

---

## Best Practices

### Recommended

* Use command-line arguments for quick testing
* Use environment variables for production
* Use NSSM with environment variables for Windows services
* Open firewall ports before starting the service

### Not Recommended

* Hardcoding ports in source code
* Running multiple instances on the same port
* Forgetting firewall configuration
* Skipping port availability checks

---

## Quick Reference

| Task                     | Command                                                                              |
| ------------------------ | ------------------------------------------------------------------------------------ |
| Run on port 9999         | `dotnet run 9999`                                                                    |
| Set environment variable | `$env:FILE_DOWNLOADER_PORT = 9999`                                                   |
| Check port usage         | `netstat -ano \| findstr :9999`                                                      |
| Kill process             | `taskkill /PID <PID> /F`                                                             |
| Test port                | `Test-NetConnection localhost -Port 9999`                                            |
| Open firewall (Windows)  | `New-NetFirewallRule -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999` |
| Open firewall (Linux)    | `sudo ufw allow 9999/tcp`                                                            |

---

## Quick Start

If port 8888 is already in use:

```powershell
dotnet run 9999
```

Then connect using:

```
localhost:9999
```

The server will start immediately on the new port.
