using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EnterpriseChat.Client
{
    //TODO:
    // 1. recheck correct cancellation
    // 2. AutoResetEvent maybe is not a good idea. We need to wait ReadCallback also...
    // 3. ....
    public class Client
    {
        private readonly int _port;
        private readonly CancellationToken _token;
        private readonly TextWriter _logger;
        private readonly AutoResetEvent eventWaitHandler = new(false);
        private static readonly Random _random = new();

        public Client(int port, TextWriter logger, CancellationToken token)
        {
            _port = port;
            _token = token;
            _logger = logger;
        }

        public void Start()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
                _logger.WriteLine($"Client will connecte on port: {_port}");
                socket.BeginConnect(localEndPoint, new AsyncCallback(ConnectCallback), socket);
                eventWaitHandler.WaitOne();
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                if (!IsAlive(client))
                {
                    return;
                }

                client.EndConnect(ar);
                _logger.WriteLine("Socket connected to {0}", client.RemoteEndPoint);

                var message = new Message { Client = client };
                client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);

                Task.Run(() =>
                {
                    var count = GetRandom(0, 10);
                    while (count-- > 0 && IsAlive(client))
                    {
                        Send(client, $"{Guid.NewGuid}{Environment.NewLine}");
                    }
                }).Wait();

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                _logger.WriteLine(e);
            }
            finally
            {
                eventWaitHandler.Set();
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var message = ar.AsyncState as Message;
            var client = message.Client;
            if (!IsAlive(client))
            {
                return;
            }

            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                message.Append(Encoding.Default.GetString(message.Buffer, 0, bytesRead));
                if (bytesRead != Message.BufferSize)
                {
                    var content = message.ToString();

                    _logger.WriteLine("Read {0} bytes from server. {1} Data : {2}", content.Length, Environment.NewLine, content);
                    message.Clear();
                }
                client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
            }
        }

        private void Send(Socket client, string data)
        {
            var byteData = Encoding.Default.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                int bytesSent = client.EndSend(ar);
                _logger.WriteLine("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex);
            }
        }

        private bool IsAlive(Socket socket) => socket.Connected && _token.IsCancellationRequested;

        private static int GetRandom(int min, int max)
        {
            lock (_random)
            {
                return _random.Next(min, max);
            }
        }
    }
}
