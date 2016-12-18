namespace DistributedFileSystem.Master
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DistributedFileSystem.Master.DataNode;
    using DistributedFileSystem.Master.FilesMetadata;

    public static class MasterEntryPoint
    {
        private static readonly Dictionary<string, Model> AllFilesMasterData = new Handler().GetMetadataForFiles();

        private static readonly Dictionary<int, DataNodeInfo> NodesContainer = new Dictionary<int, DataNodeInfo>();

        private static readonly ConcurrentQueue<int> InactiveNodesQueue = new ConcurrentQueue<int>();

        public static void Main(string[] args)
        {
            var heartbeatsReceiver = new HeartbeatsReceiver();
            Task.Run(() => heartbeatsReceiver.ReceiveHearbeatsCallback(NodesContainer, InactiveNodesQueue));

            Console.WriteLine("Waiting for connectictions ...");
            Console.ReadLine();
        }
    }
}