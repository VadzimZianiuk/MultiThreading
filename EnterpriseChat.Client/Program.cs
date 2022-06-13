using EnterpriseChat.Common;

namespace EnterpriseChat.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 1000;
            const string loggerName = "[CLIENT] ";

            var logger = new Logger(loggerName, Console.Out);

            logger.WriteLine("Simple enterprise chat.");
            logger.WriteLine("Press 'Enter' to exit...");
            logger.WriteLine();
            using var cts = new CancellationTokenSource();

            Task.Run(() => new Client(Port, logger, cts.Token).Start(), cts.Token);

            Console.ReadLine();
            cts.Cancel(); 
        }
    }
}