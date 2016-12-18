namespace DistributedFileSystem.Master.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;
    using DistributedFileSystem.Master.DataNode;
    using DistributedFileSystem.Master.FilesMetadata;

    public class ReplicationLevelHandler
    {
        private readonly UdpListener udpListener;

        private readonly Dictionary<string, ReplicatedFileData> replicatedFileIndex =
            new Dictionary<string, ReplicatedFileData>();

        public ReplicationLevelHandler(UdpListener udpListener)
        {
            this.udpListener = udpListener;
        }

        public void InitIndex(Dictionary<string, Model> allFilesMasterData)
        {
            foreach (KeyValuePair<string, Model> keyValuePair in allFilesMasterData)
            {
                replicatedFileIndex.Add(
                    keyValuePair.Key,
                    new ReplicatedFileData
                        {
                            MasterReplicationLevel = keyValuePair.Value.ReplicationLevel,
                            ActualReplicationLevel = 0,
                            NodesWhereIsReplicated = new List<int>()
                        });
            }
        }

        public void Callback(
            ConcurrentDictionary<int, DataNodeInfo> nodesContainer,
            ConcurrentQueue<int> inactiveNodesQueue,
            CancellationToken token)
        {
            while (true)
            {
                foreach (KeyValuePair<int, DataNodeInfo> node in nodesContainer)
                {
                    var clientInfo = node.Value.ClientInfo;

                    foreach (var replicatedFileData in clientInfo.Files.Select(file => replicatedFileIndex[file]))
                    {
                        replicatedFileData.ActualReplicationLevel++;
                        replicatedFileData.NodesWhereIsReplicated.Add(clientInfo.Id);
                    }
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                PrintInfos();

                ReplicateFiles(nodesContainer);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                ResetIndex();
            }
        }

        private void ReplicateFiles(ConcurrentDictionary<int, DataNodeInfo> nodesContainer)
        {
            List<int> allNodes = nodesContainer.Keys.ToList();

            foreach (KeyValuePair<string, ReplicatedFileData> replicatedFileData in replicatedFileIndex)
            {
                var file = replicatedFileData.Key;
                var fileData = replicatedFileData.Value;

                if (fileData.ActualReplicationLevel >= fileData.MasterReplicationLevel)
                {
                    continue;
                }

                try
                {
                    var randomSourceId = fileData.NodesWhereIsReplicated.Random();
                    var randomDestinationId = allNodes.Except(fileData.NodesWhereIsReplicated).ToList().Random();

                    Console.WriteLine(
                        $"{file} - {nodesContainer[randomSourceId].ClientInfo.Id} -> {nodesContainer[randomDestinationId].ClientInfo.Id}");

                    udpListener.Reply(
                        new FileDetailsForReplication
                            {
                                DestinationTcpPort = nodesContainer[randomDestinationId].ClientInfo.TcpPort,
                                FileName = file
                            },
                        nodesContainer[randomSourceId].UdpEndpointAddres);
                }
                catch (IndexOutOfRangeException)
                {
                }
                catch (KeyNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void ResetIndex()
        {
            foreach (KeyValuePair<string, ReplicatedFileData> replicatedFilePair in replicatedFileIndex)
            {
                replicatedFilePair.Value.NodesWhereIsReplicated = new List<int>();
                replicatedFilePair.Value.ActualReplicationLevel = 0;
            }
        }

        private void PrintInfos()
        {
            foreach (KeyValuePair<string, ReplicatedFileData> replicatedFilePair in replicatedFileIndex)
            {
                Console.Write(
                    $"{replicatedFilePair.Key} - ReplicationLevel: {replicatedFilePair.Value.MasterReplicationLevel} "
                    + $", Actual ReplicationLevel: {replicatedFilePair.Value.ActualReplicationLevel}. Replicated in nodes:");
                foreach (var c in replicatedFilePair.Value.NodesWhereIsReplicated)
                {
                    Console.Write($" {c}");
                }
                Console.WriteLine();
            }
        }

        private class ReplicatedFileData
        {
            public int MasterReplicationLevel { get; set; }

            public int ActualReplicationLevel { get; set; }

            public List<int> NodesWhereIsReplicated { get; set; }
        }
    }

    public static class ListExtentions
    {
        public static int Random(this List<int> nodes)
        {
            var randId = new Random().Next(0, nodes.Count);

            return nodes[randId];
        }
    }
}