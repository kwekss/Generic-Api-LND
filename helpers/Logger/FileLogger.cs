using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace helper.Logger
{
    public class FileLogger : IFileLogger
    {
        private readonly string error_dir;
        private readonly string info_dir;
        private readonly string warning_dir;
        private readonly ReaderWriterLockSlim _readWriteLock;

        public FileLogger(string baseDir)
        {
            _readWriteLock = new ReaderWriterLockSlim();

            if (string.IsNullOrWhiteSpace(baseDir)) baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            error_dir = Path.Combine(baseDir, "Errors");
            info_dir = Path.Combine(baseDir, "Info");
            warning_dir = Path.Combine(baseDir, "Warning");

            if (!Directory.Exists(error_dir)) Directory.CreateDirectory(error_dir);
            if (!Directory.Exists(info_dir)) Directory.CreateDirectory(info_dir);
            if (!Directory.Exists(warning_dir)) Directory.CreateDirectory(warning_dir);
        }
        private void writeLog(string path, string content)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Start Write Method
                using (var sw = new StreamWriter(Path.Combine(path, $"{DateTime.Now.ToString("yyyy-MMM-dd")}.log"), true))
                {
                    content += $"\nEnd Time: {DateTime.Now}\n\n";
                    sw.WriteLine(content);
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        public void LogError(string errorMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(errorMessage)) writeLog(error_dir, errorMessage);
            }
            catch (Exception) { /**/}

        }

        public void LogError(Exception errorMessage)
        {
            try
            {
                writeLog(error_dir, errorMessage.ToString());
            }
            catch (Exception) { /**/}

        }


        public void LogInfo(string infoMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(infoMessage)) writeLog(info_dir, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}:\t{infoMessage}");
            }
            catch (Exception) { /**/}
        }


        public void LogInfo(StringBuilder infoMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(infoMessage.ToString())) writeLog(info_dir, infoMessage.ToString());
            }
            catch (Exception) { /**/}
        }
        public void LogInfoBuilder(List<StringBuilder> infoMessage)
        {
            try
            {
                if (infoMessage.Count > 0)
                {
                    var logs = string.Join("\n\n", infoMessage.Select(l => l.ToString()).ToList());
                    if (!string.IsNullOrWhiteSpace(logs))
                    {
                        writeLog(info_dir, logs);
                    }
                }
            }
            catch (Exception) { /**/}
        }

        public void LogWarning(string warningMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(warningMessage)) writeLog(warning_dir, warningMessage);
            }
            catch (Exception) { /**/}
        }
    }
}
