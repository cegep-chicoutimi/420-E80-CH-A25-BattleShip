using System.Net.Sockets;
using System.Text;

namespace BattleShipLibrary
{
    public class SocketHelper
    {
        private readonly Socket _socket;

        public SocketHelper(Socket socket)
        {
            _socket = socket;
        }

        public void Send(string data)
        {
            byte[] msg = Encoding.ASCII.GetBytes(data + "?");
            _socket.Send(msg, SocketFlags.None);
        }

        public string Receive()
        {
            byte[] buffer = new byte[1024];
            int bytesRec = _socket.Receive(buffer);
            string data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
            return data.Contains("?") ? data.Substring(0, data.IndexOf("?")) : data;
        }
    }
}
