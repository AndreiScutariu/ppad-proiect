﻿namespace DistributedFileSystem.Common
{
    using System.Collections.Generic;
    using System.Net;

    public class ClientInfo
    {
        public int Id { get; set; }

        public string Ip { get; set; }

        public int TcpPort { get; set; }

        public List<string> Files { get; set; }
    }
}