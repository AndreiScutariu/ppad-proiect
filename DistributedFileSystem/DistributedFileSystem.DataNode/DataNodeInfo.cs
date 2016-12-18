namespace DistributedFileSystem.DataNode
{
    using System.IO;

    internal class DataNodeInfo
    {
        public int Id { get; set; }

        public int TcpPort { get; set; }

        public DirectoryInfo StorageDirectory { get; set; }
    }
}