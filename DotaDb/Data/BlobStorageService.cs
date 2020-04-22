using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly IMemoryCache cache;

        public BlobStorageService(BlobServiceClient blobServiceClient, IMemoryCache cache)
        {
            this.blobServiceClient = blobServiceClient;
            this.cache = cache;
        }

        public async Task<IReadOnlyCollection<string>> GetFileFromStorageAsync(string containerName, string fileName)
        {
            if(!cache.TryGetValue(fileName, out List<string> contents))
            {
                // Cache miss, get contents
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);
                var download = await blobClient.DownloadAsync();

                contents = new List<string>();
                using (StreamReader sr = new StreamReader(download.Value.Content))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = await sr.ReadLineAsync();
                        contents.Add(line);
                    }
                }

                // Set in cache
                cache.Set(fileName, contents, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromDays(1)
                });
            }

            return new ReadOnlyCollection<string>(contents);
        }
    }
}