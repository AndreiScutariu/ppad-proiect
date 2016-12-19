namespace DistributedFileSystem.Common
{
    public class ReplicateFile
    {
        public string FileName { get; set; }

        public int DestinationTcpPort { get; set; }
    }

    public class DeleteFile
    {
        public string FileName { get; set; }
    }
}