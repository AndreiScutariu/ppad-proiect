namespace DistributedFileSystem.Master.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;
    using DistributedFileSystem.Master.DataNode;

    using Newtonsoft.Json;

    public class HeartbeatsReceiver
    {
        private readonly UdpListener udpListener;

        public HeartbeatsReceiver(UdpListener udpListener)
        {
            this.udpListener = udpListener;
        }

        public async void Callback(ConcurrentDictionary<int, DataNodeInfo> nodesContainer, CancellationToken token)
        {
            while (true)
            {
                var received = await udpListener.Receive();

                var clientInfo = JsonConvert.DeserializeObject<ClientInfo>(received.Message);

                var dataNodeMetaData = new DataNodeInfo
                                           {
                                               ClientInfo = clientInfo,
                                               UdpEndpointAddres = received.From,
                                               LastReceivedHeartBeat = DateTime.UtcNow.TimeOfDay
                                           };

                if (!nodesContainer.ContainsKey(dataNodeMetaData.ClientInfo.Id))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"client with id {clientInfo.Id} has joined");
                    Console.ResetColor();

                    nodesContainer.TryAdd(dataNodeMetaData.ClientInfo.Id, dataNodeMetaData);
                }
                else
                {
                    nodesContainer.TryUpdate(
                        dataNodeMetaData.ClientInfo.Id,
                        dataNodeMetaData,
                        nodesContainer[dataNodeMetaData.ClientInfo.Id]);
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}