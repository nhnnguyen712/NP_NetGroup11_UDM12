# Server Setup on Windows Server 2012

## Overview

* **Server**: Runs on Windows Server 2012 and listens on port 8888
* **Client**: Runs on remote machines (Windows/Linux) and connects to the server
* **Protocol/Port**: TCP 8888

---

## Part 1: Preparing Windows Server 2012

### 1.1 Verify Windows Version

```powershell
# Open PowerShell as Administrator
winver
# Expected: Windows Server 2012
```

### 1.2 Install .NET 10

#### Step 1: Download .NET 10 SDK/Runtime

* Visit: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
* Select: .NET 10
* Choose OS: Windows x64 (or x86 if applicable)
* Download the installer (.exe)

#### Step 2: Install

```powershell
.\dotnet-sdk-10.0.0-win-x64.exe
# Or runtime:
.\dotnet-runtime-10.0.0-win-x64.exe
```

Follow the installation wizard and complete the setup.

#### Step 3: Verify Installation

```powershell
dotnet --version
# Expected output: 10.0.0 or later
```

---

## Part 2: Server Directory Setup

### 2.1 Transfer Server Files to Windows Server

**Option 1: Remote Desktop (RDP)**

* Connect via Remote Desktop
* Copy the `MultiFileDownloader.Server` folder
* Paste into a directory such as:
  `C:\Apps\MultiFileDownloader.Server`

**Option 2: File Sharing**

* Enable Network Discovery and File Sharing
* Access via: `\\SERVER_IP\C$`
* Copy files into the target directory

**Option 3: Archive Transfer**

* Compress the folder into a `.zip` file
* Upload and extract on the server

---

### 2.2 Create "files" Directory

```powershell
cd C:\Apps\MultiFileDownloader.Server
mkdir files
```

### 2.3 Add Downloadable Files

```powershell
copy C:\Users\Admin\Desktop\document.pdf .\files\
copy C:\Users\Admin\Desktop\video.mp4 .\files\

ls .\files\
```

---

## Part 3: Running the Server

### 3.1 Run for Testing

```powershell
cd C:\Apps\MultiFileDownloader.Server

dotnet run

# Or if published:
cd bin\Release\net10.0\publish
.\MultiFileDownloader.Server.exe
```

Expected output:

```
Server is running on port 8888
Waiting for client connections...
```

---

### 3.2 Run in Background (Production)

#### Step 1: Publish Release

```powershell
cd C:\Apps\MultiFileDownloader.Server
dotnet publish -c Release -o publish
```

#### Step 2: Create Startup Script

**Batch file (run-server.bat):**

```bat
@echo off
cd /d "C:\Apps\MultiFileDownloader.Server\publish"
MultiFileDownloader.Server.exe
pause
```

**VBScript (background execution):**

```vbs
Set objShell = CreateObject("WScript.Shell")
objShell.Run "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe", 0
```

---

## Part 4: Configure Windows Service (Recommended)

### 4.1 Install NSSM

* Download from: [https://nssm.cc/download](https://nssm.cc/download)
* Extract to a directory (e.g., `C:\Tools`)

Or install via Chocolatey:

```powershell
choco install nssm
```

### 4.2 Create Service

```powershell
cd C:\Tools\nssm-2.24\win64

.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"
```

### 4.3 Manage Service

```powershell
# Start
Start-Service FileDownloaderService

# Stop
Stop-Service FileDownloaderService

# Check status
Get-Service FileDownloaderService
```

### 4.4 Enable Auto Start

```powershell
Set-Service -Name FileDownloaderService -StartupType Automatic
```

---

## Part 5: Open Firewall Port 8888

### 5.1 Using GUI

* Open: Windows Firewall with Advanced Security (`wf.msc`)
* Go to: Inbound Rules → New Rule
* Select: Port → TCP → Port 8888
* Allow the connection
* Apply to desired profiles
* Name: File Downloader

### 5.2 Using PowerShell

```powershell
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888

Get-NetFirewallRule -DisplayName "File Downloader"
```

---

## Part 6: Verify Server Status

### 6.1 On Server

```powershell
netstat -ano | findstr 8888

# Or
Get-NetTCPConnection -LocalPort 8888
```

### 6.2 From Client

```powershell
telnet SERVER_IP 8888

# Or
Test-NetConnection -ComputerName SERVER_IP -Port 8888
```

---

## Part 7: Client Setup

### 7.1 Run Client

```powershell
cd Code\MultiFileDownloader.Client
dotnet run
```

### 7.2 Enter Server Address

```
SERVER_IP:8888
```

Example:

```
192.168.1.50:8888
```

---

## Example Deployment

### Server (192.168.1.50)

```powershell
dotnet --version

mkdir C:\Apps\MultiFileDownloader.Server

cd C:\Apps\MultiFileDownloader.Server
mkdir files

copy D:\data\*.pdf .\files\

dotnet publish -c Release

cd C:\Tools\nssm-2.24\win64
.\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

Set-Service -Name FileDownloaderService -StartupType Automatic
Start-Service FileDownloaderService

New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888
```

### Client

```powershell
dotnet run
# Enter: 192.168.1.50:8888
```

---

## Troubleshooting

### .NET Not Installed

```powershell
dotnet --version
```

Reinstall .NET 10 if necessary.

---

### Port 8888 Already in Use

```powershell
netstat -ano | findstr 8888
taskkill /PID <PID> /F
```

---

### Access Denied (Service Creation)

* Ensure PowerShell is running as Administrator

---

### Service Fails to Start

```powershell
Get-EventLog -LogName Application | Where-Object { $_.EventID -eq 7000 } | Select-Object -Last 5

Get-Service FileDownloaderService
```

---

### Firewall Blocking Port

```powershell
Get-NetFirewallRule -DisplayName "File Downloader"

# Recreate rule if needed:
New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888
```

---

## Quick Start

```powershell
# Install .NET 10

mkdir C:\Apps\MultiFileDownloader.Server

cd C:\Apps\MultiFileDownloader.Server
dotnet publish -c Release

cd publish
.\MultiFileDownloader.Server.exe

# Install NSSM and create service
# .\nssm install FileDownloaderService "C:\Apps\MultiFileDownloader.Server\publish\MultiFileDownloader.Server.exe"

Set-Service -Name FileDownloaderService -StartupType Automatic

New-NetFirewallRule -DisplayName "File Downloader" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8888

Start-Service FileDownloaderService
```

The server will start automatically after reboot, and clients can connect using the server IP and port 8888.
