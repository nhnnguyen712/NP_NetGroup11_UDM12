# Multi File Downloader

A TCP-based file transfer system built with C# and .NET 10. The application features a WPF desktop client and an asynchronous TCP server, supporting concurrent multi-file downloads, real-time search, drag-and-drop, and cross-platform server deployment.

---

## Repository

| | |
|---|---|
| **Repository** | https://github.com/nhnnguyen712/NP_NetGroup11_UDM12 |
| **Course** | Network Programming |
| **Group** | UDM12 — Group 11 |
| **Target Framework** | .NET 10 |
| **Video** | https://drive.google.com/drive/folders/1FnpQlH6oRj2fdJr8wYw1QNC0XdFApPH3 |

---

## Team Members

| No. | Name | Role |
|-----|------|------|
| 1 | Nguyễn Huỳnh Nhật Nguyên | Team Leader |
| 2 | Nguyễn Hữu Khánh Nguyên | Developer |
| 3 | Trần Đăng Khoa | Developer |
| 4 | Tống Quốc Trọng | Developer |
| 5 | Liêu Hoàng Nguyên | Developer |
| 6 | Nguyễn Lâm An | Developer |

---

## Architecture

```
NP_NetGroup11_UDM12/
├── Code/
│   ├── MultiFileDownloader.Shared/       # Shared protocol, models, and utilities
│   ├── MultiFileDownloader.Server/       # TCP server application
│   ├── MultiFileDownloader.Client/       # WPF desktop client
│   └── MultiFileDownloader.StressTest/   # Automated stress & performance testing
├── Extra/
│   ├── Documentation/                    # Deployment and configuration guides
│   └── Test report/                      # Stress test reports and raw data
├── DOCX/                                 # Project documentation (Word)
├── PPTX/                                 # Presentation slides
└── README.md
```

---

## Features

### Core

* File server over TCP/IP with custom binary protocol
* Concurrent multi-file downloads (configurable, default: 3 simultaneous)
* Real-time file search by name or extension
* Download progress tracking per file
* Drag-and-drop support for file selection

### Client (WPF)

* Loading overlay during server connection
* Button state management during operations
* Real-time connection status indicator
* Error handling with user-friendly feedback
* Responsive layout with optimized list rendering

### Server

* Asynchronous I/O with `async`/`await` for high concurrency
* Per-client task isolation via `Task.Run()`
* Shared file access (`FileShare.Read`) for concurrent downloads
* Custom port configuration via CLI argument or environment variable
* Automatic creation of server storage directory
* Deployable as a Windows Service (NSSM) or Linux systemd service

### Testing

* Automated stress and performance test suite
* 7 test categories covering latency, throughput, and scalability
* Tested up to 100 concurrent connections and 20 simultaneous downloads
* Markdown report generation with statistical analysis

---

## Network Protocol

The application uses a custom TCP protocol with a binary packet format:

```
┌──────────────┬─────────────────────────┬──────────────────┐
│   Command    │   Payload Length        │   Payload Data   │
│   (1 byte)   │   (4 bytes, Big-Endian) │   (N bytes)      │
└──────────────┴─────────────────────────┴──────────────────┘
```

| Code | Command | Direction | Description |
|------|---------|-----------|-------------|
| 1 | `RequestFileList` | Client → Server | Request the list of available files |
| 2 | `SendFileList` | Server → Client | Return pipe-delimited file names |
| 3 | `RequestDownload` | Client → Server | Request file download (payload = filename) |
| 4 | `SendFileSize` | Server → Client | Send file size (8-byte Int64) |
| 5 | `SendFileChunk` | Server → Client | Send a data chunk (up to 16 KB) |
| 6 | `DownloadComplete` | Server → Client | Signal download completion |

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
* Windows 10+ (client requires WPF)
* Visual Studio 2022+ (optional)

### Build and Run

```bash
# Clone the repository
git clone https://github.com/nhnnguyen712/NP_NetGroup11_UDM12.git
cd NP_NetGroup11_UDM12

# Build all projects
dotnet build

# Terminal 1 — Start the server
dotnet run --project Code/MultiFileDownloader.Server

# Terminal 2 — Start the client
dotnet run --project Code/MultiFileDownloader.Client
```

### Server Configuration

The server port is determined in order of priority:

| Priority | Method | Example |
|----------|--------|---------|
| 1 (highest) | CLI argument | `dotnet run 9999` |
| 2 | Environment variable | `SET FILE_DOWNLOADER_PORT=9999` |
| 3 (default) | Built-in default | Port `8888` |

### Client Connection

Enter the server address in the client's connection prompt:

```
127.0.0.1:8888          # Localhost
192.168.1.100:8888      # LAN
example.com:9999        # Remote server
```

---

## Performance

Tested on a 16-core machine with .NET 10.0.6. Full report available in `Extra/Test report/`.

| Metric | Result |
|--------|--------|
| Connection latency (P99) | 0.48 ms |
| File list query latency (P99) | 0.38 ms |
| Single-client throughput | 230–332 MB/s |
| Peak aggregate throughput (10 clients) | 1,353 MB/s |
| Concurrent connections tested | 100 (100% success) |
| Concurrent downloads tested | 20 (100% success) |

---

## Documentation

| Document | Location |
|----------|----------|
| Linux VPS deployment guide | `Extra/Documentation/SETUP_VPS_GUIDE.md` |
| Windows Server 2012 deployment guide | `Extra/Documentation/SETUP_WINDOWS_SERVER_2012_GUIDE.md` |
| Server port configuration guide | `Extra/Documentation/HOW_TO_CHANGE_SERVER_PORT.md` |
| Stress test report (English) | `Extra/Test report/StressTest_Report_Group11_EN.md` |
| Raw test data | `Extra/Test report/TestReport_20260418_100850.md` |

---

## Project Statistics

| Metric | Value |
|--------|-------|
| Source files (.cs + .xaml) | 18 |
| Lines of code | ~2,200 |
| Target framework | .NET 10 |
| Architecture | Client-Server (TCP) |
| UI framework | WPF |
| Projects in solution | 4 |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection refused | Ensure the server is running before starting the client |
| Port already in use | Run with a different port: `dotnet run 9999` |
| Missing files folder | The server creates it automatically; or run `mkdir files` manually |
| Firewall blocking | Open TCP port 8888 in firewall settings |

---

## License

This project was developed for educational purposes as part of the Network Programming course at the University of Transport Ho Chi Minh City (UTH).
