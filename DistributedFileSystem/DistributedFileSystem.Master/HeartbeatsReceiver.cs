namespace DistributedFileSystem.Master
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;
    using DistributedFileSystem.Master.DataNode;

    using Newtonsoft.Json;

    public class HeartbeatsReceiver
    {
        public Func<Dictionary<int, DataNodeInfo>, ConcurrentQueue<int>, Task> ReceiveHearbeatsCallback =
            async (nodesContainer, inactiveNodesContainer) =>
                {
                    var heartbeatsReceiver =
                        new UdpListener(new IPEndPoint(IPAddress.Any, Resources.MasterMulticastPort));

                    while (true)
                    {
                        var received = await heartbeatsReceiver.Receive();

                        var clientInfo = JsonConvert.DeserializeObject<ClientInfo>(received.Message);

                        var dataNodeMetaData = new DataNodeInfo
                                                   {
                                                       ClientInfo = clientInfo,
                                                       LastReceivedHeartBeat = DateTime.UtcNow.TimeOfDay
                                                   };

                        if (!nodesContainer.ContainsKey(dataNodeMetaData.ClientInfo.Id))
                        {
                            nodesContainer.Add(dataNodeMetaData.ClientInfo.Id, dataNodeMetaData);
                        }
                        else
                        {
                            nodesContainer[dataNodeMetaData.ClientInfo.Id] = dataNodeMetaData;
                        }

                        Console.WriteLine("received data from: {0}", clientInfo.Id);

                        RemoveInactiveNodes(nodesContainer, inactiveNodesContainer);
                    }
                };

        private static void RemoveInactiveNodes(
            IDictionary<int, DataNodeInfo> nodes,
            ConcurrentQueue<int> inactiveNodesContainer)
        {
            var timeSpan = DateTime.UtcNow.TimeOfDay.Subtract(TimeSpan.FromSeconds(1));

            foreach (KeyValuePair<int, DataNodeInfo> pair in
                nodes.Where(pair => pair.Value.LastReceivedHeartBeat < timeSpan))
            {
                var removedNode = nodes[pair.Key];

                if (!nodes.Remove(pair.Key))
                {
                    continue;
                }

                inactiveNodesContainer.Enqueue(removedNode.ClientInfo.Id);
                Console.WriteLine("client with id {0} was removed", removedNode.ClientInfo.Id);
            }
        }
    }
}