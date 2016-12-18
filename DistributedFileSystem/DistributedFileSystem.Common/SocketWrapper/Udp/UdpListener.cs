namespace DistributedFileSystem.Common.SocketWrapper.Udp
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class UdpListener : UdpBase
    {
        public UdpListener(IPEndPoint endpoint)
        {
            Client = new UdpClient(endpoint);
        }

        public void Reply(string message, IPEndPoint endpoint)
        {
            byte[] datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length, endpoint);
        }
    }
}