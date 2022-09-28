using _511Events.Models;
using System.Net;

namespace _511Events.Logic
{
    public class DataDownloader
    {
        private const string appToken = "Q6g74eGuHgtNIqhAfodCjzIJU";
        private readonly ILogger<DataDownloader> logger;

        public DataDownloader(ILogger<DataDownloader> logger)
        {
            this.logger = logger;
        }

        //public async Task<byte[]?> GetDataAsync(string sourceUri)
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Add("X-App-Token", appToken);
        //            var uri = new Uri(sourceUri);
        //            var bytes = await client.GetByteArrayAsync(uri);
        //            return bytes;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"An unexpected error occurred while trying to download data from {sourceUri}");
        //        return null;
        //    }
        //}
        public async Task<CountResult?> GetCountResultAsync(string sourceUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                    var uri = new Uri(sourceUri);
                    var countResults = await client.GetFromJsonAsync<List<CountResult>>(uri);
                    return countResults?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred while trying to download data from {sourceUri}");
                return null;
            }
        }

        public async Task<List<TrafficEventModel>?> GetTrafficEventRecordsAsync(string sourceUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-App-Token", appToken);
                    var uri = new Uri(sourceUri);
                    var trafficEvents = await client.GetFromJsonAsync<List<TrafficEventModel>>(uri);

                    if (trafficEvents == null)
                        return trafficEvents;

                    foreach (var te in trafficEvents)
                    {
                        te.EventDescription = te.EventDescription?.Replace("\"", "") ?? string.Empty;
                        te.FacilityName = te.FacilityName?.Replace("\"", " ") ?? string.Empty;
                    }

                    return trafficEvents;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred while trying to download data from {sourceUri}");
                return null;
            }
        }
    }
}
