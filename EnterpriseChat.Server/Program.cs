using EnterpriseChat.Common;

namespace EnterpriseChat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 1000;
            const string loggerName = "[SERVER] ";

            var logger = new Logger(loggerName, Console.Out);

            logger.WriteLine("Simple enterprise chat.");
            logger.WriteLine("Press 'Enter' to exit...");
            logger.WriteLine();
            using var cts = new CancellationTokenSource();

            Task.Run(() => new Server(Port, logger, cts.Token).StartListening(), cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}