using _511Events.Models;
using FileHelpers;
using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using System.Collections.Concurrent;

namespace _511Events.Logic
{
    public class DataSaver
    {
        private readonly ILogger<DataSaver> logger;
        private readonly FileHelperEngine<TrafficEventModel> engine;
        private readonly ConcurrentDictionary<string, object> fileLocks = new ConcurrentDictionary<string, object>();

        public DataSaver(
            ILogger<DataSaver> logger, 
            FileHelperEngine<TrafficEventModel> engine)
        {
            this.logger = logger;
            this.engine = engine;
        }

        public async Task<long> SaveRecordsAsync(List<TrafficEventModel> records, DataLakeFileSystemClient fileSystemClient)
        {
            var recordsByDate = records.GroupBy(r => DateOnly.FromDateTime(r.CreateTime)).ToDictionary(r => r.Key, r => r.ToList());
            var tasks = new List<Task<long>>();

            foreach (var date in recordsByDate.Keys)
                tasks.Add(SaveDateRecordsAsync(fileSystemClient, date, recordsByDate[date]));

            await Task.WhenAll(tasks);

            var totalContentSize = tasks.Sum(t => t.Result);
            return totalContentSize;
        }

        private async Task<long> SaveDateRecordsAsync(DataLakeFileSystemClient fileSystemClient, DateOnly date, List<TrafficEventModel> records)
        {            
            var content = engine.WriteString(records);
            var path = $"{date.Year}/{date.Month.ToString("00")}";
            var fileName = $"{date.Day.ToString("00")}_trafficData.csv";
            var directoryClient = fileSystemClient.GetDirectoryClient(path);            
            var fileClient = directoryClient.GetFileClient(fileName);

            if (!fileLocks.TryGetValue(fileClient.Path, out var fileLock))
            {
                fileLock = new object();
                if (!fileLocks.TryAdd(path, fileLock))
                    fileLock = fileLocks[path];
            }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
                await writer.FlushAsync();

                var contentSize = stream.Length;
                stream.Position = 0;

                lock (fileLock)
                {
                    try
                    {                        
                        if (!fileClient.Exists()) 
                            fileClient.Create();

                        var properties = fileClient.GetProperties();
                        var fileSize = properties.Value.ContentLength;
                        var position = contentSize + fileSize; // apparently the file client's Flush method expects the position parameter to represent the total expected file size after it writes its data.  Who knew?!

                        fileClient.Append(stream, offset: fileSize);
                        fileClient.Flush(position: position);

                        return contentSize;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error saving data for {date}");
                        return 0;
                    }
                }
            }
        }
    }
}
