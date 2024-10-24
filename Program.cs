using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransfer
{
    internal class Program
    {
        private static volatile bool cancelled = false;
        private static long totalSize;
        private static long bytesCopied = 0;

        static async Task Main(string[] args)
        {
            string sourceFile = @"C:\Audhithiyah\textfile.txt"; // Replace with actual source file path
            string destinationFile = @"C:\Users\audhithiyah.srishank\Documents\sample\textfile.txt"; // Replace with actual destination file path

            // Ensure the source file exists
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("Source file does not exist.");
                return;
            }

            totalSize = new FileInfo(sourceFile).Length;

            // Start file copy and progress display
            var copyTask = Task.Run(() => CopyFile(sourceFile, destinationFile));
            var progressTask = Task.Run(() => UpdateProgress());

            Console.WriteLine("Press 'c' to cancel the file copy operation.");

            // Listen for cancellation input
            while (copyTask.Status == TaskStatus.Running)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).KeyChar == 'c')
                {
                    CancelCopy();
                    break;
                }
            }

            // Wait for tasks to complete
            await Task.WhenAll(copyTask, progressTask);
            Console.WriteLine("File copy operation finished.");
        }

        private static void CopyFile(string source, string destination)
        {
            using (FileStream srcStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[1024 * 1024]; // 1 MB buffer
                int bytesRead;

                while (!cancelled && (bytesRead = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, bytesRead);
                    Interlocked.Add(ref bytesCopied, bytesRead); // Safely update bytesCopied

                    // Debugging output
                    Console.WriteLine($"Bytes Read: {bytesRead}, Total Copied: {bytesCopied}");

                    Thread.Sleep(50); // Simulate delay for testing
                }
            }
        }

        private static void UpdateProgress()
        {
            const int barWidth = 50; // Width of the progress bar
            while (!cancelled && bytesCopied < totalSize)
            {
                double progress = (double)bytesCopied / totalSize;
                int progressChars = (int)(progress * barWidth);

                // Create the progress bar string
                string progressBar = new string('#', progressChars) + new string('-', barWidth - progressChars);
                Console.Write($"\r[{progressBar}] {progress * 100:F2}%"); // Overwrite the same line
                Console.Out.Flush(); // Force the console to refresh
                Thread.Sleep(500); // Update progress every 0.5 seconds
            }

            if (cancelled)
            {
                Console.WriteLine("\nFile copy cancelled.");
            }
            else
            {
                Console.WriteLine("\nFile copy completed successfully.");
            }
        }

        private static void CancelCopy()
        {
            cancelled = true;
        }
    }
}