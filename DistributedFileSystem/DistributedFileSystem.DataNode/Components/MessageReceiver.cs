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
                                byte[] received = await udpUser.ReceiveBytes();

                                var receviedCommandType = received[0];
                                var commandDetails = new byte[received.Length - 1];

                                Array.Copy(received, 1, commandDetails, 0, received.Length - 1);

                                if (receviedCommandType == 0x01)
                                {
                                    var fileDetails =
                                        JsonConvert.DeserializeObject<ReplicateFile>(
                                            Encoding.ASCII.GetString(commandDetails, 0, commandDetails.Length));

                                    var fileSender = new FileSender(dataNodeInfo, fileDetails);
                                    fileSender.Send();
                                }

                                if (receviedCommandType == 0x02)
                                {
                                    var fileDetails =
                                        JsonConvert.DeserializeObject<DeleteFile>(
                                            Encoding.ASCII.GetString(commandDetails, 0, commandDetails.Length));

                                    File.Delete(dataNodeInfo.StorageDirectory.FullName + "\\" + fileDetails.FileName);
                                    Console.WriteLine($"file {fileDetails.FileName} was deleted");
                                }

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

            private readonly ReplicateFile replicateFile;

            private readonly Socket socket;

            public FileSender(DataNodeInfo dataNodeInfo, ReplicateFile replicateFile)
            {
                this.dataNodeInfo = dataNodeInfo;
                this.replicateFile = replicateFile;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socket.Connect(
                    new IPEndPoint(IPAddress.Parse("127.0.0.1"), replicateFile.DestinationTcpPort));
            }

            public void Send()
            {
                var path = dataNodeInfo.StorageDirectory.FullName + "\\" + replicateFile.FileName;

                byte[] fileNameBytes = Encoding.ASCII.GetBytes(replicateFile.FileName);
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