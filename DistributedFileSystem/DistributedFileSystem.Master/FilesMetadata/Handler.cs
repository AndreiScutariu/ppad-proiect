namespace DistributedFileSystem.Master.FilesMetadata
{
    using System.Collections.Generic;
    using System.IO;

    using DistributedFileSystem.Common;

    using Newtonsoft.Json;

    public class Handler
    {
        public Dictionary<string, Model> GetMetadataForFiles()
        {
            var fileContent = File.ReadAllText(Resources.MasterRepo);

            return JsonConvert.DeserializeObject<Dictionary<string, Model>>(fileContent);
        }
    }
}