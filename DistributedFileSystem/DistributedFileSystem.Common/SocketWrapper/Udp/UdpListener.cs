namespace DistributedFileSystem.Common.SocketWrapper.Udp
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    using Newtonsoft.Json;

    public class UdpListener : UdpBase
    {
        public UdpListener(IPEndPoint endpoint)
        {
            Client = new UdpClient(endpoint);
        }

        public void Reply(FileDetailsForReplication message, IPEndPoint endpoint)
        {
            byte[] datagram = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));
            Client.Send(datagram, datagram.Length, endpoint);
        }
    }
}