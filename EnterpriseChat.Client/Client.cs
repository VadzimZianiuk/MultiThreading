using EnterpriseChat.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EnterpriseChat.Client
{
    public class Client
    {
        private readonly int _port;
        private readonly CancellationToken _token;
        private readonly ILogger _logger;
        private readonly AutoResetEvent eventWaitHandler = new(false);
        private static readonly Random _random = new();
        private int _clientId = 0;

        public Client(int port, ILogger logger, CancellationToken token)
        {
            _port = port;
            _logger = logger;
            _token = token;
        }

        public void Start()
        {
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    Interlocked.Increment(ref _clientId);
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
                    socket.BeginConnect(localEndPoint, new AsyncCallback(ConnectCallback), socket);
                    eventWaitHandler.WaitOne();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                _logger.WriteLine("Client #{0} connecting to {1}", _clientId, client.RemoteEndPoint);
                if (IsAlive(client))
                {
                    client.EndConnect(ar);
                    
                    var message = new Message(client);
                    client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);

                    Task.Run(() =>
                    {
                        var toSend = GetRandom(1, 5);
                        for (int i = 0; i < toSend && IsAlive(client); i++)
                        {
                            Send(client, $"Client #{_clientId} message {i}: {Guid.NewGuid()}");
                            Task.Delay(TimeSpan.FromMilliseconds(GetRandom(500, 1000))).Wait();
                        }
                    }).Wait();

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
            }
            finally
            {
                _logger.WriteLine("Client #{0} disconnected from the server.", _clientId);
                eventWaitHandler.Set();
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var message = ar.AsyncState as Message;
            var client = message.Client;
            if (IsAlive(client))
            {
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    message.Append(Encoding.Default.GetString(message.Buffer, 0, bytesRead));
                    if (bytesRead != Message.BufferSize)
                    {
                        var content = message.ToString();
                        message = new Message(client);
                        _logger.WriteLine("Read {0} bytes from server: {1}", content.Length, content);
                    }

                    client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
                }
            }
        }

        private void Send(Socket client, string data)
        {
            if (IsAlive(client))
            {
                var byteData = Encoding.Default.GetBytes(data);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as Socket;
            if (IsAlive(client))
            {
                client.EndSend(ar);
            }
        }

        private bool IsAlive(Socket socket) => socket.Connected && !_token.IsCancellationRequested;

        private static int GetRandom(int min, int max)
        {
            lock (_random)
            {
                return _random.Next(min, max);
            }
        }
    }
}
