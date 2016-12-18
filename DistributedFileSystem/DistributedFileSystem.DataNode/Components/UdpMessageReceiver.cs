namespace DistributedFileSystem.DataNode.Components
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common.SocketWrapper.Udp;

    internal class UdpMessageReceiver
    {
        private readonly UdpUser udpUser;

        private readonly DataNodeInfo dataNodeInfo;

        private readonly CancellationToken token;

        public UdpMessageReceiver(UdpUser udpUser, DataNodeInfo dataNodeInfo, CancellationToken token)
        {
            this.udpUser = udpUser;
            this.dataNodeInfo = dataNodeInfo;
            this.token = token;
        }

        public void StartReceiveMessages()
        {
            Task.Factory.StartNew(
                async () =>
                    {
                        while (true)
                        {
                            var received = await udpUser.Receive();
                            Console.WriteLine(received.Message);

                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    },
                token);
        }
    }
}