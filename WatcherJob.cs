using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf.Options;

namespace FileWatcher
{
    public class WatcherJob : FileSystemWatcher
    {

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public WatcherJob(string jobType,  string monitoredPath, string? outputFilePath, string? filenamePattern) {

            switch (jobType)
            {
                case "FileMover":
                    this.Created += new FileSystemEventHandler((sender, e) => OnCreatedMoveFile(sender, e, outputFilePath));
                    break;
                default:
                    break;
            }
            this.Path = monitoredPath;
            this.Filter = filenamePattern;
            this.EnableRaisingEvents = true;
        }

        private void OnCreatedMoveFile(object? sender, FileSystemEventArgs e, string outputPath)
        {
            FileInfo fileInfo = new FileInfo(e.FullPath);

            string formattedTime = DateTime.Now.ToString("ddMMyy_HHmmss");

            int retryCount = 0;
            while (retryCount < 3) { 
                try { 
                    File.Move(fileInfo.FullName, $"{outputPath}{fileInfo.Name.Split(".")[0]}_{formattedTime}{fileInfo.Extension}");
                    _logger.Info("File moved.\n input: {0}\noutput: {1}", fileInfo.FullName, $"{outputPath}{fileInfo.Name.Split(".")[0]}_{formattedTime}{fileInfo.Extension}");
                }
                catch (IOException error) {
                    _logger.Error("Write error while attepting to move {0}, retrying in .250 seconds. retry count: {1} of 3", fileInfo.FullName, retryCount + 1);
                    retryCount++;
                    Thread.Sleep(250);
                }                                                                                                                                                         
            }

            e = null;
            sender = null;
            outputPath = null;
        }

        private void OnChanged(object? seb, FileSystemEventArgs e) {

        }

        private void OnDeleted(object? seb, FileSystemEventArgs e)
        {

        }
    }
}
