using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    private static readonly HttpClient client = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(2) // Increase timeout duration
    };

    static async Task Main(string[] args)
    {
        var baseUrls = new List<string> { "https://www.determined.ai/blog/" };
        for (int i = 2; i <= 12; i++)
        {
            baseUrls.Add($"https://www.determined.ai/{i}/blog/");
        }

        var allLinks = new List<string>();
        foreach (var baseUrl in baseUrls)
        {
            try
            {
                var links = await GetLinksAsync(baseUrl);
                allLinks.AddRange(links);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Failed to fetch links from {baseUrl}: {e.Message}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"Request to {baseUrl} timed out: {e.Message}");
            }
        }

        foreach (var link in allLinks)
        {
            try
            {
                await SubmitToArchiveAsync(link);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Failed to submit {link}: {e.Message}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"Submission of {link} timed out: {e.Message}");
            }
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