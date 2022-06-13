using EnterpriseChat.Common;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EnterpriseChat.Server
{
    public class Server
    {
        private const int MessageHistorySize = 10;
        private readonly AutoResetEvent _eventWaitHandler = new(false);
        private readonly ConcurrentQueue<string> _messageHistory = new();
        private readonly ConcurrentDictionary<Socket, int> _clients = new();
        private readonly object _startLock = new();
        private readonly int _port;
        private readonly CancellationToken _token;
        private readonly ILogger _logger;
        private bool _isStarted;
        private int _clientId = 0;

        public Server(int port, ILogger logger, CancellationToken token)
        {
            _port = port;
            _logger = logger;
            _token = token;
        }

        public void StartListening()
        {
            lock (_startLock)
            {
                if (_isStarted)
                {
                    return;
                }
                _isStarted = true;
            };

            try
            {
                using var listener = GetListenerSocket();
                _logger.WriteLine("Waiting for a connection to {0}...", listener.LocalEndPoint);
                while (!_token.IsCancellationRequested)
                {
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    _eventWaitHandler.WaitOne();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
            }
            finally
            {
                _isStarted = false;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _eventWaitHandler.Set();

            var listener = ar.AsyncState as Socket;
            var client = listener.EndAccept(ar);
            if (IsAlive(client))
            {
                var id = Interlocked.Increment(ref _clientId);
                _clients.TryAdd(client, id);
                _logger.WriteLine("Client #{0} connected.", id);
                Task.Run(() => SendHistory(client));

                var message = new Message(client);
                client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
            }
        }

        private void ReadCallback(IAsyncResult ar)
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
                        AddToHistory(content);

                        _clients.TryGetValue(client, out var id);
                        _logger.WriteLine("Read {0} bytes from client #{1}: {2}", content.Length, id, content);
                        Task.Run(() => BroadcastMessage(content));
                    }

                    client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
                }
            }
        }

        private void BroadcastMessage(string data)
        {
            try
            {
                var byteData = Encoding.Default.GetBytes(data);
                var clients = _clients.Select(x => x.Key).ToArray();
                var options = new ParallelOptions() 
                { 
                    CancellationToken = _token, 
                    MaxDegreeOfParallelism = Environment.ProcessorCount / 2 
                };
                Parallel.ForEach(clients, options, client =>
                {
                    if (IsAlive(client))
                    {
                        client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                    }
                });
            }
            catch (OperationCanceledException ex)
            {
                _logger.WriteLine(ex.Message);
            }
        }

        private void SendHistory(Socket client)
        {
            var messages = _messageHistory.ToArray();
            for (int i = 0; i < messages.Length && IsAlive(client); i++)
            {
                var byteData = Encoding.Default.GetBytes($"History: {messages[i]}");
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                
                Task.Delay(1).Wait();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                if (IsAlive(client))
                {
                    client.EndSend(ar);
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
            }
        }
        private Socket GetListenerSocket()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(100);
            return socket;
        }

        private bool IsAlive(Socket client)
        {
            var isAlive = client.Connected && !_token.IsCancellationRequested;
            if (!isAlive)
            {
                if (_clients.TryRemove(client, out var id))
                {
                    _logger.WriteLine("Client #{0} was disconnected.", id);
                }
            }
            return isAlive;
        }

        private void AddToHistory(string content)
        {
            _messageHistory.Enqueue(content);
            if (_messageHistory.Count > MessageHistorySize)
            {
                _messageHistory.TryDequeue(out _);
            }
        }
    }
}
