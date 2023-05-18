﻿using System.IO.Compression;
using System.Net;

namespace Tranga;

public abstract class Connector
{
    internal abstract string downloadLocation { get; }
    public abstract string name { get; }
    public abstract Publication[] GetPublications(string publicationTitle = "");
    public abstract Chapter[] GetChapters(Publication publication);
    public abstract void DownloadChapter(Publication publication, Chapter chapter); //where to?
    internal abstract void DownloadImage(string url, string path);

    internal void DownloadChapter(string[] imageUrls, string outputFolderPath)
    {
        string tempFolder = Path.GetTempFileName();
        File.Delete(tempFolder);
        Directory.CreateDirectory(tempFolder);

        int chapter = 0;
        foreach(string imageUrl in imageUrls)
            DownloadImage(imageUrl, Path.Join(tempFolder, $"{chapter++}"));
        
        ZipFile.CreateFromDirectory(tempFolder, $"{outputFolderPath}.cbz");
    }

    internal class DownloadClient
    {
        private readonly TimeSpan _requestSpeed;
        private DateTime _lastRequest;
        static readonly HttpClient client = new HttpClient();

        public DownloadClient(uint delay)
        {
            _requestSpeed = TimeSpan.FromMilliseconds(delay);
            _lastRequest = DateTime.Now.Subtract(_requestSpeed);
        }
        
        public RequestResult MakeRequest(string url)
        {
            while((DateTime.Now - _lastRequest) < _requestSpeed)
                Thread.Sleep(10);
            _lastRequest = DateTime.Now;

            HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
            HttpResponseMessage response = client.Send(requestMessage);
            Stream resultString = response.IsSuccessStatusCode ? response.Content.ReadAsStream() : Stream.Null;
            return new RequestResult(response.StatusCode, resultString);
        }

        public struct RequestResult
        {
            public HttpStatusCode statusCode { get; }
            public Stream result { get; }

            public RequestResult(HttpStatusCode statusCode, Stream result)
            {
                this.statusCode = statusCode;
                this.result = result;
            }
        }
    }
}