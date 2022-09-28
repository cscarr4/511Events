using _511Events.Models;
using Azure.Storage.Files.DataLake;
using System.Text;
using System.Diagnostics;
using Azure.Storage;

namespace _511Events.Logic
{
    public class EventGatherer
    {
        private const string baseUri = "https://data.ny.gov/resource/ah74-pg4w.json";
        private const string orderBy = "create_time";

        private readonly ILogger<EventGatherer> logger;
        private readonly IConfiguration configuration;
        private readonly DataDownloader downloader;
        private readonly DataSaver saver;
        private readonly ProgressHub progressHub;
        private readonly SemaphoreSlim semaphore;
        private readonly List<Task<long>> workTasks = new List<Task<long>>();
        private readonly List<Task> progressTasks = new List<Task>();
        private readonly int pageSize, maxSimultaneousPages;
        private readonly string datasetName;

        public EventGatherer(
            ILogger<EventGatherer> logger,
            IConfiguration configuration,
            DataDownloader downloader, 
            DataSaver saver,
            ProgressHub progressHub)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.downloader = downloader;
            this.saver = saver;
            this.progressHub = progressHub;

            pageSize = configuration.GetValue<int>("pageSize");
            maxSimultaneousPages = configuration.GetValue<int>("maxSimultaneousPages");
            datasetName = configuration.GetValue<string>("datasetName");

            semaphore = new SemaphoreSlim(maxSimultaneousPages, maxSimultaneousPages);
        }

        public async Task GatherEvents(string accountName, string accountKey)
        {
            var sw = new Stopwatch();
            sw.Start();

            await progressHub.UpdateText("Initializing");

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var uri = new Uri($"https://{accountName}.dfs.core.windows.net");
            var serviceClient = new DataLakeServiceClient(uri, credential);
            var fileSystemClient = serviceClient.GetFileSystemClient(datasetName);

            var countUri = $"{baseUri}?$query=SELECT count(*)";
            var countResult = await downloader.GetCountResultAsync(countUri);
            var recordCount = countResult?.Count ?? 0;

            var pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
                pageCount++;

            for (int page = 0; page < pageCount; page++)
                workTasks.Add(GatherPage(page, fileSystemClient));

            foreach (var wt in workTasks)
            {
                var pt = wt.ContinueWith(async t =>
                {
                    var pct = workTasks.Count(tsk => tsk.IsCompletedSuccessfully) * 100 / workTasks.Count();
                    await progressHub.UpdateFill(pct);
                });

                progressTasks.Add(pt);
            }

            await progressHub.UpdateText("Gathering Data - please wait");

            await Task.WhenAll(progressTasks);

            await progressHub.UpdateText("Finished");

            sw.Stop();

            var totalFileSize = workTasks.Sum(t => t.Result);

            logger.LogInformation($"Total bytes saved: {totalFileSize}");
            logger.LogInformation($"Elapsed Time: {sw.Elapsed}");
        }

        private async Task<long> GatherPage(int pageNumber, DataLakeFileSystemClient fileSystemClient)
        {
            try
            {
                await semaphore.WaitAsync();
                logger.LogInformation($"Gathering page {pageNumber}");

                var uri = GetDownloadUri(pageNumber);

                var records = await downloader.GetTrafficEventRecordsAsync(uri);

                if (records == null)
                    return 0;

                var fileSize = await saver.SaveRecordsAsync(records, fileSystemClient);

                logger.LogInformation($"Finished gathering page {pageNumber}; Saved {fileSize} bytes");

                return fileSize;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Gather Page {pageNumber}");
                return 0;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private string GetDownloadUri(int pageNumber)
        {
            var systemFields = $"$select=:*, *";
            var limit = $"$limit={pageSize}";
            var offset = $"$offset={pageNumber * pageSize}";
            var order = $"$order={orderBy}";
            var uri = $"{baseUri}?{systemFields}&{limit}&{offset}&{order}";
            return uri;            
        }
    }
}
