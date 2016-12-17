namespace DistributedFileSystem.Master
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;

    using Newtonsoft.Json;

    public class HeartbeatsReceiver
    {
        public Func<object, Task> ReceiveHearbeatsCallback = async nodesParam =>
            {
                var heartbeatsReceiver = new UdpListener(new IPEndPoint(IPAddress.Any, Resources.MasterMulticastPort));

                var nodes = (Dictionary<IPAddress, DataNodeMetaData>)nodesParam;

                while (true)
                {
                    var received = await heartbeatsReceiver.Receive();

                    var clientInfo = JsonConvert.DeserializeObject<ClientInfo>(received.Message);

                    var dataNodeMetaData = new DataNodeMetaData
                                               {
                                                   ClientInfo = clientInfo,
                                                   LastReceivedHeartBeat = DateTime.UtcNow.TimeOfDay
                                               };

                    if (!nodes.ContainsKey(received.From.Address))
                    {
                        nodes.Add(received.From.Address, dataNodeMetaData);
                    }
                    else
                    {
                        nodes[received.From.Address] = dataNodeMetaData;
                    }

                    Console.WriteLine("received data from: {0}", clientInfo.Id);

                    RemoveInactiveNodes(nodes);
                }
            };

        private static void RemoveInactiveNodes(IDictionary<IPAddress, DataNodeMetaData> nodes)
        {
            var timeSpan = DateTime.UtcNow.TimeOfDay.Subtract(TimeSpan.FromSeconds(1));

            foreach (KeyValuePair<IPAddress, DataNodeMetaData> pair in
                nodes.Where(pair => pair.Value.LastReceivedHeartBeat < timeSpan && pair.Key != null))
            {
                var removedNode = nodes[pair.Key];

                if (nodes.Remove(pair.Key))
                {
                    Console.WriteLine("client with id {0} was removed", removedNode.ClientInfo.Id);
                }
            }
        }
    }

    public static class MasterEntryPoint
    {
        private static readonly Dictionary<IPAddress, DataNodeMetaData> NodesContainer =
            new Dictionary<IPAddress, DataNodeMetaData>();

        public static void Main(string[] args)
        {
            var heartbeatsReceiver = new HeartbeatsReceiver();

            Task.Factory.StartNew(heartbeatsReceiver.ReceiveHearbeatsCallback, NodesContainer);

            Console.WriteLine("Waiting for connectictions ...");
            Console.ReadLine();
        }
    }
}