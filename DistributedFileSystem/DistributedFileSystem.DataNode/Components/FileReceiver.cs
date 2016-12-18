namespace DistributedFileSystem.DataNode.Components
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileReceiver
    {
        private readonly DataNodeInfo dataNodeInfo;

        private readonly CancellationToken token;

        private readonly Socket socket;

        public FileReceiver(DataNodeInfo dataNodeInfo, CancellationToken token)
        {
            this.dataNodeInfo = dataNodeInfo;
            this.token = token;

            var endpoint = new IPEndPoint(IPAddress.Any, dataNodeInfo.TcpPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socket.Bind(endpoint);
            socket.Listen(100);

            Console.WriteLine($"start listener on {dataNodeInfo.TcpPort}");
        }

        public void Start()
        {
            Task.Factory.StartNew(
                () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var client = socket.Accept();

                                Console.WriteLine("start receive file");

                                var clientData = new byte[1024 * 5000];

                                var receivedByteLen = client.Receive(clientData);

                                var fileNameLen = BitConverter.ToInt32(clientData, 0);
                                var fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);

                                Console.WriteLine($"file {fileName} was received. start writting to disk");

                                var binWritter =
                                    new BinaryWriter(
                                        File.Open(
                                            dataNodeInfo.StorageDirectory.FullName + "\\" + fileName,
                                            FileMode.CreateNew));

                                binWritter.Write(clientData, 4 + fileNameLen, receivedByteLen - 4 - fileNameLen);

                                binWritter.Close();
                                client.Close();

                                Console.WriteLine($"file {fileName} was written on disk");

                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    },
                token);
        }
    }
}