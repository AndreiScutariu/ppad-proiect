namespace DistributedFileSystem.DataNode
{
    using System;
    using System.IO;
    using System.Threading;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;
    using DistributedFileSystem.DataNode.Components;

    internal class DataNodeEntryPoint
    {
        private static void Main(string[] args)
        {
            var nodeId = int.Parse(args[0]);

            var dataNodeStoragePath = $"{Resources.StoragePath}DataNode_{nodeId}";

            if (!Directory.Exists(dataNodeStoragePath))
            {
                Directory.CreateDirectory(dataNodeStoragePath);
            }

            var info = new DataNodeInfo
                           {
                               Id = nodeId,
                               TcpPort = 44400 + nodeId,
                               StorageDirectory = new DirectoryInfo(dataNodeStoragePath)
                           };

            var tokenSource = new CancellationTokenSource();

            var udpConnection = UdpUser.ConnectTo(Resources.MasterMulticastIp, Resources.MasterMulticastPort);

            var hearbeatsSender = new HeartbeatsSender(udpConnection, info, tokenSource.Token);
            hearbeatsSender.Start();

            var udpMessageReceiver = new MessageReceiver(udpConnection, info, tokenSource.Token);
            udpMessageReceiver.Start();

            var fileReceiver = new FileReceiver(info, tokenSource.Token);
            fileReceiver.Start();

            Console.WriteLine("press any key to close this data node");
            Console.ReadLine();
            tokenSource.Cancel();
            udpConnection.Close();
        }
    }
}