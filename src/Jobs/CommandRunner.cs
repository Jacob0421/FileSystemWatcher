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
        public TimeOnly CommandRunTime { get; set; }

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public CommandRunner(int JobID, string JobName, string JobType, string Command, string Arguments, bool AdminNeeded,bool RetryOnFailure, int RetryCount,TimeOnly CommandRunTime, string[] WindowDays) { 
            this.JobID = JobID;
            this.JobName = JobName;
            this.JobType = JobType;
            this.Command = Command;
            this.Arguments = Arguments;
            this.AdminNeeded = AdminNeeded;
            this.CommandRunTime = CommandRunTime;
            this.WindowDays = WindowDays;
        }

        public override void Run() {
            int count = 0;
            while (count < RetryCount || (count == RetryCount && !RetryOnFailure))
            {
                _logger.Info($"JobID {JobID} - Initiating Command");
                count++;
               Process process = new Process();
               ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = String.Join(" ", new string[] { "/C", Command, Arguments });
                if (AdminNeeded)
                {
                    _logger.Info($"Job ID {JobID} - Command will run as admin");
                    startInfo.Verb = "runas";
                }
                process.StartInfo = startInfo;
                try
                {
                    _logger.Info($"Job ID {JobID} - Starting command");
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
                _logger.Info($"Job ID {JobID} - Command execution completed");
            }
        }
    }
}
