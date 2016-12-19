namespace DistributedFileSystem.Master.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DistributedFileSystem.Master.DataNode;

    public class InactiveNodesDetector
    {
        public void Callback(
            ConcurrentDictionary<int, DataNodeInfo> nodesContainer,
            CancellationToken token)
        {
            while (true)
            {
                var timeSpan = DateTime.UtcNow.TimeOfDay.Subtract(TimeSpan.FromSeconds(1));

                foreach (KeyValuePair<int, DataNodeInfo> pair in
                    nodesContainer.Where(pair => pair.Value.LastReceivedHeartBeat < timeSpan))
                {
                    DataNodeInfo removedNode;

                    if (!nodesContainer.TryRemove(pair.Key, out removedNode))
                    {
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("client with id {0} was removed", removedNode.ClientInfo.Id);
                    Console.ResetColor();
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}