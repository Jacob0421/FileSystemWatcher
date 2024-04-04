namespace FileWatcher.src.BatchJobs
{
    public class BatchJob
    {

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public int JobID { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string InputPath { get; set; }
        public string DestinationPath { get; set; }
        public string FileNamePattern { get; set; }
        public TimeOnly WindowStart { get; set; }
        public TimeOnly WindowEnd { get; set; }
        public string[] WindowDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsManuallyOverriden { get; set; }


        public bool ChangeActiveState()
        {
            IsActive = !IsActive;
            return IsActive;
        }

        public bool ManualOverride()
        {
            IsManuallyOverriden = true;
            return IsManuallyOverriden;
        }

        public virtual void InitiateWatcher() { }
    }
}
