namespace DistributedFileSystem.DataNode
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;

    internal class DataNodeEntryPoint
    {
        private static void Main(string[] args)
        {
            var nodeId = int.Parse(args[0]);

            var tcpPort = 20000 + nodeId;

            var dataNodeStoragePath = $"{Resources.StoragePath}DataNode_{nodeId}";

            if (!Directory.Exists(dataNodeStoragePath))
            {
                Directory.CreateDirectory(dataNodeStoragePath);
            }

            var storageDirectory = new DirectoryInfo(dataNodeStoragePath);

            var heartbeatsRecevier = UdpUser.ConnectTo(Resources.MasterMulticastIp, Resources.MasterMulticastPort);

            Task.Factory.StartNew(
                () =>
                    {
                        while (true)
                        {
                            List<string> myFiles = storageDirectory.GetFiles().Select(x => x.Name).ToList();

                            heartbeatsRecevier.Send(new ClientInfo { Id = nodeId, TcpPort = tcpPort, Files = myFiles });

                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        }
                    });

            Console.WriteLine("Press any key to close this data node.");
            Console.ReadLine();
            heartbeatsRecevier.Close();
        }
    }
}