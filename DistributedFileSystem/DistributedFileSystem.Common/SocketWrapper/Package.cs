namespace DistributedFileSystem.Common.SocketWrapper
{
    using System.Net;

    public struct Package
    {
        public IPEndPoint From;

        public string Message;
    }
}