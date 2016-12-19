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

        public void Callback(ConcurrentDictionary<int, DataNodeInfo> nodesContainer, CancellationToken token)
        {
            while (true)
            {
                try
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

                    Thread.Sleep(TimeSpan.FromSeconds(4));

                    ResetIndex();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void ReplicateFiles(IDictionary<int, DataNodeInfo> nodesContainer)
        {
            List<int> allNodes = nodesContainer.Keys.ToList();

            foreach (KeyValuePair<string, ReplicatedFileData> replicatedFileData in replicatedFileIndex)
            {
                var fileName = replicatedFileData.Key;
                var fileData = replicatedFileData.Value;

                if (fileData.ActualReplicationLevel == 0)
                {
                    Console.WriteLine($"{fileName} is not replicated in any node yet");
                    continue;
                }

                if (fileData.ActualReplicationLevel == fileData.MasterReplicationLevel)
                {
                    continue;
                }

                if (fileData.ActualReplicationLevel > fileData.MasterReplicationLevel)
                {
                    var randomSourceId = fileData.NodesWhereIsReplicated.Random();
                    Console.WriteLine($"{fileName} - delete from {nodesContainer[randomSourceId].ClientInfo.Id}");
                    udpListener.Reply(
                        new DeleteFile { FileName = fileName },
                        nodesContainer[randomSourceId].UdpEndpointAddres);
                    continue;
                }

                var missingReplications = fileData.MasterReplicationLevel - fileData.ActualReplicationLevel;

                if (missingReplications > 0)
                {
                    Console.WriteLine($"found missing replications {missingReplications} for {fileName}");

                    var destinations = new List<int>();

                    for (var i = 0; i < missingReplications; i++)
                    {
                        try
                        {
                            List<int> availableNodes = fileData.NodesWhereIsReplicated.ToList();
                            var randomSourceId = availableNodes.Random();

                            List<int> availableDestinations =
                                allNodes.Except(fileData.NodesWhereIsReplicated).Except(destinations).ToList();
                            availableDestinations.Print("available destinations ");
                            var randomDestinationId = availableDestinations.Random();

                            destinations.Add(randomDestinationId);

                            Console.WriteLine(
                                $"{fileName}: replicated from {nodesContainer[randomSourceId].ClientInfo.Id} to {nodesContainer[randomDestinationId].ClientInfo.Id}");

                            udpListener.Reply(
                                new ReplicateFile
                                    {
                                        DestinationTcpPort =
                                            nodesContainer[randomDestinationId].ClientInfo.TcpPort,
                                        FileName = fileName
                                    },
                                nodesContainer[randomSourceId].UdpEndpointAddres);

                            Thread.Sleep(TimeSpan.FromMilliseconds(50));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    PrintInfos();
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
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (KeyValuePair<string, ReplicatedFileData> replicatedFilePair in replicatedFileIndex)
            {
                var message =
                    $"{replicatedFilePair.Key} - replication level: {replicatedFilePair.Value.MasterReplicationLevel} "
                    + $", actual replication level: {replicatedFilePair.Value.ActualReplicationLevel}. replicated in nodes: ";
                replicatedFilePair.Value.NodesWhereIsReplicated.Print(message);
            }
            Console.ResetColor();
            Console.WriteLine();
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

        public static void Print(this List<int> nodes, string message)
        {
            Console.Write(message);
            foreach (var node in nodes)
            {
                Console.Write(node + " ");
            }
            Console.WriteLine();
        }
    }
}