using FileWatcher.src.Jobs;
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

            int jobID;
            string jobName, jobType, inputPath, destinationPath, fileNamePattern;
            TimeOnly windowStart, windowEnd;
            string[] windowDays;
            foreach (var JobItem in JobItems)
            {
                try {
                    jobID = Int32.Parse(JobItem.Element("JobID").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading Job ID {JobItem.Element("JobID").Value} Please confirm that this is a number that can be converted to Integer form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                jobName = JobItem.Element("JobName").Value;
                jobType = JobItem.Element("JobType").Value;
                inputPath = JobItem.Element("InputPath").Value;
                destinationPath = JobItem.Element("DestinationPath").Value;
                fileNamePattern = JobItem.Element("FileNamePattern").Value;

                try
                {
                    windowStart = TimeOnly.Parse(JobItem.Element("WindowStart").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading windowStart for Job ID {JobItem.Element("JobId").Value} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                try
                {
                    windowEnd = TimeOnly.Parse(JobItem.Element("WindowEnd").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading WindowEnd for Job ID {JobItem.Element("JobId").Value} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                windowDays = JobItem.Element("WindowDays").Value.Split("|");

                string str;
                for (int i = 0; i < windowDays.Length; i++)
                {
                    str = windowDays[i].Trim();
                    if (str.Length != 3)
                    {
                        _logger.Error(($"Error loading WindowDays for Job ID {JobItem.Element("JobId").Value} Please confirm that the string is in the valid format to be parsed by the system I.E. \"Mon|Tue|Thu etc.\"."));
                        break;
                    }
                    windowDays[i] = str;
                }

                switch(jobType)
                {
                    case "FileMover":
                        FileMover newMover = new(
                            jobID,
                            jobName,
                            jobType,
                            inputPath,
                            destinationPath,
                            fileNamePattern,
                            windowStart,
                            windowEnd,
                            windowDays
                        );
                        fromConfig.Add(newMover);
                        break;
                    default:
                        _logger.Error($"Invalid Job Type: {jobType}");
                        break;
                }
            }
            SetJobs( fromConfig );

            return _jobs;
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

        public bool IsBatchJobOpen(Job toTest)
        {
            string abbreviatedDay = DateTime.Now.DayOfWeek.ToString().Substring(0, 3);
            if (toTest.WindowDays.Contains(abbreviatedDay))
            {
                TimeOnly currentTime = TimeOnly.Parse(DateTime.Now.ToLongTimeString());
                if (currentTime > toTest.WindowStart && currentTime < toTest.WindowEnd)
                {
                    return true;
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
