using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

class FolderSync
{
    static void Main(string[] args)
    {
        // Check if the correct number of arguments is provided
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: FolderSync <source_folder> <replica_folder> <sync_interval_in_seconds> <log_file>");
            return;
        }

        // Assign arguments to variables
        string sourceFolder = args[0];
        string replicaFolder = args[1];
        int syncInterval;
        string logFile = args[3];

        // Validate sync interval, ensuring it is a valid number
        if (!int.TryParse(args[2], out syncInterval))
        {
            Console.WriteLine("Invalid sync interval. It should be a number.");
            return;
        }

        // Infinite loop to perform periodic synchronization
        while (true)
        {
            try
            {
                SynchronizeFolders(sourceFolder, replicaFolder, logFile);
            }
            catch (Exception ex)
            {
                LogMessage(logFile, $"Error during synchronization: {ex.Message}");
            }

            Thread.Sleep(syncInterval * 1000);
        }
    }

    // Function to synchronize the source and replica folders
    static void SynchronizeFolders(string source, string replica, string logFile)
    {
        if (!Directory.Exists(replica))
        {
            Directory.CreateDirectory(replica);
            LogMessage(logFile, $"Created directory: {replica}");
        }

        // Sync files from source folder to replica folder
        foreach (var sourceFilePath in Directory.GetFiles(source))
        {
            var replicaFilePath = Path.Combine(replica, Path.GetFileName(sourceFilePath));

            if (!File.Exists(replicaFilePath) || !FilesAreEqual(sourceFilePath, replicaFilePath))
            {
                File.Copy(sourceFilePath, replicaFilePath, true);
                LogMessage(logFile, $"Copied/Updated file: {sourceFilePath} to {replicaFilePath}");
            }
        }

        // Delete files from the replica folder that do not exist in the source folder
        foreach (var replicaFilePath in Directory.GetFiles(replica))
        {
            var sourceFilePath = Path.Combine(source, Path.GetFileName(replicaFilePath));

            if (!File.Exists(sourceFilePath))
            {
                File.Delete(replicaFilePath);
                LogMessage(logFile, $"Deleted file: {replicaFilePath}");
            }
        }

        // Recursively synchronize subdirectories
        foreach (var sourceDirPath in Directory.GetDirectories(source))
        {
            var replicaDirPath = Path.Combine(replica, Path.GetFileName(sourceDirPath));
           
            SynchronizeFolders(sourceDirPath, replicaDirPath, logFile);
        }

        // Delete directories from the replica folder that do not exist in the source folder
        foreach (var replicaDirPath in Directory.GetDirectories(replica))
        {
            var sourceDirPath = Path.Combine(source, Path.GetFileName(replicaDirPath));

            if (!Directory.Exists(sourceDirPath))
            {
                Directory.Delete(replicaDirPath, true);
                LogMessage(logFile, $"Deleted directory: {replicaDirPath}");
            }
        }
    }

    // Function to check if two files are identical by comparing their MD5 hashes
    static bool FilesAreEqual(string filePath1, string filePath2)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash1 = md5.ComputeHash(File.ReadAllBytes(filePath1));
            byte[] hash2 = md5.ComputeHash(File.ReadAllBytes(filePath2));

            return BitConverter.ToString(hash1) == BitConverter.ToString(hash2);
        }
    }

    // Function to log messages to both the console and a log file
    static void LogMessage(string logFile, string message)
    {
        string logEntry = $"{DateTime.Now}: {message}";
        Console.WriteLine(logEntry); 

        File.AppendAllText(logFile, logEntry + Environment.NewLine);
    }
}
