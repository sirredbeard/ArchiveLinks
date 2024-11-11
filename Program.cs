using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        string baseUrl = "https://www.determined.ai/blog/";
        var links = await GetLinksAsync(baseUrl);
        foreach (var link in links)
        {
            await SubmitToArchiveAsync(link);
        }
    }

    static async Task<List<string>> GetLinksAsync(string url)
    {
        var links = new List<string>();
        var response = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            var href = link.GetAttributeValue("href", string.Empty);
            if (href.StartsWith("/blog/"))
            {
                links.Add("https://www.determined.ai" + href);
            }
        }

        return links;
    }

    static async Task SubmitToArchiveAsync(string url)
    {
        var archiveUrl = "https://web.archive.org/save/" + url;
        var response = await client.GetAsync(archiveUrl);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Successfully submitted: {url}");
        }
        else
        {
            Console.WriteLine($"Failed to submit: {url}");
        }
    }
}