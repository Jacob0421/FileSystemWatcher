using System.Dynamic;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using FileWatcher.src.BatchFiles.Repositories;
using FileWatcher.src.BatchJobs;
using FileWatcher.src.Configuration;
using Timer = System.Timers.Timer;

namespace FileWatcher
{
    public class PollFilePath
    {
        private readonly Timer _timer;
        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        BatchJobRepository _batchRepo = new BatchJobRepository();

        public PollFilePath()
        {
            int TimerRefreshSeconds = 1;

            //Initiate Timer
            _timer = new Timer(1000 * TimerRefreshSeconds) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            IEnumerable<BatchJob> batchJobs = _batchRepo.GetAllJobs();

            foreach (var job in batchJobs)
            {
                if (_batchRepo.IsBatchJobOpen(job)) {
                    if(!job.IsActive && job.GetType() == typeof(FileMover))
                    {
                        job.InitiateWatcher();
                    }
                }
            }
        }

        public void Start()
        {
            _logger.Info("Starting FileWatcher Service");
            _logger.Info("Loading Config");
            ConfigLoader.LoadConfig();
            _logger.Info("Config Loaded Successfully.");
            _logger.Info("Parsing Batch Jobs");
            IEnumerable<BatchJob> jobs = _batchRepo.ParseFromConfig(ConfigLoader.GetConfigItem().Descendants().Elements("BatchJob"));
            _logger.Info($"Batch Job parsing Successful. Jobs found: {jobs.Count()}");

            _timer.Start();
        }

        public void Stop()
        {
            _logger.Info("Stopping FileWatcher Service");
            _logger.Info("Disposing FileSystemWatcher Instances");
            _batchRepo.DisposeWatchers();
            _timer.Stop();
            _logger.Info("FileWatcher Service timer stopped");
        }
    }
}
