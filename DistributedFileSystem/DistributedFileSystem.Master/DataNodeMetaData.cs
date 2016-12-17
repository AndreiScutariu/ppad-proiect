namespace DistributedFileSystem.Master
{
    using System;

    using DistributedFileSystem.Common;

    public class DataNodeMetaData
    {
        public ClientInfo ClientInfo { get; set; }

        public TimeSpan LastReceivedHeartBeat { get; set; }
    }
}