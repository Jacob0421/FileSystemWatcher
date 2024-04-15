namespace FileWatcher.src.Jobs
{
    public class Job
    {

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public int JobID { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
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
        public virtual void Run() { }
    }
}
