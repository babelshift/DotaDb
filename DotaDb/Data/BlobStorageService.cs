using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task<IReadOnlyCollection<string>> GetFileFromStorageAsync(string containerName, string fileName)
        {
            var contents = await File.ReadAllLinesAsync($"Data/vdf/{fileName}");

            //var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            //var blobClient = blobContainerClient.GetBlobClient(fileName);
            //var download = await blobClient.DownloadAsync();

            //var contents = new List<string>();
            //using (StreamReader sr = new StreamReader(download.Value.Content))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        string line = await sr.ReadLineAsync();
            //        contents.Add(line);
            //    }
            //}

            return new ReadOnlyCollection<string>(contents);
        }
    }
}