namespace DistributedFileSystem.Common.SocketWrapper.Udp
{
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class UdpBase
    {
        protected UdpClient Client;

        protected UdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<Package> Receive()
        {
            var result = await Client.ReceiveAsync();

            return new Package
                       {
                           Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                           From = result.RemoteEndPoint
                       };
        }

        public async Task<byte[]> ReceiveBytes()
        {
            var result = await Client.ReceiveAsync();

            return result.Buffer;
        }

        public void Close()
        {
            Client.Close();
        }
    }
}