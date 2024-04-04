﻿using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.src.BatchJobs
{
    class FileMover : BatchJob
    {

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public FileSystemWatcher? _watcher; 

        public FileMover(int JobID, string JobName, string JobType, string InputPath, string DestinationPath, string FileNamePattern, TimeOnly WindowStart, TimeOnly WindowEnd, string[] WindowDays)
        {
            this.JobID = JobID;
            this.JobName = JobName;
            this.JobType = JobType;
            this.InputPath = InputPath;
            this.DestinationPath = DestinationPath;
            this.FileNamePattern = FileNamePattern;
            this.WindowStart = WindowStart;
            this.WindowEnd = WindowEnd;
            this.WindowDays = WindowDays;
            IsActive = false;
            IsManuallyOverriden = false;
        }

        public override void InitiateWatcher()
        {
            PreProcessFiles(InputPath, DestinationPath, FileNamePattern);


            _watcher = new FileSystemWatcher();
            _watcher.Created += Created;
            _watcher.Path = InputPath;
            _watcher.Filter = FileNamePattern;
            _watcher.EnableRaisingEvents = true;
            
            ChangeActiveState();

            _logger.Info($"Job ID {JobID} is now Initiated. Now watching {InputPath}.");
        }

        private void Created(object? sender, FileSystemEventArgs e)
        {
            FileInfo fileInfo = new FileInfo(e.FullPath);

            string fullInputPath = fileInfo.FullName;

            string fileFormattedTime = DateTime.Now.ToString("_ddMMyy_hhmmss");
            string inputFileName = Path.GetFileNameWithoutExtension(fullInputPath);
            string inputFileExtension = Path.GetExtension(fullInputPath);

            string DestinationFileName = DestinationPath
                                            + inputFileName
                                            + fileFormattedTime
                                            + inputFileExtension;

            MoveFile(fullInputPath, DestinationFileName);

        }

        private void PreProcessFiles(string inputDirectory, string destinationDirectory, string fileNamePattern)
        {
            _logger.Info($"Job ID {JobID} - Starting Directory Pre-processing");

            string[] existingFiles = Directory.GetFiles(inputDirectory,FileNamePattern);

            if(!existingFiles.Any())
            {
                return;
            }

            foreach (string file in existingFiles)
            {
                _logger.Info($"Job ID {JobID} - Pre-processing file: {file}");

                if (!File.Exists(file))
                {
                    _logger.Error($"Job ID {JobID} - File not found: {file}");
                }

                string fileFormattedTime= DateTime.Now.ToString("_ddMMyy_hhmmss");
                string inputFileName = Path.GetFileNameWithoutExtension(file);
                string inputFileExtension = Path.GetExtension(file);

                string DestinationFileName = destinationDirectory 
                                                + inputFileName 
                                                + fileFormattedTime 
                                                + inputFileExtension;

                _logger.Info($"Job ID {JobID} - File Available. Attempting to Move File: {file}");

                MoveFile(file, DestinationFileName);
            }
        }

        private void WaitForFileWrite(string fileFullPath)
        {
            FileStream fs = new FileStream(fileFullPath, FileMode.OpenOrCreate);

            while (!fs.CanWrite)
            {
                Thread.Sleep(100);
            }
            fs.Close();
            fs.Dispose();
        }

        private void MoveFile(string fullInputPath, string fullDestinationPath)
        {
            bool isFileMoved = false;
            int retryCount = 0;

            while(retryCount < 3 && !isFileMoved)
            try {
                WaitForFileWrite(fullInputPath);
                File.Move(fullInputPath, fullDestinationPath);
                isFileMoved = true;
                _logger.Info($"File has been moved. New path: {fullDestinationPath}");
            } catch (Exception e) {
                _logger.Error($"Job ID {JobID} - Error moving file {fullInputPath}. Retry {retryCount + 1} of 3.\nException: {e} ");
            }
        }
    }
}
