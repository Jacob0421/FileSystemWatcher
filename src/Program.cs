using Topshelf;

namespace FileWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<PollFilePath>(s =>
                {
                    s.ConstructUsing(pollFilePath => new PollFilePath());
                    s.WhenStarted(pollFilePath => pollFilePath.Start());
                    s.WhenStopped(pollFilePath => pollFilePath.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("FileSystemWatcher");
                x.SetDisplayName("FileSystemWatcher");
                x.SetDescription("Sample File polling service to perform actions on files in certain directories.");
            });

            int exitCodeValue = Convert.ToInt32(exitCode);

            Environment.ExitCode = exitCodeValue;
        }
    }
}