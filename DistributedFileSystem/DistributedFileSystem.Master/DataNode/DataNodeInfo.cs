namespace DistributedFileSystem.Master.DataNode
{
    using System;
    using System.Net;

    using DistributedFileSystem.Common;

    public class DataNodeInfo
    {
        public ClientInfo ClientInfo { get; set; }

        public IPEndPoint UdpEndpointAddres { get; set; }

        public TimeSpan LastReceivedHeartBeat { get; set; }
    }
}