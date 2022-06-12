using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;

namespace EnterpriseChat.Server
{
    //TODO:
    // 1. recheck correct cancellation
    // 2. add delete client socket when it close connection
    // 3. ....
    public class Server
    {
        private const int MessageHistorySize = 10;
        private readonly AutoResetEvent eventWaitHandler = new(false);
        private readonly ConcurrentQueue<string> MessageHistory= new();
        private readonly ConcurrentDictionary<Socket, int> Clients = new();
        private readonly object _startLock = new();
        private readonly int _port;
        private readonly CancellationToken _token;
        private readonly TextWriter _logger;
        private bool _isStarted;
        private int _clientId = 0;

        public Server(int port, TextWriter logger, CancellationToken token)
        {
            _port = port;
            _token = token;
            _logger = logger;
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
                while (!_token.IsCancellationRequested)
                {
                    _logger.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    eventWaitHandler.WaitOne();
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex);
            }
            finally
            {
                _isStarted = false;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            eventWaitHandler.Set();
  
            var listener = ar.AsyncState as Socket;
            var client = listener.EndAccept(ar);
            
            if (client.Connected)
            {
                var id = Interlocked.Increment(ref _clientId);
                Clients.TryAdd(client, id);
                Task.Run(() => SendHistory(client));

                var message = new Message { Client = client, Id = id };
                client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var message = ar.AsyncState as Message;
            var client = message.Client;
            if (!client.Connected)
            {
                Clients.TryRemove(client, out _);
                return;
            }

            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                message.Append(Encoding.Default.GetString(message.Buffer, 0, bytesRead));  
                if (bytesRead != Message.BufferSize)
                {
                    var content = message.ToString();
                    MessageHistory.Enqueue(content);
                    if (MessageHistory.Count > MessageHistorySize)
                    {
                        MessageHistory.TryDequeue(out _);
                    }

                    _logger.WriteLine($"Read {content.Length} bytes from client:{Environment.NewLine}{content}");
                    Task.Run(() => BroadcastMessage(content));

                    message.Clear();
                }
                client.BeginReceive(message.Buffer, 0, Message.BufferSize, 0, new AsyncCallback(ReadCallback), message);
            }
        }

        private void BroadcastMessage(string data)
        {
            var byteData = Encoding.Default.GetBytes(data);
            foreach (var (client,_) in Clients.Where(kvp => kvp.Key.Connected))
            {
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
            }
        }

        private void SendHistory(Socket client)
        {
            foreach (var message in MessageHistory.ToArray())
            {
                var byteData = Encoding.Default.GetBytes(message);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = ar.AsyncState as Socket;
                int bytesSent = client.EndSend(ar);
                Clients.TryGetValue(client, out var id);
                _logger.WriteLine($"Sent {bytesSent} bytes to client {id}");
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
            _logger.WriteLine($"Chat server is listening on port: {_port}");
            return socket;
        }

    }
}
