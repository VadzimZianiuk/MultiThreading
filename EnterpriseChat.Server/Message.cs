using System.Net.Sockets;
using System.Text;

namespace EnterpriseChat.Server
{
    public class Message
    {
        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];

        public Socket Client { get; set; }
        public int Id { get; set; }

        private readonly StringBuilder sb = new();

        public void Append(string value) => sb.Append(value);

        public void Clear() => sb.Clear();

        public override string ToString() => sb.ToString();
    }
}
