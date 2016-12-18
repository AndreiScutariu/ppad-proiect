namespace DistributedFileSystem.DataNode
{
    using System.IO;

    public class DataNodeInfo
    {
        public int Id { get; set; }

        public int TcpPort { get; set; }

        public DirectoryInfo StorageDirectory { get; set; }
    }
}