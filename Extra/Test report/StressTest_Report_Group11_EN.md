# STRESS & PERFORMANCE TEST REPORT

## MULTI FILE DOWNLOADER APPLICATION

**Group 11 – UDM12**  
**Course**: Network Programming  
**Date**: April 18, 2026

---

## TABLE OF CONTENTS

1. [Introduction](#1-introduction)
2. [Test Environment](#2-test-environment)
3. [Testing Methodology](#3-testing-methodology)
4. [Performance Test Results](#4-performance-test-results)
5. [Stress Test Results](#5-stress-test-results)
6. [Analysis & Discussion](#6-analysis--discussion)
7. [Conclusion](#7-conclusion)

---

## 1. Introduction

### 1.1. Purpose

This report presents the results of automated stress and performance testing conducted on the **Multi File Downloader** application — a TCP-based file transfer system that supports multiple concurrent client connections. The objective is to evaluate the server's responsiveness, throughput, and stability under varying levels of load.

### 1.2. Scope

The test suite consists of **7 automated tests** organized into two categories:

| Category | # | Test Name | Description |
|----------|---|-----------|-------------|
| **Performance** | 1 | Connection Latency | Measures TCP handshake latency |
| | 2 | File List Latency | Measures file list query response time |
| | 3 | Single Download Throughput | Measures single-client download bandwidth |
| | 4 | Concurrent Download Scalability | Evaluates throughput scaling with 1–10 clients |
| **Stress** | 5 | Connection Stress | Tests 10–100 simultaneous TCP connections |
| | 6 | File List Stress | Tests 10–50 simultaneous file list requests |
| | 7 | Download Stress | Tests 5–20 simultaneous file downloads |

### 1.3. Test Tool

All tests are executed by the **MultiFileDownloader.StressTest** console application — a custom-built .NET program that spawns multiple TCP clients using the application's native binary protocol, ensuring tests accurately reflect real-world client behavior.

---

## 2. Test Environment

| Property | Value |
|----------|-------|
| Operating System | Microsoft Windows NT 10.0.26200.0 |
| Machine | DESKTOP-TK8T9L5 |
| CPU | 16 cores |
| .NET Runtime | 10.0.6 |
| Server Address | 192.168.1.34:8888 (LAN) |
| Files on Server | 5 |
| Test File | NP_NetGroup11_UDM12.docx (531.3 KB) |

---

## 3. Testing Methodology

### 3.1. Application Protocol

The application uses a custom TCP protocol with the following packet structure:

```
┌──────────────┬─────────────────────────┬──────────────────┐
│   Command    │   Payload Length        │   Payload Data   │
│   (1 byte)   │   (4 bytes, Big-Endian) │   (N bytes)      │
└──────────────┴─────────────────────────┴──────────────────┘
```

Protocol commands:

| Code | Command | Direction | Description |
|------|---------|-----------|-------------|
| 1 | RequestFileList | Client → Server | Request the list of available files |
| 2 | SendFileList | Server → Client | Return pipe-delimited file names |
| 3 | RequestDownload | Client → Server | Request file download (payload = filename) |
| 4 | SendFileSize | Server → Client | Send file size (8-byte Int64) |
| 5 | SendFileChunk | Server → Client | Send a data chunk (up to 16 KB) |
| 6 | DownloadComplete | Server → Client | Signal download completion |

### 3.2. Measurement Approach

- **Timer**: `System.Diagnostics.Stopwatch` (high-resolution hardware timer)
- **Concurrency**: `Task.Run()` + `Task.WhenAll()` to spawn truly parallel TCP clients on the .NET Thread Pool
- **Statistical Metrics**: Average, Min, Max, P50 (Median), P95, P99, Throughput (MB/s)

---

## 4. Performance Test Results

### 4.1. Connection Latency

**Method**: 20 sequential TCP connections to the server.

| Metric | Value |
|--------|-------|
| Average | 0.13 ms |
| Minimum | 0.09 ms |
| Maximum | 0.55 ms |
| P50 (Median) | 0.10 ms |
| P95 | 0.20 ms |
| P99 | 0.48 ms |
| Errors | 0 |

**Result**: **PASS** — Sub-millisecond latency with zero errors. The server's TCP accept loop is highly responsive.

---

### 4.2. File List Latency

**Method**: 20 file list requests on a single reused TCP connection.

| Metric | Value |
|--------|-------|
| Average | 0.12 ms |
| Minimum | 0.10 ms |
| Maximum | 0.43 ms |
| P50 (Median) | 0.10 ms |
| P95 | 0.18 ms |
| P99 | 0.38 ms |
| Errors | 0 |

**Result**: **PASS** — File list retrieval completes in under 0.5 ms consistently. The protocol overhead is negligible.

---

### 4.3. Single Download Throughput

**Method**: Download `NP_NetGroup11_UDM12.docx` (531.3 KB) 5 times sequentially, each on a fresh connection.

| Metric | Value |
|--------|-------|
| File Size | 531.3 KB |
| Average Time | 2.3 ms |
| Min Time | 1.3 ms |
| Max Time | 5.4 ms |
| Average Throughput | 230.08 MB/s |
| Errors | 0 |

**Result**: **PASS** — Average throughput of 230 MB/s demonstrates efficient I/O handling with the 16 KB chunked transfer strategy.

---

### 4.4. Concurrent Download Scalability

**Method**: Download `NP_NetGroup11_UDM12.docx` with 1, 2, 3, 5, and 10 concurrent clients.

| Concurrency | Avg Time (ms) | Total Throughput | Per-Client Throughput | Success | Failed |
|-------------|---------------|------------------|-----------------------|---------|--------|
| 1 | 1.6 | 332.37 MB/s | 332.37 MB/s | 1 | 0 |
| 2 | 2.1 | 476.73 MB/s | 238.36 MB/s | 2 | 0 |
| 3 | 1.4 | 1,000.65 MB/s | 333.55 MB/s | 3 | 0 |
| 5 | 2.6 | 953.84 MB/s | 190.77 MB/s | 5 | 0 |
| 10 | 4.1 | 988.08 MB/s | 98.81 MB/s | 10 | 0 |

**Result**: **PASS** — 100% success rate at all concurrency levels.

**Key Observations**:
- Total throughput **scales effectively**, reaching ~1 GB/s with 3+ clients.
- Per-client throughput decreases gracefully as concurrency increases — expected behavior due to shared I/O bandwidth.
- Average latency increases only modestly from 1.6 ms (1 client) to 4.1 ms (10 clients).

---

## 5. Stress Test Results

### 5.1. Connection Stress Test

**Method**: Spawn N TCP clients simultaneously and measure connection times.

| Clients | Success | Failed | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
|---------|---------|--------|----------|----------|----------|----------|
| 10 | 10 | 0 | 0.3 | 0.3 | 0.4 | 0.4 |
| 25 | 25 | 0 | 0.4 | 0.2 | 0.7 | 0.7 |
| 50 | 50 | 0 | 0.7 | 0.2 | 1.4 | 1.3 |
| 100 | 100 | 0 | 0.2 | 0.1 | 0.7 | 0.6 |

**Result**: **PASS** — The server successfully handled **100 simultaneous TCP connections** with zero failures. All connections completed in under 1.5 ms.

---

### 5.2. File List Request Stress Test

**Method**: N clients connect and request the file list simultaneously.

| Clients | Success | Failed | Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |
|---------|---------|--------|----------|----------|----------|----------|
| 10 | 10 | 0 | 1.2 | 0.3 | 1.5 | 1.5 |
| 25 | 25 | 0 | 2.3 | 0.5 | 3.1 | 3.0 |
| 50 | 50 | 0 | 2.6 | 0.9 | 3.9 | 3.7 |

**Result**: **PASS** — 100% success across all levels. Latency scales linearly, staying below 4 ms even at 50 concurrent clients.

---

### 5.3. Concurrent Download Stress Test

**Method**: N clients simultaneously download `NP_NetGroup11_UDM12.docx` (531.3 KB).

| Clients | Success | Failed | Avg (ms) | Min (ms) | Max (ms) | Total Data | Total Throughput |
|---------|---------|--------|----------|----------|----------|------------|------------------|
| 5 | 5 | 0 | 2.6 | 1.3 | 3.0 | 2.6 MB | 855.66 MB/s |
| 10 | 10 | 0 | 3.3 | 1.4 | 3.8 | 5.2 MB | 1,353.13 MB/s |
| 20 | 20 | 0 | 6.5 | 1.1 | 8.4 | 10.4 MB | 1,232.91 MB/s |

**Result**: **PASS** — All 20 concurrent downloads completed successfully with zero failures.

**Key Observations**:
- Total throughput peaks at **1,353 MB/s (~1.32 GB/s)** with 10 clients.
- At 20 clients, throughput remains above 1.2 GB/s, showing the server handles heavy load without significant degradation.
- Average latency increases from 2.6 ms (5 clients) to 6.5 ms (20 clients) — a linear and predictable scaling pattern.

---

## 6. Analysis & Discussion

### 6.1. Overall Results Summary

| Test | Status | Key Metric |
|------|--------|------------|
| Connection Latency | PASS | 0.13 ms average, 0 errors |
| File List Latency | PASS | 0.12 ms average, 0 errors |
| Single Download | PASS | 230.08 MB/s throughput |
| Concurrent Scalability (10 clients) | PASS | 988 MB/s total, 100% success |
| Connection Stress (100 clients) | PASS | 100% success, < 1.5 ms |
| File List Stress (50 clients) | PASS | 100% success, < 4 ms |
| Download Stress (20 clients) | PASS | 100% success, 1.23 GB/s |

**All 7 tests passed with a 100% success rate.**

### 6.2. Server Architecture Strengths

1. **Asynchronous I/O**: The server uses `async/await` throughout the request pipeline, allowing efficient utilization of all 16 CPU cores without thread blocking.
2. **Per-client Task isolation**: Each client connection is handled on a separate `Task.Run()`, ensuring one slow client does not block others.
3. **Shared file access**: Files are opened with `FileAccess.Read` and `FileShare.Read`, enabling multiple clients to read the same file concurrently without lock contention.
4. **Chunked transfer**: The 16 KB chunk size provides a good balance between protocol overhead and memory efficiency.

### 6.3. Throughput Scaling Analysis

```
Throughput vs. Concurrency:

  1,400 ┤                        ● 1,353 MB/s (10 clients)
  1,200 ┤                                    ● 1,233 MB/s (20 clients)
  1,000 ┤            ● 1,001 MB/s
    800 ┤                    ● 954 MB/s
    600 ┤
    400 ┤    ● 477 MB/s
    200 ┤● 332 MB/s
        └─────┬──────┬──────┬──────┬──────┬──────
              1      2      3      5     10     20    Clients
```

The system achieves near-linear throughput scaling up to 3 clients, then plateaus at approximately **1 GB/s** — likely bounded by OS loopback/LAN interface bandwidth and memory copy overhead.

### 6.4. Latency Under Load

| Load Level | Connection (ms) | File List (ms) | Download (ms) |
|------------|----------------|----------------|---------------|
| Light (10) | 0.3 | 1.2 | 3.3 |
| Medium (25-50) | 0.4–0.7 | 2.3–2.6 | — |
| Heavy (100) | 0.2 | — | — |
| Download (20) | — | — | 6.5 |

Latency remains consistently low across all load levels. Even under the heaviest download stress (20 concurrent clients), average download latency is only 6.5 ms for a 531 KB file.

---

## 7. Conclusion

### 7.1. Summary

The Multi File Downloader server has been rigorously tested under both performance and stress conditions. All 7 test categories achieved **100% success rates** with zero errors, demonstrating that the system is robust, scalable, and production-ready.

### 7.2. Key Performance Metrics

| Metric | Value |
|--------|-------|
| Maximum concurrent connections tested | **100** (100% success) |
| Maximum concurrent downloads tested | **20** (100% success) |
| Peak aggregate throughput | **1,353 MB/s** (~1.32 GB/s) |
| Single-client throughput | **230–332 MB/s** |
| Connection latency (P99) | **0.48 ms** |
| File list query latency (P99) | **0.38 ms** |

### 7.3. Assessment

| Requirement | Status | Evidence |
|-------------|--------|----------|
| Support multiple concurrent connections | Met | 100 connections, 0 failures |
| Support concurrent file downloads | Met | 20 simultaneous downloads, 0 failures |
| Low-latency response | Met | Sub-millisecond connection and query latency |
| High throughput | Met | >1 GB/s aggregate with 10+ clients |
| System stability under load | Met | Zero errors across all test scenarios |

The application meets all performance and reliability requirements for a multi-client file transfer system.

---

*Report prepared by Group 11 – UDM12, Network Programming Course, UTH.*
