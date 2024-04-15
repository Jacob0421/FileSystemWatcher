using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FileWatcher.src.Jobs
{
    public class CommandRunner : Job
    {
        public string Command { get; set; } 
        public string Arguments { get; set; }
        public bool AdminNeeded { get; set; }
        public bool RetryOnFailure { get; set; }
        public int RetryCount { get; set; }

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public CommandRunner(int JobID, string JobName, string JobType, string Command, string Arguments, bool SdminNeeded, TimeOnly WindowStart, TimeOnly WindowEnd, string[] WindowDays) { 
            this.JobID = JobID;
            this.JobName = JobName;
            this.JobType = JobType;
            this.Command = Command;
            this.Arguments = Arguments;
            this.AdminNeeded = AdminNeeded;
            this.WindowStart = WindowStart;
            this.WindowEnd = WindowEnd;
            this.WindowDays = WindowDays;
        }

        public override void Run() {
            int count = 0;
            while (count < RetryCount)
            {
                count++;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Command;
                startInfo.Arguments = Arguments;
                if (AdminNeeded)
                {
                    startInfo.Verb = "runas";
                }
                process.StartInfo = startInfo;
                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error Running Job ID {JobID}. Exception: \n{ex}");
                    _logger.Info($"Job ID {JobID} - Retrying {count} of {RetryCount}");
                    if(count == RetryCount - 1)
                    {
                        ManualOverride();
                        _logger.Info($"Job ID {JobID} has been overridden. Please correct the command and restart the service for this command to be run.");
                    }
                }
            }
        }
    }
}
