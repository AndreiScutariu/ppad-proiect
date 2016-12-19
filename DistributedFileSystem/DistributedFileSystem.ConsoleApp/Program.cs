namespace DistributedFileSystem.ConsoleApp
{
    using System.Collections.Generic;
    using System.IO;

    using DistributedFileSystem.Common;
    using DistributedFileSystem.Master.FilesMetadata;

    using Newtonsoft.Json;

    internal class Program
    {
        private static void Main(string[] args)
        {
            IDictionary<string, Model> fileInfo = GetFileInfos();

            var serializedFileInfo = JsonConvert.SerializeObject(fileInfo);

            File.WriteAllText(Resources.MasterRepo, serializedFileInfo);
        }

        private static Dictionary<string, Model> GetFileInfos()
        {
            return new Dictionary<string, Model>
                       {
                           { "file_01", new Model { ReplicationLevel = 2, Version = "1.00" } },
                           { "file_02", new Model { ReplicationLevel = 2, Version = "1.00" } },
                           { "file_03", new Model { ReplicationLevel = 7, Version = "1.00" } },
                           { "file_04", new Model { ReplicationLevel = 3, Version = "1.00" } },
                           { "file_05", new Model { ReplicationLevel = 1, Version = "1.00" } },
                           { "file_06", new Model { ReplicationLevel = 4, Version = "1.00" } },
                           { "file_07", new Model { ReplicationLevel = 2, Version = "1.00" } },
                           { "file_08", new Model { ReplicationLevel = 4, Version = "1.00" } },
                           { "file_09", new Model { ReplicationLevel = 6, Version = "1.00" } },
                           { "file_10", new Model { ReplicationLevel = 8, Version = "1.00" } }
                       };
        }
    }
}