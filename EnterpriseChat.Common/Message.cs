using System.Net.Sockets;
using System.Text;

namespace EnterpriseChat.Common
{
    public class Message
    {
        public Message(Socket client)
        {
            Client = client;
        }

        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];

        public Socket Client { get; private set; }

        private readonly StringBuilder _sb = new();

        public void Append(string value)
        {
            lock(_sb)
            {
                _sb.Append(value);
            }
        }

        public override string ToString()
        {
            lock(_sb)
            {
                var message = _sb.ToString();
                _sb.Clear();
                return message;
            }
        }
    }
}
