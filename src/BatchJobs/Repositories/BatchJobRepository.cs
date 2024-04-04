using FileWatcher.src.BatchJobs;
using System.Xml.Linq;

namespace FileWatcher.src.BatchFiles.Repositories
{

    public class BatchJobRepository
    {
        private List<BatchJob> _batchJobs = new();
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<BatchJob> ParseFromConfig(IEnumerable<XElement> BatchJobItems)
        {
            List<BatchJob> fromConfig = new();

            int jobID;
            string jobName, jobType, inputPath, destinationPath, fileNamePattern;
            TimeOnly windowStart, windowEnd;
            string[] windowDays;
            foreach (var BatchJobItem in BatchJobItems)
            {
                try {
                    jobID = Int32.Parse(BatchJobItem.Element("JobID").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading Job ID {BatchJobItem.Element("JobId").Value} Please confirm that this is a number that can be converted to Integer form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                jobName = BatchJobItem.Element("JobName").Value;
                jobType = BatchJobItem.Element("JobType").Value;
                inputPath = BatchJobItem.Element("InputPath").Value;
                destinationPath = BatchJobItem.Element("DestinationPath").Value;
                fileNamePattern = BatchJobItem.Element("FileNamePattern").Value;

                try
                {
                    windowStart = TimeOnly.Parse(BatchJobItem.Element("WindowStart").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading windowStart for Job ID {BatchJobItem.Element("JobId").Value} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                try
                {
                    windowEnd = TimeOnly.Parse(BatchJobItem.Element("WindowEnd").Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error loading WindowEnd for Job ID {BatchJobItem.Element("JobId").Value} Please confirm that this is a timestamp (format: hh:mm) that can be converted to TimeOnly form.\n" +
                        $"Exception: {e}");
                    continue;
                }

                windowDays = BatchJobItem.Element("WindowDays").Value.Split("|");

                string str;
                for (int i = 0; i < windowDays.Length; i++)
                {
                    str = windowDays[i].Trim();
                    if (str.Length != 3)
                    {
                        _logger.Error(($"Error loading WindowDays for Job ID {BatchJobItem.Element("JobId").Value} Please confirm that the string is in the valid format to be parsed by the system I.E. \"Mon|Tue|Thu etc.\"."));
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
            SetBatchJobs( fromConfig );

            return _batchJobs;
        }

        public List<BatchJob> SetBatchJobs(List<BatchJob> batchJobs)
        {
            _batchJobs = batchJobs;
            return _batchJobs;
        }


        public IEnumerable<BatchJob> GetAllJobs()
        {
            return _batchJobs;
        }

        public IEnumerable<BatchJob> GetActiveJobs()
        {
            return _batchJobs.Where(b => b.IsActive);
        }

        public IEnumerable<BatchJob> GetInactiveJobs()
        {
            return _batchJobs.Where(b => !b.IsActive);
        }

        public BatchJob GetJobByName(string jobName)
        {
            return _batchJobs.FirstOrDefault(b => b.JobName == jobName);
        }

        public BatchJob GetJobByID(int id)
        {
            return _batchJobs.FirstOrDefault(b => b.JobID == id);
        }

        public bool IsBatchJobOpen(BatchJob toTest)
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
            IEnumerable<FileMover> activeMovers = _batchJobs.Where(b => b.IsActive).OfType<FileMover>();

            foreach (FileMover mover in activeMovers)
            {
                mover._watcher.Dispose();
                _logger.Info($"Job ID {mover.JobID}'s watcher has been Disposed.");
            }
        }
    }
}
