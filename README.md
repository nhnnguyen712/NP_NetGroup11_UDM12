# Multi File Downloader

A network-based file downloading application built with C# and .NET 10. The system includes a WPF client and a TCP server, supporting concurrent downloads, real-time search, and cross-platform deployment.

## Repository Information

* Repository: https://github.com/nhnnguyen712/NP_NetGroup11_UDM12
* Branch: main
* Course: Network Programming
* Group: UDM12 - Group 11
* .NET Target: .NET 10
* Video: https://drive.google.com/drive/folders/1FnpQlH6oRj2fdJr8wYw1QNC0XdFApPH3

---

## Team Members

| No. | Name                     | Role        |
| --- | ------------------------ | ----------- |
| 1   | Nguyễn Huỳnh Nhật Nguyên | Team Leader |
| 2   | Nguyễn Hữu Khánh Nguyên  | Developer   |
| 3   | Trần Đăng Khoa           | Developer   |
| 4   | Tống Quốc Trọng          | Developer   |
| 5   | Liêu Hoàng Nguyên        | Developer   |
| 6   | Nguyễn Lâm An            | Developer   |

---

## Features

### Core Features

* File server over TCP/IP
* Multiple file downloads simultaneously
* Real-time file search by name or extension
* WPF-based user interface
* Drag and drop support
* Download progress tracking
* Concurrent downloads (default: 3 files)

### Configuration and Deployment

* Custom port configuration via command line or environment variable
* Automatic creation of server storage folder
* Cross-platform server support (Windows and Linux)
* Windows Service support using NSSM
* Linux systemd service support

### User Interface

* Loading overlay during server connection
* Button state management during operations
* Real-time connection status updates
* Error handling with user feedback
* Responsive layout with optimized list display

---

## Architecture

```
MultiFileDownloader/
├── Code/
│   ├── MultiFileDownloader.Shared/   # Shared models and utilities
│   ├── MultiFileDownloader.Server/   # Server application
│   └── MultiFileDownloader.Client/   # WPF client application
```

---

## Getting Started

### Prerequisites

* .NET 10 SDK or Runtime
* Windows 10 or Linux
* Visual Studio 2022 or later (optional)

### Installation

Clone the repository:

```
git clone https://github.com/nhnnguyen712/NP_NetGroup11_UDM12.git
cd NP_NetGroup11_UDM12
```

Build the project:

```
dotnet build
```

Run the server:

```
cd Code/MultiFileDownloader.Server
dotnet run
```

Run the client:

```
cd Code/MultiFileDownloader.Client
dotnet run
```

---

## Documentation

| File                               | Description                     |
| ---------------------------------- | ------------------------------- |
| SETUP_VPS_GUIDE.md                 | Linux VPS deployment guide      |
| SETUP_WINDOWS_SERVER_2012_GUIDE.md | Windows Server deployment guide |
| HOW_TO_CHANGE_SERVER_PORT.md       | Server port configuration       |

---

## Network Protocol

The application uses a custom TCP protocol with a binary packet format.

### Supported Commands

* RequestFileList
* SendFileList
* RequestDownload
* SendFileSize
* SendFileChunk
* DownloadComplete

---

## Configuration

### Server Port

Using command line:

```
dotnet run 9999
```

Using environment variable:

```
export FILE_DOWNLOADER_PORT=9999
dotnet run
```

### Client Connection

Format:

```
Host:Port
```

Examples:

```
127.0.0.1:8888
192.168.1.100:8888
example.com:9999
```

---

## Project Statistics

* Total files: 12
* Total lines of code: approximately 3,500
* Framework: .NET 10
* Architecture: Client-Server
* Design patterns: Observer, Factory

---

## Development

### Requirements

* Visual Studio 2022 or newer
* .NET 10 SDK
* Git

### Run for Development

```
# Terminal 1 - Server
dotnet run --project Code/MultiFileDownloader.Server

# Terminal 2 - Client
dotnet run --project Code/MultiFileDownloader.Client
```

---

## Troubleshooting

Connection refused:

* Ensure the server is running before starting the client

Port already in use:

```
dotnet run 9999
```

Missing files folder:

* The server will automatically create the folder
* Or create manually:

```
mkdir files
```

---

## Deployment

### Linux VPS

Refer to:

```
SETUP_VPS_GUIDE.md
```

### Windows Server

Refer to:

```
SETUP_WINDOWS_SERVER_2012_GUIDE.md
```

---

## Development Phases

### Phase 1

* Basic TCP client-server
* File transfer protocol
* Simple user interface

### Phase 2

* WPF interface
* Multi-file download
* Progress tracking
* Concurrent downloads
* Drag and drop

### Phase 3

* Real-time search
* Loading overlay
* Custom port configuration
* Automatic folder creation
* Error handling

### Phase 4

* Deployment documentation
* Service integration (systemd and NSSM)
* Complete documentation

---

## Educational Objectives

This project demonstrates:

* TCP/IP network programming
* Asynchronous programming in C#
* WPF application development
* Software architecture design
* Deployment and system configuration
* Technical documentation practices

---

## Support

* Repository: https://github.com/nhnnguyen712/NP_NetGroup11_UDM12
* Issue tracking: GitHub Issues
* Contact: Project team members

---

## Status

* Last updated: December 2024
* Version: 1.0.0
* Development status: Active
