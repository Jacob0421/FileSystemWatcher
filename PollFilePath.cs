using System.Timers;
using System.Xml;
using System.Xml.Linq;
using Timer = System.Timers.Timer;

namespace FileWatcher
{
    public class PollFilePath
    {
        private readonly Timer _timer;
        private XElement _config;

        private readonly string configPath = @"C:\Users\Jacob\Documents\C# Projects\FileSystemWatcher\FileSystemWatcher\FileSystemWatcher.config";

        private List<WatcherJob> _watchers = new List<WatcherJob>();

        public PollFilePath()
        {
            int TimerRefreshSeconds = 1;

            //Initiate Timer
            _timer = new Timer(1000 * TimerRefreshSeconds) { AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {

            //check XML for any batch windows that are open and not active
            var batchJobs = _config.Descendants().Where(x => x.Name == "BatchJob");

            foreach (var job in batchJobs)
            {
                //Check and Validate that today is a valid batch window day
                string[] windowDays = job.Element("WindowDays").Value.Split('|');
                string abbreviatedDay = DateTime.Now.DayOfWeek.ToString().Substring(0, 3);
                if (!windowDays.Contains(abbreviatedDay)){
                    Console.WriteLine($"today is not active day for Job ID {job.Element("JobID")}");
                    continue;
                }

                //Check and validate that Batch window is open
                TimeOnly startTime = TimeOnly.Parse(job.Element("WindowStart").Value);
                TimeOnly endTime = TimeOnly.Parse(job.Element("WindowEnd").Value);
                TimeOnly currentTime = TimeOnly.Parse(DateTime.Now.ToLongTimeString());

                if(!(currentTime > startTime && currentTime < endTime))
                {
                    job.Element("IsActive").Value = "0";
                    Console.WriteLine($"Job ID {job.Element("JobID").Value} is not within active window.");
                    continue;
                }

                //Check if job is currently running
                if (job.Element("IsActive").Value == "1")
                {
                    Console.WriteLine($"Job ID {job.Element("JobID").Value} is already running.");
                    continue;
                }

                //

                //instantiate new watcherJob
                WatcherJob newJob = new WatcherJob(job.Element("JobType").Value, job.Element("InputPath").Value, job.Element("DestinationPath").Value, job.Element("FileNamePattern").Value);
                _watchers.Add(newJob);

                //Temporarily updating value while the program is running. Would normally like this to be updated in a DB or other source instead of directly 
                job.Element("IsActive").Value = "1";
            }

            //Check if any are active and window has closed
        }

        public void Start()
        {
            //Load Config XML
            string configStr = File.ReadAllText(configPath);
            _config = XElement.Parse(configStr);

            _timer.Start();
        }

        public void Stop()
        {
            foreach (WatcherJob job in _watchers)
            {
                job.Dispose();
            }
            _timer.Stop();
        }
    }
}