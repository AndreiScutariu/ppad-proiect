namespace DistributedFileSystem.DataNode.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Common.SocketWrapper.Udp;

    internal class HeartbeatsSender
    {
        private readonly UdpUser udpUser;

        private readonly DataNodeInfo dataNodeInfo;

        private readonly CancellationToken token;

        public HeartbeatsSender(UdpUser udpUser, DataNodeInfo dataNodeInfo, CancellationToken token)
        {
            this.udpUser = udpUser;
            this.dataNodeInfo = dataNodeInfo;
            this.token = token;
        }

        public void StartSendHeartbeats()
        {
            Task.Factory.StartNew(
                () =>
                    {
                        while (true)
                        {
                            List<string> myFiles = dataNodeInfo.StorageDirectory.GetFiles().Select(x => x.Name).ToList();

                            udpUser.Send(
                                new ClientInfo { Id = dataNodeInfo.Id, TcpPort = dataNodeInfo.TcpPort, Files = myFiles });

                            Thread.Sleep(TimeSpan.FromMilliseconds(500));

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