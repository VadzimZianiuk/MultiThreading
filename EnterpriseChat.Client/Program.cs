namespace EnterpriseChat.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 1000;

            Console.WriteLine("[CLIENT] Simple enterprise chat.");
            Console.WriteLine("Press 'Enter' to exit...");
            Console.WriteLine();
            using var cts = new CancellationTokenSource();

            Task.Run(() => new Client(Port, Console.Out, cts.Token).Start(), cts.Token);

            Console.ReadLine();
            cts.Cancel(); 
        }
    }
}