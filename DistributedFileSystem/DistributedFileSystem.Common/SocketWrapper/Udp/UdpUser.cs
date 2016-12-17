namespace DistributedFileSystem.Common.SocketWrapper.Udp
{
    using System.Text;

    using Newtonsoft.Json;

    public class UdpUser : UdpBase
    {
        private UdpUser()
        {
        }

        public static UdpUser ConnectTo(string hostname, int port)
        {
            var connection = new UdpUser();

            connection.Client.Connect(hostname, port);

            return connection;
        }

        public void Send(ClientInfo metaData)
        {
            Send(JsonConvert.SerializeObject(metaData));
        }

        private void Send(string message)
        {
            byte[] datagram = Encoding.ASCII.GetBytes(message);

            Client.Send(datagram, datagram.Length);
        }
    }
}