namespace DistributedFileSystem.Master.DataNode
{
    using System;

    using DistributedFileSystem.Common;

    public class DataNodeInfo
    {
        public ClientInfo ClientInfo { get; set; }

        public TimeSpan LastReceivedHeartBeat { get; set; }
    }
}