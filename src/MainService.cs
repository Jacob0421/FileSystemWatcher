using System.Dynamic;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using FileWatcher.src.Jobs.Repositories;
using FileWatcher.src.Jobs;
using FileWatcher.src.Configuration;
using Timer = System.Timers.Timer;

namespace FileWatcher
{
    public class MainService
    {
        private readonly Timer _timer;
        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        JobRepository _jobRepo = new JobRepository();

        public MainService()
        {
            int TimerRefreshSeconds = 1;

            //Initiate Timer
            _timer = new Timer(1000 * TimerRefreshSeconds) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            IEnumerable<Job> jobs = _jobRepo.GetAllJobs();

            foreach (var job in jobs)
            {
                if (_jobRepo.IsJobOpen(job)) {
                    if(!job.IsActive && job.GetType() == typeof(FileMover) && !job.IsManuallyOverriden)
                    {
                        job.InitiateWatcher();
                    } else if(!job.IsActive && job.GetType() == typeof(CommandRunner) && !job.IsManuallyOverriden)
                    {
                        job.Run();
                    }
                }
            }
        }

        public void Start()
        {
            ConfigLoader.LoadConfig();
            IEnumerable<Job> jobs = _jobRepo.ParseFromConfig(ConfigLoader.GetConfigItem().Descendants().Elements("Job"));
            _logger.Info($"Job parsing Successful. Jobs found: {jobs.Count()}");

            _timer.Start();
            _logger.Info("Log started");
        }

        public void Stop()
        {
            _logger.Info("Stopping FileWatcher Service");
            _logger.Info("Disposing FileSystemWatcher Instances");
            _jobRepo.DisposeWatchers();
            _timer.Stop();
            _logger.Info("FileWatcher Service timer stopped");
        }
    }
}
