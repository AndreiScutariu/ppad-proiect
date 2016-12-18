namespace DistributedFileSystem.Master
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;
    using DistributedFileSystem.Master.Components;
    using DistributedFileSystem.Master.DataNode;
    using DistributedFileSystem.Master.FilesMetadata;

    public static class MasterEntryPoint
    {
        private static readonly Dictionary<string, Model> AllFilesMasterData = new Handler().GetMetadataForFiles();

        private static readonly ConcurrentDictionary<int, DataNodeInfo> NodesContainer = new ConcurrentDictionary<int, DataNodeInfo>();

        private static readonly ConcurrentQueue<int> InactiveNodesQueue = new ConcurrentQueue<int>();

        public static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var listener = new UdpListener(new IPEndPoint(IPAddress.Any, Resources.MasterMulticastPort));

            var heartbeatsReceiver = new HeartbeatsReceiver(listener);
            var replicationLevelDetector = new ReplicationLevelHandler(listener);
            var inactiveNodesRemover = new InactiveNodesDetector();

            replicationLevelDetector.InitIndex(AllFilesMasterData);

            Task.Run(() => heartbeatsReceiver.Callback(NodesContainer, InactiveNodesQueue, cancellationToken), cancellationToken);
            Task.Run(() => inactiveNodesRemover.Callback(NodesContainer, InactiveNodesQueue, cancellationToken), cancellationToken);
            Task.Run(() => replicationLevelDetector.Callback(NodesContainer, InactiveNodesQueue, cancellationToken), cancellationToken);

            Console.WriteLine("waiting for connectictions; press any key to close the master");
            Console.ReadLine();
            listener.Close();
            cancellationTokenSource.Cancel();
        }
    }
}