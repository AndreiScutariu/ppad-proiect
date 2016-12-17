namespace DistributedFileSystem.DataNode
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;

    internal class DataNodeEntryPoint
    {
        private static void Main(string[] args)
        {
            var nodeId = int.Parse(args[0]);

            var port = 20000 + nodeId;

            var dataNodeStoragePath = $"{Resources.StoragePath}DataNode_{nodeId}";

            if (!Directory.Exists(dataNodeStoragePath))
            {
                Directory.CreateDirectory(dataNodeStoragePath);
            }

            var heartbeatsRecevier = UdpUser.ConnectTo(Resources.MasterMulticastIp, Resources.MasterMulticastPort);

            Task.Factory.StartNew(
                () =>
                    {
                        while (true)
                        {
                            heartbeatsRecevier.Send(new ClientInfo { Id = nodeId, Port = port });

                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        }
                    });

            Console.WriteLine("Press any key to close this data node.");
            Console.ReadLine();
            heartbeatsRecevier.Close();
        }
    }
}