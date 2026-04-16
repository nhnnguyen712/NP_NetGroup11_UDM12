# Multi File Downloader

> A professional network file downloading application built with C# and .NET 10, featuring a modern WPF client and robust server infrastructure.

**Repository:** [nhnnguyen712/NP_NetGroup11_UDM12](https://github.com/nhnnguyen712/NP_NetGroup11_UDM12)  
**Branch:** main  
**Class/Group:** UDM12 - Network Programming - Group 11  
**.NET Target:** .NET 10  
**Link video:** https://drive.google.com/drive/folders/1FnpQlH6oRj2fdJr8wYw1QNC0XdFApPH3?usp=sharing

---

## Team Members

| #   | Name                         | Role        |
| --- | ---------------------------- | ----------- |
| 1   | **Nguyễn Huỳnh Nhật Nguyên** | Team Leader |
| 2   | Nguyễn Hữu Khánh Nguyên      | Developer   |
| 3   | Trần Đăng Khoa               | Developer   |
| 4   | Tống Quốc Trọng              | Developer   |
| 5   | Liêu Hoàng Nguyên            | Developer   |
| 6   | Nguyễn Lâm An                | Developer   |

---

## Features

### Core Features

- **File Server** - Serve files over TCP/IP network
- **Multi-file Download** - Download multiple files simultaneously
- **Real-time Search** - Instantly search files by name or extension
- **Modern UI** - Professional WPF interface with dark/light design
- **Drag & Drop** - Drag files from server to download queue
- **Progress Tracking** - Real-time download progress and speed
- **Concurrent Downloads** - 3 simultaneous downloads by default

### Configuration & Deployment

- ✅ **Custom Ports** - Easy port configuration via CLI argument or environment variable
- ✅ **Auto Folder Creation** - Server auto-creates 'files' folder on startup
- ✅ **Cross-Platform Server** - Runs on Linux VPS and Windows Server
- ✅ **Windows Service Support** - Run as Windows Service using NSSM
- ✅ **Systemd Support** - Linux systemd service integration

### UI/UX Enhancements

- ✅ **Loading Overlay** - Visual feedback during server connection
- ✅ **Button State Management** - Buttons disabled during operations
- ✅ **Real-time Status Updates** - Shows connection progress
- ✅ **Error Handling** - Clear error messages and recovery options
- ✅ **Responsive Design** - Auto-scrolling lists, text ellipsis for long names

---

## Architecture

\\\
MultiFileDownloader/
├── Code/
│ ├── MultiFileDownloader.Shared/ # Shared models & utilities
│ ├── MultiFileDownloader.Server/ # Server application
│ └── MultiFileDownloader.Client/ # WPF Client application
\\\

---

## Quick Start

### Prerequisites

- **.NET 10 SDK** or Runtime
- Windows 10+ or Linux
- Visual Studio 2022+ (optional)

### Installation

1. Clone Repository
   \\\ash
   git clone https://github.com/nhnnguyen712/NP_NetGroup11_UDM12.git
   cd NP_NetGroup11_UDM12
   git checkout dev
   \\\

2. Build Project
   \\\ash
   dotnet build
   \\\

3. Run Server
   \\\ash
   cd Code/MultiFileDownloader.Server
   dotnet run
   \\\

4. Run Client
   \\\ash
   cd Code/MultiFileDownloader.Client
   dotnet run
   \\\

---

## Documentation

| Guide                                                       | Purpose                       |
| ----------------------------------------------------------- | ----------------------------- |
| [Linux VPS Setup](./SETUP_VPS_GUIDE.md)                     | Deploy on Linux VPS           |
| [Windows Server 2012](./SETUP_WINDOWS_SERVER_2012_GUIDE.md) | Deploy on Windows Server 2012 |
| [Port Configuration](./HOW_TO_CHANGE_SERVER_PORT.md)        | Change server port            |

---

## Network Protocol

**Custom TCP Protocol:**

- Request/Response based
- Binary packet format
- Commands: RequestFileList, SendFileList, RequestDownload, SendFileSize, SendFileChunk, DownloadComplete

---

## Configuration

### Server Port

**Command line:**
\\\ash
dotnet run 9999
\\\

**Environment variable:**
\\\ash
export FILE_DOWNLOADER_PORT=9999
dotnet run
\\\

### Client Connection

\\\
Host:Port format (default: 127.0.0.1:8888)
Examples:

- 127.0.0.1:8888
- 192.168.1.100:8888
- example.com:9999
  \\\

---

## Project Statistics

- **Total Files:** 12 C# files
- **Total Lines:** ~3,500 LOC
- **Framework:** .NET 10
- **Architecture:** Multi-tier Client-Server
- **Design Patterns:** Observer, Factory

---

## Development Setup

### Prerequisites

- Visual Studio 2022 Community or newer
- .NET 10 SDK
- Git

### Build & Debug

**Visual Studio:** Open .sln and press F5
**Command Line:**
\\\ash

# Terminal 1 - Server

dotnet run --project Code/MultiFileDownloader.Server

# Terminal 2 - Client

dotnet run --project Code/MultiFileDownloader.Client
\\\

---

## Troubleshooting

**Connection Refused:**

- Start server first
- \dotnet run --project Code/MultiFileDownloader.Server\

**Port Already in Use:**

- Use different port: \dotnet run 9999\

**Files Folder Not Found:**

- Server auto-creates it on startup
- Or manually: \mkdir files\

---

## Deployment

### Linux VPS

See [SETUP_VPS_GUIDE.md](./SETUP_VPS_GUIDE.md) for detailed instructions

### Windows Server 2012

See [SETUP_WINDOWS_SERVER_2012_GUIDE.md](./SETUP_WINDOWS_SERVER_2012_GUIDE.md) for detailed instructions

---

## 📝 Implemented Features

✅ Phase 1

- Basic TCP Server/Client
- File transfer protocol
- Simple UI

✅ Phase 2

- WPF Modern UI
- Multi-file download
- Progress tracking
- Concurrent downloads
- Drag & drop

✅ Phase 3

- Real-time search
- Loading overlay UI
- Custom ports
- Auto folder creation
- Error handling

✅ Phase 4

- Deployment guides (Linux & Windows)
- Service support (systemd & NSSM)
- Comprehensive documentation

---

## Educational Value

Demonstrates:

1. Network Programming (TCP/IP)
2. C# Async/Await
3. WPF Development
4. Software Architecture
5. Deployment & DevOps
6. Documentation

---

## Support

- **GitHub:** [nhnnguyen712/NP_NetGroup11_UDM12](https://github.com/nhnnguyen712/NP_NetGroup11_UDM12)
- **Branch:** main
- **Issues:** Use GitHub Issues
- **Contact:** Team members

---

**Last Updated:** December 2024  
**Status:** ✅ Active Development  
**Version:** 1.0.0
