namespace DistributedFileSystem.DataNode.Components
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;

    using Newtonsoft.Json;

    internal class MessageReceiver
    {
        private readonly UdpUser udpUser;

        private readonly DataNodeInfo dataNodeInfo;

        private readonly CancellationToken token;

        public MessageReceiver(UdpUser udpUser, DataNodeInfo dataNodeInfo, CancellationToken token)
        {
            this.udpUser = udpUser;
            this.dataNodeInfo = dataNodeInfo;
            this.token = token;
        }

        public void Start()
        {
            Task.Factory.StartNew(
                async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var received = await udpUser.Receive();

                                var fileDetails = JsonConvert.DeserializeObject<FileDetailsForReplication>(received.Message);

                                var fileSender = new FileSender(dataNodeInfo, fileDetails);
                                fileSender.Send();

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

        private class FileSender
        {
            private readonly DataNodeInfo dataNodeInfo;

            private readonly FileDetailsForReplication fileDetailsForReplication;

            private readonly Socket socket;

            public FileSender(DataNodeInfo dataNodeInfo, FileDetailsForReplication fileDetailsForReplication)
            {
                this.dataNodeInfo = dataNodeInfo;
                this.fileDetailsForReplication = fileDetailsForReplication;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), fileDetailsForReplication.DestinationTcpPort));
            }

            public void Send()
            {
                var path = dataNodeInfo.StorageDirectory.FullName + "\\" + fileDetailsForReplication.FileName;

                byte[] fileNameBytes = Encoding.ASCII.GetBytes(fileDetailsForReplication.FileName);
                byte[] fileDataBytes = File.ReadAllBytes(path);
                byte[] fileNameLenght = BitConverter.GetBytes(fileNameBytes.Length);

                var bytesToSend = new byte[4 + fileNameBytes.Length + fileDataBytes.Length];

                fileNameLenght.CopyTo(bytesToSend, 0);
                fileNameBytes.CopyTo(bytesToSend, 4);
                fileDataBytes.CopyTo(bytesToSend, 4 + fileNameBytes.Length);

                socket.Send(bytesToSend);

                socket.Disconnect(false);
                socket.Close();
            }
        }
    }
}