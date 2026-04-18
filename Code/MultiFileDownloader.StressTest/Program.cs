using System.Diagnostics;
using System.Text;

namespace MultiFileDownloader.StressTest
{
    class Program
    {
        static string host = "127.0.0.1";
        static int port = 8888;
        static string[] serverFiles = Array.Empty<string>();
        static StringBuilder report = new StringBuilder();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Multi File Downloader – Stress & Performance Test";

            PrintBanner();

            // ── Get server address ──────────────────────────────────
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  Server address (host:port) [127.0.0.1:8888]: ");
            Console.ResetColor();

            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                var parts = input.Split(':');
                host = parts[0];

                if (parts.Length > 1 && int.TryParse(parts[1], out int p))
                    port = p;
            }

            // ── Verify connection ───────────────────────────────────
            Console.WriteLine();

            WriteColored("  Connecting to ", ConsoleColor.Gray);
            WriteLineColored($"{host}:{port}...", ConsoleColor.Yellow);

            try
            {
                using var probe = new TestClient(host, port);
                double connectTime = await probe.ConnectAsync();

                var (listTime, files) = await probe.GetFileListAsync();
                serverFiles = files;

                WriteLineColored(
                    $"  ✓ Connected in {connectTime:F1}ms — " +
                    $"{files.Length} file(s) on server",
                    ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteLineColored(
                    $"  ✗ Connection failed: {ex.Message}",
                    ConsoleColor.Red);

                Console.WriteLine("  Press any key to exit...");
                Console.ReadKey();
                return;
            }

            if (serverFiles.Length == 0)
            {
                WriteLineColored(
                    "\n  ⚠ No files on server. " +
                    "Add files to the server's ./files folder " +
                    "and restart this test.",
                    ConsoleColor.Yellow);

                Console.WriteLine("  Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // ── Show file list ──────────────────────────────────────
            Console.WriteLine();
            WriteLineColored("  Files on server:", ConsoleColor.DarkGray);

            for (int i = 0; i < serverFiles.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"    {i + 1}. ");
                Console.ResetColor();
                Console.WriteLine(serverFiles[i]);
            }

            // ── Initialize report ───────────────────────────────────
            report.AppendLine("# Stress Test & Performance Test Report");
            report.AppendLine();
            report.AppendLine("## Test Environment");
            report.AppendLine();
            report.AppendLine($"| Property | Value |");
            report.AppendLine($"|----------|-------|");
            report.AppendLine($"| Date | {DateTime.Now:yyyy-MM-dd HH:mm:ss} |");
            report.AppendLine($"| OS | {Environment.OSVersion} |");
            report.AppendLine($"| Machine | {Environment.MachineName} |");
            report.AppendLine($"| Processors | {Environment.ProcessorCount} cores |");
            report.AppendLine($"| .NET | {Environment.Version} |");
            report.AppendLine($"| Server | {host}:{port} |");
            report.AppendLine($"| Files on server | {serverFiles.Length} |");
            report.AppendLine();

            // ── Run all tests ───────────────────────────────────────
            string testFile = serverFiles[0];

            Console.WriteLine();
            PrintSectionHeader("PERFORMANCE TESTS");

            await RunConnectionLatencyTest(20);
            await RunFileListLatencyTest(20);
            await RunDownloadThroughputTest(testFile, 5);
            await RunConcurrentScalabilityTest(
                testFile, new[] { 1, 2, 3, 5, 10 });

            Console.WriteLine();
            PrintSectionHeader("STRESS TESTS");

            await RunConnectionStressTest(new[] { 10, 25, 50, 100 });
            await RunFileListStressTest(new[] { 10, 25, 50 });
            await RunDownloadStressTest(
                testFile, new[] { 5, 10, 20 });

            // ── Save report ─────────────────────────────────────────
            Console.WriteLine();
            PrintSectionHeader("REPORT");

            string reportDir = Path.Combine(
                AppContext.BaseDirectory, "TestResults");

            Directory.CreateDirectory(reportDir);

            string reportFile = Path.Combine(reportDir,
                $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.md");

            await File.WriteAllTextAsync(reportFile, report.ToString());

            WriteLineColored(
                $"  ✓ Report saved: {reportFile}",
                ConsoleColor.Green);

            Console.WriteLine();

            WriteLineColored(
                "  All tests completed. Press any key to exit...",
                ConsoleColor.Cyan);

            Console.ReadKey();
        }

        // ════════════════════════════════════════════════════════════
        //  PERFORMANCE TESTS
        // ════════════════════════════════════════════════════════════

        static async Task RunConnectionLatencyTest(int iterations)
        {
            PrintTestHeader(
                "1. Connection Latency",
                $"Establish {iterations} sequential TCP connections");

            var times = new List<double>();
            int failures = 0;

            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    using var c = new TestClient(host, port);
                    double ms = await c.ConnectAsync();
                    times.Add(ms);
                    PrintProgress(i + 1, iterations);
                }
                catch
                {
                    failures++;
                }
            }

            ClearProgress();

            if (times.Count > 0)
            {
                PrintLatencyStats(times, failures);
                AppendLatencyReport(
                    "Connection Latency", iterations, times, failures);
            }
        }

