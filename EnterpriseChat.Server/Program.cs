namespace EnterpriseChat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 1000;
            
            Console.WriteLine("Simple enterprise chat.");
            Console.WriteLine("Press 'Enter' to exit...");
            Console.WriteLine();
            using var cts = new CancellationTokenSource();

            Task.Run(() => new Server(Port, Console.Out, cts.Token).StartListening(), cts.Token);
            
            Console.ReadLine();
            cts.Cancel();
        }
    }
}