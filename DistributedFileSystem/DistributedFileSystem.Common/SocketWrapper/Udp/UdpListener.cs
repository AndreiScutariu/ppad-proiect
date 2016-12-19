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

        public void Reply(ReplicateFile message, IPEndPoint endpoint)
        {
            const byte ReplicateFile = 0x01;

            byte[] command = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));

            var bytesToSend = new byte[1 + command.Length];

            bytesToSend[0] = ReplicateFile;
            command.CopyTo(bytesToSend, 1);

            Client.Send(bytesToSend, bytesToSend.Length, endpoint);
        }

        public void Reply(DeleteFile message, IPEndPoint endpoint)
        {
            const byte DeleteFile = 0x02;

            byte[] command = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));

            var bytesToSend = new byte[1 + command.Length];

            bytesToSend[0] = DeleteFile;
            command.CopyTo(bytesToSend, 1);

            Client.Send(bytesToSend, bytesToSend.Length, endpoint);
        }
    }
}