using FileWatcher.src.Jobs;
using System.Xml;
using System.Xml.Linq;

namespace FileWatcher.src.Jobs.Repositories
{

    public class JobRepository
    {
        private List<Job> _jobs = new();
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<Job> ParseFromConfig(IEnumerable<XElement> JobItems)
        {
            List<Job> fromConfig = new();

            string jobType;
            foreach (var JobItem in JobItems)
            {
                /// **DONE** switch statement that will call separate functions for parsing different Job types base on the "JobType" field provided from config.
                ///
                /// - **DONE**Possibly need to parse out Jobtype first then use that to route XElement Items.
                /// - **DONE**Call a function and pass the JobItems XElement Value to it. Then do full parsing there.
                /// - **DONE**Return the Job which is then added to JobItems.

                jobType = JobItem.Element("JobType").Value;

                Job toAdd = new Job();

                switch (jobType)
                {
                    case "FileMover":
                        toAdd = ParseFileMoverFromXML(JobItem);
                        break;
                    case "CommandRunner":
                        toAdd = ParseCommandRunnerFromXML(JobItem);
                        break;
                }

                fromConfig.Add(toAdd);
            }
            SetJobs(fromConfig);

            return _jobs;
        }

        private FileMover ParseFileMoverFromXML(XElement JobXML)
        {
            int jobID = 0;
            string jobName,inputPath, destinationPath, fileNamePattern;
            TimeOnly windowStart, windowEnd = TimeOnly.Parse(DateTime.Now.ToShortTimeString());
            string[] windowDays;

            try
            {
                jobID = Int32.Parse(JobXML.Element("JobID").Value);
            }
            catch (Exception e)
            {
                _logger.Error($"Error loading Job ID {JobXML.Element("JobID").Value} Please confirm that this is a number that can be converted to Integer form.\n" +
                    $"Exception: {e}");
                return null;
            }
            jobName = JobXML.Element("JobName").Value;

            inputPath = JobXML.Element("InputPath").Value;
            destinationPath = JobXML.Element("DestinationPath").Value;
            fileNamePattern = JobXML.Element("FileNamePattern").Value;

            try
            {
                windowStart = TimeOnly.Parse(JobXML.Element("WindowStart").Value);
            }
            catch (Exception e)
            {
                _logger.Error($"Error loading windowStart for Job ID {jobID} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                    $"Exception: {e}");
                return null;
            }

            try
            {
                windowEnd = TimeOnly.Parse(JobXML.Element("WindowEnd").Value);
            }
            catch (Exception e)
            {
                _logger.Error($"Error loading WindowEnd for Job ID {jobID} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                    $"Exception: {e}");
            }

            windowDays = JobXML.Element("WindowDays").Value.Split("|");

            string str;
            for (int i = 0; i < windowDays.Length; i++)
            {
                str = windowDays[i].Trim();
                if (str.Length != 3)
                {
                    _logger.Error(($"Error loading WindowDays for Job ID {jobID} Please confirm that the string is in the valid format to be parsed by the system I.E. \"Mon|Tue|Thu etc.\"."));
                    return null;
                }
                windowDays[i] = str;
            }

            FileMover toBeAdded = new FileMover(jobID, jobName, "FileMover", inputPath, destinationPath, fileNamePattern, windowStart, windowEnd, windowDays);
            return toBeAdded;
        }

        private CommandRunner ParseCommandRunnerFromXML(XElement JobXML)
        {
            int jobID = 0;
            string jobName;

            try
            {
                jobID = Int32.Parse(JobXML.Element("JobID").Value);
            }
            catch (Exception e)
            {
                _logger.Error($"Error loading Job ID {JobXML.Element("JobID").Value} Please confirm that this is a number that can be converted to Integer form.\n" +
                    $"Exception: {e}");
            }
            jobName = JobXML.Element("JobName").Value;


            string command, arguments;
            bool adminNeeded, retryOnFailure;
            int retryCount;
            TimeOnly commandRunTime;
            string[] windowDays;

            command = JobXML.Element("Command").Value;
            arguments = JobXML.Element("Arguments").Value;
            adminNeeded = bool.Parse(JobXML.Element("IsAdminNeeded").Value);
            retryOnFailure = bool.Parse(JobXML.Element("RetryOnFailure").Value);
            retryCount = Int32.Parse(JobXML.Element("RetryCount").Value);
            commandRunTime = TimeOnly.Parse(JobXML.Element("CommandRunTime").Value);

            windowDays = JobXML.Element("WindowDays").Value.Split("|");

            string str;
            for (int i = 0; i < windowDays.Length; i++)
            {
                str = windowDays[i].Trim();
                if (str.Length != 3)
                {
                    _logger.Error(($"Error loading WindowDays for Job ID {jobID} Please confirm that the string is in the valid format to be parsed by the system I.E. \"Mon|Tue|Thu etc.\"."));
                    return null;
                }
                windowDays[i] = str;
            }


            CommandRunner toBeAdded = new CommandRunner(jobID, jobName, "CommandRunner", command, arguments, adminNeeded, retryOnFailure, retryCount, commandRunTime, windowDays);
            return toBeAdded;
        }

        public List<Job> SetJobs(List<Job> jobs)
        {
            _jobs = jobs;
            return jobs;
        }


        public IEnumerable<Job> GetAllJobs()
        {
            return _jobs;
        }

        public IEnumerable<Job> GetActiveJobs()
        {
            return _jobs.Where(b => b.IsActive);
        }

        public IEnumerable<Job> GetInactiveJobs()
        {
            return _jobs.Where(b => !b.IsActive);
        }

        public Job GetJobByName(string jobName)
        {
            return _jobs.FirstOrDefault(b => b.JobName == jobName);
        }

        public Job GetJobByID(int id)
        {
            return _jobs.FirstOrDefault(b => b.JobID == id);
        }

        public bool IsJobOpen(Job toTest)
        {
            string abbreviatedDay = DateTime.Now.DayOfWeek.ToString().Substring(0, 3);
            if (toTest.WindowDays.Contains(abbreviatedDay))
            {

                TimeOnly currentTime = TimeOnly.Parse(DateTime.Now.ToString("HH:mm"));

                switch (toTest.JobType)
                {
                    case "FileMover":
                        FileMover fileMoverToTest = (FileMover)toTest;
                        if (currentTime > fileMoverToTest.WindowStart && currentTime < fileMoverToTest.WindowEnd)
                        {
                            return true;
                        }
                        return false;
                    case "CommandRunner":
                        CommandRunner commandRunnerToTest = (CommandRunner)toTest;

                        if (currentTime.Equals(commandRunnerToTest.CommandRunTime))
                        {
                            return true;
                        }
                        return false;
                    default:
                        break;
                }
            }
            return false;
        }

        public void DisposeWatchers()
        {
            IEnumerable<FileMover> activeMovers = _jobs.Where(b => b.IsActive).OfType<FileMover>();

            foreach (FileMover mover in activeMovers)
            {
                mover._watcher.Dispose();
                _logger.Info($"Job ID {mover.JobID}'s watcher has been Disposed.");
            }
        }
    }
}
