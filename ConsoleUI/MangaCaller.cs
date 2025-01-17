﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ConsoleUI.Models;
using System.Web;

namespace ConsoleUI
{
    public class MangaCaller
    {

        private static readonly string baseUrl = "https://api.mangadex.org";

        public static async Task SearchManga(HttpClient client, string title, int limit)
        {

            // Add querys here
            Dictionary<string, string> titleSearch = new Dictionary<string, string> {
                {"title", $"{title}" },

            };
            var encodedQuery = new FormUrlEncodedContent(titleSearch).ReadAsStringAsync().Result;   
            
            // Add headers here
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0"); //add more details about client sending req
          
            // Make Request
            using HttpResponseMessage res = await client.GetAsync($"{baseUrl}/manga?{encodedQuery}&limit={limit}");
            res.EnsureSuccessStatusCode();
            string resBody = await res.Content.ReadAsStringAsync();

            Root? model = JsonSerializer.Deserialize<Root>($""" {resBody} """);
            for (int i = 0; i < model?.data.Count; i++)
            {
               
                Console.WriteLine($"Title: {model?.data[i]?.attributes?.title?.en}, ---> Id: {model?.data[i].id}\n" );
            }

            // Write to console something like "copy id to search for chapter list"
            
        }

        public static async Task ChapterList(HttpClient client, string id)
        {
            
            string[] contentRating = { "safe", "suggestive", "erotica"};

            string queryString = $"contentRating[]=safe&contentRating[]=suggestive&contentRating[]=erotica&includeFutureUpdates=1&order[volume]=asc&order[chapter]=asc";

            string encodedValue = HttpUtility.UrlEncode(queryString);

            // Add headers here
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0"); //add details about client sending req

            // Make Request
            using HttpResponseMessage res = await client.GetAsync($"{baseUrl}/manga/{id}/feed?{queryString}");
            res.EnsureSuccessStatusCode();
            string resBody = await res.Content.ReadAsStringAsync();

            //should write list of chapters with ids in acsending order

            
            CRoot? chapterModel = JsonSerializer.Deserialize<CRoot>(resBody);

            // No need for loop, just add translatedLanguage array as query param
            for (int i = 0; i < chapterModel.data.Count; i++)
            {
                if (chapterModel.data[i].attributes.translatedLanguage == "en")
                {
                    Console.WriteLine($"Chapter: {chapterModel.data[i].attributes.chapter}\nTitle: {chapterModel.data[i].attributes.title}\nId: {chapterModel.data[i].id}\n");
                }
            }
            
        }

        // This function should get images for given chapter, download to file system + open a view to read chapter
        public static async Task ReadManga(HttpClient client, string id)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");

            using HttpResponseMessage res = await client.GetAsync($"{baseUrl}/at-home/server/{id}");
            res.EnsureSuccessStatusCode();
            string resBody = await res.Content.ReadAsStringAsync();
            

            CRoot? images = JsonSerializer.Deserialize<CRoot>(resBody);

            var host = images?.baseUrl;
            var chapterHash = images.chapter.hash;
            var data = images.chapter.data;
            Console.WriteLine(host);
            Console.WriteLine(chapterHash);
            Console.WriteLine(data);
            Console.WriteLine(resBody);

            // loop through data image list make calls and write data to file 
            // Read input from stream, write file to directory
            string directoryPath = $"C:\\Users\\ZPC\\MangaDex/{id}";
            Directory.CreateDirectory(directoryPath);

            for (int i = 0; i < data.Count; i++)
            {
                using HttpResponseMessage resp = await client.GetAsync($"{host}/data/{chapterHash}/{data[i]}");
                resp.EnsureSuccessStatusCode();
                byte[] chapImage = await resp.Content.ReadAsByteArrayAsync();
                string filePath = Path.Combine(directoryPath, $"image{i + 1}.jpg");
                await File.WriteAllBytesAsync(filePath, chapImage);

            }
            Console.WriteLine("finished");
        }
        

    }
}
