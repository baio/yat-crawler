using HtmlAgilityPack;
using Microsoft.WindowsAzure.Jobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yat_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        static void ParseBlob([BlobInput("yat-moscow-subtrains/{name}")]TextReader inputText)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.Load("../../data/page1.txt");
            doc.LoadHtml(inputText.ReadToEnd());

            //[@class='b-timetable__row']
            var title = doc.DocumentNode.SelectSingleNode("//h1[@class='b-page-title__title']").InnerText;
            var trainNum = title.Split(',')[0].Trim();
            var trainRoute = title.Split(',')[1].Trim();
            var dateTypeRu = doc.DocumentNode.SelectSingleNode("//div[@class='b-page-title__caution']//strong").InnerText;
            var dateType = "everyday";
            if (dateTypeRu.Contains("по будням"))
            {
                dateType = "workday";
            }
            else if (dateTypeRu.Contains("по выходным"))
            {
                dateType = "weekend";
            }
            var rows = doc.DocumentNode.SelectNodes("//tr").Where(p => p.Attributes["class"] != null && p.Attributes["class"].Value.Contains("b-timetable__row"));

            var cells = rows.Select(p => p.ChildNodes.Where(s => s.Name == "td").Select(s => s.InnerText).ToArray());

            var rawStops = cells.Select(p => new
                {
                    name = p[0],
                    arrival = p[1],
                    duration = p[2],
                    departure = p[3],
                    span = p[4]
                }
            );

            var stops = rawStops.Select(p => new
            {
                name = p.name,
                arrival = string.IsNullOrEmpty(p.arrival) || p.arrival == "&nbsp" ? null : p.arrival,
                departure = string.IsNullOrEmpty(p.departure) || p.departure == "&nbsp" ? null : p.departure,
            })
            .Where(p => p.arrival != null && p.departure != null)
            .Select(p => new
            {
                name = p.name,
                departure = p.departure != null ? p.departure.Split(',')[0].Trim() : null,
                arrival = p.departure != null ? p.departure.Split(',')[0].Trim() : null
            })
            .Select(p => new
            {
                name = p.name,
                time = float.Parse((p.departure != null ? p.departure : p.arrival).Replace(":", "."))
            });


            var file = new
            {
                name = title,
                lang = "ru",
                user = "yat-crawler",
                locations = new[] { new { key = "mow", country = "россия", region = "московская область", city = "москва" } },
                stops = stops.Select(p => new { key = p.name, locationKey = "mow", name = p.name }).ToArray(),
                route = new
                {
                    name = trainRoute,
                    type = "suburb",
                    price = 0,
                    transport = "subtrain",
                    from = new DateTime(2014, 1, 1),
                    dateType = dateType,
                    stops = stops.Select(p => new { 
                        stopKey = p.name,
                        lines = new [] { new { times = new [] { p.time} } }
                    }).ToArray()
                }
            };

            var client = new MongoClient("mongodb://baio.noip.me:37017");
            var server = client.GetServer();
            var database = server.GetDatabase("sah");
            var collection = database.GetCollection<dynamic>("files");

            collection.Insert(file); 
        }
    }
}