        static async Task RunFileListLatencyTest(int iterations)
        {
            PrintTestHeader(
                "2. File List Request Latency",
                $"Request file list {iterations} times on reused connection");

            var times = new List<double>();
            int failures = 0;

            try
            {
                using var c = new TestClient(host, port);
                await c.ConnectAsync();

                for (int i = 0; i < iterations; i++)
                {
                    try
                    {
                        var (ms, _) = await c.GetFileListAsync();
                        times.Add(ms);
                        PrintProgress(i + 1, iterations);
                    }
                    catch
                    {
                        failures++;
                    }
                }
            }
            catch
            {
                failures = iterations;
            }

            ClearProgress();

            if (times.Count > 0)
            {
                PrintLatencyStats(times, failures);
                AppendLatencyReport(
                    "File List Latency", iterations, times, failures);
            }
        }

        static async Task RunDownloadThroughputTest(
            string fileName, int iterations)
        {
            PrintTestHeader(
                "3. Single Download Throughput",
                $"Download \"{fileName}\" {iterations} times sequentially");

            var times = new List<double>();
            var sizes = new List<long>();
            int failures = 0;

            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    using var c = new TestClient(host, port);
                    await c.ConnectAsync();

                    var (ms, bytes) = await c.DownloadFileAsync(fileName);

                    times.Add(ms);
                    sizes.Add(bytes);
                    PrintProgress(i + 1, iterations);
                }
                catch
                {
                    failures++;
                }
            }

            ClearProgress();

            if (times.Count > 0)
            {
                long avgSize = (long)sizes.Average();
                PrintThroughputStats(times, avgSize, failures);
                AppendThroughputReport(
                    "Single Download", fileName, iterations,
                    times, avgSize, failures);
            }
        }

        static async Task RunConcurrentScalabilityTest(
            string fileName, int[] levels)
        {
            PrintTestHeader(
                "4. Concurrent Download Scalability",
                $"Download \"{fileName}\" with varying concurrency levels");

            report.AppendLine("### Concurrent Download Scalability");
            report.AppendLine();
            report.AppendLine($"File: `{fileName}`");
            report.AppendLine();
            report.AppendLine(
                "| Concurrency | Avg Time (ms) | " +
                "Total Throughput (KB/s) | Per-Client (KB/s) | " +
                "Success | Failed |");
            report.AppendLine(
                "|-------------|---------------|" +
                "------------------------|--------------------" +
                "|---------|--------|");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌─────────────┬───────────────┬" +
                "────────────────────────┬──────────┬────────┐");

            Console.WriteLine(
                "  │ Concurrency │ Avg Time (ms) │" +
                " Total Throughput       │ Success  │ Failed │");

            Console.WriteLine(
                "  ├─────────────┼───────────────┼" +
                "────────────────────────┼──────────┼────────┤");

            Console.ResetColor();

            foreach (int level in levels)
            {
                var tasks = new List<Task<(double ms, long bytes, bool ok)>>();

                for (int i = 0; i < level; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var c = new TestClient(host, port);
                            await c.ConnectAsync();

                            var (ms, bytes) =
                                await c.DownloadFileAsync(fileName);

                            return (ms, bytes, true);
                        }
                        catch
                        {
                            return (0.0, 0L, false);
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);

                int success = results.Count(r => r.ok);
                int failed = results.Count(r => !r.ok);

                var okResults = results.Where(r => r.ok).ToArray();

                double avgMs = okResults.Length > 0
                    ? okResults.Average(r => r.ms) : 0;

                long totalBytes = okResults.Sum(r => r.bytes);

                double wallMs = okResults.Length > 0
                    ? okResults.Max(r => r.ms) : 0;

                double totalKBps = wallMs > 0
                    ? totalBytes / 1024.0 / (wallMs / 1000.0) : 0;

                double perClientKBps = level > 0
                    ? totalKBps / success : 0;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │ ");
                Console.ResetColor();

                Console.Write($"{level,11} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ResetColor();

                Console.Write($"{avgMs,13:F1} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{FormatThroughput(totalKBps),22} ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{success,8} ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");

                Console.ForegroundColor =
                    failed > 0 ? ConsoleColor.Red : ConsoleColor.Green;

                Console.Write($"{failed,6} ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("│");
                Console.ResetColor();

                report.AppendLine(
                    $"| {level} | {avgMs:F1} | " +
                    $"{FormatThroughput(totalKBps)} | " +
                    $"{FormatThroughput(perClientKBps)} | " +
                    $"{success} | {failed} |");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  └─────────────┴───────────────┴" +
                "────────────────────────┴──────────┴────────┘");
            Console.ResetColor();

            report.AppendLine();
        }

        // ════════════════════════════════════════════════════════════
        //  STRESS TESTS
        // ════════════════════════════════════════════════════════════

        static async Task RunConnectionStressTest(int[] clientCounts)
        {
            PrintTestHeader(
                "5. Connection Stress Test",
                "Simultaneous TCP connections at various scales");

            report.AppendLine("### Connection Stress Test");
            report.AppendLine();
            report.AppendLine(
                "| Clients | Success | Failed | " +
                "Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |");
            report.AppendLine(
                "|---------|---------|--------|" +
                "----------|----------|----------|----------|");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌─────────┬─────────┬────────┬" +
                "──────────┬──────────┬──────────┬──────────┐");
            Console.WriteLine(
                "  │ Clients │ Success │ Failed │" +
                " Avg (ms) │ Min (ms) │ Max (ms) │ P95 (ms) │");
            Console.WriteLine(
                "  ├─────────┼─────────┼────────┼" +
                "──────────┼──────────┼──────────┼──────────┤");
            Console.ResetColor();

            foreach (int count in clientCounts)
            {
                var tasks = new List<Task<(double ms, bool ok)>>();

                for (int i = 0; i < count; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var c = new TestClient(host, port);
                            double ms = await c.ConnectAsync();
                            return (ms, true);
                        }
                        catch
                        {
                            return (0.0, false);
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);

                int success = results.Count(r => r.ok);
                int failed = results.Count(r => !r.ok);

                var okTimes = results
                    .Where(r => r.ok)
                    .Select(r => r.ms)
                    .OrderBy(t => t)
                    .ToList();

                double avg = okTimes.Count > 0 ? okTimes.Average() : 0;
                double min = okTimes.Count > 0 ? okTimes.Min() : 0;
                double max = okTimes.Count > 0 ? okTimes.Max() : 0;
                double p95 = Percentile(okTimes, 95);

                PrintStressRow(count, success, failed, avg, min, max, p95);

                report.AppendLine(
                    $"| {count} | {success} | {failed} | " +
                    $"{avg:F1} | {min:F1} | {max:F1} | {p95:F1} |");

                // Brief pause between levels
                await Task.Delay(500);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  └─────────┴─────────┴────────┴" +
                "──────────┴──────────┴──────────┴──────────┘");
            Console.ResetColor();

            report.AppendLine();
        }

        static async Task RunFileListStressTest(int[] clientCounts)
        {
            PrintTestHeader(
                "6. File List Request Stress Test",
                "Simultaneous file list requests at various scales");

            report.AppendLine("### File List Request Stress Test");
            report.AppendLine();
            report.AppendLine(
                "| Clients | Success | Failed | " +
                "Avg (ms) | Min (ms) | Max (ms) | P95 (ms) |");
            report.AppendLine(
                "|---------|---------|--------|" +
                "----------|----------|----------|----------|");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌─────────┬─────────┬────────┬" +
                "──────────┬──────────┬──────────┬──────────┐");
            Console.WriteLine(
                "  │ Clients │ Success │ Failed │" +
                " Avg (ms) │ Min (ms) │ Max (ms) │ P95 (ms) │");
            Console.WriteLine(
                "  ├─────────┼─────────┼────────┼" +
                "──────────┼──────────┼──────────┼──────────┤");
            Console.ResetColor();

            foreach (int count in clientCounts)
            {
                var tasks = new List<Task<(double ms, bool ok)>>();

                for (int i = 0; i < count; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var c = new TestClient(host, port);
                            await c.ConnectAsync();

                            var (ms, _) = await c.GetFileListAsync();
                            return (ms, true);
                        }
                        catch
                        {
                            return (0.0, false);
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);

                int success = results.Count(r => r.ok);
                int failed = results.Count(r => !r.ok);

                var okTimes = results
                    .Where(r => r.ok)
                    .Select(r => r.ms)
                    .OrderBy(t => t)
                    .ToList();

                double avg = okTimes.Count > 0 ? okTimes.Average() : 0;
                double min = okTimes.Count > 0 ? okTimes.Min() : 0;
                double max = okTimes.Count > 0 ? okTimes.Max() : 0;
                double p95 = Percentile(okTimes, 95);

                PrintStressRow(count, success, failed, avg, min, max, p95);

                report.AppendLine(
                    $"| {count} | {success} | {failed} | " +
                    $"{avg:F1} | {min:F1} | {max:F1} | {p95:F1} |");

                await Task.Delay(500);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  └─────────┴─────────┴────────┴" +
                "──────────┴──────────┴──────────┴──────────┘");
            Console.ResetColor();

            report.AppendLine();
        }

        static async Task RunDownloadStressTest(
            string fileName, int[] clientCounts)
        {
            PrintTestHeader(
                "7. Concurrent Download Stress Test",
                $"Simultaneous downloads of \"{fileName}\"");

            report.AppendLine("### Concurrent Download Stress Test");
            report.AppendLine();
            report.AppendLine($"File: `{fileName}`");
            report.AppendLine();
            report.AppendLine(
                "| Clients | Success | Failed | " +
                "Avg (ms) | Min (ms) | Max (ms) | " +
                "Total Data | Total Throughput |");
            report.AppendLine(
                "|---------|---------|--------|" +
                "----------|----------|----------" +
                "|------------|------------------|");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌─────────┬─────────┬────────┬" +
                "──────────┬──────────┬──────────┬" +
                "────────────┬──────────────────┐");
            Console.WriteLine(
                "  │ Clients │ Success │ Failed │" +
                " Avg (ms) │ Min (ms) │ Max (ms) │" +
                " Total Data │ Throughput       │");
            Console.WriteLine(
                "  ├─────────┼─────────┼────────┼" +
                "──────────┼──────────┼──────────┼" +
                "────────────┼──────────────────┤");
            Console.ResetColor();

            foreach (int count in clientCounts)
            {
                var tasks =
                    new List<Task<(double ms, long bytes, bool ok)>>();

                for (int i = 0; i < count; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var c = new TestClient(host, port);
                            await c.ConnectAsync();

                            var (ms, bytes) =
                                await c.DownloadFileAsync(fileName);

                            return (ms, bytes, true);
                        }
                        catch
                        {
                            return (0.0, 0L, false);
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);

                int success = results.Count(r => r.ok);
                int failed = results.Count(r => !r.ok);

                var okResults = results.Where(r => r.ok).ToArray();

                var okTimes = okResults
                    .Select(r => r.ms)
                    .OrderBy(t => t)
                    .ToList();

                double avg = okTimes.Count > 0 ? okTimes.Average() : 0;
                double min = okTimes.Count > 0 ? okTimes.Min() : 0;
                double max = okTimes.Count > 0 ? okTimes.Max() : 0;

                long totalBytes = okResults.Sum(r => r.bytes);
                double wallMs = okTimes.Count > 0 ? okTimes.Max() : 1;
                double kbps = totalBytes / 1024.0 / (wallMs / 1000.0);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │ ");
                Console.ResetColor();
                Console.Write($"{count,7} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{success,7} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor =
                    failed > 0 ? ConsoleColor.Red : ConsoleColor.Green;
                Console.Write($"{failed,6} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ResetColor();
                Console.Write($"{avg,8:F1} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ResetColor();
                Console.Write($"{min,8:F1} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ResetColor();
                Console.Write($"{max,8:F1} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{FormatBytes(totalBytes),10} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{FormatThroughput(kbps),16} ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("│");
                Console.ResetColor();

                report.AppendLine(
                    $"| {count} | {success} | {failed} | " +
                    $"{avg:F1} | {min:F1} | {max:F1} | " +
                    $"{FormatBytes(totalBytes)} | " +
                    $"{FormatThroughput(kbps)} |");

                await Task.Delay(1000);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  └─────────┴─────────┴────────┴" +
                "──────────┴──────────┴──────────┴" +
                "────────────┴──────────────────┘");
            Console.ResetColor();

            report.AppendLine();
        }

        // ════════════════════════════════════════════════════════════
        //  DISPLAY HELPERS
        // ════════════════════════════════════════════════════════════

        static void PrintBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine(
                "  ╔══════════════════════════════════════════════╗");
            Console.WriteLine(
                "  ║   Multi File Downloader                     ║");
            Console.WriteLine(
                "  ║   Stress & Performance Test Suite            ║");
            Console.WriteLine(
                "  ║   Group 11 – UDM12                          ║");
            Console.WriteLine(
                "  ╚══════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintSectionHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(
                $"  ═══════ {title} ═══════");
            Console.ResetColor();

            report.AppendLine($"## {title}");
            report.AppendLine();
        }

        static void PrintTestHeader(string title, string description)
        {
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  ▶ {title}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  — {description}");
            Console.ResetColor();
        }

        static void PrintProgress(int current, int total)
        {
            Console.Write(
                $"\r  Progress: {current}/{total}  ");
        }

        static void ClearProgress()
        {
            Console.Write(
                "\r                                " +
                "                    \r");
        }

        static void PrintLatencyStats(
            List<double> times, int failures)
        {
            times.Sort();

            double avg = times.Average();
            double min = times.Min();
            double max = times.Max();
            double p50 = Percentile(times, 50);
            double p95 = Percentile(times, 95);
            double p99 = Percentile(times, 99);

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌──────────┬──────────┬──────────┬" +
                "──────────┬──────────┬──────────┬─────────┐");
            Console.WriteLine(
                "  │ Avg (ms) │ Min (ms) │ Max (ms) │" +
                " P50 (ms) │ P95 (ms) │ P99 (ms) │ Errors  │");
            Console.WriteLine(
                "  ├──────────┼──────────┼──────────┼" +
                "──────────┼──────────┼──────────┼─────────┤");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{avg,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{min,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{max,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ResetColor();
            Console.Write($"{p50,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ResetColor();
            Console.Write($"{p95,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ResetColor();
            Console.Write($"{p99,8:F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor =
                failures > 0 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($"{failures,7} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("│");

            Console.WriteLine(
                "  └──────────┴──────────┴──────────┴" +
                "──────────┴──────────┴──────────┴─────────┘");
            Console.ResetColor();
        }

        static void PrintThroughputStats(
            List<double> times, long avgSize, int failures)
        {
            times.Sort();

            double avgMs = times.Average();
            double minMs = times.Min();
            double maxMs = times.Max();

            double avgKBps =
                avgSize / 1024.0 / (avgMs / 1000.0);

            double bestKBps =
                avgSize / 1024.0 / (minMs / 1000.0);

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                "  ┌──────────────┬──────────┬──────────┬" +
                "──────────┬──────────────────┬─────────┐");
            Console.WriteLine(
                "  │ File Size    │ Avg (ms) │ Min (ms) │" +
                " Max (ms) │ Avg Throughput   │ Errors  │");
            Console.WriteLine(
                "  ├──────────────┼──────────┼──────────┼" +
                "──────────┼──────────────────┼─────────┤");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{FormatBytes(avgSize),12} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{avgMs,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{minMs,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{maxMs,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{FormatThroughput(avgKBps),16} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor =
                failures > 0 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($"{failures,7} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("│");

            Console.WriteLine(
                "  └──────────────┴──────────┴──────────┴" +
                "──────────┴──────────────────┴─────────┘");
            Console.ResetColor();
        }

        static void PrintStressRow(
            int clients, int success, int failed,
            double avg, double min, double max, double p95)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │ ");
            Console.ResetColor();
            Console.Write($"{clients,7} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{success,7} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor =
                failed > 0 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($"{failed,6} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ResetColor();
            Console.Write($"{avg,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{min,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{max,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ResetColor();
            Console.Write($"{p95,8:F1} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("│");
            Console.ResetColor();
        }

        // ════════════════════════════════════════════════════════════
        //  REPORT HELPERS
        // ════════════════════════════════════════════════════════════

        static void AppendLatencyReport(
            string name, int iterations,
            List<double> times, int failures)
        {
            times.Sort();

            report.AppendLine($"### {name}");
            report.AppendLine();
            report.AppendLine($"Iterations: {iterations}");
            report.AppendLine();
            report.AppendLine(
                "| Metric | Value |");
            report.AppendLine(
                "|--------|-------|");
            report.AppendLine(
                $"| Average | {times.Average():F2} ms |");
            report.AppendLine(
                $"| Minimum | {times.Min():F2} ms |");
            report.AppendLine(
                $"| Maximum | {times.Max():F2} ms |");
            report.AppendLine(
                $"| P50 (Median) | {Percentile(times, 50):F2} ms |");
            report.AppendLine(
                $"| P95 | {Percentile(times, 95):F2} ms |");
            report.AppendLine(
                $"| P99 | {Percentile(times, 99):F2} ms |");
            report.AppendLine(
                $"| Errors | {failures} |");
            report.AppendLine();
        }

        static void AppendThroughputReport(
            string name, string fileName, int iterations,
            List<double> times, long avgSize, int failures)
        {
            double avgMs = times.Average();
            double avgKBps = avgSize / 1024.0 / (avgMs / 1000.0);

            report.AppendLine($"### {name}");
            report.AppendLine();
            report.AppendLine($"File: `{fileName}` ({FormatBytes(avgSize)})");
            report.AppendLine($"Iterations: {iterations}");
            report.AppendLine();
            report.AppendLine("| Metric | Value |");
            report.AppendLine("|--------|-------|");
            report.AppendLine($"| File size | {FormatBytes(avgSize)} |");
            report.AppendLine($"| Average time | {avgMs:F1} ms |");
            report.AppendLine(
                $"| Min time | {times.Min():F1} ms |");
            report.AppendLine(
                $"| Max time | {times.Max():F1} ms |");
            report.AppendLine(
                $"| Avg throughput | {FormatThroughput(avgKBps)} |");
            report.AppendLine($"| Errors | {failures} |");
            report.AppendLine();
        }

        // ════════════════════════════════════════════════════════════
        //  UTILITY METHODS
        // ════════════════════════════════════════════════════════════

        static double Percentile(List<double> sorted, int pct)
        {
            if (sorted.Count == 0) return 0;
            if (sorted.Count == 1) return sorted[0];

            double index = (pct / 100.0) * (sorted.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper) return sorted[lower];

            double fraction = index - lower;

            return sorted[lower] + fraction *
                (sorted[upper] - sorted[lower]);
        }

        static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";

            if (bytes < 1024L * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F1} MB";

            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        static string FormatThroughput(double kbps)
        {
            if (kbps < 1024)
                return $"{kbps:F1} KB/s";

            return $"{kbps / 1024.0:F2} MB/s";
        }

        static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        static void WriteLineColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
